/**
 * 数据重采样
 * @param {array} data
 * @param {bumber} point
 */
function dataResample(data, point) {
  const d = data.map((p) => {
    return { frequency: p[0], amplitude: p[1] };
  });
  const resampleData = getMultiThreshold(d, point);
  return resampleData;
}

/**
 * 获取多值门限
 * @param {*} refThreshold 参考门限
 * @param {*} thresholdCount 门限数量
 * @return {*} 多值门限
 */
function getMultiThreshold(refThreshold, thresholdCount) {
  const frequencys = refThreshold.map((item) => item.frequency);
  const minFrequency = Math.min(...frequencys);
  const maxFrequency = Math.max(...frequencys);
  const step = (maxFrequency - minFrequency) / (thresholdCount - 1);
  const multiThresholds = [];
  let cursorFreq = minFrequency;
  while (cursorFreq <= maxFrequency) {
    multiThresholds.push({
      frequency: cursorFreq,
      amplitude: getAmplitude(cursorFreq, refThreshold),
    });
    cursorFreq += step;
  }
  if (multiThresholds.length < thresholdCount) {
    multiThresholds.push({
      frequency: maxFrequency,
      amplitude: getAmplitude(maxFrequency, refThreshold),
    });
  }
  return multiThresholds;
}

// 计算频率在参考门限下的幅度（线性插值）
function getAmplitude(freq, thresholds) {
  let amplitude;
  let startThreshold;
  let stopThreshold;
  for (let i = 0; i < thresholds.length - 1; i += 1) {
    if (freq >= thresholds[i].frequency && freq <= thresholds[i + 1].frequency) {
      startThreshold = thresholds[i];
      stopThreshold = thresholds[i + 1];
      break;
    }
  }
  if (startThreshold && stopThreshold) {
    amplitude =
      startThreshold.amplitude +
      ((freq - startThreshold.frequency) * (stopThreshold.amplitude - startThreshold.amplitude)) /
        (stopThreshold.frequency - startThreshold.frequency);
  }
  return amplitude;
}

export default { dataResample, getMultiThreshold };
