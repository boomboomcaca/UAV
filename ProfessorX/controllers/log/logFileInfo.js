// 基础操作 添加删除 修改 基于数据库二次封装
const moment = require('moment');
const {
  remove,
  removeList,
  autoMap,
  getSingle,
  getPageData,
  insert,
  update,
  resSuccess,
  config,
} = require('../../helper/repositoryBase');
const { syncEmitter } = require('../../manager/replayManager');
const { updateFileInfo } = require('../../db/logRepository');

const tableName = 'log_file_info';

const entityMap = [
  'id',
  { edgeID: 'edge_id' },
  { taskID: 'task_id' },
  'params',
  'path',
  { recordCount: 'record_count' },
  { sourceFile: 'source_file' },
  'type',
  'filesize',
  'edge_func_id',
  'front_func_id',
  'data_start_time',
  'data_stop_time',
  'update_time',
  'sync_time',
];

exports.addSchema = {
  description: `添加文件信息`,
  tags: [tableName],
  summary: `文件`,
  body: {
    type: 'object',
    properties: {
      //  "id": 4,
      edgeID: { type: 'string', description: '边缘端ID' },
      taskID: { type: 'string', description: '任务ID' },
      params: { type: 'string', description: '主要参数' },
      path: { type: 'string', description: '源文件路径' },
      sourceFile: { type: 'string', description: '源文件路径' },
      type: { type: 'string', description: '文件类型' },
      recordCount: { type: 'number', description: '帧数' },
      filesize: { type: 'number', description: '文件大小' },
      edge_func_id: { type: 'string', description: '边缘端功能ID' },
      front_func_id: { type: 'string', description: '前端功能ID' },
      data_start_time: {
        type: 'string',
        description: '数据保存开始时间',
      },
      data_stop_time: {
        type: 'string',
        description: '数据保存结束时间',
      },
      update_time: {
        type: 'string',
        description: '文件最后更新时间',
      },
    },
  },
};

exports.updateSchema = {
  description: `更新文件信息`,
  tags: [tableName],
  summary: `更新文件信息`,
  body: {
    type: 'object',
    properties: {
      edgeID: { type: 'string', description: '边缘端ID' },
      sourceFile: { type: 'string', description: '源文件路径' },
      recordCount: { type: 'number', description: '帧数' },
      filesize: { type: 'number', description: '文件大小' },
      data_start_time: {
        type: 'string',
        description: '数据保存开始时间',
      },
      data_stop_time: {
        type: 'string',
        description: '数据保存结束时间',
      },
      update_time: {
        type: 'string',
        description: '文件最后更新时间',
      },
      sync_time: {
        type: 'string',
        description: '文件最后同步时间',
      },
    },
  },
};

exports.deleteSchema = {
  description: `删除文件信息`,
  tags: [tableName],
  summary: `删除文件信息`,
  body: {
    type: 'object',
    properties: {
      id: { type: 'number', description: 'ID' },
    },
  },
};

exports.getSchema = {
  description: `查询文件信息`,
  tags: [tableName],
  summary: `查询文件信息`,
  query: {
    type: 'object',
    properties: {
      id: { type: 'number', description: 'ID' },
    },
  },
};

exports.syncFileSchema = {
  description: `同步程序执行同步结果`,
  tags: [tableName],
  summary: `同步程序执行同步结果`,
  body: {
    type: 'object',
    properties: {
      syncType: { type: 'number', description: '1：手动 0：自动' },
      syncCode: { type: 'number', description: '200：成功 500：失败' },
      message: { type: 'string', description: 'Code为500时，异常信息' },
      data: {
        type: 'object',
        description: '文件数据',
        properties: {
          edgeID: { type: 'string', description: '边缘端 ID' },
          sourceFile: { type: 'string', description: '源文件路径' },
          rate: { type: 'string', description: '同步进度，格式：n%' },
          sync_time: {
            type: 'number',
            description: '文件最后同步时间',
          },
        },
      },
    },
  },
};

