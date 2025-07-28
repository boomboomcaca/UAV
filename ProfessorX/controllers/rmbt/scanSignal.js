const {
  insert,
  edit,
  getPageData,
  // removeList,
  insertList,
  remove,
} = require('../../helper/repositoryBase');
const { dateFormat, getDateString } = require('../../helper/common');

let param = {
  tableName: 'rmbt_scan_signal',
  primaryKey: 'id',
  primaryKeyType: 'number',
  tableChineseName: '频段扫描信号',
};

const propertiesItem = {
  taskID: { type: 'string', description: '任务ID' },
  frequency: { type: 'number', description: '频率' },
  bandwidth: { type: 'number', description: '估测带宽' },
  maxValue: { type: 'number', description: '最大值' },
  occupancy: { type: 'number', description: '占用度' },
  lastTestTime: { type: 'string', description: '末次测量时间' },
};

exports.addSchema = {
  description: `添加${param.tableChineseName}`,
  tags: [param.tableName],
  summary: `添加${param.tableChineseName}`,
  body: {
    type: 'object',
    properties: propertiesItem,
    required: [
      'frequency',
      'bandwidth',
      'maxValue',
      'occupancy',
      'lastTestTime',
    ],
  },
};

exports.deleteSchema = {
  description: `删除${param.tableChineseName}`,
  tags: [param.tableName],
  summary: `删除${param.tableChineseName}`,
  body: {
    type: 'object',
    properties: {
      id: { type: 'number', description: '主键' },
    },
  },
};

exports.deleteListSchema = {
  description: `删除任务${param.tableChineseName}`,
  tags: [param.tableName],
  summary: `删除任务${param.tableChineseName}`,
  body: {
    type: 'object',
    properties: {
      taskID: { type: 'string', description: '任务ID' },
    },
  },
};

exports.getListSchema = {
  description: `获取${param.tableChineseName}列表`,
  tags: [param.tableName],
  summary: `获取${param.tableChineseName}列表`,
  query: {
    taskID: { type: 'string', description: '任务ID' },
  },
  post: true,
  response1: {
    200: {
      type: 'object',
      properties: {
        result: {
          type: 'array',
          description: '返回结果',
          items: {
            type: 'object',
            description: '信号数据对象',
            properties: {
              ...propertiesItem,
              id: { type: 'number', description: '主键' },
            },
          },
        },
        total: { type: 'number', description: '条数' },
      },
    },
  },
};

/**
 *获取请求参数
 *
 * @param {*} req
 * @param {*} reply
 */
function reqParam(req, reply) {
  param = { ...param, req, reply };
  return param;
}

/**
 *添加对象
 *
 * @param {*} req
 * @param {*} reply
 */
exports.add = async (req, reply) => {
  req.body.lastTestTime = getDateString(req.body.lastTestTime, dateFormat);
  await insert(reqParam(req, reply));
};

/**
 *批量添加对象
 *
 * @param {*} req
 * @param {*} reply
 */
exports.addList = async (req, reply) => {
  req.body.forEach((element) => {
    element.lastTestTime = getDateString(element.lastTestTime, dateFormat);
  });
  await insertList(reqParam(req, reply));
};

/**
 *更新对象
 *
 * @param {*} req
 * @param {*} reply
 */
exports.update = async (req, reply) => {
  if (req.body.lastTestTime) {
    req.body.lastTestTime = getDateString(req.body.lastTestTime, dateFormat);
  }
  await edit(reqParam(req, reply));
};

/**
 *根据输入条件删除
 *
 * @param {*} req
 * @param {*} reply
 */
exports.del = async (req, reply) => {
  await remove(reqParam(req, reply));
};

// /**
//  *批量删除
//  *
//  * @param {*} req
//  * @param {*} reply
//  */
// exports.delList = async (req, reply) => {
//   await removeList(reqParam(req, reply));
// };

/**
 *获取分页列表 通用封装getList
 *
 * @param {*} req
 * @param {*} reply
 */
exports.getList = async (req, reply) => {
  await getPageData(reqParam(req, reply));
};
