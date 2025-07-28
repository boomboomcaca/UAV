/* eslint-disable camelcase */
const {
  insert,
  edit,
  remove,
  getPageData,
  removeList,
  insertList,
  resSuccess,
  sqlQuery,
} = require('../../helper/repositoryBase');
const { getParentRoles } = require('../../hook/permissionAuth');

// 这些其实可以考虑通过配置存到数据库！目前在代码里面写死吧！
// 加一个联合主键
let param = {
  tableName: 'sys_role_permission',
  primaryKey: 'id',
  tableChineseName: '角色权限',
  unionKey: ['role_id', 'permission_module_id'],
};
// 可以考虑加一个Wheres

// 代码不要过度封装这样会影响效率也违背了单一职责原则

exports.addSchema = {
  description: `添加${param.tableChineseName}`,
  tags: [param.tableName],
  summary: `添加${param.tableChineseName}`,
  body: {
    type: 'object',
    properties: {
      role_id: { type: 'integer', description: '角色ID' },
      permission_module_id: { type: 'integer', description: '权限模块ID' },
    },
  },
};
exports.getRolePermissionSchema = {
  description: `获取角色权限`,
  tags: [param.tableName],
  summary: `获取角色权限`,
  query: {
    role_id: { type: 'string' },
  },
  required: ['id'],
};

// 把这段封装起来
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
  // 删除批量删除
  await remove(reqParam(req, reply));
};

/**
 *批量删除
 *
 * @param {*} req
 * @param {*} reply
 */
exports.delList = async (req, reply) => {
  // 删除批量删除
  await removeList(reqParam(req, reply));
};
/**
 *获取列表 智能获取自己被授予权限的资源
 *
 * @param {*} req
 * @param {*} reply
 */
exports.getList = async (req, reply) => {
  await getPageData(reqParam(req, reply));
};

/**
 *获取角色权限
 *
 * @param {*} req
 * @param {*} reply
 */
exports.getRolePermission = async (req, reply) => {
  // 获取角色的所有继承角色 然后再进行差集计算
  let sql = `select id,parent_id from sys_role`;
  let res = await sqlQuery(sql);
  let roles = [];
  const { role_id } = req.query;
  const role = [{ role_id }];
  roles = getParentRoles(role, res, roles);
  roles = Array.from(new Set(roles));
  // 查询出所有的模块权限
  sql = `select * from sys_role_permission where role_id in (${roles.toString()})`;
  res = await sqlQuery(sql);
  const rolePermission = res.map((item) => {
    const isParentPermission = item.role_id === role_id ? 0 : 1;
    return {
      id: item.id,
      role_id: item.role_id,
      permission_module_id: item.permission_module_id,
      isParentPermission,
    };
  });
  resSuccess({ reply, result: rolePermission });
};
