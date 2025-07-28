import parametersnamekey from 'parametersnamekey';

const freqParams = [
  parametersnamekey.frequency,
  parametersnamekey.startFrequency,
  parametersnamekey.stopFrequency,
];

/**
 * 绑定天线范围
 * @param {Array<object>} deviceAbility 设备能力
 * @param {Array<any>} moduleParameters 模块参数
 */
const bindAnt = (deviceAbility, moduleParameters) => {
  let minimum = 0;
  let maximum = 360000;
  if (deviceAbility) {
    maximum = deviceAbility.minimum;
    maximum = deviceAbility.maximum;
  }
  const antId = moduleParameters.find((p) => p.name === parametersnamekey.antennaID);
  if (!antId) return;
  if (antId.value !== antId.default) {
    const ants = moduleParameters.find((p) => p.name === parametersnamekey.antennas);
    const antParams = ants.parameters.find((ant) => ant.id === antId.value);
    if (antParams) {
      const { startFrequency, stopFrequency } = antParams;
      minimum = Math.max(startFrequency, minimum);
      maximum = Math.min(stopFrequency, maximum);
    }
  }
  freqParams.forEach((item) => {
    const pItem = moduleParameters.find((p) => p.name === item);
    if (pItem) {
      pItem.minimum = minimum;
      pItem.maximum = maximum;
      if (pItem.value > maximum) pItem.value = maximum;
      if (pItem.value < minimum) pItem.value = minimum;
    }
  });
};

export default bindAnt;
