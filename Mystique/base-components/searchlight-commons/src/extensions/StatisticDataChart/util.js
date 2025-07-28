/**
 * 抽取
 * @param {Array} spectrum
 * @param {Number} outMax
 * @returns
 */
export const combineData = (spectrum, outMax) => {
  if (spectrum.length > outMax) {
    const res = [];
    // desIndex = new int[OutMaxCount];
    let freqIndex = 0;
    const perPointGap = spectrum.length / (outMax - 1);
    let freqN = freqIndex + perPointGap / 2;
    for (let j = 0; j < outMax; j += 1) {
      let maxValue = -9999; // spectrum[freqIndex];
      const k = Math.floor(freqN);
      for (; freqIndex <= k; freqIndex += 1) {
        const val = spectrum[freqIndex];
        if (maxValue < val) {
          maxValue = val;
          // desDataIndex = freqIndex;
        }
      }
      res[j] = maxValue;
      freqN += perPointGap;
      if (freqN > spectrum.length) freqN = spectrum.length;
      // desIndex[j] = desDataIndex + CurrentStart;
    }
    return res;
  }
  const [...source] = spectrum;
  return source;
};
export const createFormateTime = (time) => {
  const nowDate = new Date(time);
  // const year = nowDate.getFullYear();
  // let month = nowDate.getMonth() + 1;
  // month = month > 9 ? month : `0${month}`;
  // let days = nowDate.getDate();
  // days = days > 9 ? days : `0${days}`;
  let hours = nowDate.getHours();
  hours = hours > 9 ? hours : `0${hours}`;
  let minutes = nowDate.getMinutes();
  minutes = minutes > 9 ? minutes : `0${minutes}`;
  let seconds = nowDate.getSeconds();
  seconds = seconds > 9 ? seconds : `0${seconds}`;
  //   return `${year}-${month}-${days} ${hours}:${minutes}:${seconds}`;
  return `${hours}:${minutes}:${seconds}`;
};

export default { combineData, createFormateTime };
