import { ChartTypes } from "../utils/enums.js";
import ScanDataHelper from "../utils/scanDataHelper.js";

/**
 * @class
 */
class SpectrumHelper {
  /**
   * @param {ScanDataHelper}
   */
  scanDataHelper;

  /**
   * @type {Function<Number>}
   */
  onThresholdChange;

  #prevSpecData;

  /**
   * @type {ZoomInfo}
   */
  zoomInfo = { start: 0, end: 1, startIndex: 0, endIndex: 1, zoomLen: 1 };
  #mouseDataIndex = {
    dataXIndex: -1,
    dataYIndex: -1,
    onChart: ChartTypes.spectrum,
  };
  maximumY = 80;
  minimumY = -20;
  #states = {
    onData: (e) => {},
    onMarkerChange: (e) => {},
    onZoomChange: (e) => {},
  };

  #chartArgs;
  /**
   * @type {Array<Marker>}
   */
  markers = [];

  /**
   * 频段信息
   * @type {Arrary<{startFrequency: Number,stopFrequency: Number,stepFrequency: Number}>}
   */
  #segmentsInfo = [];

  /**
   *
   * @param {{onData:Function<{}>,onMarkerChange:Function<Array<Marker>>,onZoomChange:Function<ZoomInfo>,onResize:Function}} props
   */
  constructor(props) {
    this.#states = { ...this.#states, ...props };
  }

  /**
   * 更新频段信息， 触发 onSegmentsChange
   * @param {Arrary<{startFrequency: Number,stopFrequency: Number,stepFrequency: Number}>} segments
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

    // this.#totalPoints = totalPoints;
    this.#segmentsInfo = segments;
    this.scanDataHelper = new ScanDataHelper({
      segments,
      totalPoint: totalPoints,
      outRain: false,
    });
    this.#resetZoom(totalPoints);
    return totalPoints;
  }

  updateChartArgs = (args) => {
    this.#chartArgs = args;
  };

  #resetZoom = (totalLen) => {
    this.zoomInfo = {
      start: 0,
      end: 1,
      startIndex: 0,
      endIndex: totalLen - 1,
      zoomLen: totalLen,
    };
  };

  resize = () => {
    // conso
    const { onResize } = this.#states;
    if (onResize) {
      onResize();
    }
  };

  /**
   *
   * @param {*} d
   */
  setData = (d) => {
    const { data, timestamp, type } = d;

    const { onData, onMarkerChange } = this.#states;
    if (type === "scan") {
      // ---$$$$$$ 参考到 0
      let avg = 0;
      let count = 0;
      for (let i = 0; i < data.length; i += 1) {
        const val = data[i];
        if (val > -9999) {
          avg += val;
          count += 1;
        }
      }
      const gap = 0 - avg / count;
      const normalized = data.map((d) => d + gap);
      let renderData = normalized;
      d.data = normalized;
      // +++$$$$$$ 参考到 0
      if (this.scanDataHelper) {
        // 处理scan数据
        const scanData = this.scanDataHelper.setData(d);
        if (!scanData) return;
        renderData = scanData.data;
        this.#prevSpecData = renderData;
        onData({
          spectrum: renderData,
        });
        // 更新marker位置
        this.markers.forEach((mk) => {
          const { dataXIndex, dataYIndex } = mk;
          const { startIndex, endIndex } = this.zoomInfo;
          if (dataXIndex >= startIndex && dataYIndex <= endIndex)
            mk.position = this.#getPositionByDataIndex();
          else mk.visible = false;
        });
        onMarkerChange(this.markers.slice(0));
      }
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
    const dataLen = this.#prevSpecData.length;
    const zoomStartPct = start / width;
    const zoomEndPct = end / width;
    const startIndex = Math.floor(dataLen * zoomStartPct);
    const endIndex = Math.round(dataLen * zoomEndPct) - 1;
    const zoomLen = endIndex - startIndex + 1;
    this.zoomInfo = {
      start: zoomStartPct,
      end: zoomEndPct,
      startIndex: startIndex,
      endIndex: endIndex,
      zoomLen: zoomLen,
    };
    const { onZoomChange } = this.#states;
    onZoomChange(this.zoomInfo);
  };

  zoomToSegment = (segmentIndex) => {
    console.log("zoomToSegment::::");
    const seg = this.#segmentsInfo[segmentIndex];
    this.zoomInfo = {
      start: seg.startIndex / this.scanDataHelper.totalPoint,
      end: (seg.startIndex + seg.pointCount) / this.scanDataHelper.totalPoint,
      startIndex: seg.startIndex,
      endIndex: seg.startIndex + seg.pointCount,
      zoomLen: seg.pointCount,
    };
    const { onZoomChange } = this.#states;
    onZoomChange(this.zoomInfo);
  };

  resetZoom = () => {
    const totalLen = this.scanDataHelper.totalPoint;
    this.zoomInfo = {
      start: 0,
      end: 1,
      startIndex: 0,
      endIndex: totalLen - 1,
      zoomLen: totalLen,
    };
    const { onZoomChange } = this.#states;
    onZoomChange(this.zoomInfo);
  };

  /**
   * x 水平方向位置%
   * y 垂直方向位置%
   * px 水平方向位置 px
   * py 垂直方向位置 py
   * @param {{x:Number,y:Number,px:Number,py:Number}} mouseArgs
   */
  updateMouseDataIndex = (args) => {
    const { x, width, px } = args;
    const { zoomLen, startIndex } = this.zoomInfo;
    const dataXIndex = Math.round((zoomLen - 1) * x + startIndex);
    let dataYIndex = -1;
    let onChart = ChartTypes.spectrum;
    this.#mouseDataIndex = { dataXIndex, dataYIndex, onChart };
    if (this.markers.length > 0) {
      this.markers.forEach((mk) => {
        // 判断哪个marker在鼠标内
        const { position, inRain } = mk;
        if (!inRain) {
          const centerPx = (position.x * width) / 100;
          mk.mouseInCenter = px >= centerPx - 3 && px <= centerPx + 1;
        }
      });
      // onMarkerChange(this.markers.slice(0));
    }
  };

  /**
   * 游标位置变化时调用
   * 用于更新游标信息
   * @returns
   */
  moveCursor = () => {
    if (!this.#prevSpecData) return undefined;
    const { dataXIndex, onChart } = this.#mouseDataIndex;
    let level = this.#prevSpecData?.[dataXIndex];
    const pos = this.#getPositionByDataIndex();

    let frequency = undefined;
    const seg = this.#segmentsInfo.find(
      (s) => s.startIndex + s.pointCount >= dataXIndex
    );
    if (seg) {
      frequency =
        seg.startFrequency +
        (seg.stepFrequency * (dataXIndex - seg.startIndex)) / 1000;
    }
    return {
      cursorPosX: pos.x,
      cursorPosY: pos.y,
      level,
      frequency,
      onChart,
    };
  };

  getXDataIndex = (pctPos, useCursor = false) => {
    const { zoomInfo, showCursor, cursorXDataIndex } = state;
    let dataXIndex = -1;
    if (showCursor && useCursor) {
      dataXIndex = cursorXDataIndex;
    } else {
      dataXIndex = Math.round(
        (zoomInfo.zoomLen - 1) * pctPos.x + zoomInfo.startIndex
      );
    }
    return dataXIndex;
  };

  #getPositionByDataIndex = () => {
    const { dataXIndex, dataYIndex, onChart } = this.#mouseDataIndex;
    const gap = 100.0;
    let y = 0;

    const axisYRange = this.maximumY - this.minimumY;
    const data = this.#prevSpecData?.[dataXIndex];
    y = ((this.maximumY - data) * gap) / axisYRange;

    if (y < 0) y = 0;
    if (y > 100) y = 100;
    const xPctGap = 100 / this.zoomInfo.zoomLen;
    const x = (dataXIndex - this.zoomInfo.startIndex + 0.5) * xPctGap;
    return { x, y };
  };

  /**
   *
   */
  addMarker = () => {
    const { dataXIndex, dataYIndex } = this.#mouseDataIndex;
    let mk = {
      id: new Date().getTime() + Math.random().toString(6),
      dataXIndex,
      dataYIndex,
      position: this.#getPositionByDataIndex(),
      selected: false,
      visible: true,
      inRain: dataYIndex > -1,
      mouseInCenter: false,
      hover: false,
    };
    this.markers.push(mk);
    const { onMarkerChange } = this.#states;
    onMarkerChange(this.markers.slice(0));
  };

  dragMarker = (args) => {
    const { dragMarker } = args;
    const { dataXIndex, dataYIndex } = this.#mouseDataIndex;
    const { onMarkerChange } = this.#states;
    const pos = this.#getPositionByDataIndex();
    const mk = this.markers.find((m) => m.id === dragMarker);
    mk.dataXIndex = dataXIndex;
    mk.dataYIndex = dataYIndex;
    mk.position = pos;
    onMarkerChange(this.markers.slice(0));
  };

  firThresholdChange = (thr) => {
    if (this.onThresholdChange) {
      const axisYRange = this.maximumY - this.minimumY;
      const offset = axisYRange * thr;
      this.onThresholdChange(this.maximumY - offset);
    }
  };
}

export default SpectrumHelper;
