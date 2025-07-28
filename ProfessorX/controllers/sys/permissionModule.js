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
  tableName: 'sys_permission_module',
  primaryKey: 'id',
  tableChineseName: '权限模块',
};

exports.addSchema = {
  description: `添加${param.tableChineseName}`,
  tags: [param.tableName],
  summary: `添加${param.tableChineseName}`,
  body: {
    type: 'object',
    properties: {
      name: { type: 'string', description: '模块名称' },
      icon: { type: 'string', description: '图标' },
      parent_id: { type: 'integer', description: '父级模块ID' },
      order_no: { type: 'integer', description: '编号' },
      url: { type: 'string', description: 'URL' },
      permission_ids: { type: 'string', description: '权限ID集合' },
      remark: { type: 'string', description: '描述' },
      enable: { type: 'integer', description: '是否启用' },
      type: { type: 'integer', description: '类型' },
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
