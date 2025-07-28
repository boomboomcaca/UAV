const {
  insert,
  edit,
  remove,
  getPageData,
  removeList,
  insertList,
} = require('../../helper/repositoryBase');

// 这些其实可以考虑通过配置存到数据库！目前在代码里面写死吧！
let param = {
  tableName: 'sys_role',
  primaryKey: 'id',
  tableChineseName: '角色',
  uniqueKey: 'name',
};
// 如何添加和修改共用Schema
exports.addSchema = {
  description: `添加${param.tableChineseName}`,
  tags: [param.tableName],
  summary: `添加${param.tableChineseName}`,
  body: {
    type: 'object',
    properties: {
      parent_id: { type: 'integer', description: '父级角色ID' },
      name: {
        type: 'string',
        minLength: 2,
        maxLength: 20,
        description: '名称',
      },
      remark: { type: 'string', description: '描述' },
      is_builtin: { type: 'integer', description: '是否内置' },
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
  req.body.object_name = '*';
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
  // todo:删除冗余数据
  // todo:
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
