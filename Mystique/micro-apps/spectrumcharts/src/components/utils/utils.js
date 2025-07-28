import dayjs from "dayjs";

export function isMobile() {
  // MD 不想搞了，就这样强制配置mobile
  if (window.chartInMobile) return true;
  let flag =
    /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(
      navigator.userAgent
    );
  return flag;
}

export function frequency2String(frequency) {
  if (frequency < 0.001) return `${Math.round(frequency * 1e6)}Hz`;
  if (frequency < 1) return `${(frequency * 1e3).toFixed(2)}kHz`;
  if (frequency < 1000) return `${frequency.toFixed(4)}MHz`;
  return `${(frequency / 1000).toFixed(4)}GHz`;
}

export function frequency2String1(frequency) {
  if (frequency < 0.001) return { no: Math.round(frequency * 1e6), unit: "Hz" };
  if (frequency < 1)
    return { no: Math.round(frequency * 1e6) / 1e3, unit: "kHz" };
  if (frequency < 1000)
    return { no: Math.round(frequency * 1e6) / 1e6, unit: "MHz" };
  return { no: Math.round(frequency * 1000) / 1e6, unit: "GHz" };
}

export function frequency2String2(frequency) {
  if (frequency < 0.001) return `${Math.round(frequency * 1e6)}Hz`;
  if (frequency < 1) return `${Math.round(frequency * 1e3)}kHz`;
  if (frequency < 1000) return `${frequency.toFixed(3)}MHz`;
  return `${(frequency / 1000).toFixed(3)}GHz`;
}

/**
 *
 * @param {Array<Number>} referData
 * @returns {{minimum:Number, maximum:Number}}
 */
export function autoAxisYRange(referData) {
  let max = referData[0];
  let min = referData[0];
  for (let i = 1; i < referData.length; i++) {
    const val = referData[1];
    if (val < -999) continue;
    if (val > max) max = val;
    else if (val < min) min = val;
  }
  if (min < -998 || max < -998) return null;

  const minFlag = Math.floor(min / 5) - 2;
  const minimum = minFlag * 5;
  const offset = max - minimum + 5;
  let maximum = -20;
  if (offset > 150) {
    maximum = minimum + 200;
  } else if (offset > 100) {
    maximum = minimum + 150;
  } else if (offset > 50) {
    maximum = minimum + 100;
  } else {
    maximum = minimum + 50;
  }

  return { minimum, maximum };
}

export function timestamp2String(timestamp) {
  if (!timestamp) return "";
  const dt = dayjs(timestamp);

  return dt.format("HH:mm:ss.SSS");
}

/**
 * 生成渐变色
 * @param {Array} colors
 * @param {Number} outCount
 * @returns
 */
export const gradientColors = (colors, outCount) => {
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
    const rsetp = (r2 - r1) / gap;
    const gstep = (g2 - g1) / gap;
    const bstep = (b2 - b1) / gap;
    for (let l = from; l < to; l += 1) {
      const r = Math.round(r1 + rsetp * (l - from));
      const g = Math.round(g1 + gstep * (l - from));
      const b = Math.round(b1 + bstep * (l - from));
      // window.console.log(r, g, b);
      // const rs = r.toString(16).padStart(2, "0");
      // const gs = g.toString(16).padStart(2, "0");
      // const bs = b.toString(16).padStart(2, "0");
      // const hex = `#${rs}${gs}${bs}`;
      outColors.push({
        R: r,
        G: g,
        B: b,
      });
    }
  }
  return outColors;
};

/**
 *
 * @param {{data:Array<Number>,  peakOffset:Number}} param0
 */
export const peakSearch = ({ data, peakOffset }) => {
  const dataLen = data.length;
  const peakGap = peakOffset || 25;
  let maxIndex = 0;
  const peaks = [];
  let max = data[0];
  for (let i = 1; i < dataLen; i += 1) {
    const val = data[i];
    if (val > max) {
      max = val;
    }
    if (i % peakGap === 0 || i === dataLen - 1) {
      peaks.push({
        peak: max,
        peakIndex: maxIndex,
      });
      i++;
      max = data[i];
      maxIndex = i;
    }
  }
  const sortPeaks = peaks.sort((a, b) => b.peak - a.peak);
  return {
    peakIndex: sortPeaks[0].peakIndex,
    peak2Index: sortPeaks[1].peakIndex,
  };
};

