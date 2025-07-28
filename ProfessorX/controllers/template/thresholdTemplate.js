const fs = require('fs');
// ----//----//
const sd = require('silly-datetime');
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
} = require('../../helper/repositoryBase');

const { getGUID } = require('../../helper/common');

let param = {
  tableName: 'rmbt_template_threshold',
  primaryKey: 'id',
  tableChineseName: '门限模板',
  entityMap: [
    'id',
    'name',
    'lines',
    'max_freq',
    'min_freq',
    { maxFreq: 'max_freq' },
    { minFreq: 'min_freq' },
    'remark',
    'status',
    'user_id',
    { userId: 'user_id' },
    // 'create_time',
    // 'update_time',
    { createTime: 'create_time' },
    { updateTime: 'update_time' },
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
  description: `添加门限模板`,
  tags: [param.tableName],
  summary: `添加门限模板信息`,
  body: {
    type: 'object',
    properties: {
      name: { type: 'string', description: '模板名称', maxLength: 50 },
      lines: { type: 'number', description: '门限类型1:单门限，2:双门限' },
      min_freq: { type: 'number', description: '最小频率' },
      max_freq: { type: 'number', description: '最大频率' },
      points: { type: 'string', description: '门限数据' },
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
  req.body.id = getGUID();
  req.body.user_id = req.user.user_id;
  req.body.status = 1;
  await insert({
    ...reqParam(req, reply),
    returnData: { id: req.body.id },
  });
};

exports.deleteSchema = {
  description: `删除门限模板`,
  tags: [param.tableName],
  summary: `删除门限模板信息`,
  body: {
    type: 'object',
    properties: {
      id: { type: 'string', description: '门限模板id' },
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
  description: `更新门限模板`,
  tags: [param.tableName],
  summary: `更新门限模板信息`,
  body: {
    type: 'object',
    properties: {
      id: { type: 'string', description: '模板id', maxLength: 36 },
      name: { type: 'string', description: '模板名称', maxLength: 50 },
      lines: { type: 'number', description: '门限类型' },
      min_freq: { type: 'number', description: '最小频率' },
      max_freq: { type: 'number', description: '最大频率' },
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
  description: `获取单条门限模板信息`,
  tags: [param.tableName],
  summary: `获取单条门限模板`,
  query: {
    id: { type: 'string', description: '模板id', maxLength: 36 },
    max_freq: { type: 'number', description: '最大频率' },
    min_freq: { type: 'number', description: '最小频率' },
  },
};

/**
 *获取单条记录
 *
 * @param {*} req
 * @param {*} reply
 */
exports.get = async (req, reply) => {
  let maxFreq = 0;
  let minFreq = 0;
  if (req.body.max_freq || req.body.maxFreq) {
    maxFreq = req.body.max_freq || req.body.maxFreq;
  }
  if (req.body.min_freq || req.body.minFreq) {
    minFreq = req.body.min_freq || req.body.minFreq;
  }
  const data = await getDetail({ ...reqParam(req, reply), isReply: false });

  const template = JSON.parse(data[0].points);
  if (minFreq > 0 || maxFreq > 0) {
    // 循环templalte 里面的数据如果X 小于最小值的都不要
    for (let i = 0; i < template.length; i++) {
      const templateItem = template[i];
      const templateItemTemp = [];
      const removeIndex = [];
      for (let j = 0; j < templateItem.length; j++) {
        const [x] = templateItem[j];
        if (x < minFreq && minFreq !== 0) {
          removeIndex.push(j);
        }
        if (x > maxFreq && maxFreq !== 0) {
          removeIndex.push(j);
        }
      }
      for (let k = 0; k < templateItem.length; k++) {
        if (removeIndex.indexOf(k) < 0) {
          templateItemTemp.push(templateItem[k]);
        }
      }
      template[i] = templateItemTemp;
    }
  }
  data[0].points = template;
  // data[0].maxFreq = data[0].max_freq;
  // data[0].minFreq = data[0].min_freq;
  data[0] = autoMap(data[0], param.entityMap, true);
  resSuccess({ reply, result: data });
};

exports.importSchema = {
  description: `更新门限模板`,
  tags: [param.tableName],
  summary: `更新门限模板信息`,
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
  summary: `导出门限模板`,
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
    `attachment; filename=thresholdTemplate_${sd.format(
      new Date(),
      'YYYY-MM-DD-HH-mm-ss'
    )}.json`
  );
  reply.header('Content-Type', 'application/octet-stream');
  reply.send(stream);
};
