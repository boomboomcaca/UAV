import chartUtils, {
  combineData,
  hex2RGBA,
  gradientColors,
} from "./chartUtils";

/**
 * 线图
 */
class DPX {
  // 最大绘制点数
  #maxPoint = 2401;
  // 参考数据和 maxPoint 一样长，这样可用避免重复new Array而节省性能
  #renderReferArray = new Array(2401);
  //
  #series = {};
  // 上一次数据长度
  #prevDataLen = 0;

  /**
   * 辅助类
   * @type {chartUtils}
   */
  #utils;
  // 2022-7-22 绘制顺序处理，线画bar，其它根据数据顺序，先来先画
  #drawOrder = { barSeries: undefined, otherSeries: [] };
  // 电平对应Y轴pixel
  #yPositions = {};

  /**
   * @type {OffscreenCanvas}
   */
  #offscreenCanvas;
  /**
   * @type {CanvasRenderingContext2D}
   */
  #offCtx;

  #dataBuffers = [];
  #imageDatas = [];
  #firstFrameTime = new Date().getTime();
  #grayColor = "#000000";

  // 瀑布图色带颜色，渐变后
  #colors = [];

  // 柱状图绘制数据，缓存下来用于多图表绘制
  /**
   * 构造函数
   * @param {HTMLElement} container 容器
   * @param {Array<string>} pallete
   * onSizeChange: ()=>{}
   * canvas宽高（像素）变化事件
   * zoomInside:true
   * true内部根据像素宽度/高度处理数据抽取，且可调用zoom方法设置缩放位置；
   * false则内部不进行数据抽取，外部需要订阅onSizeChange回调根据像素宽度/高度先进行数据抽取，如果数据不满足要求，则不进行绘制
   * barFront:false
   * true bar绘制在顶层
   * false bar绘制在底层
   * @param {{onSizeChange:Function<Number,Number>, zoomInside:boolean, barFront:boolean,bgColor:String}} optionMore
   */
  constructor(container, pallete, optionMore) {
    this.#utils = new chartUtils(container, optionMore, (width, height) => {
      this.#initYPixel();
      // 更新设置重绘
      this.#drawData();

      this.#offscreenCanvas = new OffscreenCanvas(width, height);
      this.#offCtx = this.#offscreenCanvas.getContext("2d", {
        willReadFrequently: true,
      });
      this.#offCtx.globalCompositeOperation = "source-over";
      this.#offCtx.globalAlpha = 0.06;
      this.#offCtx.shadowBlur = 30;
      this.#offCtx.shadowOffsetX = 0;
      this.#offCtx.shadowOffsetY = 0;
      this.#offCtx.willReadFrequently = true;
    });
    this.#utils.colorBlends = pallete || ["#0000FF", "#00FF00", "#FF0000"];
    this.#colors = gradientColors(this.#utils.colorBlends, 125);
  }

  // options = {
  //   // Series名称
  //   name: "real",
  //   // 线条颜色|柱状图填充色，仅作为填充色时支持透明度
  //   color: "#FF0000FF",
  //   // 线条宽度，仅对线图生效
  //   thickness: 1,
  //   // 是否显示
  //   visible: true,
  //   // Series 类型 'point'|'line'|'stepline'|'bar'
  //   type: "line",
  //   // x轴类型 'none' | 'number' | 'time' (单位秒)
  //   streamType: "none",
  //   // 流图模式X轴最大值,streamType='time' (单位秒)
  //   streamMax: 400,
  //   // 柱状图方向, 仅对柱状图生效'vertical' | 'horizontal'
  //   barOrientation: "vertical",
  //   // 柱状图锚向, 仅对柱状图生效 'right' | 'left'
  //   barAlign: "right",
  // };

  #initNormalSeries = (options) => {
    const defaultOptions = {
      // Series名称
      name: undefined,
      // 线条颜色|柱状图填充色，仅作为填充色时支持透明度
      color: "#00FF00FF",
      // 线条宽度，仅对线图生效
      thickness: 1,
      // 点图 样子 'rect'|'circle'|'diamond'
      symbol: "rect",
      // 点的宽度
      pointWidth: 4,
      // 点颜色
      pointColor: "#0000FF",
      // 仅对线图line生效
      showPoint: false,
      // 是否显示
      visible: true,
      // Series 类型 'point'|'line'|'stepline'|'bar'
      type: "line",
      // 柱状图方向, 仅对柱状图生效'vertical' | 'horizontal' , 设置为vertical时请将数据归一化到0-100再调用setData
      barOrientation: "horizontal",
      // 柱状图锚向, 仅对柱状图且barOrientation='horizontal'时生效， 'right' | 'left'
      barAlign: "right",
      // 特殊间距bar，仅对柱状图且barOrientation='vertical'时生效：<=10柱子间距64，11> && <=20柱子间距32，21> && <=30柱子间距16， 32> && <=48柱子间距8，其它保持非specialBar
      specialBar: false,
      // 牛逼的step——仅type==='stepline'时生效，点数过少（含缩放后）时做线性插值，ZTMNB
      NBStep: false,
    };
    const seriesOptions = { ...defaultOptions, ...options };
    const { name, color, type } = seriesOptions;
    if (!name) {
      throw "Invalid parameter [name]";
    }

    let c = color;
    if (type === "bar") {
      for (const key in this.#series) {
        const series = this.#series[key];
        if (key !== name && series.type === "bar") {
          throw "Only one 'bar' series can be supported.";
        }
      }
      // 缓存bar图名称
      this.#drawOrder.barSeries = name;
      // 转换颜色
      seriesOptions.colorRGBA = hex2RGBA(c);
    }
    const series = this.#series[name];
    if (series) {
      const newOptions = { ...series, ...seriesOptions };
      this.#series[name] = newOptions;
      // 更新设置重绘
      this.#drawData();
    } else {
      this.#series[name] = seriesOptions;
    }
  };

  /**
   * 初始化/修改谱线、柱状图
   * @param {{name:String, color:String, thickness:Number, pointWidth:Number, pointColor:String, showPoint:boolean, visible:boolean, type:'point'|'line'|'stepline'|'bar', barOrientation:'vertical'|'horizontal', barAlign:'right' | 'left',specialBar:boolean,NBStep:boolean,shadowColor:undefined|String}} options
   *
   */
  initSeries(options) {
    this.#initNormalSeries(options);
  }

  /**
   * 初始化流图
   * @param {{name:String, color:String, thickness:Number, visible:boolean, type:'line'|'stepline',shadowColor:undefined|String}} options
   * @param {{streamType:'number' | 'time', streamMax:800}} streamOption
   */
  // eslint-disable-next-line no-dupe-class-members
  initSeries(options, streamOption) {
    if (!streamOption) {
      this.#initNormalSeries(options);
      return;
    }
    const defaultOptions = {
      // Series名称
      name: undefined,
      // 线条颜色|柱状图填充色，仅作为填充色时支持透明度
      color: "#00FF00FF",
      // 线条宽度，仅对线图和点图生效
      thickness: 1,
      // 是否显示
      visible: true,
      // Series 类型 'point'|'line'|'stepline'|'bar'
      type: "line",
      // x轴类型 'number' | 'time' (单位秒)
      streamType: "number",
      // 流图模式X轴最大值,streamType='time' (单位秒)
      streamMax: 800,
    };
    let seriesOptions = { ...defaultOptions, ...options };
    seriesOptions = { ...seriesOptions, ...streamOption };
    const { name } = seriesOptions;
    const series = this.#series[name];
    if (series) {
      const newOptions = { ...series, ...seriesOptions };
      this.#series[name] = newOptions;
      // 更新设置重绘
      this.#drawData();
    } else {
      this.#series[name] = seriesOptions;
    }
  }

  /**
   * 移除谱线
   * @param {String} name undefined 清除所有
   */
  removeLineSeries(name) {
    if (name) {
      const line = this.#series[name];
      if (line) {
        this.#series[name] = undefined;
      }
    } else {
      this.#series = {};
    }
    // 更新设置重绘
    this.#drawData();
  }

  /**
   *
   * @param {Arrary<Number>} data
   */
  setDataToDPX(data) {
    // 1. 不断收集100ms数据，然后绘制到off canvas
    const dt = new Date().getTime();
    this.#dataBuffers.push({ data, time: dt });
    if (this.#dataBuffers.length < 2) {
      this.#firstFrameTime = dt;
    } else if (dt - this.#firstFrameTime > 200) {
      // 数据清理
      let removeEnd = -1;
      for (let i = 0; i < this.#dataBuffers.length; i += 1) {
        const frame = this.#dataBuffers[i];
        const offset = dt - frame.time;

        if (offset < 500) {
          break;
        }
        removeEnd = i;
        this.#firstFrameTime = frame.time;
      }
      if (removeEnd > -1) {
        this.#dataBuffers = this.#dataBuffers.slice(removeEnd);
      }

      // 绘制
      if (this.#offCtx) this.#drawPer100ms();
    }
  }

  #drawPer100ms() {
    if (!this.#utils || !this.#utils.rect) return;
    const { width, height } = this.#utils.rect;
    const ctx = this.#offCtx;
    ctx.clearRect(0, 0, width, height);
    this.#dataBuffers.forEach((data) => {
      this.#drawLineOnOffscreen(data.data, ctx);
    });

    const imageData = ctx.getImageData(0, 0, width, height);
    const len = width * height * 4;
    let endColor = this.#colors[this.#colors.length - 1];
    for (let i = 0; i < len; i += 4) {
      const a = imageData.data[i + 3];
      if (a < 1) continue;
      const c = this.#colors[a] || endColor;

      imageData.data[i] = c.R;
      imageData.data[i + 1] = c.G;
      imageData.data[i + 2] = c.B;
      imageData.data[i + 3] = a < 64 ? 126 + a : 255;
    }
    // const newImageData = new ImageData(imageData.data, width);
    this.#utils.ctx.putImageData(imageData, 0, 0);
  }

  #drawLineOnOffscreen(data, context) {
    const { width, height } = this.#utils.rect;
    const yRange = this.#utils.maximumY - this.#utils.minimumY;
    const len = data.length;
    const perWidth = width / len;
    const halfWidth = perWidth / 2;
    /**
     * @type {CanvasRenderingContext2D}
     */
    const ctx = context;
    ctx.beginPath();
    let y = 0;
    let first = -1;

    for (let i = 0; i < len; i += 1) {
      const x = Math.round(perWidth * i + halfWidth);
      const v = data[i];
      const val = Math.floor(v * 10);
      if (val > -9999) {
        y = this.#yPositions[val];
        if (y === undefined) {
          const scale = (v - this.#utils.minimumY) / yRange;
          y = Math.floor(height - scale * height);
        }

        if (first < 0) {
          ctx.moveTo(x, y);
          first = i;
        } else {
          ctx.lineTo(x, y);
        }
      } else {
        y = -9999;
        first = -1;
      }
    }
    if (y > -9999 && first > -1) ctx.lineTo(width, y);
    ctx.strokeStyle = this.#grayColor;
    ctx.lineWidth = 3;
    ctx.stroke();
  }

  /**
   * 设置数据
   * @param {Array<{name:String,data:Array}>} seriesData 数据
   */
  setData(seriesData) {
    if (!seriesData || seriesData.length === 0) {
      return;
    }
    // 取第一条线的数据长度判断是否需要重置缩放
    if (this.#utils.zoomInside) {
      const dataLen = seriesData[0].data.length;
      if (this.#prevDataLen !== dataLen) {
        this.#utils.zoomStart = 0;
        this.#utils.zoomEnd = dataLen - 1;
        this.#prevDataLen = dataLen;
      }
    }
    const dataSetOrder = [];
    for (let i = 0; i < seriesData.length; i++) {
      const lineData = seriesData[i];
      if (lineData) {
        const series = this.#series[lineData.name];
        if (series) {
          if (series.type !== "bar") {
            dataSetOrder.push(lineData.name);
          }
          series.sourceData = lineData.data;
        }
      }
    }
    this.#drawOrder.otherSeries = dataSetOrder;
    this.#drawData();
  }

  /**
   * 设置数据
   * @param {Array<{name:String,xData:Array,yData:Array}>} seriesData 数据
   */
  setXYData(seriesData) {
    if (!seriesData || seriesData.length === 0) {
      return;
    }

    for (let i = 0; i < seriesData.length; i++) {
      const lineData = seriesData[i];
      const series = this.#series[lineData.name];
      if (series) {
        series.sourceData = { xData: lineData.xData, yData: lineData.yData };
      }
    }
    this.#drawData();
  }

  /**
   *
   * @returns {HTMLCanvasElement}
   */
  getCanvas() {
    return this.#utils.cvs;
  }

  // 2022-7-22 处理绘制顺序，理论上来说应该可以通过接口给每个非bar定义顺序，暂时根据设置的数据来吧
  #initDrawOrder = () => {
    const allSeries = [...this.#drawOrder.otherSeries];
    for (let key in this.#series) {
      if (key === this.#drawOrder.barSeries || allSeries.includes(key)) {
        continue;
      } else {
        allSeries.unshift(key);
      }
    }
    // 2022-7-22 source-over 接下来绘制线在根据顺序，后来居上
    // this.#utils.ctx.globalCompositeOperation = 'source-over';
    // destination-over bar绘制在上面  2022-7-22 source-over bar在下面
    // --------------------------------------------------------------
    // 2022-8-4 MD，chrome下面先putImageData绘制bar，再stroke绘制线会导致bar被清理了，
    // 所以直接全用stroke吧，这样也好控制顺序
    // this.#utils.ctx.globalCompositeOperation = this.#utils.barFront
    //   ? 'destination-over'
    //   : 'source-over';
    if (this.#drawOrder.barSeries) {
      if (this.#utils.barFront) {
        // 最后画
        allSeries.push(this.#drawOrder.barSeries);
      } else {
        // 最先画
        allSeries.unshift(this.#drawOrder.barSeries);
      }
    }

    return allSeries;
  };

  #drawData = () => {
    if (!this.#utils || !this.#utils.rect) return;
    const { width, height } = this.#utils.rect;
    if (!width || width === 0 || !height || height === 0) return;
    // this.#utils.ctx.clearRect(0, 0, width, height);
    this.#utils.ctx.globalCompositeOperation = "source-over";
    // 绘制顺序处理
    const drawOrder = this.#initDrawOrder();
    for (let i = 0; i < drawOrder.length; i += 1) {
      const key = drawOrder[i];
      const series = this.#series[key];
      if (series.visible && series.sourceData) {
        let drawData = series.sourceData;
        if (drawData.xData) {
          this.#drawXYData(series);
          continue;
        }
        if (this.#utils.zoomInside) {
          if (
            this.#utils.zoomStart > 0 ||
            this.#utils.zoomEnd < this.#prevDataLen - 1
          ) {
            // 缩放了
            drawData = drawData.slice(
              this.#utils.zoomStart,
              this.#utils.zoomEnd + 1
            );
          }
        }
        if (drawData.length > this.#maxPoint) {
          // 抽取绘制
          drawData = combineData(
            drawData,
            this.#maxPoint,
            this.#renderReferArray
          );
        }
        if (series.type === "bar") {
          if (series.barOrientation === "vertical") {
            this.#drawVerticalBar(
              drawData,
              series.barAlign,
              series.color,
              series.colorRGBA
            );
          } else {
            if (series.specialBar && drawData.length < 49) {
              // 2022-7-22 特殊bar
              this.#drawSpecialBar(drawData, series.color, series.colorRGBA);
            } else {
              this.#drawStrokeBar(drawData, series.color);
            }
          }
        } else if (series.type === "point") {
          this.#drawPoint(drawData, series);
        } else if (series.type === "stepline") {
          this.#drawStepLine(drawData, series);
        } else {
          this.#drawLine(drawData, series);
          if (series.showPoint) {
            this.#drawPoint(drawData, series);
          }
        }
      }
    }
  };

  #drawXYData = (series) => {
    const { sourceData, type, showPoint } = series;
    const { xData, yData } = sourceData;
    if (type === "point") {
      this.#drawXYPoint(xData, yData, series);
    }
    if (type === "line") {
      this.#drawXYLine(xData, yData, series);
      if (showPoint) {
        this.#drawXYPoint(xData, yData, series);
      }
    }
  };

  #drawStrokeBar = (data, fillColor) => {
    const { width, height } = this.#utils.rect;
    const { ctx, minimumY, maximumY } = this.#utils;
    if (!width || width === 0 || !height || height === 0) return;
    const perWidth = width / data.length;
    let barWidth = perWidth;
    if (perWidth >= 3) {
      barWidth = perWidth - 1;
    }
    // console.log("bar chart", width, data.length, perWidth, barWidth);
    const yOffset = Math.floor(height + perWidth + 10);
    let pStart = barWidth / 2;
    const yRange = maximumY - minimumY;
    ctx.beginPath();
    for (let i = 0; i < data.length; i += 1) {
      const scale = (data[i] - minimumY) / yRange;
      // 计算值
      const y = Math.ceil(height - scale * height);
      const pStartInt = Math.floor(pStart);
      // 移动到当前值得底部位置
      ctx.moveTo(pStartInt, yOffset);
      // 移动到值得位置
      ctx.lineTo(pStartInt, y);
      pStart += perWidth;
    }
    // ctx.strokeStyle = this.hexColor;
    ctx.lineWidth = barWidth;
    ctx.strokeStyle = fillColor;
    ctx.stroke();
  };

  /**
   * 绘制方法
   * @param {Array} data
   * @param {String} fillColor
   * @param {{R:Number,G:Number,B:Number,A:Number}} colorRGBA
   * @param {Array} fillColors
   * @param {String} align
   */
  #drawVerticalBar = (data, align, fillColor, colorRGBA, fillColors) => {
    const { width, height } = this.#utils.rect;
    if (
      !width ||
      width === 0 ||
      !height ||
      height === 0 ||
      data.length > height * 2
    )
      return;

    const imgData = this.#utils.ctx.createImageData(width, height);
    const perHeight = height / data.length;

    for (let i = 0; i < data.length; i += 1) {
      const yy1 = Math.round(perHeight * i);
      const yy2 = Math.round(yy1 + perHeight) - 1;
      const val = data[i];
      const scale = Math.round((val * width) / 100);
      const x1 = align === "left" ? 0 : width - scale;
      const x2 = align === "left" ? scale : width;
      const c = fillColors ? fillColors[i] : colorRGBA;
      const { R, G, B, A } = c;
      for (let x = x1; x < x2; x += 1) {
        for (let y = yy1; y < yy2; y += 1) {
          const index = (y * width + x) * 4;
          imgData.data[index] = R;
          imgData.data[index + 1] = G;
          imgData.data[index + 2] = B;
          imgData.data[index + 3] = A;
        }
      }
    }
    this.#utils.ctx.putImageData(imgData, 0, 0);
  };

  #drawSpecialBar = (data, fillColor) => {
    // <=10柱子间距64，>11 && <=20柱子间距32，>21 && <=30柱子间距16， >30 && <=48柱子间距8
    const { width, height } = this.#utils.rect;
    const { ctx, minimumY, maximumY } = this.#utils;
    if (!width || width === 0 || !height || height === 0) return;
    let perGap = 0;
    if (data.length <= 10) {
      perGap = 64;
    } else if (data.length <= 20) {
      perGap = 30;
    } else if (data.length <= 30) {
      perGap = 16;
    } else {
      perGap = 8;
    }
    const barWidth = (width - data.length * perGap) / data.length;
    const yOffset = Math.floor(height + barWidth + 10);
    let pStart = (perGap + barWidth) / 2;
    const yRange = maximumY - minimumY;
    ctx.beginPath();
    for (let i = 0; i < data.length; i += 1) {
      const scale = (data[i] - minimumY) / yRange;
      // 计算值
      const y = Math.floor(height - scale * height);
      // 移动到当前值得底部位置
      const pStartInt = Math.round(pStart);
      ctx.moveTo(pStartInt, yOffset);
      // 移动到值得位置
      ctx.lineTo(pStartInt, y);
      pStart += barWidth + perGap;
    }
    ctx.lineWidth = barWidth;
    ctx.strokeStyle = fillColor;
    ctx.stroke();
  };

  /**
   * 绘制方法
   * @param {*} data
   */
  #drawLine = (data, series) => {
    const { color, thickness, shadowColor } = series;
    const { width, height } = this.#utils.rect;
    const yRange = this.#utils.maximumY - this.#utils.minimumY;
    const len = data.length;
    const perWidth = width / len;
    const halfWidth = perWidth / 2;
    this.#utils.ctx.shadowBlur = 25;
    this.#utils.ctx.globalAlpha = Math.random();
    // this.#utils.ctx.shadowOffsetX = 0;
    // this.#utils.ctx.shadowOffsetY = 0;
    this.#utils.ctx.shadowColor = shadowColor;
    this.#utils.ctx.beginPath();
    let y = 0;
    let first = -1;

    for (let i = 0; i < len; i += 1) {
      const x = Math.round(perWidth * i + halfWidth);
      const v = data[i];
      const val = Math.floor(v * 10);
      if (val > -9999) {
        y = this.#yPositions[val];
        if (y === undefined) {
          const scale = (v - this.#utils.minimumY) / yRange;
          y = Math.floor(height - scale * height);
        }

        if (first < 0) {
          this.#utils.ctx.moveTo(x, y);
          first = i;
        } else {
          this.#utils.ctx.lineTo(x, y);
        }
      } else {
        y = -9999;
        first = -1;
      }
    }
    if (y > -9999 && first > -1) this.#utils.ctx.lineTo(width, y);
    this.#utils.ctx.strokeStyle = color;
    this.#utils.ctx.lineWidth = thickness;
    this.#utils.ctx.stroke();
  };

  /**
   * 绘制方法
   * @param {*} data
   */
  #drawStepLine = (data, series) => {
    const { color, thickness } = series;
    const { width, height } = this.#utils.rect;
    let perWidth = width / data.length;
    this.#utils.ctx.beginPath();
    if (perWidth >= 2) {
      // 梯形线
      for (let i = 0; i < data.length; i += 1) {
        const x = Math.round(perWidth * i);
        const x2 = perWidth * (i + 1);
        const scale =
          (data[i] - this.#utils.minimumY) /
          (this.#utils.maximumY - this.#utils.minimumY);
        const y = Math.floor(height - scale * height);
        if (i === 0) {
          this.#utils.ctx.moveTo(x, y);
        } else {
          this.#utils.ctx.lineTo(x, y);
        }
        this.#utils.ctx.lineTo(x2, y);
      }
    } else {
      // 折线
      // perWidth = width / data.length;
      for (let i = 0; i < data.length; i += 1) {
        const x = Math.round(perWidth * i);
        const scale =
          (data[i] - this.#utils.minimumY) /
          (this.#utils.maximumY - this.#utils.minimumY);
        const y = Math.floor(height - scale * height);
        if (i === 0) {
          this.#utils.ctx.moveTo(x, y);
        } else {
          this.#utils.ctx.lineTo(x, y);
        }
      }
    }
    this.#utils.ctx.strokeStyle = color;
    this.#utils.ctx.lineWidth = thickness;
    this.#utils.ctx.stroke();
  };

  #drawPoint = (data, series) => {
    const { pointColor, pointWidth, symbol } = series;
    const { width, height } = this.#utils.rect;
    const yRange = this.#utils.maximumY - this.#utils.minimumY;
    const perWidth = width / data.length;
    const half = pointWidth > 1 ? Math.round(pointWidth / 2) : 0;
    const offset = (perWidth - pointWidth) / 2;
    if (symbol === "rect") {
      this.#utils.ctx.beginPath();
      for (let i = 0; i < data.length; i += 1) {
        const x = Math.round(perWidth * i + offset);
        const d = data[i];
        // 2022-7-22 临界值处理
        if (d >= this.#utils.minimumY && d <= this.#utils.maximumY) {
          const scale = (data[i] - this.#utils.minimumY) / yRange;
          const y = Math.floor(height - scale * height);
          this.#utils.ctx.moveTo(x, y);
          this.#utils.ctx.lineTo(x + pointWidth, y);
        }
      }
      this.#utils.ctx.strokeStyle = pointColor;
      this.#utils.ctx.lineWidth = pointWidth;
      this.#utils.ctx.stroke();
    } else if (symbol === "diamond") {
      const diamondOffsetX = Math.round(pointWidth * 0.707);
      const diamondOffsetY = pointWidth;
      const diamondPath = new Path2D();
      for (let i = 0; i < data.length; i += 1) {
        const x = Math.round(perWidth * i);
        const d = data[i];
        // 2022-7-22 临界值处理
        if (d >= this.#utils.minimumY && d <= this.#utils.maximumY) {
          // diamond 菱形
          // warning MD，用线来模拟有毛刺，但是性能高
          // 通过setTransform来做吧
          const scale = (data[i] - this.#utils.minimumY) / yRange;
          const y = Math.floor(height - scale * height);
          const path = new Path2D();
          path.moveTo(x - diamondOffsetX, y);
          path.lineTo(x, y - diamondOffsetY);
          path.lineTo(x + diamondOffsetX, y);
          path.lineTo(x, y + diamondOffsetY);
          path.closePath();
          diamondPath.addPath(path);
        }
      }
      this.#utils.ctx.fillStyle = pointColor;
      // this.#utils.ctx.setTransform(1, 1, -1, 1, pointWidth, 0);
      this.#utils.ctx.fill(diamondPath);
      this.#utils.ctx.resetTransform();
    } else {
      // circle
      const circlePath = new Path2D();
      for (let i = 0; i < data.length; i += 1) {
        const x = perWidth * i + offset;
        const d = data[i];
        // 2022-7-22 临界值处理
        if (d >= this.#utils.minimumY && d <= this.#utils.maximumY) {
          const scale = (data[i] - this.#utils.minimumY) / yRange;
          const y = Math.floor(height - scale * height) - half;
          const path = new Path2D();
          path.arc(x + half, y + half, pointWidth, 0, 2 * Math.PI);
          circlePath.addPath(path);
        }
      }
      this.#utils.ctx.fillStyle = pointColor;
      this.#utils.ctx.fill(circlePath);
    }
  };

  /**
   *
   * @param {Array<Number>} xData
   * @param {*} yData
   */
  #drawXYPoint = (xData, yData, series) => {
    const { pointColor, pointWidth } = series;
    let minX = xData[0];
    let maxX = xData[0];
    for (let i = 1; i < xData.length; i++) {
      const a = xData[i];
      if (a < minX) {
        minX = a;
      } else if (a > maxX) {
        maxX = a;
      }
    }

    const { width, height } = this.#utils.rect;
    const yRange = this.#utils.maximumY - this.#utils.minimumY;
    const half = pointWidth > 1 ? Math.round(pointWidth / 2) : 0;
    const xScale = width / (maxX - minX);
    this.#utils.ctx.beginPath();
    for (let i = 0; i < yData.length; i += 1) {
      const x = Math.round((xData[i] - minX) * xScale - half);
      const scale = (yData[i] - this.#utils.minimumY) / yRange;
      const y = Math.floor(height - scale * height) - half;
      this.#utils.ctx.moveTo(x, y);
      this.#utils.ctx.lineTo(x + pointWidth, y);
    }
    this.#utils.ctx.strokeStyle = pointColor;
    this.#utils.ctx.lineWidth = pointWidth;
    this.#utils.ctx.stroke();
  };

  #drawXYLine = (xData, yData, series) => {
    const { color, thickness } = series;
    let minX = xData[0];
    let maxX = xData[0];
    for (let i = 1; i < xData.length; i++) {
      const a = xData[i];
      if (a < minX) {
        minX = a;
      } else if (a > maxX) {
        maxX = a;
      }
    }
    const { width, height } = this.#utils.rect;
    const yRange = this.#utils.maximumY - this.#utils.minimumY;
    const xScale = width / (maxX - minX);
    this.#utils.ctx.beginPath();
    for (let i = 0; i < yData.length; i += 1) {
      const x = Math.round((xData[i] - minX) * xScale);
      const scale = (yData[i] - this.#utils.minimumY) / yRange;
      const y = Math.floor(height - scale * height);
      if (i === 0) {
        this.#utils.ctx.moveTo(x, y);
      } else {
        this.#utils.ctx.lineTo(x, y);
      }
    }
    this.#utils.ctx.strokeStyle = color;
    this.#utils.ctx.lineWidth = thickness;
    this.#utils.ctx.stroke();
  };

  /**
   * 缩放
   * @param {Number} start
   * @param {Number} end
   */
  zoom(start, end) {
    if (this.#utils.zoomInside) {
      this.#utils.zoomStart = start;
      this.#utils.zoomEnd =
        end < this.#prevDataLen ? end : this.#prevDataLen - 1;
      this.#drawData();
    }
  }

  /**
   * 通过设置百分比来缩放
   * @param {Number} start
   * @param {Number} end
   */
  zoomPercent(start, end) {
    // 根据百分比计算当前索引
    const { zoomPercent } = this.#utils;
    const pctGap = zoomPercent.end - zoomPercent.start;
    let newStart = start;
    let newEnd = end;
    if (start > 0 || end < 1) {
      newStart = zoomPercent.start + pctGap * start;
      newEnd = zoomPercent.start + pctGap * end;
    }
    this.#utils.zoomPercent = {
      start: newStart,
      end: newEnd,
    };
    this.zoom(
      Math.floor(this.#prevDataLen * newStart),
      Math.round(this.#prevDataLen * newEnd)
    );
  }

  resetZoom() {
    this.zoom(0, this.#prevDataLen - 1);
  }

  /**
   * Y轴范围变更
   * @param {Numer} minimum
   * @param {Numer} maximum
   */
  setAxisYRange(minimum, maximum, reRender = true) {
    this.#utils.minimumY = minimum;
    this.#utils.maximumY = maximum;
    this.#initYPixel();
    if (reRender) {
      // 重绘
      this.#drawData();
    }
  }

  clear() {
    for (const key in this.#series) {
      const series = this.#series[key];
      series.sourceData = undefined;
    }
    if (this.#utils.rect) {
      this.#utils.ctx.clearRect(
        0,
        0,
        this.#utils.rect.width,
        this.#utils.rect.height
      );
    }
  }

  #initYPixel() {
    const { minimumY, maximumY, rect } = this.#utils;
    if (rect) {
      const yRange = maximumY - minimumY;
      // 各预留 40
      const minVal = (minimumY - 40) * 10;
      const maxVal = (maximumY + 100) * 10;
      for (let i = minVal; i < maxVal; i += 1) {
        // const val = i / 10;
        const scale = (i / 10 - minimumY) / yRange;
        const y = Math.floor(rect.height - scale * rect.height);
        this.#yPositions[i] = y;
      }
    }
  }

  dispose() {
    this.#utils.dispose();
  }
}

export default DPX;
