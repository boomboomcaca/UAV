// 基础操作 添加删除 修改 基于数据库二次封装
const {
  autoMap,
  getSingle,
  add,
  addList,
  update,
  remove,
  getPageData,
  resSuccess,
  resError,
} = require('../../helper/repositoryBase');
const { getTask } = require('../task/taskController');

const tableName = 'log_task_info';

const entityMap = [
  'id',
  { edgeID: 'edge_id' },
  { deviceID: 'device_id' },
  { edgeFuncID: 'edge_func_id' },
  { planID: 'plan_id' },
  'name',
  'params',
  { startTime: 'start_time' },
  { stopTime: 'stop_time' },
  { workTime: 'work_time' },
  'account',
];

exports.getSchema = {
  description: `查询任务信息`,
  summary: `查询任务信息`,
  tags: [tableName],
  query: {
    type: 'object',
    properties: {
      id: { type: 'string', description: 'ID' },
    },
  },
};

exports.deleteSchema = {
  description: `删除任务信息`,
  tags: [tableName],
  summary: `删除任务信息`,
  body: {
    type: 'object',
    properties: {
      id: { type: 'string', description: '任务信息id' },
    },
  },
};

/**
 * 添加任务基本信息
 * @param {*} taskInfos
 */
exports.addTaskInfos = async (taskInfos) => {
  const mainData = [];
  taskInfos.forEach((element) => {
    const task = autoMap(element, entityMap);
    mainData.push(task);
  });
  await addList({ tableName, mainData });
};

/**
 * 停止任务时，更新任务信息（边缘端离线时，计划任务信息可能未发送到云端）
 * @param {*} taskInfo
 */
exports.updateTaskInfo = async (taskInfo) => {
  const data = autoMap(taskInfo, entityMap);
  const task = await getSingle({ tableName, wheres: { id: taskInfo.id } });
  if (task) {
    await update({
      tableName,
      mainData: data,
      wheres: { id: data.id },
    });
  } else {
    await add({ tableName, mainData: data });
  }
};

exports.get = async (req, reply) => {
  let data = await getSingle({ tableName, wheres: { id: req.body.id } });
  data = autoMap(data, entityMap, true);
  resSuccess({ reply, result: data });
};

/**
 *根据输入条件删除
 *
 * @param {*} req
 * @param {*} reply
 */
exports.del = async (req, reply) => {
  const task = await getTask(req.body.id);
  if (task) {
    resError({ message: '不能删除正在执行的任务信息！' });
  }
  await remove({
    req,
    reply,
    tableName,
    tableChineseName: '任务信息',
  });
};

exports.getList = async (req, reply) => {
  await getPageData({ req, reply, entityMap, tableName, isLimitMaxRow: false });
};
