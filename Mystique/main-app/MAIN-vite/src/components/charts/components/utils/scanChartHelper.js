import { ChartTypes } from "./enums.js";
import ScanDataHelper from "./scanDataHelper.js";
import SpectrumChartHelper, { EventTypes } from "./spectrumChartHelper.js";
import { isMobile, frequency2String1 } from "./utils.js";

/**
 * @class
 */
class ScanChartHelper extends SpectrumChartHelper {
  /**
   * @private
   * @param {ScanDataHelper}
   */
  scanDataHelper;
  /**
   * @private
   * @param {ScanDataHelper}
   */
  scanDFDataHelper;
  /**
   * @private
   * @type {Array<{startFrequency:Number,stopFrequency:Number,stepFrequency:Number,startIndex:Number,pointCount:Number}>}
   */
  #segmentsInfo;

  /**
   * @constructor
   *
   */
  constructor({ colorBlends, gpuRain, defaultColor }) {
    super({ colorBlends, gpuRain, defaultColor });
  }

  /**
   *
   * 更新频段信息， 触发 onSegmentsChange
   * @param {Array<{startFrequency: Number,stopFrequency: Number,stepFrequency: Number}>} segments
   */
  updateSegments(segments) {
    let totalPoints = 0;
    for (let i = 0; i < segments.length; i += 1) {
      const seg = segments[i];
      const { startFrequency, stopFrequency, stepFrequency } = seg;
      const pointCount =
        Math.round(((stopFrequency - startFrequency) * 1000) / stepFrequency) +
        1;
      seg.pointCount = pointCount;
      seg.startIndex = totalPoints;
      totalPoints += pointCount;
    }

    this.#segmentsInfo = segments;
    this.scanDataHelper = new ScanDataHelper({
      segments,
      totalPoint: totalPoints,
      outRain: true,
    });
    this.scanDFDataHelper = new ScanDataHelper({
      segments,
      totalPoint: totalPoints,
      outRain: false,
      invalidValue: -0.09,
      noStatics: true,
    });

    this.timestamps = [];
    this.totalPoints = totalPoints;
    this.prevDataLen = totalPoints;
    this.resetZoom();
    // 清楚瀑布图数据
    // this.rainDataMgr.clear();
    return totalPoints;
  }

  /**
   *
   * @param {*} d
   */
  setData = (d) => {
    const { data, timestamp, type, azimuths } = d;
    const dataChangeEvents = this.callbacks[EventTypes.DataChange];
    if (!dataChangeEvents) return;

    if (type === "scan") {
      // console.log("set data:::", data);
      let renderData = data;
      let rainLine = data;
      let newLine = true;
      // 处理scan数据
      let scanData = this.scanDataHelper.setData(d);
      if (!scanData) return;
      rainLine = scanData.rainLine;
      newLine = scanData.newLine;
      scanData = this.validateData(scanData);
      renderData = scanData;
      // newLine = scanData.newLine;
      // rainLine = scanData.rainLine;

      if (rainLine) {
        if (newLine) {
          this.timestamps.push(timestamp / 1e5);
          if (this.timestamps.length > this.totalFrame)
            this.timestamps = this.timestamps.slice(1);
        }
        if (rainLine instanceof Float32Array) {
          this.worker.postMessage({ row: rainLine }, [rainLine.buffer]);
        } else {
          const buffer = new Float32Array(rainLine);
          this.worker.postMessage({ row: buffer }, [buffer.buffer]);
        }
      }
      if (this.myStates.pause) {
        this.myStates.pause.specData = renderData;
      } else {
        this.prevSpecData = renderData;
        dataChangeEvents.forEach((e) => e({ spectrum: renderData }));
      }
    } else if (type === "dfscan") {
      // let renderData = data;
      d.data = azimuths.map((az) => az / 10);
      let scanDFData = this.scanDFDataHelper.setData(d);
      if (!scanDFData) return;
      scanDFData = this.validateData(scanDFData);
      // renderData = scanDFData;
      if (this.myStates.pause) {
        this.myStates.pause.scanDFData = scanDFData;
      } else {
        this.prevScanDFData = scanDFData;
        dataChangeEvents.forEach((e) => e({ scanDF: scanDFData }));
      }
    } else if (type === "signalsList") {
      this.getSignalInfo(d.signals);
      this.myStates.prevSignals = d.signals;
      dataChangeEvents.forEach((e) => e({ signals: d.signals }));
    } else {
      // TODO 其它数据处理
    }

    // 更新marker位置
    const dt = new Date().getTime();
    if (dt - this.myStates.prevDataTime > 99) {
      if (this.chartArgs.showCursor) this.moveCursor();
      if (this.hasMarker()) {
        this.updateMarkerInfo();
        this.fireMarkerChange();
      }
      this.myStates.prevDataTime = dt;
    }
  };

