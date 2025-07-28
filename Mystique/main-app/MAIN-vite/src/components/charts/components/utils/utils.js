import dayjs from "dayjs";

export function isMobile() {
  let flag =
    /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(
      navigator.userAgent
    );
  return flag;
}

export function frequency2String(frequency) {
  if (frequency < 0.001) return `${Math.round(frequency * 1e6)}Hz`;
  if (frequency < 1) return `${(frequency * 1e3).toFixed(3)}kHz`;
  if (frequency < 1000) return `${frequency.toFixed(6)}MHz`;
  return `${(frequency / 1000).toFixed(6)}GHz`;
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
  const max = Math.max(...referData);
  const min = Math.min(...referData);

  console.log(min, max);
  const minFlag = Math.floor(min / 5);
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
  const half = Math.ceil(spacing / 2);
  let j = 0;
  for (let i = half; i < spectrumData.length - half; i += half) {
    let max = spectrumData[j];
    let maxIndex = j;
    j++;
    for (; j < i; j += 1) {
      const val = spectrumData[j];
      if (val > max) {
        maxIndex = maxIndex;
        max = val;
      }
    }
    peaks.push({
      index: j,
      value: max,
    });
  }

  return peaks;
};
