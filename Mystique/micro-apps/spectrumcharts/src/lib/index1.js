import { GPU } from "gpu.js";
// import * as GPU from "../gpujs/gpu-browser.js";

class chartUtils {
  // 是否允许缩放，
  // true:内部根据像素宽度/高度处理数据抽取，且可调用zoom方法设置缩放位置；
  // false则内部不进行数据抽取，外部需要订阅onSizeChange回调根据像素宽度/高度先进行数据抽取，如果数据不满足要求，则不进行绘制
  zoomInside = true;
  // true bar绘制在底层 false bar绘制在顶层
  barFront = false;
  // 瀑布图渲染模式，fill 完全填充，pixel横向填充、纵向（行）按像素填充
  rainRenderMode = "fill";
  // chart像素尺寸变更回调
  onSizeChange;
  minimumY = -20;
  maximumY = 80;
  // 监听容器尺寸变更
  #tmrResizeListener;
  // 判断容器是否已被卸载，超过10次取到的rect值都为0，则判断为已被卸载，自动释放计时器
  #detachTimes = 0;
  // 内部使用，用于通知尺寸变更
  #onResize;
  // X轴方向缩放起始值 %
  zoomPercent = { start: 0, end: 1 };
  zoomPercentY = { start: 0, end: 1 };
  // zoomInfo
  // X轴方向缩放起始值 索引
  zoomStart;
  // X轴方向缩放结束值 索引
  zoomEnd;
  // Y轴方向缩放起始值 索引
  zoomStartY;
  // Y轴方向缩放结束值 索引
  zoomEndY;

