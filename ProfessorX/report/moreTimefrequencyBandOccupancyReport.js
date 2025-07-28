/* eslint-disable no-unused-vars */
const WorldReportBase = require('./worldReportBase');

class MoreTimeFrequencyBandOccupancyReport extends WorldReportBase {
  //   constructor(param) {
  //     super(param.fileName);
  //   }
  async genWorld(docx) {
    // 这个暂无什么好的方案
    // 第一步先创建模板然后在进行数据替换
    // 这个不好替换只有创建 的时候直接写进去
    let pObj = docx.createP();
    pObj.addText('88-108-25多时间', this.titleStyle);
    pObj.addText('占用度比对', { ...this.titleStyle, underline: true });
    pObj.options.align = 'center';
    // underline: true
    pObj = docx.createP();
    pObj.addText('1、对比参数', this.firstLevelTitleStyle);
    pObj.options.align = 'left';

    // 创建一个表格
    // const table = [
    //   [
    //     {
    //       val: '对比方式',
    //       opts: this.tableColum1Opt,
    //     },
    //     {
    //       val: '相同站点不同时间',
    //       opts: this.tableColum2Opt,
    //     },
    //   ],
    //   [
    //     {
    //       val: '监测站',
    //       opts: this.tableColum1Opt,
    //     },
    //     {
    //       val: '腾冲明光站',
    //       opts: this.tableColum2Opt,
    //     },
    //   ],
    //   [
    //     {
    //       val: '时间范围',
    //       opts: this.tableColum1Opt,
    //     },
    //     {
    //       val: '2018年8月1日/2018年8月2日/2018年8月3日/2018年8月4日/',
    //       opts: this.tableColum2Opt,
    //     },
    //   ],
    //   [
    //     {
    //       val: '电平门限',
    //       opts: this.tableColum1Opt,
    //     },
    //     {
    //       val: '自适应门限/自适应门限/自适应门限/自适应门限/',
    //       opts: this.tableColum2Opt,
    //     },
    //   ],
    // ];

    // pObj = docx.createTable(table, this.tableStyle);
    pObj = docx.createP();
    pObj.addText('1、测量图', this.firstLevelTitleStyle);
    pObj = docx.createP();
    pObj.options.align = 'left';
    pObj.addImage('data/template/logo.png', { cx: 500, cy: 200 });

    super.genWorld(docx);
  }
}

module.exports = MoreTimeFrequencyBandOccupancyReport;
