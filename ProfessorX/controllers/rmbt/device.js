// 基础操作 添加删除 修改 基于数据库二次封装
const { resError } = require('../../helper/handle');
const {
  insert,
  edit,
  remove,
  getPageData,
  getDetail,
  isUndefinedOrNull,
  update,
  dataTypeEnum,
  getGUID,
  resSuccess,
  getDataType,
  autoMap,
  sqlQuery,
  getFlakeId,
  tryParseJson,
  config,
  setJson,
  getJson,
} = require('../../helper/repositoryBase');
const { getEdge, getControlEdges } = require('../../db/edgeRepository');

// 站点如果涉及成多个表其实不好组合
let param = {
  tableName: 'rmbt_device',
  primaryKey: 'id',
  tableChineseName: '设备',
  // 主键类型
  primaryKeyType: 'GUID',
  // entityMap:[]
  // 实体映射
  entityMap: [
    'id',
    'mfid',
    'type',
    'manufacturer',
    'model',
    'maximumInstance',
    'sn',
    'version',
    'create_time',
    'update_time',
    { edgeid: 'edge_id' },
    { templateID: 'template_id' },
    { moduleType: 'module_type' },
    { moduleCategory: 'module_category' },
    { moduleState: 'module_state' },
    { displayName: 'name' },
    { description: 'remark' },
    { constraintScript: 'constraint_script' },
    { supportedFeatures: 'supported_features' },
    'supported_features',
    'template_id',
    'module_type',
    'module_category',
    'module_state',
    'name',
    'remark',
    'constraint_script',
  ],
  // uniqueKey: 'name',
  unionKey: ['name', 'edge_id'],
  // dicKeys: [config.edge.edgesCacheKey],
};
// 处理必填值问题
// 默认值问题
// 其实我不用转换直接要求边缘端不要传入无关字段就行！ 这样就可以直接写入数据库！
const deviceProperties = {
  id: { type: 'string', description: '设备ID' },
  edgeid: { type: 'string', description: '边缘端id' },
  mfid: { type: 'string', description: 'MFID' },
  template_id: { type: 'string', description: '模板ID' },
  type: { type: 'string', description: '' },
  module_type: { type: 'string', description: '' },
  module_category: {
    type: 'array',
    description: '',
    items: { type: 'string' },
  },
  module_state: { type: 'string', description: '' },
  supported_features: {
    type: 'array',
    description: '',
    items: { type: 'string' },
  },
  name: { type: 'string', description: '设备显示名称' },
  remark: { type: 'string', description: '' },
  manufacturer: { type: 'string', description: '' },
  model: { type: 'string', description: '' },
  maximumInstance: { type: 'string', description: '' },
  sn: { type: 'string', description: '' },
  version: { type: 'string', description: '' },
  constraint_script: { type: 'string', description: '' },
  parameters: {
    type: 'array',
    description: '参数集合',
    items: { type: 'object' },
  },
};
exports.addSchema = {
  description: `添加设备`,
  tags: [param.tableName],
  summary: `添加设备 输入设备字段信息`,
  body: {
    type: 'object',
    properties: deviceProperties,
    required: ['edgeid', 'template_id', 'name'],
  },
};
exports.updateSchema = {
  description: `更新设备`,
  tags: [param.tableName],
  summary: `更新设备 输入设备字段信息`,
  body: {
    type: 'object',
    properties: deviceProperties,
    required: ['edgeid', 'id'],
  },
};
exports.deleteSchema = {
  description: `删除设备`,
  tags: [param.tableName],
  summary: `删除设备`,
  body: {
    type: 'object',
    properties: {
      id: { type: 'string', description: '设备id' },
    },
  },
};
exports.getParamSchema = {
  description: `获取设备参数`,
  tags: [param.tableName],
  summary: `获取设备参数`,
  query: {
    id: { type: 'string', description: '设备id' },
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

async function updateEdgeConfigVersion(edgeID) {
  const mainData = { version: getFlakeId() };
  const wheres = { id: edgeID };
  const tableName = 'rmbt_edge';
  await update({ tableName, mainData, wheres });

  return mainData.version;
}

async function updateCacheControlEdge(
  edgeID,
  deviceID,
  deviceData,
  removeModule = false
) {
  if (
    deviceData.module_category === '["control"]' &&
    deviceData.module_type === 'device'
  ) {
    // 环境控制设备无实时状态，故不用保留实时状态；数据库重新获取重置边缘端，保留缓存中边缘端状态部分即可
    const edge = await getJson(
      config.edge.controlsCacheKey + config.edge.controlEdgeID[0]
    );
    const [edgeNew] = await getControlEdges();
    edgeNew.ip = edge.ip;
    edgeNew.isActive = edge.isActive;
    edgeNew.lastUpdateTime = edge.lastUpdateTime;

    await setJson(config.edge.controlsCacheKey + edgeNew.edgeID, edgeNew);
  } else {
    // 设备功能需要更新版本号，环境控制模块不更新版本号
    const version = await updateEdgeConfigVersion(edgeID);
    // 缓存获取边缘端，移除旧模块添加新模块
    const edge = await getJson(config.edge.edgesCacheKey + edgeID);
    if (removeModule) {
      edge.modules.forEach((module) => {
        if (module.id === deviceID) {
          edge.modules.remove(module);
        }
      });
    }
    const edgeNew = await getEdge(edgeID);
    edgeNew.modules.forEach((module) => {
      if (module.id === deviceID) {
        edge.modules.push(module);
      }
    });
    edge.version = version;
    await setJson(config.edge.edgesCacheKey + edgeID, edge);
  }
}

/**
 *添加对象
 *
 * @param {*} req
 * @param {*} reply
 */
exports.add = async (req, reply) => {
  // 通用拦截在方法执行前代码在底层
  const par = reqParam(req, reply, false);
  const edgeOld = await getEdge(req.body.edge_id, false);
  if (edgeOld === null) {
    resError({ message: `不存在ID为${req.body.edge_id}的边缘端，无法添加！` });
  }
  const data = req.body;
  data.id = getGUID();
  // 添加的时候 edge_id 和
  // todo:不同的边缘端 可以允许 相同的设备名称
  if (!isUndefinedOrNull(data.parameters)) {
    data.parameters = JSON.stringify(data.parameters);
  }
  // 其实应该在addBefore 之前进行拦截
  data.module_category = JSON.stringify(data.module_category);
  data.supported_features = JSON.stringify(data.supported_features);

  // par.failMessage='添加失败';
  // 设备和功能都是共用一个表处理 添加失败的错误消息
  par.tableChineseName = data.module_type === 'device' ? '设备' : '功能';

  const result = await insert(par);

  await updateCacheControlEdge(req.body.edge_id, req.body.id, data);

  resSuccess({ reply, result });
};
/**
 *更新对象
 *
 * @param {*} req
 * @param {*} reply
 */
exports.update = async (req, reply) => {
  const par = reqParam(req, reply, false);
  const edgeOld = await getEdge(req.body.edge_id, false);
  if (edgeOld === null) {
    resError({ message: `不存在ID为${req.body.edge_id}的边缘端，无法更新！` });
  }
  const data = req.body;
  if (!isUndefinedOrNull(data.parameters)) {
    data.parameters = JSON.stringify(data.parameters);
  }

  const res = await getDetail(reqParam(req, reply, false));
  const item = res[0];

  par.tableChineseName = item.module_type === 'device' ? '设备' : '功能';
  const result = await edit(par);

  await updateCacheControlEdge(req.body.edge_id, req.body.id, item, true);

  resSuccess({ reply, result });
};
// 我本来就需要联查？
/**
 *根据输入条件删除
 *
 * @param {*} req
 * @param {*} reply
 */
exports.del = async (req, reply) => {
  // 删除和批量删除处理下
  // 直接从数据库里面拿出id
  // 直接从数据库查询对应的edgeId!
  const res = await getDetail(reqParam(req, reply, false));
  const item = res[0];

  const result = await remove(reqParam(req, reply, false));

  // const tableChineseName = item.module_type === 'device' ? '设备' : '功能';
  // 记录设备还是功能？

  if (item.module_category === '["control"]' && item.module_type === 'device') {
    // 环境控制设备无实时状态，故不用保留实时状态；数据库重新获取重置边缘端，保留缓存中边缘端状态部分即可
    const edge = await getJson(
      config.edge.controlsCacheKey + config.edge.controlEdgeID[0]
    );
    const [edgeNew] = await getControlEdges();
    // edgeNew.modules = edge.modules;
    edgeNew.ip = edge.ip;
    edgeNew.isActive = edge.isActive;
    edgeNew.lastUpdateTime = edge.lastUpdateTime;

    await setJson(config.edge.controlsCacheKey + edgeNew.edgeID, edgeNew);
  } else {
    // 设备功能需要更新版本号，环境控制模块不更新版本号
    const version = await updateEdgeConfigVersion(item.edge_id);

    // 缓存获取边缘端，移除旧模块
    const edge = await getJson(config.edge.edgesCacheKey + item.edge_id);
    edge.modules.forEach((module) => {
      if (module.id === req.body.id) {
        edge.modules.remove(module);
      }
    });
    edge.version = version;
    await setJson(config.edge.edgesCacheKey + item.edge_id, edge);
  }
  resSuccess({ reply, result });
};

async function getId(req, propertyName) {
  if (!isUndefinedOrNull(req.body[propertyName])) {
    let sql = '';
    // 判断一下输入的是否是数组
    if (getDataType(req.body[propertyName]) === dataTypeEnum.array) {
      sql = 'select id from rmbt_device where';
      req.body[propertyName].forEach((s) => {
        sql += ` ${propertyName} like '%${s}%' or`;
      });
      sql = sql.substring(0, sql.length - 2);
    } else {
      sql = `select id from rmbt_device where ${propertyName} like '%${req.body[propertyName]}%'`;
    }
    // 输入的是一个逗号隔开就是一个素
    const res = await sqlQuery(sql);
    delete req.body[propertyName];
    if (res.length > 0) {
      const ids = res.map((s) => s.id);
      return ids;
    }
  }
  return [];
}
/**
 *获取分页列表 通用封装getList
 *
 * @param {*} req
 * @param {*} reply
 */
exports.getList = async (req, reply) => {
  let par = { ...reqParam(req, reply, false) };
  if (!isUndefinedOrNull(req.body.reqSource) && req.body.reqSource === 'edge') {
    if (par.entityMap.indexOf('parameters') < 0) {
      par.entityMap.push('parameters');
    }
    delete req.body.reqSource;
  } else if (par.entityMap.indexOf('parameters') >= 0) {
    par = par.entityMap.pop();
  }
  // 特殊 module_category
  // 输入属性已经被删除了
  if (
    !isUndefinedOrNull(req.body.module_category) ||
    !isUndefinedOrNull(req.body.supported_features)
  ) {
    // 查询列表为空
    let ids = await getId(req, 'module_category');
    ids = ids.concat(await getId(req, 'supported_features'));
    if (ids.length > 0) {
      req.body.id = ids;
    } else {
      resSuccess({ reply, result: [], total: 0 });
      return;
    }
  }
  const listData = await getPageData(par);
  // const [rows,total]=listData;
  const { rows } = listData;
  const { total } = listData;
  const result = rows.map((s) => {
    const data = {
      ...s,
      parameters: tryParseJson(s.parameters),
      moduleCategory: tryParseJson(s.module_category),
      supportedFeatures: tryParseJson(s.supported_features),
    };
    data.module_category = data.moduleCategory;
    data.supported_features = data.supportedFeatures;
    return data;
  });

  resSuccess({ reply, result, total });
};

/**
 *通过id获取详情 req.query.id
 *
 * @param {*} req
 * @param {*} reply
 */
exports.getOne = async (req, reply) => {
  const res = await getDetail(reqParam(req, reply, false));
  if (res.length === 0) {
    resError({ message: '查询没有设备' });
  }
  let [item] = res;
  item = {
    ...item,
    parameters: tryParseJson(item.parameters),
    moduleCategory: tryParseJson(item.module_category),
    supportedFeatures: tryParseJson(item.supported_features),
  };
  item.module_category = item.moduleCategory;
  item.supported_features = item.supportedFeatures;
  resSuccess({ reply, result: item });
};
