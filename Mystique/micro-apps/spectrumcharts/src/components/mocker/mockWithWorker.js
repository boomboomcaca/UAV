/**
 * 用于创建线程方法
 */
const workerThread = () => {
  function sineGenerator(outInterval, onDataGenerated) {
    this.rvariables = {
      amplitude: 45, // Y最大参考值
      outPointCount: 1601,
      sineMargin: 800,
      amplitudeOffset: 8,
      onDataGenerated: () => {}, // 数据生成回调
      tmr: undefined,
    };

    this.rvariables.onDataGenerated = onDataGenerated;
    if (outInterval && this.rvariables.onDataGenerated) {
      setTimeout(() => {
        this.rvariables.tmr = setInterval(() => {
          // 生产数据
          const datas = this.generate();
          this.rvariables.onDataGenerated(datas);
        }, outInterval);
      }, 1000);
    }
  }

  sineGenerator.prototype.setOption = function (options) {
    // amplitude,
    // outPointCount,
    // sineMargin,
    // amplitudeOffset
    if (options.amplitude !== undefined) {
      this.rvariables.amplitude = options.amplitude;
    }
    if (options.outPointCount !== undefined) {
      this.rvariables.outPointCount = options.outPointCount;
    }
    if (options.sineMargin !== undefined) {
      this.rvariables.sineMargin = options.outPosineMarginintCount;
    }
    if (options.amplitudeOffset !== undefined) {
      this.rvariables.amplitudeOffset = options.amplitudeOffset;
    }
  };

  sineGenerator.prototype.generate = function () {
    // console.log("generate dataF");
    const ampOffsetMin = 0 - this.rvariables.amplitudeOffset;
    const ampOffsetRange = this.rvariables.amplitudeOffset * 2;
    const waveMarginOffsetMmin = 0 - this.rvariables.sineMargin;
    const waveMarginOffsetRange = this.rvariables.sineMargin * 2;
    const count = this.rvariables.outPointCount;
    const waveMargin =
      this.rvariables.sineMargin +
      waveMarginOffsetMmin +
      Math.floor(Math.random() * waveMarginOffsetRange);
    const radGap = 179.99 / (count - waveMargin * 2);
    const max =
      this.rvariables.amplitude +
      ampOffsetMin +
      Math.floor(Math.random() * ampOffsetRange);
    const datas = [];
    for (let i = 0; i < count; i++) {
      const mk = ampOffsetMin + Math.floor(Math.random() * ampOffsetRange);
      if (i < waveMargin || i > count - waveMargin) {
        // 2022-8-31 liujian * 10
        datas[i] = Math.round(mk) * 10;
      } else {
        const angle = (i - waveMargin) * radGap;
        const scale = Math.sin((angle * Math.PI) / 180);
        const d = max * scale;
        const offset = mk * (1 - scale);
        // 2022-8-31 liujian * 10
        datas[i] = Math.round(d + offset) * 10;
      }
    }
    return datas;
  };

  sineGenerator.prototype.setConfig = function (options) {
    // outPointCount
    // sineMargin
    // amplitude
    // onlyOverZero
    // amplitudeOffset
    if (options.outPointCount !== undefined) {
      this.rvariables.outPointCount = options.outPointCount;
    }
    if (options.sineMargin !== undefined) {
      this.rvariables.sineMargin = options.sineMargin;
    }
    if (options.amplitude !== undefined) {
      this.rvariables.amplitude = options.amplitude;
    }
    if (options.onlyOverZero !== undefined) {
      this.rvariables.onlyOverZero = options.onlyOverZero;
    }
    if (options.amplitudeOffset !== undefined) {
      this.rvariables.amplitudeOffset = options.amplitudeOffset;
    }
    if (this.rvariables.sineMargin >= this.rvariables.outPointCount / 2) {
      this.rvariables.sineMargin = this.rvariables.outPointCount / 3;
    }
  };

  sineGenerator.prototype.dispose = function () {
    if (this.rvariables.tmr) {
      clearInterval(this.rvariables.tmr);
    }
  };

  /**
   *
   * @param {Function} onDataGenerated
   * @param {boolean} split
   * @param {Number} splitPoints
   */
  function scanGenerator(outPointCount) {
    this.singalSineCount = 601; // 但信号波形占据的品点数
    // this.split = split;
    // let onePack = Math.floor(outPointCount / 3);
    // if (onePack % 2 === 0) {
    //   onePack += 1;
    // }
    // this.splitPoints = onePack;
    // this.onDataGenerated = onDataGenerated;
    this.outPointCount = outPointCount;
    this.amplitude = 45; // Y最大参考值
    this.amplitudeOffset = 8;
    this.sineGenerator = new sineGenerator();
    // 信号
    let sineCount = Math.floor(this.outPointCount / this.singalSineCount);
    if (this.outPointCount % this.singalSineCount > 0) sineCount += 1;
    let outSignalCount = 2; // this.outPointCount / 400;
    if (outSignalCount >= sineCount) outSignalCount = sineCount - 1;
    const signalIndexes = [];

    for (let i = 0; i < outSignalCount; i++) {
      let indx = -1;
      while (indx === -1 || signalIndexes.includes(indx)) {
        indx = Math.floor(Math.random() * sineCount);
      }
      signalIndexes.push(indx);
    }
    this.signalIndexes = signalIndexes;
  }

  scanGenerator.prototype.generate = function () {
    const ampOffsetMin = 0 - this.amplitudeOffset;
    const ampOffsetRange = this.amplitudeOffset * 2;
    const count = this.outPointCount;
    const datas = [];
    let dataStart = 0; // 数据生成位置
    let sineStart = 0; // 第几个波
    while (dataStart < count) {
      let needLen = this.singalSineCount;
      if (dataStart + needLen >= count) {
        needLen = count - dataStart;
      }
      if (this.signalIndexes.includes(sineStart)) {
        // 生成不同宽度的信号
        const margin = 6 + Math.floor(Math.random() * (needLen / 2));
        this.sineGenerator.setConfig({
          sineMargin: margin,
          outPointCount: needLen,
        });
        const oneSine = this.sineGenerator.generate();
        datas.push(...oneSine);
      } else {
        for (let i = dataStart; i < dataStart + needLen; i++) {
          datas[i] = (ampOffsetMin + Math.random() * ampOffsetRange) * 10;
        }
      }

      dataStart += needLen;
      sineStart += 1;
    }
    return datas;
  };

  /**
   *
   * @param {Array} spectrum
   * @param {Number} outMax
   * @returns {Array}
   */
  const combineData = (spectrum, outMax) => {
    if (spectrum.length > outMax) {
      const res = [];
      let freqIndex = 0;
      const perPointGap = spectrum.length / (outMax - 1);
      let freqN = freqIndex + perPointGap / 2;
      for (let j = 0; j < outMax; j += 1) {
        let maxValue = spectrum[freqIndex];
        const k = Math.round(freqN);
        for (; freqIndex < k; freqIndex += 1) {
          if (maxValue < spectrum[freqIndex]) {
            maxValue = spectrum[freqIndex];
          }
        }
        res[j] = maxValue;
        freqN += perPointGap;
        if (freqN > spectrum.length) freqN = spectrum.length;
      }
      return res;
    }
    return spectrum;
  };

  /**
   *
   * @param {{Array<{segmentIndex:Number,startFrequency:Number,stopFrequency:Number}>}} segments
   * @param {Number} frameInterval
   * @param {Function} onData
   */
  function scanMocker(segments, frameInterval, onData) {
    this.extractMax = [200001, 200001, 200001, 200001, 200001];
    this.zoomInfo = {
      segmentIndex: 0,
      startIndex: 0,
      endIndex: 2000,
    };
    let totalSplitTimes = 0;
    // 计算点数
    for (let i = 0; i < segments.length; i += 1) {
      const seg = segments[i];
      const { startFrequency, stopFrequency, stepFrequency } = seg;
      const pointCount =
        Math.round(((stopFrequency - startFrequency) * 1000) / stepFrequency) +
        1;
      seg.pointCount = pointCount;
      // 构造数据构造器
      seg.generator = new scanGenerator(pointCount);
      // 计算单包发送量&分包次数
      let onePack = Math.floor(pointCount / 2);
      if (onePack % 2 === 0) {
        onePack += 1;
      }
      seg.onePack = onePack;
      const splitTime = Math.ceil(pointCount / onePack);
      seg.splitTime = splitTime;
      totalSplitTimes += splitTime;
    }
    this.segments = segments;
    let prevOver = true;
    const splitInterval = Math.ceil(frameInterval / totalSplitTimes);
    let prevFrameTime = new Date().getTime();
    const dataSender = () => {
      const dt = new Date().getTime();
      if (dt - prevFrameTime < frameInterval) {
        requestAnimationFrame(() => dataSender());
        return;
      }
      prevFrameTime = dt;
      requestAnimationFrame(() => dataSender());
      if (!prevOver) return;
      prevOver = false;
      // 1. 构造所有频段的数据&分发
      const segsData = [];
      this.segments.forEach((s, index) => {
        const segData = s.generator.generate();
        for (let i = 0; i < s.splitTime; i += 1) {
          segsData.push({
            type: "scan",
            segmentOffset: index,
            startFrequency: s.startFrequency,
            stopFrequency: s.stopFrequency,
            stepFrequency: s.stepFrequency,
            offset: i * s.onePack,
            total: s.pointCount,
            data: segData.slice(i * s.onePack, (i + 1) * s.onePack),
          });
        }
      });
      const sendOneSplit = () => {
        const oneSplitData = segsData.shift();
        // 发射
        if (onData) {
          onData(oneSplitData);
        }
        if (segsData.length === 0) {
          prevOver = true;
          return;
        }
        setTimeout(() => {
          sendOneSplit();
        }, splitInterval);
      };
      sendOneSplit();
    };

    dataSender();
  }

  /**
   *
   * @param {{Array<{segmentIndex:Number,startFrequency:Number,stopFrequency:Number}>}} segments
   * @param {Number} frameInterval
   * @param {Function} onData
   */
  function scanMocker1(segments, frameInterval, onData) {
    this.extractMax = [200001, 200001, 200001, 200001, 200001];
    this.zoomInfo = {
      segmentIndex: 0,
      startIndex: 0,
      endIndex: 2000,
    };
    // 计算点数
    for (let i = 0; i < segments.length; i += 1) {
      const seg = segments[i];
      const { startFrequency, stopFrequency, stepFrequency } = seg;
      const pointCount =
        Math.round(((stopFrequency - startFrequency) * 1000) / stepFrequency) +
        1;
      seg.pointCount = pointCount;
      // 构造数据构造器
      seg.generator = new scanGenerator(pointCount);
      seg.onePack = pointCount;
      seg.splitTime = 1;
    }
    this.segments = segments;

    if (!onData) return;
    // 1. 构造所有频段的数据&分发
    const dataList = [];
    for (let i = 0; i < 1000; i++) {
      const segData = this.generate();
      dataList.push(segData);
    }
    console.log("data created:::", dataList.length);
    const dt1 = new Date().getTime();
    let count = -1;
    this.tmr = setInterval(() => {
      count++;
      if (count >= dataList.length) {
        clearInterval(this.tmr);
        const dt2 = new Date().getTime();
        console.log("send over:::", dt2 - dt1);
        return;
      }

      const segsData = dataList[count];
      const oneSplitData = segsData[0];
      // 发射
      if (onData) {
        onData(oneSplitData);
      }
    }, frameInterval);
  }

  scanMocker1.prototype.generate = function () {
    const segsData = [];
    this.segments.forEach((s, index) => {
      const segData = s.generator.generate();
      for (let i = 0; i < s.splitTime; i += 1) {
        segsData.push({
          type: "scan",
          segmentOffset: index,
          startFrequency: s.startFrequency,
          stopFrequency: s.stopFrequency,
          stepFrequency: s.stepFrequency,
          offset: i * s.onePack,
          total: s.pointCount,
          data: segData.slice(i * s.onePack, (i + 1) * s.onePack),
        });
      }
    });

    return segsData;
  };

  /**
   *
   * @param {Array<Number>} outMax
   */
  scanMocker.prototype.setOutMax = function (outMax) {
    this.extractMax = outMax;
  };

  /**
   *
   * @param {{segmentIndex:Number,startFrequency:Number,stopFrequency:Number}} options
   */
  scanMocker.prototype.zoom = function (options) {
    // 计算索引位置
    const seg = this.segments[options.segmentIndex];
    const startIndex = Math.ceil(
      ((options.startFrequency - seg.startFrequency) * 1000) / seg.stepFrequency
    );
    const endIndex = Math.floor(
      ((options.stopFrequency - seg.startFrequency) * 1000) / seg.stepFrequency
    );
    this.zoomInfo = {
      segmentIndex: options.segmentIndex,
      startIndex,
      endIndex,
    };
  };

  scanMocker.prototype.dispose = function () {
    if (this.tmr) {
      clearInterval(this.tmr);
      this.tmr = undefined;
    }
  };

  scanMocker1.prototype.dispose = function () {
    if (this.tmr) {
      clearInterval(this.tmr);
      this.tmr = undefined;
    }
  };

  // 模拟测试
  let demoMocker;
  // let mocker;
  /**
   *
   * @param {{frame:Number, type:String,segments:Array<any>,performance:boolean,typedArray:boolean,getMode:boolean}} options
   * @param {Function} onData
   * @returns {scanMocker | sineGenerator}
   */
  function demoData(options, onData) {
    const { frame, type, performance, typedArray, getMode } = options;
    if (type === "spectrum") {
      const interval = Math.round(1000 / frame);
      if (getMode) {
        demoMocker = new sineGenerator(interval, null);
      } else {
        demoMocker = new sineGenerator(interval, (d) => {
          if (onData) {
            // const dd = d.map((d) => d + 10);
            onData({
              type: "spectrum",
              frequency: 101.7,
              span: 200,
              data: typedArray ? new Float32Array(d) : d,
            });
          }
        });
      }
      // 每5s 变化一次数据长度
      // setTimeout(() => {
      //   let pointCount = Math.floor(1000 + Math.random() * 1000);
      //   pointCount = pointCount % 2 === 0 ? pointCount + 1 : pointCount;
      //   if (mocker) {
      //     mocker.setOption({ outPointCount: pointCount });
      //   }
      // }, 5000);
    }
    if (type === "scan") {
      console.log("init scan mocker:::", options);
      if (performance) {
        if (getMode) {
          demoMocker = new scanMocker1(
            options.segments,
            Math.round(950 / frame),
            null
          );
        } else {
          demoMocker = new scanMocker1(
            options.segments,
            Math.round(950 / frame),
            (d) => {
              if (onData) {
                if (typedArray) {
                  d.data = new Float32Array(d.data);
                }
                onData(d);
              }
            }
          );
        }
      } else {
        demoMocker = new scanMocker(
          options.segments,
          Math.round(950 / frame),
          (d) => {
            if (onData) {
              if (typedArray) {
                d.data = new Float32Array(d.data);
              }
              onData(d);
            }
          }
        );
      }
    }
    return demoMocker;
  }

  function getFrame() {
    if (demoMocker) {
      if (demoMocker instanceof sineGenerator) {
        const d = demoMocker.generate();
        return {
          type: "spectrum",
          frequency: 101.7,
          span: 200,
          data: typedArray ? new Float32Array(d) : d,
        };
      } else {
        return demoMocker.generate()[0];
      }
    }
    return null;
  }

  addEventListener("message", (e) => {
    if (e.data.action === "init") {
      demoMocker = demoData(e.data.data, (e) => {
        postMessage(e);
      });
    }
    if (e.data.action === "getFrame") {
      postMessage(getFrame());
    }
    if (e.data.action === "dispose") {
      demoMocker.dispose();
      demoMocker = null;
    }
  });
};
// let workerBlob;
// /**
//  * @type {Worker}
//  */
// let worker;
/**
 *
 * @param {{frame:Number, type:String,segments:Array<any>,performance:boolean,typedArray:boolean,getMode:boolean}} options
 * @param {Function} onData
 */
function demoData(options, onData) {
  // 初始化线程
  const workerJs = `(${workerThread.toString()})()`;
  this.workerBlob = new Blob([workerJs], { type: "text/javascript" });
  const blogUrl = window.URL.createObjectURL(this.workerBlob);
  this.worker = new Worker(blogUrl, {
    name: `mockworker-${Number(Math.random().toString().substring(2)).toString(
      36
    )}`,
  });
  this.worker.onmessage = (e) => {
    onData(e.data);
  };
  this.worker.postMessage({ action: "init", data: options });
}

demoData.prototype.getFrame = function () {
  this.worker.postMessage({ action: "getFrame", data: null });
};

demoData.prototype.dispose = function () {
  if (this.worker) {
    this.worker.postMessage({ action: "dispose" });
    this.worker.terminate();
    this.worker = null;
  }
  window.URL.revokeObjectURL(this.workerBlob);
};

export default demoData;
