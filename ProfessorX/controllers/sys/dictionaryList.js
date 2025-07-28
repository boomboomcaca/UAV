// 基础操作 添加删除 修改 基于数据库二次封装
const {
  edit,
  remove,
  getPageData,
  removeList,
  config,
  insert,
  insertList,
} = require('../../helper/repositoryBase');

let param = {
  tableName: 'sys_dictionarylist',
  primaryKey: 'id',
  tableChineseName: '字典明细',
  dicKeys: [config.cacheKey.dic],
};

// 字典初步思路 用到什么加载什么但是sql不缓存！
// 直接全部查询出来在内存缓存！所有的通过RedisHash对象去查询 速度快！
// 如果没有的直接从数据库查询！如果包含sql不存储！
// 每次更新删除所有字典缓存！
// 通过Hash
exports.addSchema = {
  description: `添加${param.tableChineseName}`,
  tags: [param.tableName],
  summary: `添加${param.tableChineseName}`,
  body: {
    type: 'object',
    properties: {
      dic_id: { type: 'integer' },
      order_no: { type: 'integer' },
      key: { type: 'string' },
      value: { type: 'string' },
      enable: { type: 'integer' },
      remark: { type: 'string' },
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
 *批量添加对象
 *
 * @param {*} req
 * @param {*} reply
 */
exports.addList = async (req, reply) => {
  await insertList(reqParam(req, reply));
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
 *批量删除
 *
 * @param {*} req
 * @param {*} reply
 */
exports.delList = async (req, reply) => {
  await removeList(reqParam(req, reply));
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
