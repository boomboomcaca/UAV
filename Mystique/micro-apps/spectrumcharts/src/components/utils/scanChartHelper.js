import SpectrumChartHelper from "./spectrumChartHelper.js";
import { EventTypes } from "../utils/enums.js";

/**
 * @class
 */
class ScanChartHelper extends SpectrumChartHelper {
  /**
   * @private
   * @type {Array<{startFrequency:Number,stopFrequency:Number,stepFrequency:Number,startIndex:Number,pointCount:Number}>}
   */
  #segmentsInfo;
  /**
   * 离散扫描频段
   * @type {Array<Number>}
   */
  #frequencyList;

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
  updateSegments(segments, frequencyList = null) {
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

    this.#frequencyList = frequencyList;
    this.#segmentsInfo = segments;
    this.worker.postMessage({ segments });

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
    if (!this.worker) return;
    const { type, segmentOffset, offset, timestamp } = d;
    const dataChangeEvents = this.callbacks[EventTypes.DataChange];
    if (!dataChangeEvents) return;

    if (type === "scan") {
      this.worker.postMessage({ scan: d }, [d.data.buffer]);

      if (segmentOffset === 0 && offset === 0) {
        this.timestamps.push(timestamp);
        if (this.timestamps.length > this.totalFrame)
          this.timestamps = this.timestamps.slice(1);
      }
      // }
    } else if (type === "dfscan") {
      this.worker.postMessage({ dfScan: d });
    } else if (type === "signalsList") {
      // 信号背景
      this.getSignalInfo(d.signals);
      this.myStates.prevSignals = d.signals;
      dataChangeEvents.forEach((e) => e({ signals: d.signals }));
    } else if (type === "occupancy") {
      // 占用度
      this.worker.postMessage({ occ: d }, [d.data.buffer]);
    } else {
      // TODO 其它数据处理
    }
  };

  /**
   *
   * @param {{segmentOffset:Number,data:Array<Number>}} data
   */
  setCustomThreshold(data) {
    this.worker.postMessage({ threshold: data });
  }

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
    const { zoomInfo } = this.myStates;
    const { zoomLen, startIndex } = zoomInfo;
    if (this.#frequencyList) {
      return this.#frequencyList[dataXIndex + startIndex];
    }
    const scale = dataXIndex / this.prevSpecData.data.length;

    const scaleDataLen = Math.round(zoomLen * scale);
    let freq = -1;
    let segmentIndex = -1;
    for (let i = 0; i < this.#segmentsInfo.length; i += 1) {
      const seg = this.#segmentsInfo[i];
      if (seg.startIndex + seg.pointCount > startIndex + scaleDataLen) {
        const segPointIndex = startIndex + scaleDataLen - seg.startIndex;
        freq = seg.startFrequency + (seg.stepFrequency * segPointIndex) / 1000;
        segmentIndex = i;
        break;
      }
    }

    return { freq, segmentIndex };
  }

  getFrequencyOnNoData(pixelXScale) {
    if (this.#frequencyList) {
      const xIndex = Math.floor(pixelXScale * this.#frequencyList.length);
      return this.#frequencyList[xIndex];
    }
    for (let i = 0; i < this.#segmentsInfo.length; i += 1) {
      const seg = this.#segmentsInfo[i];
      const startScale = seg.startIndex / this.totalPoints;
      const endScale = (seg.startIndex + seg.pointCount) / this.totalPoints;
      if (pixelXScale >= startScale && pixelXScale <= endScale) {
        const segScale = endScale - startScale;
        const offsetScale = (pixelXScale - startScale) / segScale;
        const span = seg.stopFrequency - seg.startFrequency;
        const segPointIndex = Math.round(
          (span * offsetScale * 1000) / seg.stepFrequency
        );

        return seg.startFrequency + (segPointIndex * seg.stepFrequency) / 1000;
      }
    }
    return NaN;
  }

  getRenderXIndexByFrequency(frequency, segmentIndex) {
    if (this.#frequencyList) {
      const index = this.#frequencyList.indexOf(frequency);
      const { zoomInfo } = this.myStates;
      const { startIndex, zoomLen } = zoomInfo;
      if (index >= startIndex) {
        const renderDataXIndex = index - startIndex;
        const freqScale = renderDataXIndex / zoomLen;
        return { freqScale, renderDataXIndex };
      }
      return null;
    }

    let pointSum = 0;
    for (let i = 0; i <= segmentIndex; i += 1) {
      const seg = this.#segmentsInfo[i];
      if (i === segmentIndex) {
        const offset = frequency - seg.startFrequency;
        const pointCount = Math.round((offset * 1000) / seg.stepFrequency) + 1;
        pointSum += pointCount;
      } else {
        const pointCount =
          Math.round(
            ((seg.stopFrequency - seg.startFrequency) * 1000) /
              seg.stepFrequency
          ) + 1;
        pointSum += pointCount;
      }
    }
    const freqScale = pointSum / this.totalPoints;
    const renderDataLen = this.prevSpecData.data.length;
    const { zoomInfo } = this.myStates;
    const scaleOfRender =
      (freqScale - zoomInfo.start) / (zoomInfo.end - zoomInfo.start);
    // 4.2 根据4.1获得mk所在显示数据的索引
    let renderDataXIndex = Math.round(renderDataLen * scaleOfRender);

    return { freqScale, renderDataXIndex };

    // for (let i = 0; i < this.#segmentsInfo.length; i += 1) {
    //   const seg = this.#segmentsInfo[i];
    //   if (frequency >= seg.startFrequency && frequency <= seg.stopFrequency) {
    //     const freqScale = pointSum / this.totalPoints;
    //     const renderDataLen = this.prevSpecData.data.length;
    //     const { zoomInfo } = this.myStates;
    //     const scaleOfRender =
    //       (freqScale - zoomInfo.start) / (zoomInfo.end - zoomInfo.start);
    //     // 4.2 根据4.1获得mk所在显示数据的索引
    //     let renderDataXIndex = Math.round(renderDataLen * scaleOfRender);

    //     return { freqScale, renderDataXIndex };
    //   } else {
    //   }
    //   pointSum += seg.pointCount;
    // }

    // return null;
  }

  /**
   *
   * @param {{id:String,frequency:Number,index:Number}} param0
   */
  setMarkerFrequency({ id, frequency, index }) {
    if (id && this.prevSpecData) {
      const marker = this.myStates.markers.find((mk) => mk && mk.id === id);

      if (marker) {
        if (this.#frequencyList) {
          marker.freqMHz = this.#frequencyList[index];
        } else {
          marker.freqMHz = frequency;
        }
        this.updateMarkerInfo();
        this.fireMarkerChange();
        return true;
      }
    }
    return false;
  }

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
    const zoomChangeEvents = this.callbacks[EventTypes.ZoomChange];
    if (zoomChangeEvents) {
      zoomChangeEvents.forEach((e) => e(this.myStates.zoomInfo));
    }
  };

  /**
   *
   * @param {Array<{segmentIdx:Number,freqIdxs:{startFreqIdx:Number,stopFreqIdx:Number},azimuth:Number}>} signals
   */
  getSignalInfo = (signals) => {
    const { zoomInfo } = this.myStates;
    signals.forEach((item) => {
      item.id =
        item.guid ||
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

  getScanSource(callback) {
    this.getSource(callback);
    // if (!this.getSourceCallBack) {
    //   this.getSourceCallBack = callback;
    //   this.worker.postMessage({
    //     getSource: "get max avg",
    //   });
    // }
  }

  clear() {
    // this.scanDFDataHelper.clear();
    super.clear();
  }
}

export default ScanChartHelper;
