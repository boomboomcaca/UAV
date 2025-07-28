import chartUtils, { combineData, gradientColors } from "./chartUtils";

/**
 * 用于创建线程方法
 */
const workerThread = () => {
  var prevDataLen;
  /**
   * @type {Uint8ClampedArray}
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
    const startIndex = pixelWidth * 4;
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
        const startIndex = width * 4 * index;
        rainImageData.set(rowImageData, startIndex);
      }

      const copy = rainImageData.slice(0);
      self.postMessage({ imageData: copy }, [copy.buffer]);
    }
  }

  addEventListener("message", (e) => {
    const { imageData, colors: cs, row, axisYRange, zoomInfo, rect } = e.data;

    if (imageData) {
      rainImageData = imageData;
    } else if (row) {
      addRow(row);
    } else if (cs) {
      colors = cs;
    } else if (axisYRange) {
      states = { ...states, ...axisYRange };
    } else if (zoomInfo) {
      states = { ...states, ...zoomInfo };
    } else if (rect) {
      states = { ...states, ...rect };
      rowDefault = new Uint8ClampedArray(rect.width * 4);
    } else {
    }
    if (cs || axisYRange || zoomInfo || rect) {
      // 重绘
      reRender();
    }
  });
};

/**
 * 雨点图/瀑布图
 */
class RainChartWorker {
  // 记录上一次单行数据长度
  #prevDataLen = 1;
  // 上一次像素高度
  #prevPixelHeight = 1;
  // 上一次像素宽度
  #prevPixelWidth = 1;
  // 记录历史源数据（矩阵），主要是用于重绘
  #prevSourceMatrix;
  // 瀑布图色带颜色，渐变后
  #colors = [];
  // 瀑布图数据缓存
  #rainImageData = [];
  // 辅助
  #utils;
  // 上一次行数
  #prevRowCount = 1;
  // 是否所有数据都已经收起
  #preIsOver = true;

  workerBlob;

  /**
   * @type {Worker}
   */
  worker;

  /**
   * 构造函数
   * @param {HTMLElement} container
   * @param {Array<string>} pallete 色带颜色，从大到小
   * @param {{onSizeChange:Function<Number,Number>, zoomInside:boolean}} optionMore
   */
  constructor(container, pallete, optionMore) {
    // super(container, optionMore);

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
    };
    this.worker.onerror = (er) => {
      console.log("worker error:::", er);
    };
    const options = optionMore || {};
    this.#utils = new chartUtils(container, {
      zoomInside: options.zoomInside || true,
      onSizeChange: (w, h) => {
        if (options.onSizeChange) options.onSizeChange(w, h);
        console.log("size change:::", w, h);
        this.worker.postMessage({
          rect: {
            width: w,
            height: h,
          },
        });
      },
    });
    this.#utils.colorBlends = pallete || ["#FF0000", "#00FF00", "#0000FF"];
    // this.#colors = gradientColors(colorBlends, 100);
    this.#initColors();
    this.worker.postMessage({ colors: this.#colors });
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
      // 将 imageData 作为 Transferable 对象传输给 Web Worker
      // worker.postMessage({ imageData: imageData }, [imageData.data.buffer]);
      this.worker.postMessage({ imageData: rainImgData.data });
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
   * @param {Array|Float32Array} data
   * @param {Number} timestamp
   */
  addRow(data) {
    let rowCount = this.#utils.rect.height;
    let pixelWidth = this.#utils.rect.width;
    // 初始化buffer
    this.#initImageData(data.length, rowCount, pixelWidth, rowCount);
    // 将 imageData 作为 Transferable 对象传输给 Web Worker
    if (data instanceof Float32Array) {
      this.worker.postMessage({ row: data }, [data.buffer]);
    } else {
      const buffer = new Float32Array(data);
      this.worker.postMessage({ row: buffer }, [buffer.buffer]);
    }
  }

  /**
   *
   * @param {Uint8ClampedArray} data
   */
  #drawRows = (data) => {
    let rowCount = this.#utils.rect.height;
    let pixelWidth = this.#utils.rect.width;
    const newImageData = new ImageData(data, pixelWidth, rowCount);
    this.#utils.ctx.putImageData(newImageData, 0, 0);
  };

  setFixedMatrix(matrix) {
    const { width, height } = this.#utils.rect;
    const rowCount = matrix.length;
    const colCount = matrix[0].length;
    // 初始化buffer
    this.#initImageData(colCount, rowCount, width, height);
    let drawMatrix = matrix;
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
    this.#drawMatrix(drawMatrix);
  }

  /**
   *
   * @param {Array<Array>} matrix 倒序二维矩阵
   */
  #drawMatrix = (matrix) => {
    const rowCount = matrix.length;
    const validRow = matrix[0];
    const colCount = validRow.length;
    const { width, height } = this.#utils.rect;

    const { minimumY, maximumY } = this.#utils;
    const perWidth = width / colCount;
    const perHeight = height / rowCount;
    let startY = 0;
    let row = 0;
    for (let y = rowCount - 1; y >= 0; y -= 1) {
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
    this.#utils.minimumY = minimum;
    this.#utils.maximumY = maximum;
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
    this.#utils.ctx.clearRect(
      0,
      0,
      this.#utils.rect.width,
      this.#utils.rect.height
    );
    // 清除数据缓存
    this.#prevDataLen = 0;
    // this.#rowsBuffer = [];
    this.#prevSourceMatrix = undefined;
    // this.#preDrawRows = [];
  }

  // resize() {
  //   this.#utils.resize();
  // }

  dispose() {
    this.#utils.dispose();
  }
}

export default RainChartWorker;