  resetZoom = () => {
    console.log("reset zoom");
    const totalLen = this.totalPoints;
    this.myStates.zoomInfo = {
      start: 0,
      end: 1,
      startIndex: 0,
      endIndex: totalLen - 1,
      zoomLen: totalLen,
    };
    this.worker.postMessage({
      zoomInfo: {
        zoomStart: 0,
        zoomEnd: totalLen - 1,
        zoomLen: totalLen,
      },
      clear: "clear",
    });
    const zoomChangeEvents = this.callbacks[EventTypes.ZoomChange];
    if (zoomChangeEvents) {
      zoomChangeEvents.forEach((e) => e(this.myStates.zoomInfo));
    }
  };

  /**
   * Marker 或 Cursor 根据所在位置获取freq
   * @param {*} dataXIndex  渲染数据的索引，非源数据
   * @returns
   */
  getFrequencyByIndex(dataXIndex) {
    const scale = dataXIndex / this.prevSpecData.data.length;
    const { zoomInfo } = this.myStates;
    const { zoomLen, startIndex } = zoomInfo;
    const scaleDataLen = Math.round(zoomLen * scale);
    let freq = -1;
    for (let i = 0; i < this.#segmentsInfo.length; i += 1) {
      const seg = this.#segmentsInfo[i];
      if (seg.startIndex + seg.pointCount > startIndex + scaleDataLen) {
        const segPointIndex = startIndex + scaleDataLen - seg.startIndex;
        freq = seg.startFrequency + (seg.stepFrequency * segPointIndex) / 1000;
        break;
      }
    }

    return freq;
  }

  getFrequencyOnNoData(x) {
    for (let i = 0; i < this.#segmentsInfo.length; i += 1) {
      const seg = this.#segmentsInfo[i];
      const startScale = seg.startIndex / this.totalPoints;
      const endScale = (seg.startIndex + seg.pointCount) / this.totalPoints;
      if (x >= startScale && x <= endScale) {
        const segScale = endScale - startScale;
        const offsetScale = (x - startScale) / segScale;
        const span = seg.stopFrequency - seg.startFrequency;
        const segPointIndex = Math.round(
          (span * offsetScale * 1000) / seg.stepFrequency
        );

        return seg.startFrequency + (segPointIndex * seg.stepFrequency) / 1000;
      }
    }
    return NaN;
  }

  getRenderXIndexByFrequency(frequency) {
    for (let i = 0; i < this.#segmentsInfo.length; i += 1) {
      const seg = this.#segmentsInfo[i];
      if (frequency >= seg.startFrequency) {
        const offset = frequency - seg.startFrequency;
        const pointCount = Math.round((offset * 1000) / seg.stepFrequency);
        pointSum += pointCount;
        const freqScale = pointCount / this.totalPoints;
        const renderDataLen = this.prevSpecData.data.length;
        const { zoomInfo } = this.myStates;
        const scaleOfRender =
          (freqScale - zoomInfo.start) / (zoomInfo.end - zoomInfo.start);
        // 4.2 根据4.1获得mk所在显示数据的索引
        let renderDataXIndex = Math.round(renderDataLen * scaleOfRender);

        return { freqScale, renderDataXIndex };
      }
      pointSum += seg.pointCount;
    }

    return null;
  }

  // /**
  //  *
  //  * @param {{id:String,frequency:Number}} param0
  //  */
  // setMarkerFrequency({ id, frequency }) {
  //   // TODO 后面考虑绑定段号
  // }

  zoomToSegment = (segmentIndex) => {
    console.log("zoomToSegment::::");
    const seg = this.#segmentsInfo[segmentIndex];
    this.myStates.zoomInfo = {
      start: seg.startIndex / this.totalPoint,
      end: (seg.startIndex + seg.pointCount) / this.totalPoint,
      startIndex: seg.startIndex,
      endIndex: seg.startIndex + seg.pointCount,
      zoomLen: seg.pointCount,
    };
    this.worker.postMessage({
      zoomInfo: {
        zoomStart: seg.startIndex,
        zoomEnd: seg.startIndex + seg.pointCount,
        zoomLen: seg.pointCount,
      },
    });
  };

  checkMarkerOnParamChange(d) {}

  /**
   *
   * @param {Array<{segmentIdx:Number,freqIdxs:{startFreqIdx:Number,stopFreqIdx:Number},azimuth:Number}>} signals
   */
  getSignalInfo = (signals) => {
    const { zoomInfo } = this.myStates;
    signals.forEach((item) => {
      item.id =
        item.guid ||
        item.Guid ||
        `${item.segmentIdx}-${item.freqIdxs[0]}-${item.freqIdxs[1]}`;
      const seg = this.#segmentsInfo[item.segmentIdx];
      const startInTotal = item.freqIdxs[0] + seg.startIndex;
      const indexWidth = item.freqIdxs[1] - item.freqIdxs[0] + 1;

      if (
        startInTotal >= zoomInfo.startIndex &&
        startInTotal < zoomInfo.endIndex
      ) {
        item.left =
          ((startInTotal - zoomInfo.startIndex) * 100) / zoomInfo.zoomLen;
        item.width = (indexWidth * 100) / zoomInfo.zoomLen;
      } else {
        // 不显示
        item.left = -1;
      }
    });
  };

  clear() {
    // this.scanDFDataHelper.clear();
  }
}

export default ScanChartHelper;
