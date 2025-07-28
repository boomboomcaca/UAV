class ScanDataHelper {
  totalPoint = 0;
  realData;
  maxData;
  avgData;
  minData;
  thrData;
  rainLine;
  #isPrevOver = false;
  #defaultLine;
  // 是否输出瀑布图数据
  #outRainLine = false;
  // 是否没有统计值
  #noStatics = false;
  // #lastSegmentDataLen = 0;
  #segments = [];
  #invalidValue = -9999;
  /**
   *
   * @param {{segments:Array<{pointCount:Number,startIndex:Number}>,totalPoint:Number,outRain:boolean,invalidValue:Number}} props
   */
  constructor(props) {
    const { segments, totalPoint, outRain, invalidValue, noStatics } = props;
    this.totalPoint = totalPoint;
    this.#segments = segments;
    this.#invalidValue = invalidValue !== undefined ? invalidValue : -9999;
    // this.#lastSegmentDataLen = segments[segments.length - 1].pointCount;
    this.#defaultLine = new Float32Array(this.totalPoint).fill(-9999);
    this.realData = this.#defaultLine.slice(0);
    if (!noStatics) {
      this.maxData = this.#defaultLine.slice(0);
      this.avgData = this.#defaultLine.slice(0);
      this.minData = this.#defaultLine.slice(0);
      this.thrData = this.#defaultLine.slice(0);
    }
    this.rainLine = this.realData.slice(0);
    this.#outRainLine = outRain;
    this.#noStatics = noStatics;
  }

  /**
   *
   * @param {{segmentOffset:Number, offset:Number,data:Array<Number>,max:Array<Number>,avg:Array<Number>,min:Array<Number>,thr:Array<Number>}} scanData
   * @param {Number} pixelWidth
   * @returns {{data:Array<Number>,isOver:Boolean}}
   */
  setData(scanData, pixelWidth) {
    // if (!this.#noStatics) {
    //   console.log("get datat:::", scanData);
    // }
    const newLine = this.#isPrevOver;
    if (newLine) {
      this.rainLine = this.#defaultLine.slice(0);
    }

    const { offset, data, segmentOffset, max, avg, min, threshold } = scanData;
    const seg = this.#segments[segmentOffset];
    if (!seg) return undefined;
    const temp = [];
    const temp1 = [];
    if (max) {
      temp.push(max);
      temp1.push(this.maxData);
    }
    if (avg) {
      temp.push(avg);
      temp1.push(this.avgData);
    }
    if (min) {
      temp.push(min);
      temp1.push(this.minData);
    }
    if (threshold) {
      temp.push(threshold);
      temp1.push(this.thrData);
    }

    const desStart = seg.startIndex + offset;
    const dataLen = data.length;
    const tempLen = temp.length;
    for (let i = 0; i < dataLen; i += 1) {
      const val = data[i];
      const index = i + desStart;
      // 无效值保留上一次值
      // if (val > this.#invalidValue) this.realData[index] = val;
      this.realData[index] = val;
      this.rainLine[index] = val;
      for (let t = 0; t < tempLen; t += 1) {
        temp1[t][index] = temp[t][i];
      }
    }
    this.#isPrevOver =
      segmentOffset === this.#segments.length - 1 &&
      offset + dataLen === seg.pointCount;
    return {
      data: this.realData,
      max: max ? this.maxData : undefined,
      avg: avg ? this.avgData : undefined,
      min: min ? this.minData : undefined,
      thr: threshold ? this.thrData : undefined,
      rainLine: this.#outRainLine ? this.rainLine.slice(0) : undefined,
      isOver: this.#isPrevOver,
      newLine,
    };
  }

  clear() {
    this.realData = this.#defaultLine.slice(0);
    if (!this.#noStatics) {
      this.maxData = this.#defaultLine.slice(0);
      this.avgData = this.#defaultLine.slice(0);
      this.minData = this.#defaultLine.slice(0);
      this.thrData = this.#defaultLine.slice(0);
    }
    this.rainLine = this.realData.slice(0);
  }
}

export default ScanDataHelper;
