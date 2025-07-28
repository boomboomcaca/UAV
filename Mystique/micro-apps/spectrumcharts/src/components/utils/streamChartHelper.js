import SpectrumChartHelper from "./spectrumChartHelper.js";
import { EventTypes } from "../utils/enums.js";

/**
 * @class
 */
class StreamChartHelper extends SpectrumChartHelper {
  #timestamps;
  /**
   * @type {Array<Number> | undefined}
   */
  #levelData;
  #prevDistribution;
  #prevCalTime = new Date().getTime();

  /**
   * 2s 产生一个点，最多4h,7200个点
   * 数据范围dBμV -14-86
   * 数据归一化到 0%-50%
   * @type {Array<Number> | undefined}
   */
  #rssiData = [];
  #prevRssiTime = new Date().getTime();
  #maxRssi = -9999;
  #onRssiData;

  /**
   * @constructor
   * @param {{distributionBar:boolean}} props
   */
  constructor({ distributionBar }) {
    super({ gpuRain: false });
    this.distributeBar = distributionBar;
  }

  /**
   *
   * @param {*} d
   */
  setData = (d) => {
    const { data, timestamp, type } = d;
    const dataChangeEvents = this.callbacks[EventTypes.DataChange];
    if (!dataChangeEvents) return;
    if (type === "level") {
      let ts = 0;
      const time = this.chartArgs.streamTime * 1e3;
      if (this.#timestamps) {
        if (timestamp - this.#timestamps[0] > time) {
          ts = time;
          let startIndex = 1;
          for (let i = 1; i < this.#timestamps.length; i += 1) {
            if (timestamp - this.#timestamps[0] <= time) {
              startIndex = i;
              break;
            }
          }
          this.#timestamps = this.#timestamps.slice(startIndex);
          this.#levelData = this.#levelData.slice(startIndex);
        } else {
          ts = timestamp - this.#timestamps[0];
        }
        this.#timestamps.push(timestamp);
        this.#levelData.push(data);
      } else {
        this.#timestamps = [timestamp];
        this.#levelData = [data];
      }

      // RSSI
      if (data > this.#maxRssi) {
        this.#maxRssi = data;
      }
      if (timestamp - this.#prevRssiTime > 1999) {
        this.#rssiData.push(this.#maxRssi);
        this.#maxRssi = -9999;
        this.#prevRssiTime = timestamp;
        const rssiPoints = (this.chartArgs.rssiTime || 7200) / 2;
        if (this.#rssiData.length > rssiPoints) {
          this.#rssiData = this.#rssiData.slice(1);
        }
        if (
          this.#rssiData.length > 9 &&
          !this.myStates.pause &&
          this.#onRssiData
        ) {
          this.#onRssiData(this.#rssiData);
        }
      }

      if (!this.myStates.pause) {
        if (this.distributeBar) {
          const dtNow = new Date().getTime();
          if (dtNow - this.#prevCalTime > 999) {
            this.#prevCalTime = dtNow;
            // 统计电平分布
            const { minimumY, axisYTickGap } = this.myStates;

            const steps = [];
            for (let i = 0; i < 10; i += 1) {
              steps.push({ ste: minimumY + i * axisYTickGap, count: 0 });
            }

            const dataLen = this.#levelData.length;
            for (let i = 0; i < dataLen; i++) {
              const d = this.#levelData[i];
              for (let j = 0; j < 10; j++) {
                const step = steps[j];
                if (d < step.ste) {
                  step.count++;
                  break;
                }
              }
            }
            this.#prevDistribution = steps.map((s) => s.count);
          }
        }

        dataChangeEvents.forEach((e) =>
          e({
            spectrum: {
              data: this.#levelData,
              distBar: this.#prevDistribution,
            },
            streamTimeSpan: ts,
          })
        );
      }
    }

    // 更新marker位置
    //     const dt = new Date().getTime();
    //     if (dt - this.myStates.prevDataTime > 99) {
    //       if (isMobile() && this.chartArgs.showCursor) this.moveCursor();
    //       this.updateMarkerOnData();
    //       this.myStates.prevDataTime = dt;
    //     }
  };

  onRssiData(callback) {
    this.#onRssiData = callback;
  }

  clear() {
    this.#timestamps = null;
    this.#rssiData = [];
    this.#prevRssiTime = new Date().getTime();
    this.#maxRssi = -9999;
  }
}

export default StreamChartHelper;
