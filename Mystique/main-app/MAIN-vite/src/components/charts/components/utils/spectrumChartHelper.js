import { combineData } from "../../lib/chartUtils.js";
import { ChartTypes, SeriesNames, SeriesTypes } from "./enums.js";
import {
  isMobile,
  frequency2String1,
  autoAxisYRange,
  peakSearch,
  findPeaks,
} from "./utils.js";

/**
 * 用于创建线程方法
 */
const workerThread = () => {
  // var prevDataLen;
  /**
   * @type {Uint8ClampedArray|Uint8Array}
   */

  var rainImageData;
  var colors = [];
  // var preDrawMatrix = [];
  // var pauseMatrix;
  // 缓存源数据
  var rowsBuffer = [];
  /**
   * @type {Uint8ClampedArray|Uint8Array}
   */
  //
  var rowDefault;
  var states = {
    zoomStart: -1,
    zoomEnd: 100,
    zoomLen: 101,
    minimumY: -20,
    maximumY: 80,
    // 绘图区宽度 px
    width: 100,
    // 瀑布图绘图区高度 px
    height: 100,
    // 默认黑色
    defaultColor: 0,
  };
  var prevRender = new Date().getTime();
  var prevRowOver = true;
  var colorBit = 3;
  var dataForGPU = false;

  /**
   * 生成渐变色
   * @param {Array} colors
   * @param {Number} outCount
   * @returns
   */
  function gradientColors(colors, outCount) {
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
      const rStep = (r2 - r1) / gap;
      const gStep = (g2 - g1) / gap;
      const bStep = (b2 - b1) / gap;
      for (let l = from; l < to; l += 1) {
        const r = Math.round(r1 + rStep * (l - from));
        const g = Math.round(g1 + gStep * (l - from));
        const b = Math.round(b1 + bStep * (l - from));
        outColors.push({
          R: r,
          G: g,
          B: b,
        });
      }
    }
    return outColors;
  }

  function initColors(colorBlends) {
    const { maximumY, minimumY } = states;
    const colors = {};
    let cIndx = 0;
    try {
      const cs = gradientColors(colorBlends, maximumY - minimumY + 1);
      for (let i = maximumY; i >= minimumY; i -= 1) {
        colors[i] = cs[cIndx] || cs[cs.length - 1];
        cIndx += 1;
      }
    } catch (er) {
      console.log("init colors error", er);
    } finally {
    }
    console.log("================", colors);
    return colors;
  }

  /**
   * 抽取
   * @param {Array} spectrum
   * @param {Number} outMax
   * @param {Float32Array} referData 参考数据和outMax一样长，这样可用避免重复new Array而节省性能
   * @returns
   */
  function combineData(spectrum, outMax, referData) {
    const spectrumLength = spectrum.length; // 缓存数组长度
    if (spectrumLength > outMax) {
      const res = referData ? referData.slice(0) : new Float32Array(outMax); // 预先定义结果数组的长度
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
    const rowData = rowDefault.slice(0);
    const { minimumY, maximumY } = states;
    // 默认给最小值颜色？
    // let c =
    let startX = 0;
    if (dataForGPU) {
      for (let i = 0; i < dataLen; i += 1) {
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
            const cIndex = x * colorBit;
            rowData[cIndex] = c.R;
            rowData[cIndex + 1] = c.G;
            rowData[cIndex + 2] = c.B;
            // MD，只是为了极致的性能，少这一行多了一堆
            // rowData[cIndex + 3] = 255;
          }
          startX = endX;
        }
      }
    } else {
      for (let i = 0; i < dataLen; i += 1) {
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
            const cIndex = x * colorBit;
            rowData[cIndex] = c.R;
            rowData[cIndex + 1] = c.G;
            rowData[cIndex + 2] = c.B;
          }
          startX = endX;
        }
      }
    }

    return rowData;
  };

  rowDataCast = (data, pixelWidth, isOver) => {
    let drawData = data;
    // 缓存数据
    if (isOver) cacheRows(data);
    // 如果外面一开始就不显示rain，则不会初始化高度和宽度进来
    if (rowDefault) {
      if (states.zoomStart > 0 || states.zoomEnd < data.length) {
        // 缩放了
        drawData = data.slice(states.zoomStart, states.zoomEnd);
      }
      if (drawData.length > pixelWidth) {
        // 抽取数据
        drawData = combineData(data, pixelWidth);
      }
      const rowImageData = data2ImageData(drawData, pixelWidth);
      if (prevRowOver) {
        // 插入第一行
        const startIndex = pixelWidth * colorBit;
        rainImageData.copyWithin(
          startIndex,
          0,
          rainImageData.length - startIndex
        );
      }
      rainImageData.set(rowImageData, 0);

      // MD，好别扭的感觉...  发到外面去好查看marker的值，更新Y轴范围时再传进来重绘...
      const copy = drawData.slice(0);
      self.postMessage({ renderRow: copy }, [copy.buffer]);
      return true;
    }
    return false;
  };

  cacheRows = (data) => {
    const cacheRowLen = rowsBuffer.length;
    if (cacheRowLen > 0 && data.length * (cacheRowLen + 1) > 50000) {
      rowsBuffer = rowsBuffer.slice(1);
    }
    rowsBuffer.push(data);
  };

  /**
   * 添加一行数据
   * @param {Array} data
   */
  function addRow(data) {
    let pixelWidth = states.width;
    const dataLen = data.length;
    if (dataLen !== states.zoomLen) {
      states.zoomStart = 0;
      states.zoomEnd = dataLen - 1;
      states.zoomLen = dataLen;
    }
    const isOver = data[dataLen - 1] != -9999;
    if (rowDataCast(data, pixelWidth, isOver)) {
      // 判断是否收完
      prevRowOver = isOver;
      const dt1 = new Date().getTime();
      if (dt1 - prevRender > 40) {
        const copy = rainImageData.slice(0);
        self.postMessage({ imageData: copy }, [copy.buffer]);
        prevRender = dt1;
      }
    }
  }

  function initRenderBuffer() {
    const oneRow = states.width * colorBit;
    const bufferLen = oneRow * states.height;
    if (states.defaultColor !== 0) {
      rainImageData = dataForGPU
        ? new Uint8Array(bufferLen).fill(states.defaultColor)
        : new Uint8ClampedArray(bufferLen).fill(states.defaultColor);
    } else {
      rainImageData = dataForGPU
        ? new Uint8Array(bufferLen)
        : new Uint8ClampedArray(bufferLen);
    }
    rowDefault = rainImageData.slice(0, oneRow);
    if (states.defaultColor != 255 && colorBit === 4) {
      for (let i = 0; i < states.width; i++) {
        rowDefault[i * 4 + 3] = 255;
      }
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
        const startIndex = width * colorBit * index;
        rainImageData.set(rowImageData, startIndex);
      }

      const copy = rainImageData.slice(0);
      self.postMessage({ imageData: copy }, [copy.buffer]);
    }
  }

  addEventListener("message", (e) => {
    const {
      colors: cs,
      row,
      data,
      axisYRange,
      zoomInfo,
      // rect,
      clear,
      enableGPU,
      plotArea,
      defaultColor,
    } = e.data;
    let plotAreaChange = false;
    if (enableGPU) {
      dataForGPU = enableGPU === "true";
      colorBit = enableGPU === "true" ? 3 : 4;
    }
    if (defaultColor) {
      states.defaultColor = defaultColor;
    }
    if (data) {
    } else if (row) {
      addRow(row);
    } else if (cs) {
      states.colorBlends = cs;
      colors = initColors(cs);
    } else if (axisYRange) {
      states = { ...states, ...axisYRange };
      colors = initColors(states.colorBlends || cs);
      console.log(
        "change rain chart aixs y range::",
        axisYRange,
        states.colorBlends
      );
    } else if (zoomInfo) {
      states = { ...states, ...zoomInfo };
      // TODO 从源数据抽点重绘
    }
    //  else if (rect) {
    //   states = { ...states, ...rect };
    //   if (!clear) {
    //     initRenderBuffer();
    //     // TODO 从源数据抽点重绘
    //   }
    // }
    else if (plotArea) {
      const { width, height } = plotArea;
      plotAreaChange =
        (width && width !== states.width) ||
        (height && height !== states.height);
      if (plotAreaChange) {
        states = { ...states, ...plotArea };
        if (!clear) {
          console.log("???????");
          initRenderBuffer();
          // TODO 从源数据抽点重绘
        }
      }
    } else if (clear) {
      // 清除数据&通知外部
      initRenderBuffer();
      // preDrawMatrix = [];
      rowsBuffer = [];
      const copy = rainImageData.slice(0);
      self.postMessage({ imageData: copy }, [copy.buffer]);
    } else {
    }
    if (cs || axisYRange || zoomInfo || plotAreaChange) {
      // 重绘
      reRender();
    }
  });
};