export const findPeaks = (data, spacing = 10) => {
  if (data.length < 301) return findPeaks_point(data);
  const peak1 = findPeaks1(data, spacing);
  const peaks = [];
  let isUp = peak1[1].value > peak1[0].value;
  if (!isUp) {
    peaks.push({
      index: 0,
      value: peak1[0].value,
    });
  }
  // 之前是否为上升
  let prevUp = false;
  for (let i = 2; i < peak1.length; i += 1) {
    const pi = peak1[i];
    const pi0 = peak1[i - 1];
    const val = pi.value;
    isUp = val > pi0.value;
    // 开始下降
    if (!isUp && prevUp) {
      peaks.push({
        index: pi0.index,
        value: pi0.value,
      });
    }
    prevUp = isUp;
  }
  const sortPeaks = peaks.sort((a, b) => b.value - a.value);

  return {
    peakIndex: sortPeaks[0].index,
    peak2Index: sortPeaks[1].index,
  };
};

export const findPeaks1 = (spectrumData, spacing = 10) => {
  const peaks = [];
  let half = Math.ceil(spacing / 2);
  const dataLen = spectrumData.length;
  if (spacing * 2 >= dataLen) {
    half = 2;
  }
  let j = 0;
  for (let i = half; i < dataLen - half; i += half) {
    let max = spectrumData[j];
    let maxIndex = j;
    let sameMaxCount = 0;
    j++;
    for (; j < i; j += 1) {
      const val = spectrumData[j];
      if (val >= max) {
        if (val > max) {
          sameMaxCount = 0;
          maxIndex = j;
          max = val;
        }
        sameMaxCount += 1;
      }
    }
    peaks.push({
      index: maxIndex + Math.floor(sameMaxCount / 2),
      value: max,
    });
  }

  return peaks;
};

/**
 *
 * @param {Array<Number>} spectrumData
 * @param {Number} threshold
 */
export const extractSignal = (spectrumData, threshold) => {
  const offset = spectrumData.map((d) => d - threshold);
  let start = -1;
  let end = -1;
  const signals = [];
  for (let i = 0; i < offset.length; i += 1) {
    const val = offset[i];
    if (val > 0) {
      if (start === -1) {
        start = i;
      }
      end = i;
    } else {
      if (start > -1) {
        const centerIndex = Math.round((end + start) / 2);
        signals.push({
          centerIndex,
          bandwidth: end - start + 1,
        });
        start = -1;
      }
    }
  }
  return signals;
};

/**
 * 不做波峰判断，直接比大小，目前主要是用于离散
 * @param {Array<Number>} spectrumData
 */
const findPeaks_point = (spectrumData) => {
  const sortPoints = spectrumData.slice(0).sort((a, b) => b - a);
  return {
    peakIndex: spectrumData.indexOf(sortPoints[0]),
    peak2Index: spectrumData.indexOf(sortPoints[1]),
  };
};

/**
 *
 * @param {Array<Number>} spectrumData
 * @param {Number} windowSize
 */
export const getSolidLine = (spectrumData, windowSize = 0) => {
  const spacing = windowSize || Math.round(spectrumData.length / 100);
  var smoothedPoints = [];
  var halfWindowSize = Math.floor(spacing / 2);
  for (var i = 0; i < spectrumData.length; i++) {
    var sum = 0;
    var count = 0;
    for (var j = i - halfWindowSize; j <= i + halfWindowSize; j++) {
      if (j >= 0 && j < spectrumData.length) {
        sum += spectrumData[j];
        count++;
      }
    }
    smoothedPoints.push(sum / count);
  }

  return smoothedPoints;
};

// Apply adaptive thresholding to the signal data
export const getAutoThreshold = (signalData, windowSize, coefficient = 5) => {
  const spacing = windowSize || Math.round(signalData.length / 100);
  var thresholdedData = [];
  for (var i = 0; i < signalData.length; i++) {
    var windowStart = Math.max(0, i - spacing);
    var windowEnd = Math.min(signalData.length - 1, i + spacing);

    // Calculate local standard deviation
    var localStdDev = standardDeviation(
      signalData.slice(windowStart, windowEnd + 1)
    );

    // Calculate threshold
    var threshold = coefficient * localStdDev;

    // Apply threshold
    thresholdedData.push(signalData[i] >= threshold ? signalData[i] : 0);
  }
  // console.log();
  const solid = getSolidLine(thresholdedData);
  return solid;
};

// Calculate standard deviation of an array of numbers
function standardDeviation(values) {
  var avg = average(values);
  var squareDiffs = values.map(function (value) {
    var diff = value - avg;
    return diff * diff;
  });
  var avgSquareDiff = average(squareDiffs);
  return Math.sqrt(avgSquareDiff);
}

// Calculate average of an array of numbers
function average(values) {
  var sum = values.reduce(function (sum, value) {
    return sum + value;
  }, 0);
  return sum / values.length;
}
