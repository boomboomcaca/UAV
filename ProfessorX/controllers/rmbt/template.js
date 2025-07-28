// 基础操作 添加删除 修改 基于数据库二次封装
const {
  insert,
  edit,
  getPageData,
  // removeList,
  getDetail,
  resError,
  isUndefinedOrNull,
  isJSON,
  resSuccess,
  // insertList,
  remove,
} = require('../../helper/repositoryBase');

// 指定查询那些字段//通过数据库动态查询出表的字段？在排查效率低了一点把
// 站点如果涉及成多个表其实不好组合
// 站点管理
let param = {
  tableName: 'rmbt_template',
  primaryKey: 'id',
  tableChineseName: '模板',
  entityMap: [
    'id',
    'name',
    'version',
    'remark',
    'module_type',
    'module_category',
    'create_time',
    'update_time',
  ],
  unionKey: ['name', 'version'],
};

exports.addSchema = {
  description: `添加模板`,
  tags: [param.tableName],
  summary: `添加模板`,
  body: {
    type: 'object',
    properties: {
      name: { type: 'string', description: '名称', maxLength: 100 },
      remark: { type: 'string', description: '备注', maxLength: 100 },
      template: { type: 'object', description: '参数集合' },
    },
    required: ['name'],
  },
};
exports.updateSchema = {
  description: `更新模板`,
  tags: [param.tableName],
  summary: `更新模板`,
  body: {
    type: 'object',
    properties: {
      id: { type: 'string', description: 'id' },
      name: { type: 'string', description: '名称', maxLength: 100 },
      remark: { type: 'string', description: '备注', maxLength: 100 },
      template: { type: 'object', description: '参数对象' },
    },
  },
};

exports.deleteSchema = {
  description: `删除模板`,
  tags: [param.tableName],
  summary: `删除模板`,
  body: {
    type: 'object',
    properties: {
      id: { type: 'string', description: '模板id' },
    },
  },
};
exports.getParamSchema = {
  description: `获取模板参数`,
  tags: [param.tableName],
  summary: `获取模板参数`,
  query: {
    id: { type: 'string', description: '模板id' },
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

function addBefore(req) {
  const data = {
    ...req.body,
    module_type: req.body.template.moduleType,
    module_category: req.body.template.moduleCategory,
    version: req.body.template.version,
  };
  if (isUndefinedOrNull(data.module_type) || isUndefinedOrNull(data.version)) {
    resError({ message: '模板错误' });
  }
  data.template = JSON.stringify(data.template);
  req.body = data;
}
function updateBefore(req) {
  addBefore(req);
}
/**
 *添加对象
 *
 * @param {*} req
 * @param {*} reply
 */
exports.add = async (req, reply) => {
  addBefore(req);
  await insert(reqParam(req, reply));
};
// /**
//  *批量添加对象
//  *
//  * @param {*} req
//  * @param {*} reply
//  */
// exports.addList = async (req, reply) => {
//   // 暂时不支持批量添加
//   await insertList(reqParam(req, reply));
// };
/**
 *更新对象
 *
 * @param {*} req
 * @param {*} reply
 */
exports.update = async (req, reply) => {
  // 更新模板
  updateBefore(req);
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
function getNewDeviceItem(item) {
  // a += 1;
  item.template = isJSON(item.template) ? JSON.parse(item.template) : null;
  return item;
}
/**
 *获取分页列表 通用封装getList
 *
 * @param {*} req
 * @param {*} reply
 */
exports.getList = async (req, reply) => {
  const listData = await getPageData({
    ...reqParam(req, reply),
    isReply: false,
  });
  listData.rows = listData.rows.map((s) => {
    return getNewDeviceItem(s);
  });
  resSuccess({ reply, result: listData.rows, total: listData.total });
};

/**
 *通过id获取详情 req.query.id
 *
 * @param {*} req
 * @param {*} reply
 */
exports.getOne = async (req, reply) => {
  const items = await getDetail({ ...reqParam(req, reply), isReply: false });
  if (items.length === 0) {
    resSuccess({ reply, result: {} });
    return;
  }
  const item = getNewDeviceItem(items[0]);
  resSuccess({ reply, result: item });
};