/**
 *添加对象
 *
 * @param {*} req
 * @param {*} reply
 */
/* istanbul ignore next */
exports.add = async (req, reply) => {
  const data = autoMap(req.body, entityMap);
  data.update_time = moment(data.update_time).format('YYYY-MM-DD HH:mm:ss');
  if (data.sync_time) {
    data.sync_time = moment(data.sync_time).format('YYYY-MM-DD HH:mm:ss');
  }
  data.data_start_time = moment(data.data_start_time).format(
    'YYYY-MM-DD HH:mm:ss'
  );
  data.data_stop_time = moment(data.data_stop_time).format(
    'YYYY-MM-DD HH:mm:ss'
  );
  if (config.SyncSystem.useSync === false && data.sync_time === undefined) {
    data.sync_time = data.update_time;
  }
  await insert({ req, reply, tableName });
};

/* istanbul ignore next */
exports.update = async (req, reply) => {
  const data = autoMap(req.body, entityMap);
  if (data.update_time) {
    data.update_time = moment(data.update_time).format('YYYY-MM-DD HH:mm:ss');
    data.data_start_time = moment(data.data_start_time).format(
      'YYYY-MM-DD HH:mm:ss'
    );
    data.data_stop_time = moment(data.data_stop_time).format(
      'YYYY-MM-DD HH:mm:ss'
    );
  }
  if (data.sync_time) {
    data.sync_time = moment(data.sync_time).format('YYYY-MM-DD HH:mm:ss');
  }
  if (config.SyncSystem.useSync === false && data.sync_time === undefined) {
    data.sync_time = data.update_time;
  }
  const result = await update({
    tableName,
    mainData: data,
    wheres: { edge_id: data.edge_id, source_file: data.source_file },
  });

  resSuccess({ reply, result });
};

/* istanbul ignore next */
exports.del = async (req, reply) => {
  await remove({ req, reply, tableName, tableChineseName: '' });
};

/**
 *批量删除
 *
 * @param {*} req
 * @param {*} reply
 */
exports.delList = async (req, reply) => {
  await removeList({ req, reply, tableName, tableChineseName: '文件' });
};

exports.get = async (req, reply) => {
  let data = await getSingle({ tableName, wheres: { id: req.body.id } });
  data = autoMap(data, entityMap, true);
  resSuccess({ reply, result: data });
};

exports.getList = async (req, reply) => {
  getPageData({ req, reply, tableName, entityMap, isLimitMaxRow: false });
};

/**
 * 同步程序执行rsync同步结果通知
 *
 * @param {*} req
 * @param {*} reply
 */
exports.syncFile = async (req, reply) => {
  // syncCode 200 同步成功 500 同步失败；syncType 1 手动同步，0 自动同步
  if (req.body.syncCode === 200) {
    req.body.data.sync_time = moment(
      new Date(req.body.data.sync_time / 1000000)
    ).format('YYYY-MM-DD HH:mm:ss');

    // 发送进度消息！！！
    syncEmitter.emit('sync', {
      syncCode: req.body.syncCode, // 200：成功 500：失败
      edgeID: req.body.data.edgeID,
      sourceFile: req.body.data.sourceFile,
      rate: req.body.data.rate,
    });

    if (req.body.data.rate === '100%') {
      delete req.body.syncType;
      delete req.body.data.rate;
      const data = await getSingle({
        tableName,
        wheres: {
          edge_id: req.body.data.edgeID,
          source_file: req.body.data.sourceFile,
        },
      });
      if (data != null && data.update_time > req.body.data.sync_time) {
        req.body.data.sync_time = data.update_time;
      }

      updateFileInfo(req.body.data);
    }
  } else {
    // 发送同步文件失败消息！！！
    syncEmitter.emit('sync', {
      syncCode: req.body.syncCode, // 200：成功 500：失败
      message: req.body.message, // Code为500时，异常信息
      edgeID: req.body.data.edgeID,
      sourceFile: req.body.data.sourceFile,
    });
  }

  resSuccess({ reply });
};
