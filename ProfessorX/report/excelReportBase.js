/* eslint-disable class-methods-use-this */
/* eslint-disable no-console */
/* eslint-disable no-unused-vars */
/* eslint-disable no-param-reassign */
// exceljs
// https://github.com/exceljs/exceljs
const ExcelJS = require('exceljs');
const moment = require('moment');
const { getLogger, readFile } = require('../helper/repositoryBase');

const logger = getLogger('excelReportBase');

class ExcelReportBase {
  templateFileName = 'test.xlsx'; // 模板文件名称

  fileName = 'newtest1.xls'; // 文件名称

  workbook1;

  // 生成的新的文件名称我觉得直接 在原来的基础上加上时分秒就可以了！
  constructor(param) {
    this.templateFileName = param.templateFileName;
    this.fileName = param.fileName;
  }
  // 强行把两个报表合二为一

  // 允许设置某一行 某一列的值
  // 减少循环判断次数
  data = {
    independentCellData: [{ cellName: '', cellValue: '', remark: '' }], // 独立单元格数据
    rowData: [{ row: 0, values: [] }], // 一行数据
    columnData: [{ column: 0, values: [] }], // 一列数据
    continuousRowData: [{ cellName: '', cellEndName: '', values: [] }], // 连续行单元格数据,如果Values里面只有一个值，则连续的都是同样的值
    continuousColumData: [{ cellName: '', cellEndName: '', values: [] }], // 连续列单元格数据
  };

  // 默认最大支持52个字母
  letterArray = [];

  async init() {
    for (let i = 0; i < 26; i++) {
      this.letterArray.push(String.fromCharCode(65 + i)); // 输出A-Z 26个大写字母
    }
    for (let i = 0; i < 26; i++) {
      this.letterArray.push(`A${String.fromCharCode(65 + i)}`); // 输出A-Z 26个大写字母
    }
  }

  // 产生数据
  // eslint-disable-next-line class-methods-use-this
  async initData() {
    logger.debug('initData');
  }

  // 处理数据
  async processData(ws, data) {
    const { letterArray } = this;
    data.independentCellData.forEach((item) => {
      ws.getCell(item.cellName).value = item.cellValue;
    });
    data.continuousRowData.forEach((item) => {
      const startCellNumber = item.cellName.replace(/[^0-9]/gi, '');
      const startCellLetter = item.cellName.replace(startCellNumber, '');
      const endCellLetter = item.cellEndName.replace(startCellNumber, '');
      const startCellIndex = letterArray.indexOf(startCellLetter);
      const endCellIndex = letterArray.indexOf(endCellLetter);
      for (let i = startCellIndex; i <= endCellIndex; i++) {
        const cell = `${letterArray[i]}${startCellNumber}`;
        ws.getCell(cell).value = item.values[i - startCellIndex];
      }
    });
  }

  async createReport() {
    await this.init();
    await this.initData();
    const workbook = new ExcelJS.Workbook();
    await workbook.xlsx.readFile(this.templateFileName);
    const ws = workbook.worksheets[0];
    await this.processData(ws, this.data);
    // todo: 测试阶段不加时间戳
    // 封装成Promise
    await workbook.xlsx.writeFile(`${this.fileName}.xlsx`);

    let res = await readFile(`${this.fileName}.xlsx`);
    res = { fileName: `${this.fileName}.docx`, data: res };
    return res;
  }
}
module.exports = ExcelReportBase;
