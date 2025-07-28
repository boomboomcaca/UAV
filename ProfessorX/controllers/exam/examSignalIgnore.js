const {
  insert,
  edit,
  getPageData,
  remove,
} = require('../../helper/repositoryBase');

let param = {
  tableName: 'rmbt_exam_signal_ignore',
  primaryKey: 'id',
  primaryKeyType: 'GUID',
  tableChineseName: '考试保障忽略信号',
};

const propertiesItem = {
  bandwidth: { type: 'number', description: '带宽' },
  frequency: { type: 'number', description: '频率' },
};

exports.addSchema = {
  description: `添加${param.tableChineseName}`,
  tags: [param.tableName],
  summary: `添加${param.tableChineseName}`,
  body: {
    type: 'object',
    properties: propertiesItem,
    required: ['frequency'],
  },
};

exports.deleteSchema = {
  description: `删除${param.tableChineseName}`,
  tags: [param.tableName],
  summary: `删除${param.tableChineseName}`,
  body: {
    type: 'object',
    properties: {
      id: { type: 'string', description: '主键' },
    },
  },
};

exports.getListSchema = {
  description: `获取${param.tableChineseName}列表`,
  tags: [param.tableName],
  summary: `获取${param.tableChineseName}列表`,
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
            description: '模板对象',
            properties: {
              ...propertiesItem,
              id: { type: 'string', description: '主键' },
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
  await insert(reqParam(req, reply));
};

/**
 *更新对象
 *
 * @param {*} req
 * @param {*} reply
 */
exports.update = async (req, reply) => {
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

/**
 *获取分页列表
 *
 * @param {*} req
 * @param {*} reply
 */
exports.getList = async (req, reply) => {
  await getPageData(reqParam(req, reply));
};
