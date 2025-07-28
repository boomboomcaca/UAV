/* eslint-disable no-param-reassign */
// 基础操作 添加删除 修改 基于数据库二次封装
// eslint-disable-next-line no-param-reassign
const {
  insert,
  edit,
  remove,
  getPageData,
  isUndefinedOrNull,
  getFlakeId,
  resSuccess,
  autoMap,
  execSql,
  config,
  setJson,
  getJson,
  deleteKey,
} = require('../../helper/repositoryBase');
const { getEdge } = require('../../db/edgeRepository');

// 缓存基本操作
// 站点如果涉及成多个表其实不好组合
// 站点管理
// node js 雪花算法 https://github.com/T-PWK/flake-idgen
let param = {
  tableName: 'rmbt_edge',
  primaryKey: 'id',
  tableChineseName: '边缘端',
  // 主键类型，区分 int
  primaryKeyType: 'GUID',
  entityMap: [
    { edgeid: 'id' },
    { edgeID: 'id' },
    'id',
    'type',
    'category',
    'mfid',
    'name',
    'ip',
    'port',
    'longitude',
    'latitude',
    'altitude',
    'address',
    'version',
    'remark',
    'tag',
    'create_time',
    'update_time',
  ],
  uniqueKey: 'mfid',
  // dicKeys: [config.edge.edgesCacheKey],
};

const edgeProperties = {
  id: { type: 'string', description: 'id' },
  type: { type: 'string', description: '站点类型(固定站、移动站等)' },
  category: { type: 'string', description: '站点分类' },
  mfid: { type: 'string', description: '边缘端ID' },
  name: { type: 'string', description: '边缘端名称' },
  ip: {
    type: 'string',
    description: 'IP地址',
    pattern:
      '^((25[0-5]|2[0-4]\\d|[1]{1}\\d{1}\\d{1}|[1-9]{1}\\d{1}|\\d{1})($|(?!\\.$)\\.)){4}$',
  },
  port: { type: 'string', description: '端口' },
  longitude: { type: 'number', description: '经度' },
  latitude: { type: 'number', description: '纬度' },
  altitude: { type: 'number', description: '海拔' },
  address: { type: 'string', description: '安装位置' },
  remark: { type: 'string', description: '备注' },
  // 处理Tag Tag 加上判断你提供的参数不在的我就留全部给你加到Tag里面去
  // 可以考虑对前端没有Tag而言 我后端处理 根据加上就可以了
};
exports.addSchema = {
  description: `添加站点`,
  tags: [param.tableName],
  summary: `添加站点`,
  body: {
    type: 'object',
    properties: edgeProperties,
  },
};
exports.updateSchema = {
  description: `更新站点`,
  tags: [param.tableName],
  summary: `更新站点`,
  body: {
    type: 'object',
    properties: edgeProperties,
  },
};
exports.deleteSchema = {
  description: `删除站点`,
  tags: [param.tableName],
  summary: `删除站点`,
  body: {
    type: 'object',
    properties: {
      id: { type: 'string', description: '站点id' },
    },
  },
};
/**
 *获取请求参数
 *
 * @param {*} req
 * @param {*} reply
 */
function reqParam(req, reply, isReply = true) {
  req.body = autoMap(req.body, param.entityMap);
  param = { ...param, req, reply, isReply };
  return param;
}

function addBefore(data) {
  // 把多余的字段全部写入到tag;
  // version
  data.version = getFlakeId().toString();
  const dataAttribute = Object.entries(data);
  const tagAttribute = dataAttribute.filter(
    (s) => !param.entityMap.includes(s[0])
  );
  const tag = {};
  // 考虑不用delete
  tagAttribute.forEach((item) => {
    const [key, value] = item;
    tag[key] = value;
    delete data[key];
  });
  // 键值对重新转换成为对象
  data.tag = JSON.stringify(tag);
}

function updateBefore(req) {
  addBefore(req.body);
}

/**
 *添加对象
 *
 * @param {*} req
 * @param {*} reply
 */
exports.add = async (req, reply) => {
  addBefore(req.body);
  const result = await insert(reqParam(req, reply, false));
  [req.body.id] = result;
  // 更新主键缓存
  const edgeIds = await getJson(config.edge.edgesCacheKey);
  if (!edgeIds.includes(req.body.id)) {
    edgeIds.push(req.body.id);
    await setJson(config.edge.edgesCacheKey, edgeIds);
  }
  const edgeNew = await getEdge(req.body.id);
  await setJson(config.edge.edgesCacheKey + req.body.id, edgeNew);
  resSuccess({ reply, result });
};
/**
 *更新对象
 *
 * @param {*} req
 * @param {*} reply
 */
exports.update = async (req, reply) => {
  updateBefore(req);
  const result = await edit(reqParam(req, reply, false));

  const edge = await getJson(config.edge.edgesCacheKey + req.body.id);
  if (edge) {
    const edgeNew = await getEdge(req.body.id, false);
    edgeNew.modules = edge.modules;
    edgeNew.ip = edge.ip;
    edgeNew.isActive = edge.isActive;
    edgeNew.lastUpdateTime = edge.lastUpdateTime;

    await setJson(config.edge.edgesCacheKey + req.body.id, edgeNew);
  } else {
    const edgeNew = await getEdge(req.body.id);
    await setJson(config.edge.edgesCacheKey + req.body.id, edgeNew);
  }

  resSuccess({ reply, result });
};

/**
 *根据输入条件删除
 *
 * @param {*} req
 * @param {*} reply
 */
exports.del = async (req, reply) => {
  // 删除边缘端的时候进行级联删除 删除device
  // 直接使用Lamda 表达式进行级联删除
  // todo：优化使用事务
  const sql = `delete from rmbt_device where edge_id='${req.body.id}'`;
  await execSql(sql);
  const result = await remove(reqParam(req, reply, false));
  // 删除该边缘端
  deleteKey(config.edge.edgesCacheKey + req.body.id);
  // 删除主键缓存
  const edgeIds = await getJson(config.edge.edgesCacheKey);
  if (edgeIds.includes(req.body.id)) {
    edgeIds.remove(req.body.id);
    await setJson(config.edge.edgesCacheKey, edgeIds);
  }

  // 若该边缘端包含环境控制模块，移除；通知环境控制边缘端
  let include = false;
  const controlEdge = await getJson(
    config.edge.controlsCacheKey + config.edge.controlEdgeID[0]
  );
  controlEdge.modules.forEach((module) => {
    if (module.edgeID === req.body.id) {
      controlEdge.modules.remove(module);
      include = true;
    }
  });
  if (include) {
    await setJson(
      config.edge.controlsCacheKey + config.edge.controlEdgeID[0],
      controlEdge
    );
  }

  resSuccess({ reply, result });
};
/**
 *获取分页列表 通用封装getList
 *
 * @param {*} req
 * @param {*} reply
 */
exports.getList = async (req, reply) => {
  let par = reqParam(req, reply);
  par = { ...par, isReply: false };
  const listData = await getPageData(par);
  listData.rows = listData.rows.map((s) => {
    if (!isUndefinedOrNull(s.tag)) {
      s = { ...s, ...JSON.parse(s.tag) };
    }
    delete s.tag;
    return s;
  });
  // 我需要对这个数据进行处理
  resSuccess({ reply, result: listData.rows, total: listData.total });
};
