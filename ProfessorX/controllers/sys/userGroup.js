// 基础操作 添加删除 修改 基于数据库二次封装
const {
  insert,
  edit,
  remove,
  getPageData,
  removeList,
  insertList,
} = require('../../helper/repositoryBase');
// 基础帮助方法 返回成功 失败  添加时间 更新时间赋值

let param = {
  tableName: 'sys_user_group',
  primaryKey: 'id',
  tableChineseName: '用户用户组关系表',
  unionKey: ['user_id', 'group_id'],
};

exports.addSchema = {
  description: `添加${param.tableChineseName}`,
  tags: [param.tableName],
  summary: `添加${param.tableChineseName}`,
  body: {
    type: 'object',
    properties: {
      user_id: { type: 'integer', description: '用户ID' },
      group_id: { type: 'integer', description: '用户组ID' },
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

// exports.test = async (req, reply) => {
//   await getPageData(reqParam(req, reply));
// };
