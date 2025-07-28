const { resSuccess } = require('../../helper/repositoryBase');
const edgeManager = require('../../manager/edgeManager');

const tableName = 'log_task_info';

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

const getTask = async (id) => {
  const cacheTasks = await edgeManager.getTasks();
  return cacheTasks.find((t) => t.id === id);
};

exports.getTask = getTask;

/**
 * 查询正在运行的任务详细信息
 * @param {*} req
 * @param {*} reply
 */
exports.get = async (req, reply) => {
  // const cacheTasks = await edgeManager.getTasks();
  let task = await getTask(req.body.id); //  cacheTasks.find((t) => t.id === req.body.id);
  if (!task) {
    task = {};
  }
  resSuccess({ reply, result: task });
};

exports.getListSchema = {
  description: `查询任务信息`,
  summary: `查询任务信息`,
  tags: [tableName],
  query: {},
};

/**
 * 查询正在运行的任务基本信息
 * @param {*} req
 * @param {*} reply
 */
exports.getList = async (req, reply) => {
  const cacheTasks = await edgeManager.getTasks();
  cacheTasks.forEach((element) => {
    delete element.frontFuncID;
    delete element.parameters;
    delete element.dataEndpoint;
  });
  resSuccess({ reply, result: cacheTasks });
};
