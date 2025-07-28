const fs = require('fs');
const { Canvas } = require('canvas');
const sd = require('silly-datetime');
const math = require('lodash/math');
const { writeImage } = require('../../helper/common');
const {
  insert,
  edit,
  remove,
  getPageData,
  getDetail,
  insertList,
  resSuccess,
  resError,
  autoMap,
  config,
} = require('../../helper/repositoryBase');

const { getGUID } = require('../../helper/common');

let param = {
  tableName: 'rmbt_template_frequency',
  primaryKey: 'id',
  tableChineseName: '频率模板',
  entityMap: [
    'id',
    'name',
    'bandwidth',
    'remark',
    'status',
    // 'user_id',
    { userId: 'user_id' },
    { createTime: 'create_time' },
    { updateTime: 'update_time' },
    'path',
    // 'create_time',
    // 'update_time',
  ],
  uniqueKey: 'name',
};

/**
 *获取请求参数
 *
 * @param {*} req
 * @param {*} reply
 */
function reqParam(req, reply) {
  req.body = autoMap(req.body, param.entityMap);
  param = { ...param, req, reply };
  return param;
}

exports.addSchema = {
  description: `添加频率模板`,
  tags: [param.tableName],
  summary: `添加频率模板信息`,
  body: {
    type: 'object',
    properties: {
      name: { type: 'string', description: '模板名称', maxLength: 50 },
      bandwidth: { type: 'number', description: '带宽' },
      points: { type: 'string', description: '频点数据' },
      remark: { type: 'string', description: '来源及备注', maxLength: 50 },
    },
    required: ['name'],
  },
};

/**
 *新增
 *
 * @param {*} req
 * @param {*} reply
 */
exports.add = async (req, reply) => {
  // 添加 320*320
  // 添加的时候查询的时候我还是要以
  req.body.id = getGUID();
  req.body.status = 1;
  req.body.user_id = req.user.userId;
  // 把这个转换成为Json
  const width = config.frequencyTemplateImageWidth;
  const height = config.frequencyTemplateImageHeight;
  const canvas = new Canvas(width, height);
  const ctx = canvas.getContext('2d');

  const points = JSON.parse(req.body.points);
  const data = points.sourceData.filter((s) => s != null);
  let startX = 0;
  const newPoints = [];
  for (let i = 0; i < data.length / 10; i++) {
    const arr = [];
    for (let j = 0; j < 10; j++) {
      arr.push(data[i * 10 + j]);
    }
    const maxData = math.max(arr);
    newPoints.push(maxData);
  }
  let ymin = 0;
  const min = math.min(newPoints);
  if (min < 0) {
    ymin = min * -1;
  }
  const max = math.max(newPoints);
  const yInterval = height / (max - min);
  const xInterval = width / newPoints.length;
  for (let i = 0; i < newPoints.length; i += 1) {
    // 必须要进行抽样 抽出最大320个点
    // y值是100，nameY的值就是
    ctx.moveTo(startX, height - (newPoints[i] + ymin) * yInterval); // 设置起点状态
    ctx.lineTo(
      startX + xInterval,
      height - (newPoints[i + 1] + ymin) * yInterval
    ); // 设置末端状态
    startX += xInterval;
  }
  ctx.lineWidth = 1; // 设置线宽状态
  ctx.strokeStyle = '#FFFF00';
  ctx.stroke(); // 进行绘制
  const path = `${config.frequencyTemplateImagePath}/${req.body.id}.png`;
  req.body.path = path;
  await writeImage(canvas, `${process.cwd()}/${path}`);
  await insert({
    ...reqParam(req, reply),
    returnData: { id: req.body.id },
  });
};

exports.deleteSchema = {
  description: `删除频率模板`,
  tags: [param.tableName],
  summary: `删除频率模板信息`,
  body: {
    type: 'object',
    properties: {
      id: { type: 'string', description: '频率模板id' },
    },
    required: ['id'],
  },
};

