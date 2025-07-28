import { gradientColors } from "./chartUtils";
import {
  Application,
  Sprite,
  Texture,
  BufferResource,
  BaseTexture,
  Rectangle,
  FORMATS,
  ALPHA_MODES,
  WRAP_MODES,
} from "pixi.js";

/**
 * 用于创建线程方法
 */
const workerThread = () => {
  var prevDataLen;
  /**
   * @type {Uint8Array}
   */
  var rainImageData;
  var colors = [];
  var preDrawRows = [];
  // 缓存源数据
  var rowsBuffer = [];
  var rowDefault;
  var states = {
    zoomInside: true,
    zoomStart: -1,
    zoomEnd: 100,
    zoomLen: 101,
    minimumY: -20,
    maximumY: 80,
    width: 100,
    height: 100,
  };
  var prevRender = new Date().getTime();

  /**
   * 抽取
   * @param {Array} spectrum
   * @param {Number} outMax
   * @param {Array} referData 参考数据和outMax一样长，这样可用避免重复new Array而节省性能
   * @returns
   */
  function combineData(spectrum, outMax, referData) {
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
  }

  data2ImageData = (drawData, pixelWidth) => {
    const dataLen = drawData.length;
    const perWidth = pixelWidth / dataLen;
    // const rowData = new Uint8ClampedArray(pixelWidth * 4);
    const rowData = rowDefault.slice(0);
    const { minimumY, maximumY } = states;
    // 默认给最小值颜色？
    // let c =
    let startX = 0;
    for (let i = 0; i < dataLen; i += 1) {
      // let colorIndex = Math.round(drawData[i]) - this.#utils.minimumY; // 坐标轴 -20
      const val = drawData[i];
      if (val > -9999) {
        let colorIndex = Math.round(val);
        if (colorIndex < minimumY) {
          colorIndex = minimumY;
        }
        if (colorIndex > maximumY) {
          colorIndex = maximumY;
        }

        const c = colors[colorIndex];
        const endX = Math.round(perWidth * i + perWidth);
        for (let x = startX; x < endX; x += 1) {
          const cIndex = x * 3;
          rowData[cIndex] = c.B;
          rowData[cIndex + 1] = c.G;
          rowData[cIndex + 2] = c.R;
          // rowData[cIndex + 3] = 255;
        }
        startX = endX;
      }
    }
    return rowData;
  };

  rowDataCast = (data, rowCount, pixelWidth) => {
    let drawData = data;
    // if (states.zoomInside) {
    // if (!cacheMode) {
    // 缓存数据
    cacheRows(data);
    // }
    // if (states.zoomStart > 0 || states.zoomEnd < data.length) {
    //   // 缩放了
    //   drawData = data.slice(states.zoomStart, states.zoomEnd);
    // }
    if (drawData.length > pixelWidth) {
      // 抽取数据
      drawData = combineData(data, pixelWidth);
    }
    // } else {
    //   // 不允许缩放状态，判断数据是否符合要求
    //   if (drawData.length > pixelWidth) {
    //     return false;
    //   }
    // }
    // console.log(pixelWidth);
    const rowImageData = data2ImageData(drawData, pixelWidth);
    // 插入第一行
    const startIndex = pixelWidth * 3;
    rainImageData.copyWithin(startIndex, 0, rainImageData.length - startIndex);
    // const sd = rainImageData.slice(0, startIndex * (rowCount - 1));
    // rainImageData.set(sd, startIndex);
    rainImageData.set(rowImageData, 0);

    // 缓存绘图数据，主要是修改Y轴范围时更新颜色
    preDrawRows.push(drawData);
    if (preDrawRows.length > rowCount) {
      preDrawRows = preDrawRows.slice(1);
    }
    return true;
  };

  cacheRows = (data) => {
    // 数据变了，重置缓存
    if (prevDataLen !== data.length) {
      rowsBuffer = [];
    }
    if (
      rowsBuffer.length > 0 &&
      data.length * (rowsBuffer.length + 1) > 50000
    ) {
      rowsBuffer = rowsBuffer.slice(1);
    }
    rowsBuffer.push(data);
  };

  /**
   * 添加一行数据
   * @param {Array} data
   */
  function addRow(data) {
    let rowCount = states.height;
    let pixelWidth = states.width;
    const dataLen = data.length;
    if (dataLen !== states.zoomLen) {
      states.zoomStart = 0;
      states.zoomEnd = dataLen - 1;
      states.zoomLen = dataLen;
    }
    if (!rowDataCast(data, rowCount, pixelWidth)) {
      return;
    }
    const dt1 = new Date().getTime();
    if (dt1 - prevRender > 40) {
      const copy = rainImageData.slice(0);
      self.postMessage({ imageData: copy }, [copy.buffer]);
      prevRender = dt1;
    }
  }

  function reRender() {
    const { width } = states;
    const len = rowsBuffer.length;
    if (len > 0) {
      // 重置数据
      let index = 0;
      for (let i = len - 1; i >= 0; i -= 1) {
        const row = rowsBuffer[i];
        const rowImageData = data2ImageData(row, width);
        const startIndex = width * 3 * index;
        rainImageData.set(rowImageData, startIndex);
      }

      const copy = rainImageData.slice(0);
      self.postMessage({ imageData: copy }, [copy.buffer]);
    }
  }

  addEventListener("message", (e) => {
    const { imageData, colors: cs, row, axisYRange, zoomInfo, rect } = e.data;

    // if (imageData) {
    //   rainImageData = imageData;
    // } else
    if (row) {
      addRow(row);
    } else if (cs) {
      console.log(cs);
      colors = cs;
    } else if (axisYRange) {
      states = { ...states, ...axisYRange };
    } else if (zoomInfo) {
      states = { ...states, ...zoomInfo };
    } else if (rect) {
      states = { ...states, ...rect };
      rainImageData = new Uint8Array(rect.width * rect.height * 3);
      rowDefault = rainImageData.slice(0, rect.width * 3); // new Uint8Array(rect.width * 3);
    } else {
    }
    if (cs || axisYRange || zoomInfo || rect) {
      // 重绘
      reRender();
    }
  });
};