  /**
   *
   * @param {HTMLElement} container
   * @param {{onSizeChange:Function<Number,Number>, zoomInside:boolean, barFront:boolean,renderMode:String }} option
   * @param {Function} onResize
   * @param {boolean} useGPU
   */
  constructor(container, option, onResize, useGPU) {
    this.container = container;
    if (option) {
      this.zoomInside =
        option.zoomInside === undefined ? true : option.zoomInside;
      this.onSizeChange = option.onSizeChange;
      this.barFront = option.barFront;
      this.rainRenderMode = option.renderMode || "fill";
    }
    this.#onResize = onResize;

    const cvs = document.createElement("canvas");
    cvs.style.aspectRatio = "unset";
    container.appendChild(cvs);
    if (!useGPU) {
      this.ctx = cvs.getContext("2d");
      this.ctx1 = cvs.getContext("2d");
      this.ctx.imageSmoothingEnabled = false;
    }
    this.cvs = cvs;
    this.resize();
    // this.#initCanvasSize();
    // this.#onResize(container);
    this.#tmrResizeListener = setTimeout(() => {
      this.resize();
    }, 1000);
  }

  /**
   * canvas自适应容器尺寸
   */
  resize = () => {
    const rec = this.container.getBoundingClientRect();
    const x = Math.round(rec.x);
    const y = Math.round(rec.y);
    const width = Math.round(rec.width);
    const height = Math.round(rec.height);
    if (x === 0 && y === 0 && width === 0 && height === 0) {
      this.#detachTimes += 1;
      if (this.#detachTimes > 10) {
        try {
          clearInterval(this.#tmrResizeListener);
        } catch {
          /* empty */
        }
      }
      return;
    }
    if (
      !this.rect ||
      this.rect.x !== x ||
      this.rect.y !== y ||
      this.rect.width !== width ||
      this.rect.height !== height
    ) {
      this.rect = {
        x,
        y,
        width,
        height,
        left: Math.round(rec.left),
        right: Math.round(rec.right),
        top: Math.round(rec.top),
        bottom: Math.round(rec.bottom),
      };
      const cvs = this.container.querySelector("canvas");
      cvs.width = this.rect.width;
      cvs.height = this.rect.height;
      this.fireSizeChange(this.rect.width, this.rect.height);
    }
  };

  // 2021-12-30  liujian MD 会死循环
  // /**
  //  * 容器尺寸大小变更处理
  //  */
  // #onResize = () => {
  //   this.resizeObserver = new ResizeObserver((entries) => {
  //     if (entries.length > 0) {
  //       const rect = entries[0].contentRect;
  //       this.rect = {
  //         x: Math.round(rect.x),
  //         y: Math.round(rect.y),
  //         width: Math.round(rect.width),
  //         height: Math.round(rect.height),
  //         left: Math.round(rect.left),
  //         right: Math.round(rect.right),
  //         top: Math.round(rect.top),
  //         bottom: Math.round(rect.bottom),
  //       };
  //       console.log(this.rect);
  //       this.#initCanvasSize();
  //       this.fireSizeChange(this.rect.width, this.rect.height);
  //     }
  //   });
  //   this.resizeObserver.observe(this.container);
  // };

  fireSizeChange(width, height) {
    if (this.#onResize) {
      this.#onResize(width, height);
    }
    if (this.onSizeChange) {
      this.onSizeChange(width, height);
    }
  }

  /**
   * 资源释放
   * 释放尺寸变更监听
   */
  dispose = () => {
    const child = this.container.childElementCount;
    for (let i = child - 1; i > -1; i -= 1) {
      this.container.removeChild(this.container.children[i]);
    }
    try {
      clearInterval(this.#tmrResizeListener);
    } catch {
      /* empty */
    }
  };
  // /**
  //  * 初始化canvas 尺寸
  //  */
  // #initCanvasSize = () => {
  //   const cvs = this.container.querySelector("canvas");
  //   this.resizeObserver.disconnect();
  //   cvs.width = this.rect.width;
  //   cvs.height = this.rect.height;
  //   this.resizeObserver.observe(this.container);
  // };
}

/**
 * 抽取
 * @param {Array} spectrum
 * @param {Number} outMax
 * @param {Array} referData 参考数据和outMax一样长，这样可用避免重复new Array而节省性能
 * @returns
 */
const combineData = (spectrum, outMax, referData) => {
  const spectrumLength = spectrum.length; // 缓存数组长度
  if (spectrumLength > outMax) {
    const res = referData ? referData.slice(0) : new Array(outMax); // 预先定义结果数组的长度
    let freqIndex = 0;
    const perPointGap = spectrumLength / (outMax - 1);
    let freqN = freqIndex + perPointGap / 2;
    for (let j = 0; j < outMax; j += 1) {
      let maxValue = spectrum[freqIndex];
      const k = Math.round(freqN);
      for (; freqIndex < k; freqIndex += 1) {
        const vl = spectrum[freqIndex];
        if (maxValue < vl) {
          maxValue = vl;
        }
      }
      res[j] = maxValue;
      freqN += perPointGap;
      if (freqN > spectrumLength) freqN = spectrumLength;
    }
    return res;
  }
  return spectrum.slice(0);
};

/**
 * 生成渐变色
 * @param {Array} colors
 * @param {Number} outCount
 * @returns
 */
const gradientColors = (colors, outCount) => {
  const gap = outCount / (colors.length - 1);
  const outColors = [];
  for (let i = 1; i < colors.length; i += 1) {
    const from = Math.round(gap * (i - 1));
    const to = Math.round(gap * i);
    const r1 = parseInt(String(colors[i - 1]).slice(1, 3), 16);
    const g1 = parseInt(String(colors[i - 1]).slice(3, 5), 16);
    const b1 = parseInt(String(colors[i - 1]).slice(5, 7), 16);
    const r2 = parseInt(String(colors[i]).slice(1, 3), 16);
    const g2 = parseInt(String(colors[i]).slice(3, 5), 16);
    const b2 = parseInt(String(colors[i]).slice(5, 7), 16);
    const rsetp = (r2 - r1) / gap;
    const gstep = (g2 - g1) / gap;
    const bstep = (b2 - b1) / gap;
    for (let l = from; l < to; l += 1) {
      const r = Math.round(r1 + rsetp * (l - from));
      const g = Math.round(g1 + gstep * (l - from));
      const b = Math.round(b1 + bstep * (l - from));
      // window.console.log(r, g, b);
      // const rs = r.toString(16).padStart(2, "0");
      // const gs = g.toString(16).padStart(2, "0");
      // const bs = b.toString(16).padStart(2, "0");
      // const hex = `#${rs}${gs}${bs}`;
      outColors.push({
        R: r,
        G: g,
        B: b,
      });
    }
  }
  return outColors;
};

/**
 * 生成渐变色
 * @param {Array} colors
 * @param {Number} outCount
 * @returns
 */
const gradientColorsGPU = (colors, outCount) => {
  const gap = outCount / (colors.length - 1);
  const outColors = [];
  for (let i = 1; i < colors.length; i += 1) {
    const from = Math.round(gap * (i - 1));
    const to = Math.round(gap * i);
    const r1 = parseInt(String(colors[i - 1]).slice(1, 3), 16);
    const g1 = parseInt(String(colors[i - 1]).slice(3, 5), 16);
    const b1 = parseInt(String(colors[i - 1]).slice(5, 7), 16);
    const r2 = parseInt(String(colors[i]).slice(1, 3), 16);
    const g2 = parseInt(String(colors[i]).slice(3, 5), 16);
    const b2 = parseInt(String(colors[i]).slice(5, 7), 16);
    const rsetp = (r2 - r1) / gap;
    const gstep = (g2 - g1) / gap;
    const bstep = (b2 - b1) / gap;
    for (let l = from; l < to; l += 1) {
      const r = Math.round(r1 + rsetp * (l - from));
      const g = Math.round(g1 + gstep * (l - from));
      const b = Math.round(b1 + bstep * (l - from));
      outColors.push(r);
      outColors.push(g);
      outColors.push(b);
    }
  }
  return outColors;
};

/**
 * 应光谱图
 */
class DPXChart {
  #prevSourceMatrix;
  // 瀑布图色带颜色，渐变后
  #colors = [];
  // 辅助
  #utils;
  #colorBlends;

  /**
   * 构造函数
   * @param {HTMLElement} container 容器
   * @param {Array<string>} pallete
   * @param {{onSizeChange:Function<Number,Number>, zoomInside:boolean}} optionMore
   */
  constructor(container, pallete, optionMore) {
    this.#utils = new chartUtils(container, optionMore);
    this.#colorBlends = pallete || ["#0000FF", "#00FF00", "#FF0000"];
    this.setAxisYRange(0, 30);
    this.#colors = gradientColors(this.#colorBlends, this.#utils.maximumY);
  }
  /**
   * 设置二维矩阵数据
   *
   * @param {Array<Array>} matrix
   */
  setMatrix(matrix) {
    const colCount = matrix[0].length;
    const { width, height } = this.#utils.rect;
    if (
      !width ||
      width === 0 ||
      !height ||
      height === 0 ||
      !this.#colors ||
      this.#colors.length !== this.#utils.maximumY
    )
      return;
    let drawMatrix = matrix;
    // if (this.zoomEnabled) {
    // 初始化buffer
    // this.#initImageData(colCount, rowCount, width, height);

    if (this.#utils.zoomInside) {
      this.#prevSourceMatrix = matrix;
      // 处理抽取
      if (
        colCount > width ||
        this.#utils.zoomStart > 0 ||
        this.#utils.zoomEnd < colCount
      ) {
        const data = [];
        matrix.forEach((item) => {
          let d = item;
          if (this.#utils.zoomStart > 0 || this.#utils.zoomEnd < colCount) {
            // 缩放了
            d = item.slice(this.#utils.zoomStart, this.#utils.zoomEnd);
          }
          // 抽取数据
          if (d.length > width) {
            d = combineData(d, width);
          }
          data.push(d);
        });
        drawMatrix = data;
      }
    } else {
      if (colCount > width) return;
    }

    // } else {
    //   // 不允许缩放状态，判断数据是否符合要求
    //   if (rowCount !== height || colCount !== width) return;
    // }
    this.#drawMatrix(drawMatrix);
  }

  #drawMatrix = (matrix) => {
    const rowCount = matrix.length;
    const colCount = matrix[0].length;
    const { width, height } = this.#utils.rect;
    const perWidth = width / colCount;
    const perHeight = height / rowCount;
    const imgMatrix = this.#utils.ctx.createImageData(width, height);
    const imgData = imgMatrix.data;
    let startY = 0;

    for (let y = 0; y < matrix.length; y += 1) {
      const endY = Math.round(perHeight * y + perHeight);
      const oneRow = matrix[y];
      let startX = 0;
      for (let x = 0; x < oneRow.length; x += 1) {
        let colorIndex = Math.round(oneRow[x]);
        const endX = Math.round(perWidth * x + perWidth);
        if (colorIndex > 0) {
          if (colorIndex >= this.#utils.maximumY)
            colorIndex = Math.floor(this.#utils.maximumY - 1);
          const c = this.#colors[colorIndex];
          for (let px = startX; px < endX; px += 1) {
            for (let py = startY; py < endY; py += 1) {
              const cIndex = (px + width * py) * 4;
              imgData[cIndex] = c.R;
              imgData[cIndex + 1] = c.G;
              imgData[cIndex + 2] = c.B;
              imgData[cIndex + 3] = 255;
            }
          }
        }
        startX = endX;
      }
      startY = endY;
    }
    this.#utils.ctx.putImageData(imgMatrix, 0, 0);
  };

  /**
   * 缩放
   * @param {Number} start
   * @param {Number} end
   */
  zoom(start, end) {
    if (this.#utils.zoomInside) {
      this.#utils.zoomStart = start;
      this.#utils.zoomEnd = end;
      const { ctx, width, height } = this.#utils.rect;
      ctx.clearRect(width, height);
      if (this.#prevSourceMatrix) {
        this.setMatrix(this.#prevSourceMatrix);
      }
    }
  }

  /**
   * Y轴范围变更
   * @param {Numer} minimum
   * @param {Numer} maximum
   */
  setAxisYRange(minimum, maximum) {
    // super.setAxisYRange(minimum, maximum);
    if (minimum >= 0 && maximum > minimum) {
      this.#utils.minimumY = minimum;
      this.#utils.maximumY = maximum;
      this.#colors = gradientColors(this.#colorBlends, this.#utils.maximumY);
      // 重绘
      if (this.#prevSourceMatrix) {
        this.#drawMatrix(this.#prevSourceMatrix);
      }
    }
  }

  clear() {
    this.#utils.ctx.clearRect(
      0,
      0,
      this.#utils.rect.width,
      this.#utils.rect.height
    );
    // this.rainImageData = new Uint8ClampedArray(this.rainImageData.length).fill(
    //   0
    // );
  }

  resize() {
    this.#utils.resize();
  }

  dispose() {
    this.#utils.dispose();
  }
}

/**
 * 雨点图/瀑布图
 */
class Rainchart$1 {
  // 记录上一次单行数据长度
  #prevDataLen = 1;
  // 上一次像素高度
  #prevPixelHeight = 1;
  // 上一次像素宽度
  #prevPixelWidth = 1;
  // 记录历史源数据（矩阵），主要是用于重绘
  #prevSourceMatrix;
  // 缓存源数据行，最大数据点数50000
  #rowsBuffer = [];
  // 瀑布图色带颜色，渐变后
  #colors = [];
  // 瀑布图数据缓存
  #rainImageData = [];
  // 辅助
  #utils;
  // 上一次行数
  #prevRowCount = 1;
  // TODO 保证在调整Y轴范围时不会丢数据重绘
  #preDrawRows = [];
  // 是否所有数据都已经收起
  #preIsOver = true;
  // 已经收起的行数
  #overRows = 0;

  /**
   * 构造函数
   * @param {HTMLElement} container
   * @param {Array<string>} pallete 色带颜色，从大到小
   * @param {{onSizeChange:Function<Number,Number>, zoomInside:boolean}} optionMore
   */
  constructor(container, pallete, optionMore) {
    // super(container, optionMore);
    this.#utils = new chartUtils(container, optionMore);
    this.#utils.colorBlends = pallete || ["#FF0000", "#00FF00", "#0000FF"];
    // this.#colors = gradientColors(colorBlends, 100);
    this.#initColors();
  }

  /**
   * 初始化绘制数据
   * @param {Number} dataLen
   * @param {Number} pixelWidth
   * @param {Number} rowCount
   */
  #initImageData = (dataLen, rowCount, pixelWidth, pixelHeight) => {
    if (
      this.#prevDataLen !== dataLen ||
      this.#prevPixelHeight !== pixelHeight ||
      this.#prevPixelWidth !== pixelWidth ||
      this.#prevRowCount !== rowCount
    ) {
      this.#prevDataLen = dataLen;
      this.#prevPixelHeight = pixelHeight;
      this.#prevPixelWidth = pixelWidth;
      this.#prevRowCount = rowCount;
      const rainImgData = this.#utils.ctx.createImageData(
        pixelWidth,
        pixelHeight
      );
      this.#rainImageData = rainImgData.data;
      this.#utils.ctx.clearRect(0, 0, pixelWidth, pixelHeight);
    }
  };

  #initColors = () => {
    const { minimumY, maximumY, colorBlends } = this.#utils;

    const colors = {};
    let cIndx = 0;
    try {
      const cs = gradientColors(colorBlends, maximumY - minimumY + 1);
      for (let i = maximumY; i >= minimumY; i -= 1) {
        colors[i] = cs[cIndx] || cs[cs.length - 1];
        cIndx += 1;
      }
      // colors[-9999] = hexColor2Rgb(colorBlends[colorBlends.length - 1]);
    } catch (er) {
      console.log("init colors error", er);
    } finally {
      // colors[-9999] = hexColor2Rgb('#00FF00');
      // console.log(colors, colors[-6]);
    }

    this.#colors = colors;
  };

  /**
   * 添加一行数据
   * @param {Array} data
   * @param {Number} timestamp
   */
  addRow(data) {
    let rowCount = this.#utils.rect.height;
    let pixelWidth = this.#utils.rect.width;
    // 初始化buffer
    this.#initImageData(data.length, rowCount, pixelWidth, rowCount);
    if (!this.#rowDataCast(data, rowCount, pixelWidth)) {
      return;
    }
    this.#drawRows(pixelWidth, rowCount);
  }

  #rowDataCast = (data, rowCount, pixelWidth, cacheMode) => {
    let drawData = data;
    if (this.#utils.zoomInside) {
      if (!cacheMode) {
        // 缓存数据
        this.#cacheRows(data);
      }
      if (this.#utils.zoomStart > 0 || this.#utils.zoomEnd < data.length) {
        // 缩放了
        drawData = data.slice(this.#utils.zoomStart, this.#utils.zoomEnd);
      }
      if (drawData.length > pixelWidth) {
        // 抽取数据
        drawData = combineData(data, pixelWidth);
      }
    } else {
      // 不允许缩放状态，判断数据是否符合要求
      if (drawData.length > pixelWidth) {
        return false;
      }
    }
    const rowImageData = this.#data2ImageData(drawData, pixelWidth);
    const sd = this.#rainImageData.slice(0, pixelWidth * 4 * (rowCount - 1));
    this.#rainImageData.set(sd, pixelWidth * 4);
    this.#rainImageData.set(rowImageData, 0);

    // 缓存绘图数据，主要是修改Y轴范围时更新颜色
    this.#preDrawRows.push(drawData);
    if (this.#preDrawRows.length > rowCount) {
      // this.#preDrawRows.shift();
      this.#preDrawRows = this.#preDrawRows.slice(1);
    }
    return true;
  };

  #cacheRows = (data) => {
    // 数据变了，重置缓存
    if (this.#prevDataLen !== data.length) {
      this.#rowsBuffer = [];
    }
    if (
      this.#rowsBuffer.length > 0 &&
      data.length * (this.#rowsBuffer.length + 1) > 50000
    ) {
      // this.#rowsBuffer.shift();
      this.#rowsBuffer = this.#rowsBuffer.slice(1);
    }
    this.#rowsBuffer.push(data);
  };

  #data2ImageData = (drawData, pixelWidth) => {
    const perWidth = pixelWidth / drawData.length;
    const rowData = new Uint8ClampedArray(pixelWidth * 4);
    const { minimumY, maximumY } = this.#utils;
    // 默认给最小值颜色？
    // let c =
    let startX = 0;
    for (let i = 0; i < drawData.length; i += 1) {
      // let colorIndex = Math.round(drawData[i]) - this.#utils.minimumY; // 坐标轴 -20
      const val = drawData[i];
      if (val > -9999 && val < 9999) {
        let colorIndex = Math.round(val);
        if (colorIndex < minimumY) {
          colorIndex = minimumY;
        }
        if (colorIndex > maximumY) {
          colorIndex = maximumY;
        }

        const c = this.#colors[colorIndex];
        const endX = Math.round(perWidth * i + perWidth);
        for (let x = startX; x < endX; x += 1) {
          const cIndex = x * 4;
          rowData[cIndex] = c.R;
          rowData[cIndex + 1] = c.G;
          rowData[cIndex + 2] = c.B;
          rowData[cIndex + 3] = 255;
        }
        startX = endX;
      }
    }
    return rowData;
  };
  #drawRows = (pixelWidth, rowCount) => {
    const newImageData = new ImageData(
      this.#rainImageData,
      pixelWidth,
      rowCount
    );
    this.#utils.ctx.putImageData(newImageData, 0, 0);
  };

  /**
   * 设置二维矩阵数据
   * @param {Array} matrix 矩阵数据
   * @param {Number} updateRows 更新数据行数  0表示全部更新
   * @param {boolean} isOver 最后一帧是否已经完成
   * @returns
   */
  setMatrix(matrix, updateRows = 0, isOver) {
    const { width, height } = this.#utils.rect;
    if (!matrix || matrix.length === 0) {
      // this.#utils.ctx.clearRect(0, 0, width, height);
      this.clear();
      return;
    }
    const rowCount = matrix.length;
    const colCount = matrix[0].length;

    if (!width || width === 0 || !height || height === 0) return;
    // 初始化buffer
    this.#initImageData(colCount, rowCount, width, height);
    let drawMatrix = matrix;

    if (this.#utils.zoomInside) {
      this.#prevSourceMatrix = matrix;
      // 处理抽取
      if (
        colCount > width ||
        this.#utils.zoomStart > 0 ||
        this.#utils.zoomEnd < colCount
      ) {
        const data = [];
        matrix.forEach((item) => {
          let d = item;
          if (this.#utils.zoomStart > 0 || this.#utils.zoomEnd < colCount) {
            // 缩放了
            d = item.slice(this.#utils.zoomStart, this.#utils.zoomEnd);
          }
          // 抽取数据
          if (d.length > width) {
            d = combineData(d, width);
          }
          data.push(d);
        });
        drawMatrix = data;
      }
    } else {
      // 不允许缩放状态，判断数据是否符合要求
      if (rowCount > height || colCount > width) return;
      // if (rowCount !== height || colCount !== width) return;
    }
    this.#drawMatrix(drawMatrix, updateRows, isOver);
    // this.#drawMatrix(drawMatrix, 15);
  }

  /**
   *
   * @param {Array<Array>} matrix 倒序二维矩阵
   */
  #drawMatrix = (matrix, updateRows, isOver) => {
    const rowCount = matrix.length;
    const validRow = matrix[0];
    const colCount = validRow.length;
    let fixedRows = 0;
    const { width, height } = this.#utils.rect;
    if (updateRows > 0 && updateRows < rowCount) {
      // 顺位
      let cpyTarget = 0;
      let cpyStart = 0;
      fixedRows = rowCount - updateRows;
      cpyTarget = updateRows * width * 4;
      if (!this.#preIsOver) {
        // 上回没收完
        cpyStart = width * 4;
      }
      if (cpyTarget > 0) {
        this.#rainImageData.copyWithin(
          cpyTarget,
          cpyStart,
          this.#rainImageData.length - cpyTarget
        );
      }
    }
    this.#preIsOver = isOver;

    const { minimumY, maximumY } = this.#utils;
    const perWidth = width / colCount;
    const perHeight = height / rowCount;
    let startY = 0;
    let row = 0;
    for (let y = rowCount - 1; y >= fixedRows; y -= 1) {
      const endY = Math.round(perHeight * row + perHeight);
      const oneRow = matrix[y];
      let startX = 0;
      for (let x = 0; x < colCount; x += 1) {
        const val = oneRow[x];
        const endX = Math.round(perWidth * x + perWidth);

        if (
          Number.isNaN(val) ||
          val === undefined ||
          val <= -9999 ||
          val > 9999
        ) {
          // 无填充，透明
          for (let px = startX; px < endX; px += 1) {
            for (let py = startY; py < endY; py += 1) {
              const cIndex = (px + width * py) * 4;
              this.#rainImageData[cIndex + 3] = 0;
            }
          }
        } else {
          // 根据颜色填充
          let colorIndex = Math.round(val);
          if (colorIndex < minimumY) {
            colorIndex = minimumY;
          }
          if (colorIndex > maximumY) {
            colorIndex = maximumY;
          }
          const c = this.#colors[colorIndex];

          for (let px = startX; px < endX; px += 1) {
            for (let py = startY; py < endY; py += 1) {
              const cIndex = (px + width * py) * 4;
              this.#rainImageData[cIndex] = c.R;
              this.#rainImageData[cIndex + 1] = c.G;
              this.#rainImageData[cIndex + 2] = c.B;
              this.#rainImageData[cIndex + 3] = 255;
            }
          }
        }
        startX = endX;
      }
      startY = endY;
      row += 1;
    }
    const newImageData = new ImageData(this.#rainImageData, width);
    this.#utils.ctx.putImageData(newImageData, 0, 0);
  };

  #drawMatrix1 = (matrix) => {
    const rowCount = matrix.length;
    const colCount = matrix[0].length;
    const { width, height } = this.#utils.rect;
    // console.log(colCount, width, height);
    this.#rainImageData = new Uint8ClampedArray(rowCount * colCount);
    const perWidth = width / colCount;
    // const perHeight = height / rowCount;
    // let startY = 0;
    // console.log("dddd", matrix.length);
    for (let y = 0; y < matrix.length; y += 1) {
      // const endY = Math.round(perHeight * y + perHeight);
      const oneRow = matrix[y];
      let startX = 0;
      for (let x = 0; x < oneRow.length; x += 1) {
        let colorIndex = Math.round(oneRow[x]) - this.#utils.minimumY; // 坐标轴 -20
        if (colorIndex > 99) {
          colorIndex = 99;
        }
        if (colorIndex < 0) {
          colorIndex = 0;
        }
        const c = this.#colors[colorIndex];
        const endX = Math.round(perWidth * x + perWidth);
        for (let px = startX; px < endX; px += 1) {
          // for (let py = startY; py < endY; py += 1) {
          try {
            const cIndex = (px + colCount * y) * 4;
            this.#rainImageData[cIndex] = c.R;
            this.#rainImageData[cIndex + 1] = c.G;
            this.#rainImageData[cIndex + 2] = c.B;
            this.#rainImageData[cIndex + 3] = 255;
          } catch {
            console.log("error");
          }
          // }
        }
        startX = endX;
      }
      // startY = endY;
    }
    const newImageData = new ImageData(this.#rainImageData, width);
    // this.ctx.scale(1, perHeight);
    this.#utils.ctx.putImageData(newImageData, 0, 0, 0, 0, width, height);
  };

  /**
   * 缩放
   * @param {Number} start
   * @param {Number} end
   */
  zoom(start, end) {
    if (this.#utils.zoomInside) {
      this.#utils.zoomStart = start;
      this.#utils.zoomEnd = end;
      // 重绘
      const { width, height } = this.#utils.rect;
      // this.#utils.ctx.clearRect(0, 0, width, height);
      if (this.#prevSourceMatrix) {
        this.setMatrix(this.#prevSourceMatrix);
      }
      if (this.#rowsBuffer.length > 0) {
        // 重置数据
        const rainImgData = this.#utils.ctx.createImageData(width, height);
        this.#rainImageData = rainImgData.data;
        this.#rowsBuffer.forEach((row) => {
          this.#rowDataCast(row, height, width, true);
        });
        this.#drawRows(width, height);
      }
    }
  }

  /**
   * Y轴范围变更
   * @param {Numer} minimum
   * @param {Numer} maximum
   */
  setAxisYRange(minimum, maximum) {
    if (minimum >= maximum) return;
    this.#utils.minimumY = minimum;
    this.#utils.maximumY = maximum;
    this.#initColors();
    // 更新颜色
    const { width, height } = this.#utils.rect;
    this.#utils.ctx.clearRect(0, 0, width, height);
    if (this.#prevSourceMatrix) {
      this.setMatrix(this.#prevSourceMatrix);
    }
    if (this.#preDrawRows.length > 0) {
      // 重置数据
      const rainImgData = this.#utils.ctx.createImageData(width, height);

      this.#rainImageData = rainImgData.data;
      this.#preDrawRows.forEach((row, index) => {
        const rowImageData = this.#data2ImageData(row, width);
        this.#rainImageData.set(rowImageData, index * width * 4);
      });
      this.#drawRows(width, height);
    }
  }

  clear() {
    // 清除绘制
    this.#utils.ctx.clearRect(
      0,
      0,
      this.#utils.rect.width,
      this.#utils.rect.height
    );
    // 清除数据缓存
    this.#prevDataLen = 0;
    this.#rowsBuffer = [];
    this.#prevSourceMatrix = undefined;
    this.#preDrawRows = [];
  }

  resize() {
    this.#utils.resize();
  }

  dispose() {
    this.#utils.dispose();
  }
}

