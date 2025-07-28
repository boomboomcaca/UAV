/*
 * @Author: liujian
 * @Date: 2020-11-19 11:40:52
 * @LastEditTime: 2020-11-19 11:44:26
 * @LastEditors: Please set LastEditors
 * @Description: In User Settings Edit
 * @FilePath: \Commons\src\Components\SpectrumUnitConverter\SpectrumUnitConverter.js
 */

// 绝大部分射频配备的标准测试仪器则将 50Ω作为标准的接口阻抗。而有线电视(CATV)系统工作在 75Ω环境下
const R = 50.0;

const dBm2dBuV = (input) => {
  return input + 10 * Math.log10(R) + 90.0;
};

const dBm2V = (input) => {
  return Math.sqrt(R * 10 ** (input / 10 - 3));
};

const dBm2mW = (input) => {
  return 10 ** (input / 10);
};

const dBuV2dBm = (input) => {
  return input - 90 - 10 * Math.log10(R);
};

const dBuV2V = (input) => {
  return 10 ** (input / 20 - 6);
};

const dBuV2uV = (input) => {
  return dBuV2V(input) * 10 ** 6;
};
const dBuV2mW = (input) => {
  return 10 ** (input / 10 - 9) / R;
};

const V2dBm = (input) => {
  if (input <= 0) return -200;

  return 30 + 20 * Math.log10(input) - 10 * Math.log10(R);
};

const V2dBuV = (input) => {
  if (input <= 0) return -200;
  return 20 * (Math.log10(input) + 6);
};

const V2mW = (input) => {
  return (1000 * input ** 2) / R;
};

const mW2dBm = (input) => {
  if (input <= 0) return -50;

  return 10 * Math.log10(input);
};

const mW2dBuV = (input) => {
  if (input <= 0) return -200;

  return 10 * Math.log10(input * R) + 90;
};

const mW2V = (input) => {
  return (0.001 * input * R) ** 0.5;
};

const spectrum2dBm = (spectrum) => {
  // 更健康 内存少
  const newData = spectrum.map((s) => {
    return dBuV2dBm(s);
  });
  return newData;

  // 更快
  // const newData = [];
  // spectrum.forEach((s, index, arr) => {
  //   newData[index] = dBuV2dBm(s);
  // });
  // return newData;
};

const converters = {
  dBm2dBuV,
  dBm2V,
  dBm2mW,
  dBuV2dBm,
  dBuV2V,
  dBuV2uV,
  dBuV2mW,
  V2dBm,
  V2dBuV,
  V2mW,
  mW2dBm,
  mW2dBuV,
  mW2V,
  spectrum2dBm,
};

export default converters;