class RainChartGPUWorker {
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
  maximumY = 80;
  minimumY = -20;
  // gpu;
  #id = Number(Math.random().toString().substring(2)).toString(36);

  workerBlob;

  /**
   * @type {Worker}
   */
  worker;

  rect = { width: 100, height: 100 };
  rainRGBData = new Uint8Array(this.rect.width * this.rect.height * 3);
  /**
   * @type {Application}
   */
  pixi;

  /**
   * @type {BufferResource}
   */
  pixiBuffer;

  /**
   * @type {Texture}
   */
  texture;

  /**
   * @type {Sprite}
   */
  sprite;

  // 是否正在修改size
  resizing = false;

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
    this.options = optionMore || {};
    // 初始化线程
    const workerJs = `(${workerThread.toString()})()`;
    this.workerBlob = new Blob([workerJs], { type: "text/javascript" });
    const blogUrl = window.URL.createObjectURL(this.workerBlob);
    this.worker = new Worker(blogUrl, {
      name: `rainChartWorker-${Number(
        Math.random().toString().substring(2)
      ).toString(36)}`,
    });
    this.worker.onmessage = (e) => {
      const data = e.data.imageData;
      // console.log("get message from worker:::", e);
      this.#drawRows(data);

      // this.setImageData(data);

      // this.#drawMatrix(data);
    };
    this.worker.onerror = (er) => {
      console.log("worker error:::", er);
    };
    this.container = container;
    this.pixi = new Application({
      width: 955,
      height: 242,
      backgroundColor: 0x000000,
      resizeTo: container,
    });

    container.appendChild(this.pixi.view);

    // const { width, height } = this.rect;
    this.pixiBuffer = new BufferResource(this.rainRGBData, {
      width: this.rect.width,
      height: this.rect.height,
      unpackAlignment: 1,
    });
    // 创建一个 Texture 对象，并指定 bufferResource 作为像素数据来源
    this.texture = new Texture(
      new BaseTexture(this.pixiBuffer, {
        format: FORMATS.RGB,
        alphaMode: ALPHA_MODES.NPM,
      })
      // new Rectangle(0, 0, this.rect.width, this.rect.height)
    );
    this.sprite = new Sprite(this.texture);
    this.sprite.position.set(0, 0);
    this.pixi.stage.addChild(this.sprite);
    this.#onResize();