/**
 * @class
 */
class SpectrumChartHelper {
  /**
   * 绘制数据，非源数据
   * @protected
   * @type {Array<Number>|undefined}
   */
  prevSpecData;
  prevPeakInfo;
  // 源数据长度
  prevDataLen;
  preDrawMatrix = [];
  pauseMatrix = [];
  // 瀑布图总行数
  totalFrame = 100;
  // 瀑布图时间戳
  timestamps = [];
  pauseTimestamps;
  plotArea = { width: 100, height: 100 };
  prevFreq = 98;
  prevBandwidth = 200;

  workerBlob;

  /**
   * @type {Worker}
   */
  worker;

  /**
   * @protected
   */
  callbacks = {
    onData: (e) => {},
    onMarkerChange: (e) => {},
    onZoomChange: (e) => {},
    onMarkerSelectChange: (e) => {},
    onThresholdChange: (e) => {},
    onAxisYRange: () => {},
    onCursorChange: () => {},
    onSetSeriesVisible: (e) => {},
  };

  /**
   * @protected
   */
  chartArgs = {};

  /**
   * @protected
   */
  myStates = {
    /**
     * @type {Array<Marker>}
     */
    markers: [null, null, null, null, null],
    /**
     * @type {ZoomInfo}
     */
    zoomInfo: { start: 0, end: 1, startIndex: 0, endIndex: 1, zoomLen: 1 },
    mousePosition: {
      dataXIndex: -1,
      dataYIndex: -1,
      chart: ChartTypes.spectrum,
    },
    maximumY: 80,
    minimumY: -20,
    axisYTickGap: 10,
    threshold: 0,
    prevDataTime: 0,
    pause: undefined,
    hideSeries: [],
    gpuRain: true,
    prevSignals: null,
  };

  #onMarkerChange;