const chartKernels = {};

/**
 * 雨点图/瀑布图
 */
class Rainchart {
  // 记录上一次单行数据长度
  #prevDataLen = 1;
  // 上一次像素高度
  #prevPixelHeight = 1;
  // 上一次像素宽度
  #prevPixelWidth = 1;
  // 记录历史源数据（矩阵），主要是用于重绘
  #prevSourceMatrix;
  // 缓存源数据行，最大数据点数50000
  #rowsBuffer = [];
  // 瀑布图色带颜色，渐变后
  #colors = [];
  // 瀑布图数据缓存
  #rainImageData = [];
  // 辅助
  #utils;
  // 上一次行数
  #prevRowCount = 1;
  // TODO 保证在调整Y轴范围时不会丢数据重绘
  #preDrawRows = [];

  // gpu;
  #id = Number(Math.random().toString().substring(2)).toString(36);

  /**
   * 构造函数
   * @param {HTMLElement} container
   * @param {Array<string>} pallete 色带颜色，从大到小
   * onSizeChange canvas像素尺寸变更回调
   * zoomInside 是否支持内部缩放，默认 true，false则调用zoom()不生效
   * renderMode 瀑布图渲染模式，fill 完全填充，pixel横向填充、纵向（行）按像素填充
   * @param {{onSizeChange:Function<Number,Number>, zoomInside:boolean, renderMode:String}} optionMore
   */
  constructor(container, pallete, optionMore) {
    // super(container, optionMore);
    this.#utils = new chartUtils(container, optionMore, () => {}, true);
    this.#utils.colorBlends = pallete || ["#0000FF", "#00FF00", "#FF0000"];
    console.log(this.#utils);
    this.#initColors();
    window.onbeforeunload = () => {
      chartKernels[this.#id]?.destroy();
    };
  }

  #initGpuKernel = () => {
    const cvs = this.#utils.cvs;
    let mode = "";
    if (GPU.isWebGL2Supported) {
      mode = "webgl2";
    } else if (GPU.isWebGLSupported) {
      mode = "webgl";
    }
    const gpu = new GPU({ mode, canvas: cvs });
    // const gpu = new GPU({ mode: "cpu", canvas: cvs });
    gpu.addFunction(`function getMaximum_chart(
      matrix,
      dyStart,
      dyEnd,
      dxStart,
      dxEnd
    ) {
      var maxData = -9999;
      for (var dy = dyStart; dy <= dyEnd; dy++) {
        for (var dx = dxStart; dx <= dxEnd; dx++) {
          var data = matrix[dy][dx];
          if (data > maxData) {
            maxData = data;
          }
        }
      }
      return maxData;
    }`);

    gpu.addFunction(`function renderWidthFill_chart(matrix, colors, params) {
      var width = params[0];
      var height = params[1];
      var rowCount = params[2];
      var colCount = params[3];
      var minY = params[4];
      var maxY = params[5];

      // 1.
      var zoomStartX = params[7];
      if (zoomStartX < 0) zoomStartX = 0;
      var zoomEndX = params[8];
      if (zoomEndX < 0) zoomEndX = colCount - 1;
      colCount = zoomEndX - zoomStartX + 1;
      var zoomStartY = params[9];
      if (zoomStartY < 0) zoomStartY = 0;
      var zoomEndY = params[10];
      if (zoomEndY < 0) zoomEndY = rowCount - 1;
      rowCount = zoomEndY - zoomStartY + 1;
      var rowPixel = this.thread.y;
      var colPixel = this.thread.x;
      var data = -9999;
      if (colCount > 0) {
        if (width >= colCount) {
          var perWidth = width / colCount;
          var dxIndex = zoomStartX + Math.floor(colPixel / perWidth);
          if (height >= rowCount) {
            var perHeight = height / rowCount;
            var dyIndex = Math.floor(rowPixel / perHeight);
            data = matrix[dyIndex][dxIndex];
            // TODO 无效值处理
          } else {
            // 高度不够
            var perGap = rowCount / height;
            var dyStart = Math.floor(rowPixel * perGap);
            var dyEnd = Math.round(dyStart + perGap);
            // eslint-disable-next-line no-undef
            data = getMaximum_chart(matrix, dyStart, dyEnd, dxIndex, dxIndex);
          }
        } else {
          // 宽度不够
          var perGapX = colCount / width;
          var dxStart = zoomStartX + Math.floor(colPixel * perGapX);
          var dxEnd = Math.round(dxStart + perGapX);
          if (height >= rowCount) {
            var _perHeight = height / rowCount;
            var _dyIndex = Math.floor(rowPixel / _perHeight);
            // eslint-disable-next-line no-undef
            data = getMaximum_chart(matrix, _dyIndex, _dyIndex, dxStart, dxEnd);
          } else {
            // 高度不够
            var _perGap = rowCount / height;
            var _dyStart = Math.floor(rowPixel * _perGap);
            var _dyEnd = Math.round(_dyStart + _perGap);
            // eslint-disable-next-line no-undef
            data = getMaximum_chart(matrix, _dyStart, _dyEnd, dxStart, dxEnd);
          }
        }
      }
      if (data > -9999) {
        if (data > maxY) data = maxY;
        if (data < minY) data = minY;
        var cIndex = Math.floor(data - minY);
        if (cIndex < 0) cIndex = 0;
        if (cIndex > maxY - minY) cIndex = maxY - minY + 1;
        var colorStart = cIndex * 3;
        this.color(
          colors[colorStart] / 255.0,
          colors[colorStart + 1] / 255.0,
          colors[colorStart + 2] / 255.0
        );
      } else {
        this.color(0, 0, 0);
      }
    }`);

    gpu.addFunction(`function renderWidthPixel_chart(matrix, colors, params) {
      var width = params[0];
      var height = params[1];
      var rowCount = params[2];
      var colCount = params[3];
      var minY = params[4];
      var maxY = params[5];

      // 1. 横向缩放数据索引
      var zoomStartX = params[7];
      if (zoomStartX < 0) zoomStartX = 0;
      var zoomEndX = params[8];
      if (zoomEndX < 0) zoomEndX = colCount - 1;
      colCount = zoomEndX - zoomStartX + 1;

      const rowPixel = this.thread.y;
      const colPixel = this.thread.x;
      let rowOffset = 0;
      if (height > rowCount) {
        rowOffset = height - rowCount;
      }
      let data = -9999;
      if (rowPixel <= rowCount) {
        // 数据不够时需要-offset
        const dataRow = rowPixel;        
        if (width >= colCount) {
          const perWidth = width / colCount;
          const dxIndex = zoomStartX + Math.floor(colPixel / perWidth);
          data = matrix[dataRow][dxIndex];
        } else {
          // 宽度不够
          var perGapX = colCount / width;
          var dxStart = zoomStartX + Math.floor(colPixel * perGapX);
          var dxEnd = Math.round(dxStart + perGapX);
          // eslint-disable-next-line no-undef
          data = getMaximum_chart(matrix, dataRow, dataRow, dxStart, dxEnd);
        }        
      }
      if (data > -9999) {
        if (data > maxY) data = maxY;
        if (data < minY) data = minY;
        var cIndex = Math.floor(data - minY);
        if (cIndex < 0) cIndex = 0;
        if (cIndex > maxY - minY) cIndex = maxY - minY + 1;
        var colorStart = cIndex * 3;
        this.color(
          colors[colorStart] / 255.0,
          colors[colorStart + 1] / 255.0,
          colors[colorStart + 2] / 255.0
        );
      } else {
        this.color(0, 0, 0);
      }
    }`);
    chartKernels[this.#id] = gpu.createKernel(
      /**
       * matrix 数据矩阵 [[],[],...]
       * colors 颜色数组 [r,g,b,r,g,b,...]
       * params 辅助参数 [width,height,rowCount,colCount,minY,maxY,fillMode]
       * fillMode: 0-fill 区域完全填充 1-pixel 一个像素对应一行
       */
      new Function(
        `return function (matrix, colors, params) {
        const fillMode = params[6];
        if (fillMode === 0) {
          // eslint-disable-next-line no-undef
          renderWidthFill_chart(matrix, colors, params);
        } else {
          // eslint-disable-next-line no-undef
          renderWidthPixel_chart(matrix, colors, params);
        }
      };`
      )(),
      {
        dynamicArguments: true,
        dynamicOutput: true,
        graphical: true,
      }
    );
  };

  /**
   * 初始化绘制数据
   * @param {Number} dataLen
   * @param {Number} pixelWidth
   * @param {Number} rowCount
   */
  #initImageData = (dataLen, rowCount, pixelWidth, pixelHeight) => {
    if (
      this.#prevDataLen !== dataLen ||
      this.#prevPixelHeight !== pixelHeight ||
      this.#prevPixelWidth !== pixelWidth ||
      this.#prevRowCount !== rowCount
    ) {
      this.#prevDataLen = dataLen;
      this.#prevPixelHeight = pixelHeight;
      this.#prevPixelWidth = pixelWidth;
      this.#prevRowCount = rowCount;
      const kernel = chartKernels[this.#id];
      if (!kernel) {
        this.#initGpuKernel();
      }
      chartKernels[this.#id].setOutput([pixelWidth, pixelHeight]);
    }
  };

  #initColors = () => {
    const { minimumY, maximumY, colorBlends } = this.#utils;
    this.#colors = gradientColorsGPU(colorBlends, maximumY - minimumY + 1);
  };

  /**
   * 添加一行数据
   * @param {Array} data
   * @param {Number} timestamp
   */
  addRow(data) {
    let rowCount = this.#utils.rect.height;
    let pixelWidth = this.#utils.rect.width;
    // 初始化buffer
    this.#initImageData(data.length, rowCount, pixelWidth, rowCount);
    if (!this.#rowDataCast(data, rowCount, pixelWidth)) {
      return;
    }
    this.#drawRows(pixelWidth, rowCount);
  }

  #rowDataCast = (data, rowCount, pixelWidth, cacheMode) => {
    let drawData = data;
    if (this.#utils.zoomInside) {
      if (!cacheMode) {
        // 缓存数据
        this.#cacheRows(data);
      }
      if (this.#utils.zoomStart > 0 || this.#utils.zoomEnd < data.length) {
        // 缩放了
        drawData = data.slice(this.#utils.zoomStart, this.#utils.zoomEnd);
      }
      if (drawData.length > pixelWidth) {
        // 抽取数据
        drawData = combineData(data, pixelWidth);
      }
    } else {
      // 不允许缩放状态，判断数据是否符合要求
      if (drawData.length > pixelWidth) {
        return false;
      }
    }
    const rowImageData = this.#data2ImageData(drawData, pixelWidth);
    const sd = this.#rainImageData.slice(0, pixelWidth * 4 * (rowCount - 1));
    this.#rainImageData.set(sd, pixelWidth * 4);
    this.#rainImageData.set(rowImageData, 0);

    // 缓存绘图数据，主要是修改Y轴范围时更新颜色
    this.#preDrawRows.push(drawData);
    if (this.#preDrawRows.length > rowCount) {
      // this.#preDrawRows.shift();
      // slice性能更好？
      this.#preDrawRows = this.#preDrawRows.slice(1);
    }
    return true;
  };

  #cacheRows = (data) => {
    // 数据变了，重置缓存
    if (this.#prevDataLen !== data.length) {
      this.#rowsBuffer = [];
    }
    if (
      this.#rowsBuffer.length > 0 &&
      data.length * (this.#rowsBuffer.length + 1) > 50000
    ) {
      // slice性能更好？
      // this.#rowsBuffer.shift();
      this.#rowsBuffer = this.#rowsBuffer.slice(1);
    }
    this.#rowsBuffer.push(data);
  };

  #data2ImageData = (drawData, pixelWidth) => {
    const perWidth = pixelWidth / drawData.length;
    const rowData = new Uint8ClampedArray(pixelWidth * 4);
    const { minimumY, maximumY } = this.#utils;
    // 默认给最小值颜色？
    // let c =
    let startX = 0;
    for (let i = 0; i < drawData.length; i += 1) {
      // let colorIndex = Math.round(drawData[i]) - this.#utils.minimumY; // 坐标轴 -20
      const val = drawData[i];
      if (val > -9999 && val < 9999) {
        let colorIndex = Math.round(val);
        if (colorIndex < minimumY) {
          colorIndex = minimumY;
        }
        if (colorIndex > maximumY) {
          colorIndex = maximumY;
        }

        const c = this.#colors[colorIndex];
        const endX = Math.round(perWidth * i + perWidth);
        for (let x = startX; x < endX; x += 1) {
          const cIndex = x * 4;
          rowData[cIndex] = c.R;
          rowData[cIndex + 1] = c.G;
          rowData[cIndex + 2] = c.B;
          rowData[cIndex + 3] = 255;
        }
        startX = endX;
      }
    }
    return rowData;
  };
  #drawRows = (pixelWidth, rowCount) => {
    const newImageData = new ImageData(
      this.#rainImageData,
      pixelWidth,
      rowCount
    );
    this.#utils.ctx.putImageData(newImageData, 0, 0);
  };

  /**
   * 设置二维矩阵数据
   * @param {Array} matrix 矩阵数据
   * @param {Number} updateRows 更新数据行数  0表示全部更新
   * @param {boolean} isOver 最后一帧是否已经完成
   * @returns
   */
  setMatrix(matrix, updateRows = 0, isOver) {
    const { width, height } = this.#utils.rect;
    if (!matrix || matrix.length === 0) {
      // this.#utils.ctx.clearRect(0, 0, width, height);
      this.clear();
      return;
    }
    const rowCount = matrix.length;
    const colCount = matrix[0].length;

    if (!width || width === 0 || !height || height === 0) return;
    // 初始化buffer
    this.#initImageData(colCount, rowCount, width, height);
    this.#drawMatrix(matrix, updateRows, isOver);
  }

  /**
   *
   * @param {Array<Array>} matrix 倒序二维矩阵
   */
  #drawMatrix = (matrix) => {
    const { minimumY, maximumY } = this.#utils;
    const { width, height } = this.#utils.rect;
    const rowCount = matrix.length;
    const validRow = matrix[0];
    const colCount = validRow.length;
    this.#prevSourceMatrix = matrix;
    const args = [
      width,
      height,
      rowCount,
      colCount,
      minimumY,
      maximumY,
      0,
      this.#utils.zoomStart || -1,
      this.#utils.zoomEnd || -1,
      this.#utils.zoomStartY || -1,
      this.#utils.zoomEndY || -1,
    ];
    chartKernels[this.#id]?.(matrix, this.#colors, args);
  };

  /**
   * 缩放
   * @param {Number} startX %
   * @param {Number} endX %
   * @param {Number} startY %
   * @param {Number} endY %
   */
  zoom(startX, endX, startY, endY) {
    if (this.#utils.zoomInside) {
      this.#utils.zoomStart = startX;
      this.#utils.zoomEnd =
        endX < this.#prevDataLen ? endX : this.#prevDataLen - 1;
      this.#utils.zoomStartY = startY;
      this.#utils.zoomEndY = endY;
      if (endY)
        this.#utils.zoomEndY =
          endY < this.#prevRowCount ? endY : this.#prevRowCount - 1;

      // 重绘
      // const { width, height } = this.#utils.rect;
      // this.#utils.ctx.clearRect(0, 0, width, height);
      if (this.#prevSourceMatrix) {
        this.setMatrix(this.#prevSourceMatrix);
      }
      // if (this.#rowsBuffer.length > 0) {
      //   // 重置数据
      //   const rainImgData = this.#utils.ctx.createImageData(width, height);
      //   this.#rainImageData = rainImgData.data;
      //   this.#rowsBuffer.forEach((row) => {
      //     this.#rowDataCast(row, height, width, true);
      //   });
      //   this.#drawRows(width, height);
      // }
    }
  }
  /**
   * 通过设置百分比来缩放
   * @param {Number} startX
   * @param {Number} endX
   */
  zoomPercent(startX, endX, startY = 0, endY = 1) {
    // 根据百分比计算当前索引
    const { zoomPercent, zoomPercentY } = this.#utils;

    // X 轴方向
    let newStart = startX;
    let newEnd = endX;
    if (startX > 0 || endX < 1) {
      const pctGap = zoomPercent.end - zoomPercent.start;
      newStart = zoomPercent.start + pctGap * startX;
      newEnd = zoomPercent.start + pctGap * endX;
    }
    this.#utils.zoomPercent = {
      start: newStart,
      end: newEnd,
    };
    // Y 轴方向
    let newStartY = startY;
    let newEndY = endY;
    const pctGapY = zoomPercentY.end - zoomPercentY.start;
    if (startY > 0 || endY < 1) {
      newStartY = zoomPercentY.start + pctGapY * startY;
      newEndY = zoomPercentY.start + pctGapY * endY;
    }
    this.#utils.zoomPercentY = {
      start: newStartY,
      end: newEndY,
    };

    this.zoom(
      Math.floor(this.#prevDataLen * newStart),
      Math.round(this.#prevDataLen * newEnd),
      Math.floor(this.#prevRowCount * newStartY),
      Math.round(this.#prevRowCount * newEndY)
    );
  }

  resetZoom() {
    this.zoom(0, this.#prevDataLen - 1, 0, this.#prevRowCount - 1);
  }

  /**
   * Y轴范围变更
   * @param {Numer} minimum
   * @param {Numer} maximum
   */
  setAxisYRange(minimum, maximum) {
    if (minimum >= maximum) return;
    this.#utils.minimumY = minimum;
    this.#utils.maximumY = maximum;
    this.#initColors();
    // 更新颜色
    const { width, height } = this.#utils.rect;
    // this.#utils.ctx.clearRect(0, 0, width, height);
    if (this.#prevSourceMatrix) {
      this.setMatrix(this.#prevSourceMatrix);
    }
    if (this.#preDrawRows.length > 0) {
      // 重置数据
      const rainImgData = this.#utils.ctx.createImageData(width, height);

      this.#rainImageData = rainImgData.data;
      this.#preDrawRows.forEach((row, index) => {
        const rowImageData = this.#data2ImageData(row, width);
        this.#rainImageData.set(rowImageData, index * width * 4);
      });
      this.#drawRows(width, height);
    }
  }

  clear() {
    // 清除绘制
    chartKernels[this.#id]?.([[0]], [0], [1, 1, 0, 0, 0, 1, 0, -1, -1, -1, -1]);
    // 清除数据缓存
    this.#prevDataLen = 0;
    this.#rowsBuffer = [];
    this.#prevSourceMatrix = undefined;
    this.#preDrawRows = [];
  }

  resize() {
    this.#utils.resize();
  }

  dispose() {
    this.#utils.dispose();
    chartKernels[this.#id]?.destroy();
  }
}

/**
 * 线图
 */
class NormalChart {
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

  // 柱状图绘制数据，缓存下来用于多图表绘制
  /**
   * 构造函数
   * @param {HTMLElement} container 容器
   * onSizeChange: ()=>{}
   * canvas宽高（像素）变化事件
   * zoomInside:true
   * true内部根据像素宽度/高度处理数据抽取，且可调用zoom方法设置缩放位置；
   * false则内部不进行数据抽取，外部需要订阅onSizeChange回调根据像素宽度/高度先进行数据抽取，如果数据不满足要求，则不进行绘制
   * barFront:false
   * true bar绘制在顶层
   * false bar绘制在底层
   * @param {{onSizeChange:Function<Number,Number>, zoomInside:boolean, barFront:boolean}} optionMore
   */
  constructor(container, optionMore) {
    this.#utils = new chartUtils(container, optionMore, () => {
      // 更新设置重绘
      this.#drawData();
    });
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
        if (series.type === "bar") {
          throw "Only one 'bar' series can be supported.";
        }
      }
      // 缓存bar图名称
      this.#drawOrder.barSeries = name;
      // 转换颜色
      const R = parseInt(`0x${c.substring(1, 3)}`);
      const G = parseInt(`0x${c.substring(3, 5)}`);
      const B = parseInt(`0x${c.substring(5, 7)}`);
      let A = 255;
      if (c.length > 8) {
        A = parseInt(`0x${c.substring(7, 9)}`);
      }
      seriesOptions.colorRGBA = { R, G, B, A };
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
   * @param {{name:String, color:String, thickness:Number, pointWidth:Number, pointColor:String, showPoint:boolean, visible:boolean, type:'point'|'line'|'stepline'|'bar', barOrientation:'vertical'|'horizontal', barAlign:'right' | 'left',specialBar:boolean,NBStep:boolean}} options
   *
   */
  initSeries(options) {
    this.#initNormalSeries(options);
  }

  /**
   * 初始化流图
   * @param {{name:String, color:String, thickness:Number, visible:boolean, type:'line'|'stepline'}} options
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
    this.#utils.ctx.clearRect(0, 0, width, height);
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
        // let outMax =
        // drawData.length > this.#maxPoint ? this.#maxPoint : drawData.length;
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
          if (series.NBStep) {
            const newData = this.#test(drawData);
            this.#drawStepLine(newData, series);
          } else {
            this.#drawStepLine(drawData, series);
          }
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
      const y = Math.floor(height - scale * height);
      // 移动到当前值得底部位置
      ctx.moveTo(pStart, yOffset);
      // 移动到值得位置
      ctx.lineTo(pStart, y);
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
      ctx.moveTo(pStart, yOffset);
      // 移动到值得位置
      ctx.lineTo(pStart, y);
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
    const { color, thickness } = series;
    const { width, height } = this.#utils.rect;
    const yRange = this.#utils.maximumY - this.#utils.minimumY;
    const perWidth = width / data.length;
    const halfWidth = perWidth / 2;
    this.#utils.ctx.beginPath();
    let y = 0;
    let first = -1;
    for (let i = 0; i < data.length; i += 1) {
      const x = perWidth * i + halfWidth;
      const val = data[i];
      if (val > -9999) {
        const scale = (val - this.#utils.minimumY) / yRange;
        y = Math.floor(height - scale * height);
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
        const x = perWidth * i;
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
        const x = perWidth * i;
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

  /**
   * 老板要求插值，ca jj
   * 整个牛逼的做法
   * @param {*} data
   */
  #test = (data) => {
    const { width } = this.#utils.rect;

    const scale = width / 2 / data.length;
    const n = Math.floor(scale - 1);
    if (n > 0) {
      const newData = [];
      for (let i = 0; i < data.length - 1; i += 1) {
        const d1 = data[i];
        const d2 = data[i + 1];
        newData.push(d1);
        const gap = (d2 - d1) / n;
        for (let j = 1; j <= n; j += 1) {
          newData.push(d1 + gap * j);
        }
      }
      newData.push(data[data.length - 1]);
      return newData;
    }
    return data;
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
        const x = perWidth * i + offset;
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
      const diamondOffsetX = pointWidth * 0.707;
      const diamondOffsetY = pointWidth;
      const diamondPath = new Path2D();
      for (let i = 0; i < data.length; i += 1) {
        const x = perWidth * i;
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
      const x = (xData[i] - minX) * xScale - half;
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
      const x = (xData[i] - minX) * xScale;
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
  setAxisYRange(minimum, maximum) {
    this.#utils.minimumY = minimum;
    this.#utils.maximumY = maximum;
    // 重绘
    this.#drawData();
  }

  clear() {
    for (const key in this.#series) {
      const series = this.#series[key];
      series.sourceData = undefined;
    }
    this.#utils.ctx.clearRect(
      0,
      0,
      this.#utils.rect.width,
      this.#utils.rect.height
    );
  }

  resize() {
    this.#utils.resize();
  }

  dispose() {
    this.#utils.dispose();
  }
}

export {
  DPXChart,
  NormalChart,
  Rainchart$1 as RainChart,
  Rainchart as RainChartGPU,
  combineData,
  gradientColors,
};