    this.colorBlends = pallete || ["#0000FF", "#00FF00", "#FF0000"];
    console.log("===========", this.colorBlends);
    this.#initColors();
    console.log("===========", this.#colors);
    this.worker.postMessage({ colors: this.#colors });
    // window.onbeforeunload = () => {
    //   chartKernels[this.#id]?.destroy();
    // };
  }

  #resizePixi() {
    // this.pixiBuffer.dispose();
    // this.pixiBuffer.destroy();
    console.log("resizePixi--------", this.rect);
    this.pixiBuffer = new BufferResource(this.rainRGBData, {
      width: this.rect.width,
      height: this.rect.height,
      unpackAlignment: 1,
    });
    // 创建一个新的 BaseTexture 对象，指定新的宽度和高度
    const newBaseTexture = new BaseTexture(this.pixiBuffer, {
      format: FORMATS.RGB,
      alphaMode: ALPHA_MODES.NPM,
      // width: this.rect.width,
      // height: this.rect.height,
    });
    // 更新纹理对象的 BaseTexture
    this.texture.baseTexture = newBaseTexture;
    // 更新纹理对象的 UV 坐标
    this.texture.frame = new Rectangle(0, 0, this.rect.width, this.rect.height);
    this.texture.updateUvs();
  }

  /**
   * 容器尺寸大小变更处理
   */
  #onResize = () => {
    this.resizeObserver = new ResizeObserver((entries) => {
      if (entries.length > 0) {
        const rect = entries[0].contentRect;
        this.rect = {
          x: Math.round(rect.x),
          y: Math.round(rect.y),
          width: Math.round(rect.width),
          height: Math.round(rect.height),
        };
        console.log(this.rect);
        // 先变更初始化
        this.pixiBuffer.dispose();
        this.pixiBuffer.destroy();
        this.pixiBuffer = null;
        const { onSizeChange } = this.options;
        if (onSizeChange) onSizeChange(this.rect.width, this.rect.height);
        this.worker.postMessage({
          rect: {
            width: this.rect.width,
            height: this.rect.height,
          },
        });
      }
    });
    this.resizeObserver.observe(this.container);
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

      // const { width, height } = this.rect;
      // this.pixiBuffer = new BufferResource(data, { width, height });
      // // 创建一个 Texture 对象，并指定 bufferResource 作为像素数据来源
      // this.texture = new Texture(
      //   new BaseTexture(this.pixiBuffer, {
      //     format: FORMATS.RGB,
      //     alphaMode: ALPHA_MODES.NPM,
      //   }),
      //   new Rectangle(0, 0, width, height)
      // );
      // const sprite = new Sprite(this.texture);
      // sprite.position.set(0, 0);
      // this.pixi.stage.addChild(sprite);
    }
  };

  #initColors = () => {
    // const { minimumY, maximumY, colorBlends } = this.#utils;
    const colors = {};
    let cIndx = 0;
    try {
      const cs = gradientColors(
        this.colorBlends,
        this.maximumY - this.minimumY + 1
      );
      for (let i = this.maximumY; i >= this.minimumY; i -= 1) {
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
   * @param {Array|Float32Array} data
   * @param {Number} timestamp
   */
  addRow(data) {
    // let rowCount = this.rect.height;
    // let pixelWidth = this.rect.width;
    if (data.length != this.#prevDataLen) {
      this.#prevDataLen = data.length;
      if (this.pixiBuffer) {
        this.pixiBuffer.dispose();
        this.pixiBuffer.destroy();
        this.pixiBuffer = null;
      }
    }
    // 初始化buffer
    // this.#initImageData(data.length, rowCount, pixelWidth, rowCount);
    // 将 imageData 作为 Transferable 对象传输给 Web Worker
    if (data instanceof Float32Array) {
      this.worker.postMessage({ row: data }, [data.buffer]);
    } else {
      const buffer = new Float32Array(data);
      this.worker.postMessage({ row: buffer }, [buffer.buffer]);
    }
  }

  /**
   * @param {Uint8Array} data
   */
  setImageData(data) {
    const isChange = data.length !== this.rainRGBData.length;
    this.rainRGBData = data;
    console.log("setImageData::", isChange, data.length, this.pixiBuffer);
    if (isChange || this.pixiBuffer === null) {
      this.#resizePixi();
    } else {
      this.pixiBuffer.data = data;
      this.texture.update();
    }
  }

  /**
   *
   * @param {Uint8Array} data
   */
  #drawRows = (data) => {
    this.rainRGBData = data;
    console.log(this.rect, data.length);
    if (this.pixiBuffer === null) {
      this.#resizePixi();
    } else {
      this.pixiBuffer.data = data;
      this.texture.update();
    }
  };

  /**
   * 缩放
   * @param {Number} start
   * @param {Number} end
   */
  zoom(start, end) {
    this.#utils.zoomStart = start;
    this.#utils.zoomEnd = end;
    this.worker.postMessage({
      zoomInfo: {
        zoomStart: start,
        zoomEnd: end,
        zoomLen: end - start + 1,
      },
    });
  }

  /**
   * Y轴范围变更
   * @param {Numer} minimum
   * @param {Numer} maximum
   */
  setAxisYRange(minimum, maximum) {
    if (minimum >= maximum) return;
    this.#initColors();

    const { width, height } = this.#utils.rect;
    this.#utils.ctx.clearRect(0, 0, width, height);

    this.worker.postMessage({
      colors: this.#colors,
      axisYRange: {
        minimum,
        maximum,
      },
    });
  }

  clear() {
    // 清除绘制
    this.rainRGBData = new Uint8Array(this.rect.width * this.rect.height * 3);
    this.#resizePixi();
  }

  dispose() {
    if (this.pixiBuffer) {
      this.pixiBuffer.dispose();
      this.pixiBuffer.destroy();
    }
    this.texture.destroy();
    this.sprite.destroy();
    this.pixi.destroy();
    if (this.worker) {
      window.URL.revokeObjectURL(this.workerBlob);
      this.worker.terminate();
      this.worker = null;
    }
  }
}

export default RainChartGPUWorker;
