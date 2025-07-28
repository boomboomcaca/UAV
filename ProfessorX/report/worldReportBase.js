/* eslint-disable no-console */
/* eslint-disable class-methods-use-this */
/* eslint-disable no-unused-vars */
const officegen = require('officegen');
const fs = require('fs');
const moment = require('moment');
const { getLogger, readFile } = require('../helper/repositoryBase');

const logger = getLogger('worldReportBase');
// eslint-disable-next-line no-unused-vars
class WorldReportBase {
  // todo:暂无好的方案，只有一行一行自己绘制world.
  constructor(param) {
    this.fileName = param.fileName;
    // this.fileName = fileName;

    // 一号 对应字体26
    // 三号  对应字体16
    // 小五  10.5
    // back (string) - background color code, for example: 'ffffff' (white) or '000000' (black).
    // shdType (string) - Optional pattern code to use: 'clear' (no pattern), 'pct10', 'pct12', 'pct15', 'diagCross', 'diagStripe', 'horzCross', 'horzStripe', 'nil', 'thinDiagCross', 'solid', etc.
    // shdColor (string) - The front color for the pattern (used with shdType).
    // bold (boolean) - true to make the text bold.
    // border (string) - the border type: 'single', 'dashDotStroked', 'dashed', 'dashSmallGap', 'dotDash', 'dotDotDash', 'dotted', 'double', 'thick', etc.
    // color (string) - color code, for example: 'ffffff' (white) or '000000' (black).
    // italic (boolean) - true to make the text italic.
    // underline (boolean) - true to add underline.
    // font_face (string) - the font to use.
    // font_face_east (string) - advanced setting: the font to use for east asian. You must set also font_face.
    // font_face_cs (string) - advanced setting: the font to use (cs). You must set also font_face.
    // font_face_h (string) - advanced setting: the font to use (hAnsi). You must set also font_face.
    // font_hint (string) - optional. Either 'ascii' (the default), 'eastAsia', 'cs' or 'hAnsi'.
    // font_size (number) - the font size in points.
    // rtl (boolean) - add this to any text in rtl language.
    // highlight (string) - highlight color. Either 'black', 'blue', 'cyan', 'darkBlue', 'darkCyan', 'darkGray', 'darkGreen', 'darkMagenta', 'darkRed', 'darkYellow', 'green', 'lightGray', 'magenta', 'none', 'red', 'white' or 'yellow'.
    // strikethrough (boolean) - true to add strikethrough.
    // superscript (boolean) - true to lower the text in this run below the baseline and change it to a smaller size, if a smallersize is available. Supported in officegen 0.5.0 and later.
    // subscript (boolean) - true to raise the text in this run above the baseline and change it to a smaller size, if a smaller size is available. Supported in officegen 0.5.0 and later.

    this.totalStyle = {
      border: 'dotted',
      borderSize: 12,
      borderColor: '88CCFF',
      blod: true,
      font_face: '宋体',
      font_size: 25,
      underline: true,
      back: '00ffff',
      shdType: 'pct12',
      shdColor: 'ff0000',
      backline: 'E0E0E0',
    };
    // 标题样式
    this.titleStyle = {
      // border: 'dotted',
      // borderSize: 12,
      // borderColor: '88CCFF',
      bold: true,
      font_face: 'Calibri',
      font_size: 26,
    };
    this.firstLevelTitleStyle = {
      font_face: '宋体',
      font_size: 16,
      bold: true,
    };
    this.tableStyle = {
      tableColWidth: 8500,
      tableSize: 18,
      tableAlign: 'left',
      tableFontFamily: 'Comic Sans MS',
      spacingBefor: 120, // default is 100
      spacingAfter: 120, // default is 100
      spacingLine: 240, // default is 240
      spacingLineRule: 'atLeast', // default is atLeast
      indent: 100, // table indent, default is 0
      fixedLayout: true, // default is false
      borders: true, // default is false. if true, default border size is 4
      borderSize: 2, // To use this option, the 'borders' must set as true, default is 4
      underline: true,
      // columns: [{ width: 1500 }, { width: 7500 }], // Table logical columns
    };
    this.tableColum1Opt = {
      cellColWidth: 1500,
      b: true,
      fontFamily: '宋体',
      bold: true,
      underline: true,
    };
    this.tableColum2Opt = {
      cellColWidth: 7500,
      b: true,
      fontFamily: '宋体',
      bold: true,
      underline: true,
    };
  }

  async genWorld(docx) {
    logger.debug('genWorld');
  }

  async createReport() {
    await this.generateReport();
    console.log(`${this.fileName}.docx`);
    // 其实也可以考虑http文件服务器 直接对外面提供文件服务
    let res = await readFile(`${this.fileName}.docx`);
    res = { fileName: `${this.fileName}.docx`, data: res };
    return res;
  }

  async generateReport() {
    // todo: 还需要进行进一步封装,需要把文件转换成二进制流直接返回给前端
    return new Promise((resolve, reject) => {
      const docx = officegen('docx');
      this.genWorld(docx);
      // todo: linux 上还是要在测试下，时间问题，考虑进行封装提供统一的时间服务
      // const mtime = moment(new Date()).format('YYYYMMDDHHMMSS');
      // todo: 测试阶段不加时间戳
      const out = fs.createWriteStream(`${this.fileName}.docx`); // 创建文件
      docx.generate(out, {
        finalize(data) {},
        error: reject,
      });
      out.on('finish', () => {
        resolve(true);
      });
    });
  }
}
module.exports = WorldReportBase;
// new WorldReportBase('test2.docx').createWorld();