  /**
   * @constructor
   * defaultColor 只能设置纯色，RGB都相同
   * @param {{colorBlends, gpuRain, defaultColor}} options
   */
  constructor({ colorBlends, gpuRain, defaultColor }) {
    this.myStates.gpuRain = gpuRain;
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
      const { renderRow, imageData } = e.data;
      if (imageData) {
        const onDataEvents = this.callbacks[EventTypes.DataChange];
        if (onDataEvents && !this.myStates.pause) {
          onDataEvents.forEach((ev) =>
            ev({
              rain: e.data.imageData,
              currentPct: this.timestamps.length / this.totalFrame,
              rainTimeSpan:
                this.timestamps[this.timestamps.length - 1] -
                this.timestamps[0],
            })
          );
        }
      }
      if (renderRow) {
        // 缓存矩阵
        let isOver = renderRow[renderRow.length - 1] !== -9999;
        const matrixLen = this.pauseMatrix.length;
        if (this.myStates.pause) {
          if (isOver) {
            this.pauseMatrix.push(renderRow);
            if (this.pauseMatrix.length > this.totalFrame) {
              this.pauseMatrix = this.pauseMatrix.slice(1);
            }
          } else {
            this.pauseMatrix[matrixLen - 1] = renderRow;
          }
        } else {
          if (isOver) {
            this.preDrawMatrix.push(renderRow);
            if (this.preDrawMatrix.length > this.totalFrame) {
              this.preDrawMatrix = this.preDrawMatrix.slice(1);
            }
          } else {
            this.pauseMatrix[matrixLen - 1] = renderRow;
          }
        }
      }
    };
    this.worker.onerror = (er) => {
      console.log("worker error:::", er);
    };
    this.worker.postMessage({
      colors: colorBlends || ["#FF0000", "#00FF00", "#0000FF"],
      enableGPU: gpuRain ? "true" : "false",
      defaultColor,
    });
  }

  /**
   *
   * @param {String} type
   * @param {Function} callback
   */
  on(type, callback) {
    const callBacks = this.callbacks[type];
    if (callBacks) {
      callBacks.push(callback);
    } else {
      this.callbacks[type] = [callback];
    }
  }

  // resize(width, height) {
  //   this.worker.postMessage({
  //     rect: { width, height },
  //   });
  //   this.totalFrame = height;
  // }

  /**
   * width 容器宽度，height 瀑布图高度！！！！
   * @param {{width:Number,height:Number}} plotArea
   */
  setPlotArea(plotArea) {
    console.log("set plot area");
    const { width, height } = plotArea;
    if (width) {
      this.plotArea.width = width;
    }
    if (height) {
      this.plotArea.height = height;
    }
    if (this.worker) {
      this.worker.postMessage({
        plotArea,
      });
    }
    this.totalFrame = height;
  }

  validateData(d) {
    const { data, max, avg, min, thr } = d;
    const { startIndex, zoomLen } = this.myStates.zoomInfo;
    // let data1 = data,
    //   max1 = this.myStates.hideSeries.includes(SeriesTypes.max) ? null : max,
    //   avg1 = this.myStates.hideSeries.includes(SeriesTypes.avg) ? null : avg,
    //   min1 = this.myStates.hideSeries.includes(SeriesTypes.min) ? null : min;

    let data1 = data,
      max1 = max,
      avg1 = avg,
      min1 = min,
      thr1 = thr;

    // 数据抽取
    if (zoomLen !== data.length) {
      const sliceEnd = startIndex + zoomLen;
      data1 = data.slice(startIndex, sliceEnd);
      if (max) max1 = max.slice(startIndex, sliceEnd);
      if (avg) avg1 = avg.slice(startIndex, sliceEnd);
      if (min) min1 = min.slice(startIndex, sliceEnd);
      if (thr) thr1 = thr.slice(startIndex, sliceEnd);
    }
    if (zoomLen > this.plotArea.width) {
      data1 = combineData(data1, this.plotArea.width);
      if (max) max1 = combineData(max1, this.plotArea.width);
      if (avg) avg1 = combineData(avg1, this.plotArea.width);
      if (min) min1 = combineData(min1, this.plotArea.width);
      if (thr) thr1 = combineData(thr1, this.plotArea.width);
    }
    return { data: data1, max: max1, avg: avg1, min: min1, thr: thr1 };
  }

  /**
   *
   * @param {*} d
   */
  setData(d) {
    const { data, timestamp } = d;

    // const { frequency, bandwidth } = this.chartArgs;
    const mainParam =
      d.frequency !== this.prevFreq || d.bandwidth !== this.prevBandwidth;
    if (!this.prevSpecData || data.length !== this.prevDataLen || mainParam) {
      // 重置缩放
      this.myStates.zoomInfo = {
        start: 0,
        end: 1,
        startIndex: 0,
        endIndex: data.length - 1,
        zoomLen: data.length,
      };
      this.worker.postMessage({
        zoomInfo: {
          zoomStart: 0,
          zoomEnd: data.length - 1,
          zoomLen: data.length,
        },
        clear: "clear",
      });
      this.prevDataLen = data.length;
      this.timestamps = [];

      this.prevFreq = d.frequency;
      this.prevBandwidth = d.bandwidth;
    }
    // const { startIndex, zoomLen } = this.myStates.zoomInfo;
    // let data1 = data,
    //   max1 = max,
    //   avg1 = avg,
    //   min1 = min;
    // if (zoomLen > this.plotArea.width) {
    //   // 数据抽取
    //   if (zoomLen !== data.length) {
    //     data1 = data.slice(startIndex, zoomLen);
    //     if (max) max1 = max.slice(startIndex, zoomLen);
    //     if (avg) avg1 = avg.slice(startIndex, zoomLen);
    //     if (min) min1 = min.slice(startIndex, zoomLen);
    //   }
    //   data1 = combineData(data1, this.plotArea.width);
    //   if (max) max1 = combineData(max1, this.plotArea.width);
    //   if (avg) avg1 = combineData(avg1, this.plotArea.width);
    //   if (min) min1 = combineData(min1, this.plotArea.width);
    // }

    this.timestamps.push(timestamp / 1e5);
    if (this.timestamps.length > this.totalFrame)
      this.timestamps = this.timestamps.slice(1);

    const dataForRain = data.slice(0);
    if (dataForRain instanceof Float32Array) {
      this.worker.postMessage({ row: dataForRain }, [dataForRain.buffer]);
    } else {
      const buffer = new Float32Array(dataForRain);
      this.worker.postMessage({ row: buffer }, [buffer.buffer]);
    }
    const renderData = this.validateData(d);
    // renderData.data = data1;
    // renderData.max = max1;
    // renderData.avg = avg1;
    // renderData.min = min1;
    if (this.myStates.pause) {
      this.myStates.pause = { specData: renderData };
    } else {
      this.prevSpecData = renderData;
      const onDataEvents = this.callbacks[EventTypes.DataChange];
      if (onDataEvents) {
        onDataEvents.forEach((e) => e({ spectrum: renderData }));
      }
      // 更新marker位置
      const dt = new Date().getTime();
      if (dt - this.myStates.prevDataTime > 99) {
        if (this.chartArgs.showCursor) this.moveCursor();
        // this.updateMarkerOnData();
        if (this.hasMarker()) {
          this.updateMarkerInfo();
          this.fireMarkerChange();
        }
        this.myStates.prevDataTime = dt;
      }
    }
  }

  /**
   * 更新marker水平位置
   * 当参数变更+拖动+设置中心频率 时使用
   */
  updateMarkerDataXIndex() {}

  updateChartArgs = (args) => {
    this.chartArgs = { ...this.chartArgs, ...args };
    if (args.visibleCharts) {
      // 1. 更新门限位置
      // this.setThreshold(this.myStates.threshold);
      // 2. 遍历marker
      // this.checkMarkerVisible();
      this.myStates.markers.forEach((mk) => {
        if (mk) {
          mk.visible = this.chartArgs.visibleCharts.includes(mk.anchorChart);
        }
      });
      this.updateMarkerInfo();
      this.fireMarkerChange();
    }
  };

  /**
   * start 缩放起始位置 px
   * end 缩放结束位置 px
   * width 容器宽度
   * @param {{start:Number,end:Number,width:Number}} args
   */
  updateZoom = (args) => {
    const { start, end, width } = args;
    if (start < 0) {
      // 缩小
      this.myStates.zoomInfo = {
        start: 0,
        end: 1,
        startIndex: 0,
        endIndex: this.prevDataLen - 1,
        zoomLen: this.prevDataLen,
      };
    } else {
      console.log("zoom out");
      // 放大
      const { zoomInfo } = this.myStates;
      if (zoomInfo.zoomLen <= 20) return;
      const dataLen = zoomInfo.zoomLen;
      const zoomStartPct = start / width;
      const zoomEndPct = end / width;
      let startIndex = Math.floor(dataLen * zoomStartPct) + zoomInfo.startIndex;
      let endIndex = Math.round(dataLen * zoomEndPct) - 1 + zoomInfo.startIndex;
      let zoomLen = endIndex - startIndex + 1;
      if (zoomLen < 20) {
        zoomLen = 20;
        endIndex = startIndex + 19;
        if (endIndex >= this.prevDataLen - 1) {
          endIndex = this.prevDataLen - 1;
          startIndex = endIndex - 19;
        }
      }
      this.myStates.zoomInfo = {
        start: zoomStartPct,
        end: zoomEndPct,
        startIndex,
        endIndex,
        zoomLen,
      };
    }

    this.worker.postMessage({
      zoomInfo: {
        zoomStart: this.myStates.zoomInfo.startIndex,
        zoomEnd: this.myStates.zoomInfo.endIndex,
        zoomLen: this.myStates.zoomInfo.zoomLen,
      },
    });

    const zoomChangeEvents = this.callbacks[EventTypes.ZoomChange];
    if (zoomChangeEvents) {
      zoomChangeEvents.forEach((e) => e(this.myStates.zoomInfo));
    }
    // TODO 重绘线图
    // TODO 更新marker显示位置
  };

  /**
   *
   * @param {Number} thr
   */
  setThreshold = (thr) => {
    this.myStates.threshold = thr;

    const rangeGap = this.myStates.maximumY - this.myStates.minimumY;
    let pct = (this.myStates.maximumY - thr) / rangeGap;
    const { visibleCharts } = this.chartArgs;
    console.log("setThreshold::", visibleCharts);
    if (visibleCharts.length > 1) {
      pct = pct / 2;
    }
    // const { chartBounding } = this.chartArgs;
    // if (chartBounding) {
    //   const specBounding = chartBounding[ChartTypes.spectrum];
    //   if (specBounding) {
    const thrChangeEvents = this.callbacks[EventTypes.ThresholdChange];
    if (thrChangeEvents)
      thrChangeEvents.forEach((e) => e({ threshold: thr, top: pct * 100 }));
    //   }
    // }
  };

  #fireAxisYRangeChange() {
    const { minimumY, maximumY, axisYTickGap } = this.myStates;
    const axisYRangeEvents = this.callbacks[EventTypes.AxisYRangeChange];
    if (axisYRangeEvents) {
      axisYRangeEvents.forEach((e) => e(minimumY, maximumY, axisYTickGap));
    }
    this.worker.postMessage({
      axisYRange: {
        minimumY,
        maximumY,
      },
    });
  }

  /**
   * 设置Y轴范围
   * 50  100  150  200
   * @param {Number} min
   * @param {Number} max
   * @param {Boolean} validate 是否校验
   */
  setAxisYRange = (min, max, validate = true) => {
    let minY = min;
    let gap = max - min;
    if (validate) {
      if (gap > 150) gap = 200;
      else if (gap > 100) gap = 150;
      else if (gap > 50) gap = 100;
      else gap = 50;
      minY = min - (min % 5);
    }
    this.myStates.minimumY = minY;
    this.myStates.maximumY = minY + gap;
    this.myStates.axisYTickGap = gap / 10;
    this.#fireAxisYRangeChange();
    // const axisYRangeEvents = this.callbacks[EventTypes.AxisYRangeChange];
    // if (axisYRangeEvents) {
    //   axisYRangeEvents.forEach((e) =>
    //     e(
    //       this.myStates.minimumY,
    //       this.myStates.maximumY,
    //       this.myStates.axisYTickGap
    //     )
    //   );
    // }
    // this.worker.postMessage({
    //   axisYRange: {
    //     minimumY: this.myStates.minimumY,
    //     maximumY: this.myStates.maximumY,
    //   },
    // });
  };

  setAxisYTickGap(tickGap) {
    this.myStates.maximumY = this.myStates.minimumY + tickGap * 10;
    this.myStates.axisYTickGap = tickGap;
    this.#fireAxisYRangeChange();
  }

  autoAxisYRange = () => {
    if (this.prevSpecData) {
      const range = autoAxisYRange(this.prevSpecData.data);
      this.setAxisYRange(range.minimum, range.maximum);
    }
  };

  pauseRender(pause) {
    if (pause) {
      this.myStates.pause = {};
    } else {
      this.prevSpecData = this.myStates.pause.specData;
      this.myStates.pause = undefined;
      if (this.pauseMatrix) {
        const arr = this.preDrawMatrix.concat(this.pauseMatrix);
        const offset = arr.length - this.totalFrame;
        this.preDrawMatrix = arr.slice(offset);
      }
    }
  }

  /**
   * @protected
   * x 水平方向位置%
   * y 垂直方向位置%
   * px 水平方向位置 px
   * py 垂直方向位置 py
   * @param {{x:Number,y:Number,px:Number,py:Number,visibleCharts:Array<String>}} mouseArgs
   */
  updateMouseDataIndex = (args) => {
    const mouseInChart = this.getMouseInChart(args);
    const dataPosition = this.getRenderDataPosition(args, mouseInChart);

    if (dataPosition) {
      dataPosition.chart = mouseInChart;
      this.myStates.mousePosition = dataPosition;
    }
  };

  /**
   * @protected
   * 根据数据索引移动游标位置
   * @returns
   */
  moveCursor = () => {
    const { dataXIndex, dataYIndex, chart, x, y, yTick } =
      this.myStates.mousePosition;
    let dataInfo;
    let pos;
    let cursorFreq;
    if (dataXIndex > -1) {
      dataInfo = this.#getCursorCaption(chart, dataXIndex, dataYIndex);
      pos = this.getPositionByRenderDataIndex(
        dataXIndex,
        dataYIndex,
        chart,
        false
      );
    }
    cursorFreq =
      dataXIndex > -1
        ? this.getFrequencyByIndex(dataXIndex)
        : this.getFrequencyOnNoData(x);

    const cursorChangeEvents = this.callbacks[EventTypes.CursorChange];
    if (cursorChangeEvents) {
      cursorChangeEvents.forEach((e) =>
        e({
          cursorPosX: pos ? pos.x : x * 100,
          cursorPosY: pos && pos.y > -1 ? pos.y : y * 100,
          dataInfo,
          frequency: cursorFreq,
          tickValue: yTick !== undefined ? String(yTick) : "",
        })
      );
    }
    // return ;
  };

  getPositionByRenderDataIndex = (
    dataXIndex,
    dataYIndex,
    anchorChart,
    followData = true
  ) => {
    const { chartBounding } = this.chartArgs;
    if (chartBounding) {
      // const { zoomInfo } = this.myStates;
      const renderDataLen = this.prevSpecData.data.length;
      const bounding = chartBounding[anchorChart];
      const pctRange = bounding.y2 - bounding.y1;

      let y = -1;
      if (dataYIndex > -1) {
        // 游标在瀑布图上
        // warning 如果行数太少，可能不是在中间，需要根据每行高度在计算一次，后面再说
        y = ((dataYIndex + 0.5) / this.totalFrame) * pctRange + bounding.y1;
      } else {
        // 20230728 不跟着数据走
        if (followData) {
          const { maximumY, minimumY } = this.myStates;
          const axisYRange = maximumY - minimumY;
          const data = this.prevSpecData.data[dataXIndex];
          const dataOffset = (maximumY - data) / axisYRange;
          y = dataOffset * pctRange + bounding.y1;
          if (y < 0) y = 0;
          if (y > 1) y = 1;
        }
      }

      const xPctGap = 100 / renderDataLen;
      const x = (dataXIndex + 0.5) * xPctGap;
      return { x, y: y > -1 ? y * 100 : y };
    }
    return { x: -1, y: -1 };
  };

  /**
   *
   * @param {{id:String}} options
   */
  addMarker = (options) => {
    if (!this.prevSpecData) {
      console.log("Can not find spectrum data!!");
      return;
    }
    let id = new Date().getTime() + Math.random().toString(6);
    const { visibleCharts } = this.chartArgs;
    let { dataXIndex, dataYIndex, chart } = this.myStates.mousePosition;

    if (options) {
      id = options.id;
      // freq = this.chartArgs.frequency;
      dataXIndex = Math.floor(this.prevSpecData.data.length / 2);
      chart = visibleCharts[0];
      if (chart === ChartTypes.rain) {
        dataYIndex = 0;
      }
    }
    const freq = this.getFrequencyByIndex(dataXIndex);
    let markerIndex = -1;
    // 清除选中 & 查找可用的位置
    this.myStates.markers.forEach((mk, indx) => {
      if (mk) {
        mk.selected = false;
      } else if (markerIndex < 0) {
        markerIndex = indx;
      }
    });

    let mk = {
      id,
      markerIndex,
      dataYIndex,
      freqMHz: freq,
      selected: true,
      visible: true,
      anchorChart: chart,
    };
    this.myStates.markers[markerIndex] = mk;
    this.updateMarkerInfo(id);
    this.fireMarkerChange();
    const markerSelectEvents = this.callbacks[EventTypes.MarkerSelectChange];

    if (markerSelectEvents) {
      markerSelectEvents.forEach((e) => e(mk));
    }
  };

  /**
   * @protected
   * 鼠标拖动marker
   * @param {*} args
   */
  dragMarker = (args) => {
    const { id, dragX, dragY } = args;
    const mk = this.myStates.markers.find((m) => m && m.id === id);
    // 非瀑布图拖动Y 直接返回
    if (isMobile() && dragY && mk.anchorChart !== ChartTypes.rain) return;
    let mouseInChart = mk.anchorChart;
    if (dragY) {
      mouseInChart = this.getMouseInChart(args);
      mk.anchorChart = mouseInChart;
    }
    const { dataXIndex, dataYIndex } = this.getRenderDataPosition(
      args,
      mouseInChart
    );
    if (dragY) {
      mk.rowIndex = dataYIndex;
    }
    if (dragX) {
      mk.freqMHz = this.getFrequencyByIndex(dataXIndex);
    }
    this.updateMarkerInfo(id);
    this.fireMarkerChange();
  };

  /**
   * 上下 左右 移动marker
   * @param {*} param0
   * @returns
   */
  moveMarker({ action, id }) {
    const mk = this.myStates.markers.find((m) => m.id === id);
    let { rowIndex, freqMHz, anchorChart } = mk;
    const { visibleCharts } = this.chartArgs;

    const chartIndex = visibleCharts.indexOf(anchorChart);
    switch (action) {
      case "up":
        {
          if (anchorChart === ChartTypes.rain) {
            rowIndex -= 1;
          } else if (chartIndex > 0) {
            // 上移一个图表
            anchorChart = visibleCharts[chartIndex - 1];
            if (anchorChart === ChartTypes.rain) {
              rowIndex = 0;
            } else {
              rowIndex = -1;
            }
          } else {
            return;
          }
        }
        break;
      case "down":
        {
          if (anchorChart === ChartTypes.rain) {
            rowIndex += 1;
          } else if (chartIndex < visibleCharts.length - 1) {
            // 下移一个图表
            anchorChart = visibleCharts[chartIndex + 1];
            if (anchorChart === ChartTypes.rain) {
              rowIndex = 0;
            } else {
              rowIndex = -1;
            }
          } else {
            return;
          }
        }
        break;
      default:
        // 横向移动，每次移动一个渲染点位，更新频率
        let { renderDataXIndex } = this.getRenderXIndexByFrequency(mk.freqMHz);
        if (action === "left") {
          renderDataXIndex -= 1;
          if (renderDataXIndex < 0) return;
        } else {
          renderDataXIndex += 1;
          if (renderDataXIndex > this.prevSpecData.data.length - 1) return;
        }
        freqMHz = this.getFrequencyByIndex(renderDataXIndex);
        break;
    }
    mk.rowIndex = rowIndex;
    mk.anchorChart = anchorChart;
    mk.freqMHz = freqMHz;
    console.log("move marker");
    this.updateMarkerInfo(id);
    this.fireMarkerChange();
  }

  /**
   *
   * @param {{id:String,select:boolean}} options
   */
  selectMarker(options) {
    const { id, select } = options;
    const marker = this.myStates.markers.find((mk) => mk && mk.id === id);
    // 选中marker
    this.myStates.markers.forEach((mk) => {
      if (mk) {
        mk.selected = mk.id === id ? select : false;
      }
    });
    if (marker) {
      console.log("sel marker:::", this.myStates.markers);
      // marker.selected = select;
      this.fireMarkerChange();
      const onMarkerSelectChange =
        this.callbacks[EventTypes.MarkerSelectChange];
      if (onMarkerSelectChange) {
        onMarkerSelectChange.forEach((e) => e(select ? marker : undefined));
      }
    } else {
      // 不存在就添加
      this.addMarker(options);
    }
  }

  /**
   * 监听marker变化，主要用于组件外部监听
   * 本来应该由组件通过props回调出去，但是这样性能太低
   * 传入id则监听指定的marker，否则就是所有
   * 监听指定有必要？
   * @param {Function<{}>} onChange
   * @param {Array<String>} ids
   */
  watchMarker(onChange, ids) {
    this.#onMarkerChange = onChange;
  }

  /**
   * marker更新统一入口
   * 其它地方更新：freqMHz、keepPeak、rowIndex、anchorChart
   * warning 这样做感觉上会造成性能损耗，但是逻辑简单
   * @param {String} id 更新特定的marker
   */
  updateMarkerInfo(id = null, peak = false, peak2 = false) {
    // 1. 更新峰值保持marker 的freq
    // 2. 判断是否需要被移除
    // 3. 判断是否需要被隐藏
    // 4. 更新数据
    // 4.1 计算当前频率在当前zoom范围内的比例
    // 4.2 根据4.1获得mk所在显示数据的索引  这么做的目的是为了点少的时候能够落在中间
    // 4.3 计算显示位置
    // 4.4 计算移动端滑块位置
    // 4.5 获取数据信息
    const { markers, zoomInfo } = this.myStates;
    for (let i = 0; i < markers.length; i += 1) {
      const mk = markers[i];
      if (mk && (!id || mk.id === id)) {
        // 1. 更新峰值保持marker 的freq
        if ((mk.keepPeak || peak || peak2) && this.prevSpecData) {
          const peakInfo = findPeaks(this.prevSpecData.data);
          if (mk.keepPeak || peak) {
            mk.freqMHz = this.getFrequencyByIndex(peakInfo.peakIndex);
          } else {
            mk.freqMHz = this.getFrequencyByIndex(peakInfo.peak2Index);
          }
        } else {
          // 2. 判断marker是否需要被移除
          const xIndexInfo = this.getRenderXIndexByFrequency(mk.freqMHz);
          if (xIndexInfo) {
            const { freqScale, renderDataXIndex } = xIndexInfo;
            const { visibleCharts } = this.chartArgs;
            // 3. 判断是否需要被隐藏
            mk.visible =
              freqScale >= zoomInfo.start &&
              freqScale <= zoomInfo.end &&
              visibleCharts.includes(mk.anchorChart);

            // 4. 更新数据
            // 4.1 计算当前频率在当前zoom范围内的比例
            // 4.2 根据4.1获得mk所在显示数据的索引

            // 4.3 计算显示位置
            mk.position = this.getPositionByRenderDataIndex(
              renderDataXIndex,
              mk.rowIndex,
              mk.anchorChart
            );

            if (isMobile()) {
              const { chartBounding } = this.chartArgs;
              let sliderPosition = undefined;
              // 计算移动端滑块的位置
              if (mk.anchorChart === ChartTypes.rain) {
                sliderPosition = mk.position;
              } else {
                const bounding = chartBounding[mk.anchorChart];
                sliderPosition = {
                  x: mk.position.x,
                  y: (bounding.y1 + (bounding.y2 - bounding.y1) / 2) * 100,
                };
              }
              mk.sliderPosition = sliderPosition;
            }

            const dataInfo = this.#getCursorCaption(
              mk.anchorChart,
              renderDataXIndex,
              mk.rowIndex
            );
            mk.dataInfo = dataInfo;
            // marker.freqMHz = freq;
            mk.freqInfo = frequency2String1(mk.freqMHz);
          } else {
            this.myStates.markers[i] = null;
          }
        }
      }
    }
  }

  /**
   *
   * @param {{id:String,frequency:Number}} param0
   */
  setMarkerFrequency({ id, frequency }) {
    if (id && frequency && this.prevSpecData) {
      const marker = this.myStates.markers.find((mk) => mk && mk.id === id);
      marker.freqMHz = frequency;
      if (marker) {
        this.updateMarkerInfo();
        this.fireMarkerChange();
        return true;
      }
    }
    return false;
  }

  peakMarker({ id, keepPeak, peak2 }) {
    const marker = this.myStates.markers.find((mk) => mk && mk.id === id);
    if (marker && marker.anchorChart === ChartTypes.spectrum) {
      marker.keepPeak = keepPeak;
      this.updateMarkerInfo(id, !keepPeak && !peak2, peak2);
      this.fireMarkerChange();
    }
  }

  /**
   * @protected
   * 根据鼠标位置获取数据索引
   * @param {*} args
   * @param {*} mouseInChart
   * @returns
   */
  getRenderDataPosition = (args, mouseInChart) => {
    const { x, y } = args;
    // const { zoomLen, startIndex } = this.myStates.zoomInfo;
    let dataXIndex = -1;
    let dataYIndex = -1;
    let yTick = -9999;
    if (this.prevSpecData) {
      const renderDataLen = this.prevSpecData.data.length;
      const { chartBounding } = this.chartArgs;
      if (chartBounding) {
        dataXIndex = Math.round((renderDataLen - 1) * x);
        const cb = chartBounding[mouseInChart];
        const yPct = (y - cb.y1) / (cb.y2 - cb.y1);
        if (mouseInChart === ChartTypes.rain) {
          //  计算y 索引
          dataYIndex = Math.floor(this.totalFrame * yPct);
        } else {
          // 计算Y轴刻度值
          let axisYRange = 360;
          yTick = Math.round((360 - 360 * yPct) * 10) / 10;
          if (mouseInChart === ChartTypes.spectrum) {
            const { minimumY, maximumY } = this.myStates;
            axisYRange = maximumY - minimumY;
            yTick = Math.round((maximumY - axisYRange * yPct) * 10) / 10;
          }
        }
      }
    }
    return { dataXIndex, dataYIndex, x, y, yTick };
  };

  getMouseInChart = (args) => {
    const { y } = args;
    const { visibleCharts, chartBounding } = this.chartArgs;
    let mouseInChart = ChartTypes.spectrum;
    visibleCharts.forEach((vc, index) => {
      const cb = chartBounding[vc];
      if (y >= cb.y1 && y <= cb.y2) {
        mouseInChart = vc;
      }
    });
    // console.log(mouseInChart);
    return mouseInChart;
  };

  hasMarker = () => {
    return this.myStates.markers.findIndex((mk) => mk != null) > -1;
  };

  getRenderXIndexByFrequency(frequency) {
    const halfBand = this.chartArgs.bandwidth / 2000;
    const startFreq = this.chartArgs.frequency - halfBand;
    const stopFreq = this.chartArgs.frequency + halfBand;
    if (frequency < startFreq && frequency > stopFreq) {
      return null;
    }
    const { zoomInfo } = this.myStates;
    const freqScale =
      ((frequency - startFreq) * 1000) / this.chartArgs.bandwidth;

    const renderDataLen = this.prevSpecData.data.length;
    const scaleOfRender =
      (freqScale - zoomInfo.start) / (zoomInfo.end - zoomInfo.start);
    // 4.2 根据4.1获得mk所在显示数据的索引
    let renderDataXIndex = Math.round(renderDataLen * scaleOfRender);

    return { freqScale, renderDataXIndex };
  }

  /**
   * Marker 或 Cursor 根据所在位置获取freq
   * @param {*} dataXIndex  渲染数据的索引，非源数据
   * @returns
   */
  getFrequencyByIndex(dataXIndex) {
    const { frequency, bandwidth } = this.chartArgs;
    const { zoomInfo } = this.myStates;
    const halfBand = bandwidth / 2000;
    const startFreq = frequency - halfBand;
    const renderDataLen = this.prevSpecData.data.length;
    const scale =
      (dataXIndex / renderDataLen) * (zoomInfo.end - zoomInfo.start) +
      zoomInfo.start;

    const freq = (scale * bandwidth) / 1000 + startFreq;
    return freq;
  }

  getFrequencyOnNoData(x) {
    const { frequency, bandwidth } = this.chartArgs;
    const halfBand = bandwidth / 2000;
    const startFreq = frequency - halfBand;
    const freq = (x * bandwidth) / 1000 + startFreq;
    return freq;
  }

  /**
   * @protected
   * 触发marker变更通知事件
   */
  fireMarkerChange() {
    const markerChangeEvents = this.callbacks[EventTypes.MarkerChange];
    if (markerChangeEvents) {
      markerChangeEvents.forEach((e) => e(this.myStates.markers.slice(0)));
    }
    if (this.#onMarkerChange) {
      this.#onMarkerChange(this.myStates.markers.filter((mk) => mk !== null));
    }
  }

  /**
   *
   * @param {Array<String>} ids
   */
  setSeriesVisible(ids) {
    const allSeries = Object.values(SeriesTypes);
    this.myStates.hideSeries = allSeries.filter((s) => !ids.includes(s));
    const onDataEvents = this.callbacks[EventTypes.DataChange];
    if (onDataEvents) {
      onDataEvents.forEach((e) => e({ spectrum: this.prevSpecData }));
    }
    // if (!visible) {
    //   ids.forEach((id) => {
    //     if (!this.myStates.hideSeries.includes(id)) {
    //       this.myStates.hideSeries.push(id);
    //     }
    //   });
    // } else {
    //   this.myStates.hideSeries = this.myStates.hideSeries.filter(
    //     (i) => !ids.includes(i)
    //   );
    // }
    const setSeriesVisible = this.callbacks[EventTypes.SetSeriesVisible];
    if (setSeriesVisible) {
      setSeriesVisible.forEach((e) => e(ids));
    }
  }

  #getCursorCaption(chart, dataXIndex, dataYIndex) {
    let levelCaption = "";
    // if (dataXIndex < 0 && dataYIndex < 0) return { levelCaption };
    let level = {};
    let unit = "dBμV";
    if (chart === ChartTypes.spectrum) {
      level.real = this.prevSpecData.data[dataXIndex];
      levelCaption = `${SeriesNames.real} ${Number(level.real).toFixed(1)}`;
      if (
        !this.myStates.hideSeries.includes(SeriesTypes.max) &&
        this.prevSpecData.max
      ) {
        level.max = this.prevSpecData.max[dataXIndex];
        levelCaption = `${levelCaption} ${SeriesNames.max} ${Number(
          level.max
        ).toFixed(1)}`;
      }
      if (
        !this.myStates.hideSeries.includes(SeriesTypes.avg) &&
        this.prevSpecData.avg
      ) {
        level.avg = this.prevSpecData.avg[dataXIndex];
        levelCaption = `${levelCaption} ${SeriesNames.avg} ${Number(
          level.avg
        ).toFixed(1)}`;
      }

      if (
        !this.myStates.hideSeries.includes(SeriesTypes.min) &&
        this.prevSpecData.min
      ) {
        level.min = this.prevSpecData.min[dataXIndex];
        levelCaption = `${levelCaption} ${SeriesNames.min} ${Number(
          level.min
        ).toFixed(1)}`;
      }
      if (
        !this.myStates.hideSeries.includes(SeriesTypes.thr) &&
        this.prevSpecData.thr
      ) {
        level.thr = this.prevSpecData.thr[dataXIndex];
        levelCaption = `${levelCaption} ${SeriesNames.thr} ${Number(
          level.thr
        ).toFixed(1)}`;
      }
    }

    let timestamp = 0;
    if (chart === ChartTypes.rain) {
      // 有点别扭...瀑布图缓存的是当前绘制矩阵，而非源数据，所以不能使用dataXIndex来获取值
      // const rx = Math.floor(this.preDrawMatrix[0].length * x);
      // 游标在瀑布图上
      // warning 如果行数太少，可能不是在中间，需要根据每行高度在计算一次，后面再说
      const rowIndex = this.preDrawMatrix.length - dataYIndex - 1;
      levelCaption = "--";
      level.real = -9999;
      timestamp = 0;
      if (rowIndex > -1) {
        level.real = this.preDrawMatrix[rowIndex][dataXIndex];
        if (level.real > -9999) {
          levelCaption = `${Number(level.real).toFixed(1)}`;
          timestamp = this.timestamps[this.timestamps.length - dataYIndex - 1];
        }
      }
    }
    if (chart === ChartTypes.wbdf) {
      level.real = this.prevScanDFData.data[dataXIndex];
      levelCaption = `示向度 ${Number(level.real).toFixed(1)}`;
      unit = "°";
    }
    return { level, levelCaption, timestamp, unit };
  }

  clear() {
    // 要不要干这个事情？
  }

  dispose() {
    if (this.worker) {
      window.URL.revokeObjectURL(this.workerBlob);
      this.worker.terminate();
      this.worker = null;
      this.workerBlob = null;
    }
  }
}

export default SpectrumChartHelper;

export const EventTypes = {
  DataChange: "DataChange",
  MarkerChange: "MarkerChange",
  MarkerSelectChange: "MarkerSelectChange",
  ZoomChange: "ZoomChange",
  ThresholdChange: "ThresholdChange",
  AxisYRangeChange: "AxisYRangeChange",
  CursorChange: "CursorChange",
  SetSeriesVisible: "SetSeriesVisible",
};
