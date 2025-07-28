/* eslint-disable no-unused-vars */
/* eslint-disable import/no-dynamic-require */
/* eslint-disable global-require */
// 报表管理
// 报表名称集合,如果输入的名称不在这里面则会提示失败
// 这里应该也是http 请求入口进来 可以直接使用resError
// todo:也可以考虑直接读取该目录下文件名称，也是一个方案
const reportNameList = [
  'frequencyBandOccupancyDailyReport',
  'moreTimeFrequencyBandOccupancyReport',
];
exports.getReport = async (reportName, param) => {
  const Report = require(`./${reportName}`);
  const myReport = new Report(param);
  const res = await myReport.createReport();
  return res;
};