/**
 *根据输入条件删除
 *
 * @param {*} req
 * @param {*} reply
 */
exports.del = async (req, reply) => {
  await remove({
    ...reqParam(req, reply),
    returnData: { id: req.body.id },
  });
};

exports.updateSchema = {
  description: `更新频率模板`,
  tags: [param.tableName],
  summary: `更新频率模板信息`,
  body: {
    type: 'object',
    properties: {
      id: { type: 'string', description: '模板id', maxLength: 36 },
      name: { type: 'string', description: '模板名称', maxLength: 50 },
      bandwidth: { type: 'number', description: '带宽' },
      points: { type: 'string', description: '频点数据' },
      remark: { type: 'string', description: '来源及备注', maxLength: 50 },
      status: { type: 'number', description: '状态' },
    },
    required: ['id'],
  },
};

/**
 *更新对象
 *
 * @param {*} req
 * @param {*} reply
 */
exports.update = async (req, reply) => {
  await edit({
    ...reqParam(req, reply),
    returnData: { id: req.body.id },
  });
};

// /**
//  *获取分页列表
//  *
//  * @param {*} req
//  * @param {*} reply
//  */
exports.getList = async (req, reply) => {
  req.body.status = 1;
  await getPageData(reqParam(req, reply));
};

exports.getSchema = {
  description: `获取单条频率模板信息`,
  tags: [param.tableName],
  summary: `获取单条频率模板`,
  query: {
    id: { type: 'string', description: '模板id', maxLength: 36 },
  },
};

/**
 *获取单条记录
 *
 * @param {*} req
 * @param {*} reply
 */
exports.get = async (req, reply) => {
  await getDetail(reqParam(req, reply));
};

exports.importSchema = {
  description: `更新频率模板`,
  tags: [param.tableName],
  summary: `更新频率模板信息`,
};

/**
 *导入数据
 *
 * @param {*} req
 * @param {*} reply
 */
exports.import = async (request, reply) => {
  // request.file is the `avatar` file
  // request.body will hold the text fields, if there were any

  await fs.readFile(request.file.path, async (err, data) => {
    // 读取文件失败/错误
    if (err) {
      throw err;
    }
    // 读取文件成功

    const templateArr = JSON.parse(data.toString());
    templateArr.map((templateItem) => {
      delete templateItem.update_time;
      templateItem.id = getGUID();
      templateItem.status = 1;
      templateItem.user_id = request.user.userId;
      return templateItem;
    });
    let res = '';
    // 批量插入到数据库
    if (templateArr.length > 0) {
      request.body = templateArr;
      res = await insertList({
        req: request,
        reply,
        tableName: param.tableName,
        tableChineseName: param.tableChineseName,
        isReply: false,
      });
    }
    resSuccess({
      reply,
      message: `导入${param.tableChineseName}成功`,
      data: res,
    });
  });
};

exports.exportSchema = {
  description: `导出数据`,
  tags: [param.tableName],
  summary: `导出频率模板`,
  query: {
    id: {
      type: 'string',
      description: '模板id,支持一个或多个模板同时导出,多个用英文逗号分隔',
    },
  },
};

exports.export = async (req, reply) => {
  req.body.status = 1;
  let stream = '[]';
  // 查询数据
  const templateData = await getPageData({
    req,
    reply,
    tableName: param.tableName,
    isReply: false,
    isLimitMaxRow: false,
  });
  // 数据转字符串
  if (templateData && templateData.rows.length > 0)
    stream = JSON.stringify(templateData.rows);
  else resError({ message: '未查询到对应模板数据' });

  reply.header(
    'Content-Disposition',
    `attachment; filename=frequencyTemplate_${sd.format(
      new Date(),
      'YYYY-MM-DD-HH-mm-ss'
    )}.json`
  );
  reply.header('Content-Type', 'application/octet-stream');
  reply.send(stream);
};
