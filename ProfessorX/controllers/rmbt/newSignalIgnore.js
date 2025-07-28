const {
  insert,
  edit,
  getPageData,
  remove,
} = require('../../helper/repositoryBase');

let param = {
  tableName: 'rmbt_new_signal_ignore',
  primaryKey: 'id',
  primaryKeyType: 'GUID',
  tableChineseName: '忽略信号',
  entityMap: [
    'id',
    'frequency',
    'bandwidth',
    { createTime: 'create_time' },
    { updateTime: 'update_time' },
  ],
};
const propertiesItem = {
  bandwidth: { type: 'number', description: '带宽', maxLength: 36 },
  frequency: { type: 'number', description: '频率', maxLength: 36 },
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
  query: {
    templateId: { type: 'string', description: '模板ID' },
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
  // 获取请求参数的时候不要带上autoMap的？
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
// /**
//  *批量添加对象（id不会自动赋值，暂不支持批量添加）
//  *
//  * @param {*} req
//  * @param {*} reply
//  */
// exports.addList = async (req, reply) => {
//   for (let i = 0; i < req.body.length; i++) {
//     let querySql = `select * from ${param.tableName} where frequency = ${req.body[i].frequency}`;
//     if (req.body[i].bandwidth) {
//       querySql = `select * from ${param.tableName} where frequency = ${req.body[i].frequency} and bandwidth = ${req.body[i].bandwidth}`;
//     }
//     const res = await sqlQuery(querySql);
//     if (res && res.length > 0) {
//       resError({ message: `忽略信号已存在` });
//     }
//   }
//   await insertList(reqParam(req, reply));
// };
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
  // 通过
  await getPageData(reqParam(req, reply));
};
