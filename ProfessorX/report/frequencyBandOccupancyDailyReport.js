/* eslint-disable no-useless-escape */
/* eslint-disable no-unused-vars */
const moment = require('moment');
const ExcelReportBase = require('./excelReportBase');

// 频段占用度日报
class FrequencyBandOccupancyDailyReport extends ExcelReportBase {
  async initData() {
    const mtime = moment(new Date()).format('YYYY年MM月DD日');

    // 获取年月日
    this.data.independentCellData = [
      // 可以进行对象映射建立一个对象然后映射过来//不然这一行太麻烦了
      {
        cellName: 'A1',
        cellValue: `88-108MHz-25kHz频段占用度测量表(${mtime})`,
        remark: '标题',
      },
      { cellName: 'D3', cellValue: '腾冲明光站' },
      { cellName: 'M3', cellValue: 99.166001 },
      { cellName: 'V3', cellValue: 25.123306 },
      { cellName: 'D4', cellValue: 'RFeye(21-6)' },
      { cellName: 'M4', cellValue: '' },
      { cellName: 'V4', cellValue: 1 },
      { cellName: 'D5', cellValue: 89 },
      { cellName: 'M5', cellValue: 109 },
      { cellName: 'V5', cellValue: 26 },
      { cellName: 'D6', cellValue: 26 },
      { cellName: 'M6', cellValue: 'FAST' },
      { cellName: 'V6', cellValue: 34.2 },
      { cellName: 'D7', cellValue: 16 },
      { cellName: 'M7', cellValue: 0.5 },
      { cellName: 'V7', cellValue: mtime },
      { cellName: 'V8', cellValue: mtime },
    ];
    this.data.continuousRowData = [];

    // this.fileInfo = await logRepository.getReplayFileInfo(
    //   decodeMsg.params.fileID
    // );
    // const indexFileName = config.SyncSystem.path
    //   .concat(this.fileInfo.sourceFile)
    //   .concat('.idx');
    // const contentFileName = config.SyncSystem.path
    //   .concat(this.fileInfo.sourceFile)
    //   .concat('.dat');
    // let contentMsg;
    // try {
    //   this.indexDatas = await getIndexData(indexFileName);
    //   this.fd = await getFileDescriptor(contentFileName);

    for (let i = 0; i < 802; i++) {
      // 13个数字
      const num = i + 11;
      const item = { cellName: `E${num}`, cellEndName: `Q${num}`, values: [] };
      for (let j = 0; j < 13; j++) {
        item.values.push(
          Math.round(Math.random() * 100) / 100 +
            Math.round(Math.random() * 100)
        );
      }
      this.data.continuousRowData.push(item);
    }

    // 调用基类的方法
    super.initData();
  }
}
module.exports = FrequencyBandOccupancyDailyReport;
