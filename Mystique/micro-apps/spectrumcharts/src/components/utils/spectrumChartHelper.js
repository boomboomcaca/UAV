import { combineData } from "../../lib/chartUtils.js";
import { ChartTypes, SeriesNames, SeriesTypes, EventTypes } from "./enums.js";
import {
  isMobile,
  frequency2String1,
  autoAxisYRange,
  peakSearch,
  findPeaks,
} from "./utils.js";
import { getChartConfig, saveChartConfig } from "../assets/colors.js";

/**
 * 用于创建线程方法
 */
const workerThread = () => {
  /**
   *
   * @param {{segments:Array<{pointCount:Number,startIndex:Number}>,totalPoint:Number,outRain:boolean,invalidValue:Number,noStatics:boolean}} props
   */
  function ScanDataHelper(props) {
    const { segments, totalPoint, outRain, invalidValue, noStatics } = props;
    this.totalPoint = totalPoint;
    this.segments = segments;
    this.invalidValue = invalidValue !== undefined ? invalidValue : -9999;
    // this.#lastSegmentDataLen = segments[segments.length - 1].pointCount;
    this.defaultLine = new Float32Array(this.totalPoint).fill(-9999);
    this.realData = this.defaultLine.slice(0);
    if (!noStatics) {
      this.maxData = this.defaultLine.slice(0);
      this.avgData = this.defaultLine.slice(0);
      this.minData = new Float32Array(this.totalPoint).fill(9999);
      this.thrData = this.defaultLine.slice(0);
      this.sumData = new Float32Array(this.totalPoint).fill(0);
      this.times = new Uint32Array(this.totalPoint).fill(0);

      // 滑窗统计
      this.periodTimestamp = [];
      this.periodSum = this.sumData.slice(0);
      this.periodTimes = [];
      this.periodMax = this.maxData.slice(0);
      this.periodMin = this.minData.slice(0);
      this.periodAvg = this.avgData.slice(0);
      // [{timestatme:ddd}]
      this.periodStatInfo = [];
    }
    this.rainLine = this.realData.slice(0);
    this.outRainLine = outRain || true;
    this.noStatics = noStatics;
    // 默认都不显示
    // this.showOptions = {};
    // const series = Object.values(SeriesTypes);
    // this.showOptions = {};
    // series.forEach((s) => {
    //   this.showOptions[s] = s === SeriesTypes.real ? 2 : 1;
    // });
  }

  /**
   *
   * @param {{segmentOffset:Number, offset:Number,data:Array<Number>,max:Array<Number>,avg:Array<Number>,min:Array<Number>,thr:Array<Number>}} scanData
   * @returns
   */
  ScanDataHelper.prototype.setData = function (scanData) {
    const { offset, data, segmentOffset } = scanData;

    const seg = this.segments[segmentOffset];
    if (!seg) return undefined;
    const start = seg.startIndex + offset;
    const len = data.length;
    if (this.noStatics) {
      for (let i = 0; i < len; i += 1) {
        const d = Math.floor(data[i]) / 10;
        const si = start + i;
        this.realData[si] = d;
        if (this.outRainLine) this.rainLine[si] = d;
      }
    } else {
      const restart = _resetStaticsLen !== this.realData.length;
      if (_staticsPeriod > 0) {
        let si = start;

        for (let i = 0; i < len; i += 1) {
          const d = Math.floor(data[i]) / 10;
          si = start + i;
          this.realData[si] = d;
          if (this.outRainLine) this.rainLine[si] = d;
          if (restart) {
            this.periodMax[si] = d;
            this.periodMin[si] = d;
            this.periodSum[si] = d;
            this.periodTimes[si] = 1;
          } else {
            let times = this.periodTimes[si] || 0;
            times += 1;
            this.periodTimes[si] = times;
            if (d < this.periodMin[si]) {
              this.periodMin[si] = d;
            } else if (d > this.periodMax[si]) {
              this.periodMax[si] = d;
            }
            const s = this.periodSum[si] + d;
            this.periodSum[si] = s;
            // this.periodAvg[si] = Math.floor((s * 10) / times) / 10;
          }
        }
        if (restart) _resetStaticsLen = start + len;
        const dt1 = new Date().getTime();
        if (
          dt1 - _prevStaticTime > _staticsStep &&
          start + len === this.realData.length
        ) {
          // restart = true;
          _prevStaticTime = dt1;
          _resetStaticsLen = 0;

          // 保存数据
          this.periodStatInfo.push({
            timestamp: dt1,
            times: this.periodTimes.slice(0),
            max: this.periodMax.slice(0),
            // avg: this.periodAvg.slice(0),
            min: this.periodMin.slice(0),
            sum: this.periodSum.slice(0),
          });
          // this.periodTimes = [];
          // 计算最新的统计值
          let stepStart = -1;
          let pTimes = [];
          for (let p = 0; p < this.periodStatInfo.length; p++) {
            const periodData = this.periodStatInfo[p];
            if (dt1 - periodData.timestamp < _staticsPeriod) {
              if (stepStart < 0) {
                this.maxData = periodData.max;
                this.minData = periodData.min;
                this.sumData = periodData.sum;
                pTimes = periodData.times;
                stepStart = p;
              } else {
                for (let s = 0; s < this.realData.length; s += 1) {
                  let tms = pTimes[s];
                  tms += periodData.times[s];
                  pTimes[s] = tms;
                  const smax = periodData.max[s];
                  if (smax > this.maxData[s]) {
                    this.maxData[s] = smax;
                  }
                  const smin = periodData.min[s];
                  if (smin < this.minData[s]) {
                    this.minData[s] = smin;
                  }
                  const ssum = periodData.sum[s];
                  const ss = this.sumData[s] + ssum;
                  this.avgData[s] = Math.floor((ss * 10) / tms) / 10;
                  this.sumData[s] = ss;
                }
              }
            }
          }
          // 移除数据
          if (stepStart > 0) {
            this.periodStatInfo = this.periodStatInfo.slice(stepStart);
          }
        }
      } else {
        const times = this.times[start] + 1;
        this.times[start] = times;
        for (let i = 0; i < len; i += 1) {
          const d = Math.floor(data[i]) / 10;
          const si = start + i;
          this.realData[si] = d;
          if (this.outRainLine) this.rainLine[si] = d;
          if (d < this.minData[si]) {
            this.minData[si] = d;
          } else if (d > this.maxData[si]) {
            this.maxData[si] = d;
          }
          const s = this.sumData[si] + d;
          this.sumData[si] = s;
          this.avgData[si] = Math.floor((s * 10) / times) / 10;
        }
      }
    }
    const rainLine = this.outRainLine ? this.rainLine.slice(0) : undefined;
    if (rainLine[this.totalPoint - 1] != -9999) {
      this.rainLine = this.defaultLine.slice(0);
    }
    return {
      line: {
        data: this.realData,
        max: this.maxData,
        avg: this.avgData,
        min: this.minData,
        thr: this.thrData,
      },
      rainLine,
    };
  };

  /**
   *
   * @param {{segmentOffset:Number, data:Array<Number>}} scanData
   * @returns
   */
  ScanDataHelper.prototype.setThreshold = function (scanData) {
    const { data, segmentOffset } = scanData;

    const seg = this.segments[segmentOffset];
    if (!seg) return undefined;
    const start = seg.startIndex;

    // 门限只能一段一段来
    this.thrData.set(data, start);

    return {
      line: {
        data: this.realData,
        max: this.maxData,
        avg: this.avgData,
        min: this.minData,
        thr: this.thrData,
      },
    };
  };

  /**
   *
   * @param {{max:Number,avg:Number,min:Number,thr:Number}|Array<String>} options
   */
  ScanDataHelper.prototype.setVisible = function (options) {
    console.log("set series visible:::", options);
    // options.map((s) => {
    //   // this.showOptions[s.name] = s.visible ? 2 : 1;
    // });
  };

  ScanDataHelper.prototype.getSource = function () {
    if (!this.noStatics) {
      const sourceData = {
        real: this.realData,
        max: this.maxData,
        avg: this.avgData,
      };
      // 拆分源数据
      const segReal = [];
      const segMax = [];
      const segAvg = [];
      this.segments.forEach((seg) => {
        const r = this.realData.slice(
          seg.startIndex,
          seg.startIndex + seg.pointCount
        );
        segReal.push(new Int16Array(r));
        const m = this.maxData.slice(
          seg.startIndex,
          seg.startIndex + seg.pointCount
        );
        segMax.push(new Int16Array(m));
        const a = this.avgData.slice(
          seg.startIndex,
          seg.startIndex + seg.pointCount
        );
        segAvg.push(new Int16Array(a));
      });
      sourceData.segMax = segMax;
      sourceData.segAvg = segAvg;
      sourceData.segReal = segReal;

      return sourceData;
    }
    return null;
  };

  ScanDataHelper.prototype.clear = function () {
    this.realData = this.defaultLine.slice(0);
    if (!this.noStatics) {
      this.maxData = this.defaultLine.slice(0);
      this.avgData = this.defaultLine.slice(0);
      this.minData = this.defaultLine.slice(0);
      this.thrData = this.defaultLine.slice(0);
    }
    this.rainLine = this.realData.slice(0);
  };

  /**
   *
   * @param {{invalidValue:Number}} props
   */
  function SpectrumDataHelper(props) {
    const { invalidValue } = props || {};
    this.invalidValue = invalidValue !== undefined ? invalidValue : -9999;
    this.real = null;
    this.max = [];
    this.avg = [];
    this.min = [];
    this.sum = [];
    this.times = 1;
    // 滑窗统计
    this.periodTimestamp = [];
    this.periodSum = [];
    this.periodTimes = 0;
    this.periodMax = [];
    this.periodMin = [];
    this.periodAvg = [];
    this.restart = false;
    // [{timestatme:ddd}]
    this.periodStatInfo = [];
    this.prevStatMax = -9999;

    this.prevStatMin = 9999;
    this.prevStatSum = 0;
    this.prevStatTimes = 0;
    // 默认都不显示
    // const series = Object.values(SeriesTypes);
    // this.showOptions = {};
    // series.forEach((s) => {
    //   this.showOptions[s] = s === SeriesTypes.real ? 2 : 1;
    // });
  }

  /**
   *
   * @param {Int16Array<Number>} specRow
   */
  SpectrumDataHelper.prototype.setData = function (specRow) {
    const len = specRow.length;
    if (this.real && this.real.length === len) {
      if (_staticsPeriod > 0) {
        for (let i = 0; i < len; i += 1) {
          const d = Math.floor(specRow[i]) / 10;
          this.real[i] = d;

          if (this.restart) {
            this.periodMax[i] = d;
            this.periodMin[i] = d;
            this.periodSum[i] = d;
            this.periodTimes = 0;
          } else {
            if (d < this.periodMin[i]) {
              this.periodMin[i] = d;
            } else if (d > this.periodMax[i]) {
              this.periodMax[i] = d;
            }
            const s = this.periodSum[i] + d;
            this.periodSum[i] = s;
            // this.periodAvg[i] = Math.floor((s * 10) / this.periodTimes) / 10;
          }
        }
        this.restart = false;
        this.periodTimes += 1;

        const dt1 = new Date().getTime();
        if (dt1 - _prevStaticTime > _staticsStep) {
          this.restart = true;
          _prevStaticTime = dt1;
          // 保存数据
          this.periodStatInfo.push({
            timestamp: dt1,
            times: this.periodTimes,
            max: this.periodMax.slice(0),
            // avg: this.periodAvg.slice(0),
            min: this.periodMin.slice(0),
            sum: this.periodSum.slice(0),
          });
          // 计算最新的统计值
          let stepStart = -1;
          let pTimes = 0;
          for (let p = 0; p < this.periodStatInfo.length; p++) {
            const periodData = this.periodStatInfo[p];
            if (dt1 - periodData.timestamp < _staticsPeriod) {
              if (stepStart < 0) {
                this.max = periodData.max.slice(0);
                this.min = periodData.min.slice(0);
                this.sum = periodData.sum.slice(0);
                pTimes = periodData.times;
                stepStart = p;
              } else {
                pTimes += periodData.times;
                for (let s = 0; s < len; s += 1) {
                  const smax = periodData.max[s];
                  if (smax > this.max[s]) {
                    this.max[s] = smax;
                  }
                  const smin = periodData.min[s];
                  if (smin < this.min[s]) {
                    this.min[s] = smin;
                  }
                  const ssum = periodData.sum[s];
                  const ss = this.sum[s] + ssum;
                  this.avg[s] = Math.floor((ss * 10) / pTimes) / 10;
                  this.sum[s] = ss;
                }
              }
            }
          }
          // 移除数据
          if (stepStart > 0) {
            this.periodStatInfo = this.periodStatInfo.slice(stepStart);
          }
        }
      } else {
        for (let i = 0; i < len; i += 1) {
          const d = Math.floor(specRow[i]) / 10;
          this.real[i] = d;
          if (d < this.min[i]) {
            this.min[i] = d;
          } else if (d > this.max[i]) {
            this.max[i] = d;
          }
          const s = this.sum[i] + d;
          this.sum[i] = s;
          this.avg[i] = Math.floor((s * 10) / this.times) / 10;
        }
        this.times += 1;
      }
    } else {
      for (let i = 0; i < len; i += 1) {
        specRow[i] /= 10;
      }
      this.real = specRow;
      if (_staticsPeriod > 0) {
        this.periodMax = specRow.slice(0);
        this.periodSum = specRow.slice(0);
        this.periodMin = specRow.slice(0);
        this.periodAvg = specRow.slice(0);
        this.periodTimes = 1;
      }
      this.max = specRow.slice(0);
      this.avg = specRow.slice(0);
      this.min = specRow.slice(0);
      this.sum = specRow.slice(0);
      this.times = 1;
    }

    return {
      data: this.real,
      max: this.max,
      avg: this.avg,
      min: this.min,
    };
  };

  /**
   *
   * @param {Array<{name:String,visible:boolean}>} options
   */
  SpectrumDataHelper.prototype.setVisible = function (options) {
    // options.map((s) => {
    //   this.showOptions[s.name] = s.visible ? 2 : 1;
    // });
  };

  SpectrumDataHelper.prototype.getSource = function () {
    const sourceData = {
      real: this.real.slice(0),
      max: this.max.slice(0),
      avg: this.avg.slice(0),
    };

    return sourceData;
  };

  /**
   * @type {Uint8ClampedArray|Uint8Array}
   */
  var rainImageData;
  // rainImageData 的数据长度
  var imageDataLen = 0;
  var colors = [];
  // 缓存源数据
  var rowsBuffer = [];
  /**
   * @type {Uint8ClampedArray|Uint8Array}
   */
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
  var prevRowOver = false;
  var colorBit = 3;
  var dataForGPU = false;
  var prevSendScan = new Date().getTime();
  var prevSendSpec = new Date().getTime();
  // 统计值滑窗周期 0 表示一直统计
  var _staticsPeriod = 0;
  // 滑窗统计步长 ms
  var _staticsStep = 100;
  var _prevStaticTime = 0;
  // 重置点数，scan的时候需要重置整帧
  var _resetStaticsLen = 0;

  /**
   * @type {ScanDataHelper}
   */
  var scanDataHelper;
  /**
   * @type {ScanDataHelper}
   */
  var scanDFDataHelper;

  /**
   * @type {ScanDataHelper}
   */
  var occDataHelper;

  var prevSourceData;

  var specDataHelper = new SpectrumDataHelper();
  // 外部是否在获取瀑布图渲染数据
  var needRenderRainBuffer = false;

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

  function clipLineData(d) {
    const { data, max, avg, min, thr } = d;
    const { zoomStart, zoomEnd, zoomLen, width } = states;
    const renderEnd = zoomEnd + 1;
    let data1 = data,
      max1 = max,
      avg1 = avg,
      min1 = min,
      thr1 = thr;

    if (zoomLen !== data.length) {
      // 缩放了
      data1 = data.slice(zoomStart, renderEnd);
      if (max) max1 = max.slice(zoomStart, renderEnd);
      if (avg) avg1 = avg.slice(zoomStart, renderEnd);
      if (min) min1 = min.slice(zoomStart, renderEnd);
      if (thr) thr1 = thr.slice(zoomStart, renderEnd);
    }
    if (zoomLen > width) {
      data1 = combineData(data1, width);
      if (max) max1 = combineData(max1, width);
      if (avg) avg1 = combineData(avg1, width);
      if (min) min1 = combineData(min1, width);
      if (thr) thr1 = combineData(thr1, width);
    }
    return { data: data1, max: max1, avg: avg1, min: min1, thr: thr1 };
  }

  function clipRainRow(data) {
    const { zoomStart, zoomEnd, width, zoomLen } = states;
    let drawData = data;
    if (zoomLen !== data.length) {
      // 缩放了
      drawData = data.slice(zoomStart, zoomEnd + 1);
    }
    if (drawData.length > width) {
      // 抽取数据
      drawData = combineData(drawData, width);
    }
    return drawData;
  }

  function rowDataCast(data, pixelWidth, isOver) {
    // 缓存数据
    if (isOver) cacheRows(data);
    let drawData = data;
    // 如果外面一开始就不显示rain，则不会初始化高度和宽度进来
    drawData = clipRainRow(data);
    const rowImageData = data2ImageData(drawData, pixelWidth);
    if (prevRowOver) {
      // 整体往后移动一行
      const startIndex = pixelWidth * colorBit;
      rainImageData.copyWithin(startIndex, 0, imageDataLen - startIndex);
    }
    // } else {
    // 当前数据插入第一行
    rainImageData.set(rowImageData, 0);
    // }
    return drawData;
  }

  cacheRows = (data) => {
    const cacheRowLen = rowsBuffer.length;
    if (cacheRowLen > 0 && data.length * (cacheRowLen + 1) > 1000000) {
      rowsBuffer = rowsBuffer.slice(1);
    }
    rowsBuffer.push(data.slice(0));
  };

  /**
   * 添加一行数据
   * @param {Array} data
   */
  function addRow(data) {
    let pixelWidth = states.width;
    const dataLen = data.length;
    const isOver = data[dataLen - 1] != -9999;
    const renderRow = rowDataCast(data, pixelWidth, isOver);
    // 判断是否收完
    prevRowOver = isOver;
    const dt1 = new Date().getTime();
    // 控制帧率
    if (dt1 - prevRender > 49) {
      // 发送到外面用于marker显示
      const rowForMarkerVal = renderRow.slice(0);
      self.postMessage({ renderRow: rowForMarkerVal }, [
        rowForMarkerVal.buffer,
      ]);
      const copy = rainImageData.slice(0);
      self.postMessage({ imageData: copy }, [copy.buffer]);
      prevRender = dt1;
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
    imageDataLen = bufferLen;
    rowDefault = rainImageData.slice(0, oneRow);
    if (states.defaultColor != 255 && colorBit === 4) {
      for (let i = 0; i < states.width; i++) {
        rowDefault[i * 4 + 3] = 255;
      }
    }
  }

  function reRender() {
    initRenderBuffer();
    const { width } = states;
    const len = rowsBuffer.length;
    if (len > 0 && imageDataLen > 0) {
      // 重置数据
      for (let i = len - 1; i >= 0; i -= 1) {
        let row = rowsBuffer[i];
        row = clipRainRow(row);
        const rowImageData = data2ImageData(row, width);
        const startIndex = width * colorBit * i;
        if (startIndex < imageDataLen)
          rainImageData.set(rowImageData, startIndex);
      }

      const copy = rainImageData.slice(0);
      self.postMessage({ imageData: copy }, [copy.buffer]);
    }
  }

  function initScanDataHelper(segments) {
    const endSegment = segments[segments.length - 1];
    const totalPoint = endSegment.startIndex + endSegment.pointCount;
    scanDataHelper = new ScanDataHelper({
      segments,
      totalPoint,
      outRain: true,
      invalidValue: -9999,
      noStatics: false,
    });
    scanDFDataHelper = new ScanDataHelper({
      segments,
      totalPoint,
      outRain: false,
      invalidValue: -1,
      noStatics: true,
    });

    occDataHelper = new ScanDataHelper({
      segments,
      totalPoint,
      outRain: false,
      invalidValue: -1,
      noStatics: true,
    });
  }
  addEventListener("message", (e) => {
    const {
      colors: cs,
      // 频谱瀑布图行
      specRow,
      axisYRange,
      zoomInfo,
      clear,
      enableGPU,
      plotArea,
      defaultColor,
      segments,
      scan,
      dfScan,
      getSource,
      threshold,
      staticsPeriod,
      occ,
    } = e.data;
    let plotAreaChange = false;
    if (enableGPU) {
      dataForGPU = enableGPU === "true";
      colorBit = enableGPU === "true" ? 3 : 4;
    }
    if (defaultColor) {
      states.defaultColor = defaultColor;
    }
    if (specRow) {
      let dr = specRow;
      if (!specRow.buffer) dr = new Int16Array(specRow);
      const renderData = specDataHelper.setData(dr);
      if (renderData) {
        addRow(renderData.data);
        const drawData = clipLineData(renderData);
        const dt = new Date().getTime();
        if (dt - prevSendSpec > 40) {
          prevSendSpec = dt;
          self.postMessage({ spec: drawData });
        }
      }
    } else if (cs) {
      states.colorBlends = cs;
      colors = initColors(cs);
      // TODO 重绘
    } else if (axisYRange) {
      states.minimumY = axisYRange.minimumY;
      states.maximumY = axisYRange.maximumY;
      colors = initColors(states.colorBlends || cs);
      // TODO 重绘
    } else if (zoomInfo) {
      states.zoomStart = zoomInfo.zoomStart;
      states.zoomEnd = zoomInfo.zoomEnd;
      states.zoomLen = zoomInfo.zoomLen;
    } else if (plotArea) {
      const { width, height } = plotArea;
      plotAreaChange =
        (width && width !== states.width) ||
        (height && height !== states.height);
      if (plotAreaChange) {
        states.width = width || states.width;
        states.height = height || states.height;
      }
    } else if (clear) {
      // 清除数据&通知外部
      specDataHelper = new SpectrumDataHelper();
      if (scanDataHelper) {
        scanDataHelper.clear();
      }
      initRenderBuffer();
      // preDrawMatrix = [];
      rowsBuffer = [];
      const copy = rainImageData.slice(0);
      self.postMessage({ imageData: copy }, [copy.buffer]);
    } else if (segments) {
      // 处理扫描数据拼装工作
      initScanDataHelper(segments);
      initRenderBuffer();
      rowsBuffer = [];
    } else if (scan) {
      const renderData = scanDataHelper.setData(scan);
      prevSourceData = renderData.line;
      if (renderData.rainLine) addRow(renderData.rainLine);
      const dt = new Date().getTime();
      if (dt - prevSendScan > 40) {
        prevSendScan = dt;
        const drawData = clipLineData(renderData.line);
        self.postMessage({ scan: drawData });
      }
    } else if (threshold) {
      const renderData = scanDataHelper.setThreshold(threshold);
      const drawData = clipLineData(renderData.line);
      self.postMessage({ scan: drawData });
    } else if (dfScan) {
      const renderData = scanDFDataHelper.setData(dfScan);
      const drawData = clipLineData(renderData.line);
      self.postMessage({ dfScan: drawData });
    } else if (getSource) {
      // 获取源数据
      // 转换成整型？
      let sourceData = null;
      if (scanDataHelper) {
        const source = scanDataHelper.getSource();
        if (source) {
          const len = source.max.length;
          const max = new Int16Array(len);
          max.set(source.max, 0);
          const avg = new Int16Array(len);
          avg.set(source.avg, 0);
          const real = new Int16Array(len);
          real.set(source.real, 0);
          sourceData = {
            real,
            max,
            avg,
            segMax: source.segMax,
            segAvg: source.segAvg,
            segReal: source.segReal,
          };
        }
      } else {
        sourceData = specDataHelper.getSource();
      }

      self.postMessage({ sourceData });
    } else if (staticsPeriod) {
      _staticsPeriod = staticsPeriod;
      const count = staticsPeriod / _staticsStep;
      if (count > 100) _staticsStep = 500;
      else if (count > 50) _staticsStep = 250;
      else if (count > 20) _staticsStep = 200;
      else _staticsStep = 100;
      _prevStaticTime = new Date().getTime();
    } else if (occ) {
      const renderData = occDataHelper.setData(occ);
      const drawData = clipLineData(renderData.line);
      self.postMessage({ occ: drawData });
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
  prevScanDFData;
  prevOccData;
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

  /**
   * @type {{rainColors:Array<String>,max:String,avg:String,min:String,real:String,thr:String,streamTime:Number}}
   */
  chartConfig;

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
    // hideSeries: [ ],
    seriesVisible: {},
    // [
    //   { name: SeriesTypes.real, visible: true },
    //   { name: SeriesTypes.max, visible: false },
    //   { name: SeriesTypes.avg, visible: false },
    //   { name: SeriesTypes.min, visible: false },
    //   { name: SeriesTypes.thr, visible: false },
    // ],
    gpuRain: true,
    prevSignals: null,
    lightTheme: false,
    // 统计值保持时长，0表示一直统计
    statePeriod: 0,
  };

  #onMarkerChange;

  /**
   * @constructor
   * @param {{gpuRain, lightTheme:boolean}} options
   */
  constructor({ gpuRain, lightTheme }) {
    this.myStates.gpuRain = gpuRain;
    this.myStates.lightTheme = lightTheme;
    this.chartConfig = getChartConfig(lightTheme);
    // 初始化默认显示线条
    const allSeries = Object.values(SeriesTypes);
    allSeries.forEach((item) => {
      this.myStates.seriesVisible[item] = item === SeriesTypes.real;
    });
    // 初始化线程
    const workerJs = `(${workerThread.toString()})()`;
    this.workerBlob = new Blob([workerJs], { type: "text/javascript" });
    const blogUrl = window.URL.createObjectURL(this.workerBlob);
    this.worker = new Worker(blogUrl, {
      name: `ChartWorker-${Number(
        Math.random().toString().substring(2)
      ).toString(36)}`,
    });
    this.worker.onmessage = (e) => {
      const { renderRow, imageData, scan, dfScan, spec, sourceData, occ } =
        e.data;
      const onDataEvents = this.callbacks[EventTypes.DataChange];
      if (imageData) {
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

        if (this.myStates.pause) {
          const matrixLen = this.pauseMatrix.length;
          if (isOver) {
            this.pauseMatrix.push(renderRow);
            if (this.pauseMatrix.length > this.totalFrame) {
              this.pauseMatrix = this.pauseMatrix.slice(1);
            }
          } else {
            this.pauseMatrix[matrixLen - 1] = renderRow;
          }
        } else {
          const matrixLen = this.preDrawMatrix.length;
          this.preDrawMatrix[matrixLen - 1] = renderRow.slice(0);
          if (isOver) {
            this.preDrawMatrix.push([]);
            if (this.preDrawMatrix.length > this.totalFrame) {
              this.preDrawMatrix = this.preDrawMatrix.slice(1);
            }
          }
        }
      }
      if (scan || dfScan || spec || occ) {
        if (onDataEvents) {
          if (this.myStates.pause) {
            // this.myStates.pause.specData = renderData;
          } else {
            const specRenderData = scan || spec;
            if (specRenderData) {
              if (this.customThreshold)
                specRenderData.thr = this.customThreshold;
              this.prevSpecData = specRenderData;
            }
            if (dfScan) {
              this.prevScanDFData = dfScan;
            }
            if (occ) {
              this.prevOccData = occ;
            }
            onDataEvents.forEach((e) =>
              e({ spectrum: specRenderData, scanDF: dfScan, occ })
            );

            // this.prevSpecData = renderData;
            // onDataEvents.forEach((e) =>
            //   e({
            //     spectrum: renderData,
            //     scanDF: dfScan ? renderData : undefined,
            //   })
            // );
            // 更新marker位置
            const dt = new Date().getTime();
            if (dt - this.myStates.prevDataTime > 149) {
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
      }
      if (sourceData && this.getSourceCallBack) {
        this.getSourceCallBack(sourceData);
        this.getSourceCallBack = null;
        // const onSourceEvents = this.callbacks[EventTypes.ScanSource];
        // onSourceEvents.forEach((e) => e(scanSource));
      }
    };
    this.worker.onerror = (er) => {
      console.log("worker error:::", er);
    };
    this.worker.postMessage({
      colors: this.chartConfig.rainColors,
      enableGPU: gpuRain ? "true" : "false",
      // defaultColor 只能设置纯色，RGB都相同
      defaultColor: lightTheme ? 255 : 0,
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

  /**
   * width 容器宽度，height 瀑布图高度！！！！
   * @param {{width:Number,height:Number}} plotArea
   */
  setPlotArea(plotArea) {
    const { width, height } = plotArea;
    if (width) {
      this.plotArea.width = width;
    }
    if (height) {
      this.plotArea.height = height;
      // 这里可能会导致瀑布图高度变化，需要更新marker的位置 行号
      this.myStates.markers.forEach((mk) => {
        if (mk && mk.anchorChart === ChartTypes.rain && mk.rowIndex > -1) {
          const scale = mk.rowIndex / this.totalFrame;
          mk.rowIndex = Math.round(scale * height);
        }
      });
      this.totalFrame = height;
      if (this.timestamps.length > height) {
        this.timestamps = this.timestamps.slice(0, height);
      }
    }
    if (this.worker) {
      this.worker.postMessage({
        plotArea,
      });
    }
  }

  setTheme(lightTheme) {
    // 内部更新主题
    this.myStates.lightTheme = lightTheme;
    this.chartConfig = getChartConfig(lightTheme);
    this.worker.postMessage({
      colors: this.chartConfig.rainColors,
    });
    const confChangeEvents = this.callbacks[EventTypes.ConfigChange];
    if (confChangeEvents) {
      confChangeEvents.forEach((e) => e(this.chartConfig));
    }
  }

  // 外部更新，会触发存储动作
  setConfig(config) {
    // 外部设置
    this.chartConfig = config;
    saveChartConfig(config, this.myStates.lightTheme);

    this.worker.postMessage({
      colors: this.chartConfig.rainColors,
    });
    const confChangeEvents = this.callbacks[EventTypes.ConfigChange];
    if (confChangeEvents) {
      confChangeEvents.forEach((e) => e(this.chartConfig));
    }
  }

  validateData(d) {
    const { data, max, avg, min, thr } = d;
    const { startIndex, zoomLen } = this.myStates.zoomInfo;
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

    this.timestamps.push(timestamp);
    if (this.timestamps.length > this.totalFrame)
      this.timestamps = this.timestamps.slice(1);

    const dataForRain = data.slice(0);
    if (this.worker) this.worker.postMessage({ specRow: dataForRain });
  }

  /**
   * 更新marker水平位置
   * 当参数变更+拖动+设置中心频率 时使用
   */
  updateMarkerDataXIndex() {}

  updateChartArgs = (args) => {
    this.chartArgs = { ...this.chartArgs, ...args };
    if (args.visibleCharts || args.chartBounding) {
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

  /**
   *
   * @param {data:Array<Number>} data
   */
  setCustomThreshold(data) {
    this.customThreshold = data;
  }

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
    const maxY = minY + gap;
    if (this.myStates.minimumY !== minY || this.myStates.maximumY !== maxY) {
      this.myStates.minimumY = minY;
      this.myStates.maximumY = maxY;
      this.myStates.axisYTickGap = gap / 10;
      this.#fireAxisYRangeChange();
      return gap / 10;
    }
    return this.myStates.axisYTickGap;
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
      if (range) return this.setAxisYRange(range.minimum, range.maximum);
    }
    return null;
  };

  pauseRender(pause) {
    if (pause) {
      this.myStates.pause = {};
    } else {
      this.myStates.pause = undefined;
      // 缓存最新的瀑布图数据
      const arr = this.preDrawMatrix.concat(this.pauseMatrix);
      const offset = arr.length - this.totalFrame;
      this.preDrawMatrix = arr.slice(offset);
    }
  }

  /**
   * @protected
   * x 水平方向位置%
   * y 垂直方向位置%
   * px 水平方向位置 px
   * py 垂直方向位置 py
   * @param {{x:Number,y:Number,px:Number,py:Number,visibleCharts:Array<String>}} mouseArgs
   * @param {boolean} nearPeak 是否为移动端，移动端需要做临近信号查找
   */
  updateMouseDataIndex = (args, nearPeak) => {
    if (this.chartArgs.chartBounding) {
      const mouseInChart = this.getMouseInChart(args);
      const dataPosition = this.getRenderDataPosition(args, mouseInChart);
      if (nearPeak && dataPosition.dataXIndex > 0) {
        // 临近40点，最大值查找
        let minX = dataPosition.dataXIndex - 25;
        if (minX < 0) minX = 0;
        const maxX = dataPosition.dataXIndex + 25;
        let maxIndex = dataPosition.dataXIndex;
        const referData = this.prevSpecData.max
          ? this.prevSpecData.max
          : this.prevSpecData.data;
        let maxData = referData[maxIndex];
        for (let i = minX; i < maxX; i += 1) {
          const d = referData[i];
          if (d > maxData) {
            maxData = d;
            maxIndex = i;
          }
        }
        dataPosition.dataXIndex = maxIndex;
      }
      if (dataPosition) {
        dataPosition.chart = mouseInChart;
        this.myStates.mousePosition = dataPosition;
      }
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
    if (x > -1) {
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
      if (dataXIndex > -1) {
        cursorFreq = this.getFrequencyByIndex(dataXIndex).freq;
      } else {
        cursorFreq = this.getFrequencyOnNoData(x);
      }

      const cursorChangeEvents = this.callbacks[EventTypes.CursorChange];
      if (cursorChangeEvents) {
        let unit = this.chartConfig.unit;

        cursorChangeEvents.forEach((e) =>
          e({
            cursorPosX: pos ? pos.x : x * 100,
            cursorPosY: pos && pos.y > -1 ? pos.y : y * 100,
            dataInfo,
            frequency: cursorFreq,
            tickValue:
              yTick !== undefined
                ? String(unit == "dBm" ? yTick - 105 : yTick)
                : "",
          })
        );
      }
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
          let { maximumY, minimumY } = this.myStates;
          let data = this.prevSpecData.data[dataXIndex];
          if (anchorChart === ChartTypes.wbdf) {
            maximumY = 360;
            minimumY = 0;
            data = this.prevScanDFData.data[dataXIndex];
          }
          if (anchorChart === ChartTypes.occupancy) {
            maximumY = 100;
            minimumY = 0;
            data = this.prevOccData.data[dataXIndex];
          }
          const axisYRange = maximumY - minimumY;
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
   * @param {{id:String,text:String}} options
   * @param {Number} forTip
   */
  addMarker1 = (options, forTip) => {
    if (!this.prevSpecData) {
      console.log("Add marker failed:: Can not find spectrum data!!");
      return;
    }
    let id = new Date().getTime() + Math.random().toString(6);
    const { visibleCharts } = this.chartArgs;
    let { dataXIndex, dataYIndex, chart } = this.myStates.mousePosition;
    console.log("add marker:::", options, this.myStates.mousePosition);
    if (options) {
      id = options.id;
      if (options.frequency) {
        dataXIndex = this.getRenderXIndexByFrequency(
          options.frequency
        ).renderDataXIndex;
      } else {
        dataXIndex = Math.floor(this.prevSpecData.data.length / 2);
      }
      chart = visibleCharts[0];
      if (chart === ChartTypes.rain) {
        dataYIndex = 0;
      }
    }
    const { freq, segmentIndex } = this.getFrequencyByIndex(dataXIndex);

    let markerIndex = -1;
    // 清除选中 & 查找可用的位置
    this.myStates.markers.forEach((mk, indx) => {
      if (mk) {
        mk.selected = false;
      } else if (markerIndex < 0) {
        markerIndex = indx;
      }
    });
    if (forTip > 0) {
      if (markerIndex < 0) {
        markerIndex = this.myStates.markers.length;
      }
      if (markerIndex >= forTip) {
        return;
      }
    } else if (markerIndex < 0) {
      return;
    }
    let mk = {
      id,
      markerIndex,
      rowIndex: dataYIndex,
      freqMHz: freq,
      selected: true,
      visible: true,
      anchorChart: chart,
      text: options ? options.text : "",
      segmentIndex,
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
   *
   * @param {{id:String,text:String,frequency:Number}} options
   * @param {Number} forTip
   */
  addMarker = (options) => {
    this.addMarker1(options, false);
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
      if (mouseInChart) mk.anchorChart = mouseInChart;
    }

    const { dataXIndex, dataYIndex } = this.getRenderDataPosition(
      args,
      mouseInChart
    );
    let freq = mk.freqMHz;
    let segmentIndex = mk.segmentIndex;
    if (dragX) {
      const freqInfo = this.getFrequencyByIndex(dataXIndex);
      freq = freqInfo.freq;
      segmentIndex = freqInfo.segmentIndex;
    }
    if (
      dataYIndex !== mk.rowIndex ||
      freq !== mk.freqMHz ||
      segmentIndex !== mk.segmentIndex
    ) {
      if (dragY) {
        mk.rowIndex = dataYIndex;
      }
      if (dragX) {
        mk.freqMHz = freq;
        mk.segmentIndex = segmentIndex;
      }
      this.updateMarkerInfo(id);
      this.fireMarkerChange();
    }
  };

  /**
   * 上下 左右 移动marker
   * @param {*} param0
   * @returns
   */
  moveMarker({ action, id }) {
    const mk = this.myStates.markers.find((m) => m.id === id);
    let { rowIndex, freqMHz, anchorChart, segmentIndex } = mk;
    const { visibleCharts } = this.chartArgs;

    let chartIndex = visibleCharts.indexOf(anchorChart);
    switch (action) {
      case "up":
        {
          if (anchorChart === ChartTypes.rain) {
            rowIndex -= 1;
            if (rowIndex < 0) {
              chartIndex = visibleCharts.indexOf(ChartTypes.spectrum);
              if (chartIndex < 0) rowIndex = 0;
            }
          }
          if (chartIndex >= 0) {
            // 上移一个图表
            anchorChart = visibleCharts[chartIndex];
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
        let { renderDataXIndex } = this.getRenderXIndexByFrequency(
          mk.freqMHz,
          mk.segmentIndex
        );
        if (action === "left") {
          renderDataXIndex -= 1;
          if (renderDataXIndex < 0) return;
        } else {
          renderDataXIndex += 1;
          if (renderDataXIndex > this.prevSpecData.data.length - 1) return;
        }
        const freqInfo = this.getFrequencyByIndex(renderDataXIndex);
        freqMHz = freqInfo.freq;
        segmentIndex = freqInfo.segmentIndex;
        mk.dataXIndex = renderDataXIndex;
        break;
    }
    mk.rowIndex = rowIndex;
    mk.anchorChart = anchorChart;
    mk.freqMHz = freqMHz;
    mk.segmentIndex = segmentIndex;
    this.updateMarkerInfo(id);
    this.fireMarkerChange();
  }

  /**
   *
   * @param {{id:String,select:boolean,text:String}} options
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

  delMarker(id) {
    const markerIndex = this.myStates.markers.findIndex(
      (mk) => mk && mk.id === id
    );
    const mk = this.myStates.markers[markerIndex];
    const select = mk.selected;
    this.myStates.markers[markerIndex] = null;
    this.fireMarkerChange();
    if (select) {
      const onMarkerSelectChange =
        this.callbacks[EventTypes.MarkerSelectChange];
      if (onMarkerSelectChange) {
        onMarkerSelectChange.forEach((e) => e(undefined));
      }
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
   * @param {boolean} peak 峰值
   * @param {boolean} peak2 次峰值
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
          let freqInfo;
          if (mk.keepPeak || peak) {
            freqInfo = this.getFrequencyByIndex(peakInfo.peakIndex);
          } else {
            freqInfo = this.getFrequencyByIndex(peakInfo.peak2Index);
          }
          mk.freqMHz = freqInfo.freq;
          mk.segmentIndex = freqInfo.segmentIndex;
        }
        // 2. 判断marker是否需要被移除
        const xIndexInfo = this.getRenderXIndexByFrequency(
          mk.freqMHz,
          mk.segmentIndex
        );
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

  /**
   *
   * @param {{id:String,frequency:Number}} param0
   */
  setMarkerFrequency({ id, frequency }) {
    if (id && frequency && this.prevSpecData) {
      const marker = this.myStates.markers.find((mk) => mk && mk.id === id);

      if (marker) {
        marker.freqMHz = frequency;
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
    // if (x < 0) {
    //   return { dataXIndex, dataYIndex, x, y, yTick };
    // }

    if (this.prevSpecData && mouseInChart) {
      const renderDataLen = this.prevSpecData.data.length;
      const { chartBounding } = this.chartArgs;
      if (chartBounding) {
        dataXIndex = Math.round((renderDataLen - 1) * x);
        if (dataXIndex < 0) dataXIndex = 0;
        else if (dataXIndex > renderDataLen - 1) dataXIndex = renderDataLen - 1;
        const cb = chartBounding[mouseInChart];
        let yPct = (y - cb.y1) / (cb.y2 - cb.y1);
        if (yPct < 0) yPct = 0;
        else if (yPct > 1) yPct = 1;
        if (mouseInChart === ChartTypes.rain) {
          //  计算y 索引
          dataYIndex = Math.floor((this.totalFrame - 1) * yPct);
        } else if (mouseInChart === ChartTypes.wbdf) {
          // 计算Y轴刻度值
          yTick = Math.ceil((360 - 360 * yPct) * 10) / 10;
        } else if (mouseInChart === ChartTypes.occupancy) {
          yTick = Math.ceil((100 - 100 * yPct) * 10) / 10;
        } else {
          // if (mouseInChart === ChartTypes.spectrum) {
          const { minimumY, maximumY } = this.myStates;
          const axisYRange = maximumY - minimumY;
          yTick = Math.ceil((maximumY - axisYRange * yPct) * 10) / 10;
          // }
        }
      }
    }
    return { dataXIndex, dataYIndex, x, y, yTick };
  };

  getMouseInChart = (args) => {
    const { y } = args;
    const { visibleCharts, chartBounding } = this.chartArgs;
    let mouseInChart = null;

    visibleCharts.forEach((vc, index) => {
      const cb = chartBounding[vc];
      if (y >= cb.y1 && y <= cb.y2) {
        mouseInChart = vc;
      }
    });
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
    return { freq };
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
   * @param {Array<{name:String,visible:boolean}>} ids
   */
  setSeriesVisible(ids) {
    ids.map((item) => {
      this.myStates.seriesVisible[item.name] = item.visible;
    });
    const onDataEvents = this.callbacks[EventTypes.DataChange];
    if (onDataEvents) {
      onDataEvents.forEach((e) => e({ spectrum: this.prevSpecData }));
    }
    const setSeriesVisible = this.callbacks[EventTypes.SetSeriesVisible];
    if (setSeriesVisible) {
      setSeriesVisible.forEach((e) => e(this.myStates.seriesVisible));
    }
  }

  setSatisticPeriod(period) {
    this.myStates.statePeriod = period;
    if (this.worker) {
      this.worker.postMessage({ staticsPeriod: period });
    }
  }

  #getCursorCaption(chart, dataXIndex, dataYIndex) {
    let levelCaption = "";
    // if (dataXIndex < 0 && dataYIndex < 0) return { levelCaption };
    let level = {};
    let unit = this.chartConfig.unit;
    if (chart === ChartTypes.spectrum) {
      let realVal = this.prevSpecData.data[dataXIndex];
      if (unit === "dBm") realVal -= 107;
      level.real = realVal;
      levelCaption = `${SeriesNames.real} ${Number(level.real).toFixed(1)}`;
      if (
        this.myStates.seriesVisible[SeriesTypes.max] &&
        // !this.myStates.hideSeries.includes(SeriesTypes.max) &&
        this.prevSpecData.max
      ) {
        let maxVal = this.prevSpecData.max[dataXIndex];
        if (unit === "dBm") maxVal -= 107;
        level.max = maxVal;
        levelCaption = `${levelCaption} ${SeriesNames.max} ${Number(
          level.max
        ).toFixed(1)}`;
      }
      if (
        this.myStates.seriesVisible[SeriesTypes.avg] &&
        // !this.myStates.hideSeries.includes(SeriesTypes.avg) &&
        this.prevSpecData.avg
      ) {
        let avgVal = this.prevSpecData.avg[dataXIndex];
        if (unit === "dBm") avgVal -= 107;
        level.avg = avgVal;
        levelCaption = `${levelCaption} ${SeriesNames.avg} ${Number(
          level.avg
        ).toFixed(1)}`;
      }

      // if (
      //   !this.myStates.hideSeries.includes(SeriesTypes.min) &&
      //   this.prevSpecData.min
      // ) {
      //   level.min = this.prevSpecData.min[dataXIndex];
      //   levelCaption = `${levelCaption} ${SeriesNames.min} ${Number(
      //     level.min
      //   ).toFixed(1)}`;
      // }
      // if (
      //   !this.myStates.hideSeries.includes(SeriesTypes.thr) &&
      //   this.prevSpecData.thr
      // ) {
      //   level.thr = this.prevSpecData.thr[dataXIndex];
      //   levelCaption = `${levelCaption} ${SeriesNames.thr} ${Number(
      //     level.thr
      //   ).toFixed(1)}`;
      // }
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
        let realVal1 = this.preDrawMatrix[rowIndex][dataXIndex];
        if (unit === "dBm") realVal1 -= 107;
        level.real = realVal1;
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
    if (chart === ChartTypes.occupancy) {
      level.real = this.prevOccData.data[dataXIndex];
      levelCaption = `占用度 ${Number(level.real).toFixed(1)}`;
      unit = "%";
    }
    return { level, levelCaption, timestamp, unit };
  }

  getSource(callback) {
    if (!this.getSourceCallBack) {
      this.getSourceCallBack = callback;
      this.worker.postMessage({
        getSource: "get max avg",
      });
    }
  }

  clear() {
    // 要不要干这个事情？
    // 2023-11-29 要，设置中心频率和带宽时并没有传递到瀑布图数据处理线程，需要发送清除指令
    // 也可能会主动清除
    if (this.worker) {
      this.worker.postMessage({ clear: new Date() });
    }
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
