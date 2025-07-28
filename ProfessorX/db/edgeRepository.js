const { getList, update, getOne } = require('./dbHelper');
const { resError } = require('../helper/handle');
const { isUndefinedOrNull, config } = require('../helper/repositoryBase');

const edgeTable = 'rmbt_edge';
const deviceTable = 'rmbt_device';

const queryEdgeColumn = [
  { edgeID: 'id' },
  'type',
  'category',
  'mfid',
  'name',
  'longitude',
  'latitude',
  'altitude',
  'address',
  'ip',
  'port',
  'tag',
  'version',
  'remark',
];
const queryDeviceColumn = [
  'id',
  { edgeID: 'edge_id' },
  'mfid',
  'type',
  'template_id',
  { moduleType: 'module_type' },
  { moduleCategory: 'module_category' },
  { moduleState: 'module_state' },
  { supportedFeatures: 'supported_features' },
  { displayName: 'name' },
  { description: 'remark' },
  'manufacturer',
  'model',
  'maximumInstance',
  'sn',
  'version',
  { constraintScript: 'constraint_script' },
  'parameters',
  { updateTime: 'update_time' },
  { createTime: 'create_time' },
];

function handleTagProperty(edges) {
  const newEdges = edges.map((edge) => {
    const tagObject = JSON.parse(edge.tag);
    let item = {};
    if (!isUndefinedOrNull(tagObject)) {
      item = { ...edge, ...tagObject };
    } else {
      item = edge;
    }
    // es6 删除 tag 属性
    const { tag, ...newEdge } = item;
    return newEdge;
  });
  return newEdges;
}

function convertModules(modules) {
  const newModules = modules.map((module) => {
    module.moduleState =
      module.moduleState === 'disabled' ? 'disabled' : 'offline';
    const newModule = {
      id: module.id,
      edgeID: module.edgeID,
      mfid: module.mfid,
      type: module.type,
      moduleType: module.moduleType,
      moduleCategory: JSON.parse(module.moduleCategory),
      moduleState: module.moduleState,
      supportedFeatures: JSON.parse(module.supportedFeatures),
      displayName: module.displayName,
      description: module.description,
      manufacturer: module.manufacturer,
      model: module.model,
      maximumInstance: module.maximumInstance,
      sn: module.sn,
      version: module.version,
      constraintScript: module.constraintScript,
      parameters: JSON.parse(module.parameters),
      createTime: module.createTime,
      updateTime: module.updateTime,
    };
    return newModule;
  });
  return newModules;
}

async function getEdgeAttach(edges) {
  const promises = edges.map(async (edge) => {
    const modules = await getList({
      tableName: deviceTable,
      wheres: { edge_id: edge.edgeID },
      queryColumn: queryDeviceColumn,
    });
    edge.modules = convertModules(modules);
    edge.isActive = false;
    edge.lastUpdateTime = null;
  });
  return Promise.all(promises);
}

/**
 * 获取边缘端
 * @param {边缘端ID} edgeId
 * @param {是否获取模块，默认true} module
 */
async function getEdge(edgeId, module = true) {
  const edges = await getList({
    tableName: edgeTable,
    wheres: { id: edgeId },
    queryColumn: queryEdgeColumn,
  });
  if (edges.length === 0) {
    return null;
  }
  if (module) {
    await getEdgeAttach(edges);
  }
  const newEdges = handleTagProperty(edges);
  return newEdges[0];
}

async function getEdges() {
  const edges = await getList({
    tableName: edgeTable,
    queryColumn: queryEdgeColumn,
  });
  await getEdgeAttach(edges);
  const newEdges = handleTagProperty(edges);
  return newEdges;
}

async function getEdgeSchedules() {
  const edges = await getList({ tableName: edgeTable });
  const syncSchedules = edges.map((edge) => {
    const schedule = {
      id: edge.id,
      name: edge.name,
      syncSchedule: JSON.parse(edge.sync_schedule),
      ip: edge.ip,
    };
    return schedule;
  });
  return syncSchedules;
}

async function getConfiguration(withAttach) {
  const edges = await getOne({ tableName: edgeTable, wheres: withAttach });
  if (edges.length === 0) {
    resError({ message: '对应的监测站信息不存在' });
  }
  const [edge] = edges;
  const schedule = {
    id: edge.id,
    name: edge.name,
    syncSchedule: JSON.parse(edge.sync_schedule),
    ip: edge.ip,
  };
  return schedule;
}

async function updateEdgeSchedules(edgeValues) {
  const promises = edgeValues.map(async (edgeValue) => {
    const updateModel = {
      tableName: edgeTable,
      mainData: { sync_schedule: JSON.stringify(edgeValue.syncSchedule) },
      wheres: { id: edgeValue.id },
    };
    await update(updateModel);
  });
  return Promise.all(promises);
}

async function getControlModules() {
  const modules = await getList({
    tableName: deviceTable,
    wheres: { module_category: '["control"]', module_type: 'device' },
    queryColumn: queryDeviceColumn,
  });
  const devices = convertModules(modules);
  return devices;
}

const getControlEdges = async () => {
  const controlModules = await getControlModules();
  const edges = [
    {
      edgeID: config.edge.controlEdgeID[0],
      version: '',
      name: '环境控制服务',
      ip: '',
      isActive: false,
      lastUpdateTime: null,
      modules: controlModules,
    },
  ];

  // 配置版本号为环境控制模块添加时间及更新时间最大值（环境控制边缘段最后更新时间）
  edges.forEach((controlEdge) => {
    const arrTime = [];
    controlEdge.modules.forEach((element) => {
      const time =
        element.updateTime == null ? element.createTime : element.updateTime;
      arrTime.push(new Date(time).getTime());
    });
    const version = Math.max.apply(null, arrTime);
    controlEdge.version = version.toString();
  });
  return edges;
};

async function updateEdge(edgeValue) {
  const updateModel = {
    tableName: edgeTable,
    mainData: { ip: edgeValue.ip },
    wheres: { id: edgeValue.id },
  };
  await update(updateModel);
}

module.exports = {
  getEdge,
  getEdges,
  getEdgeSchedules,
  getConfiguration,
  updateEdgeSchedules,
  getControlEdges,
  // getControlModules,
  updateEdge,
};
