const {
  autoMap,
  getSingle,
  add,
  update,
  config,
} = require('../helper/repositoryBase');
const { requestPost } = require('../helper/requestHelper');

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

exports.getReplayFileInfo = async (fileID) => {
  let data = await getSingle({ tableName, wheres: { id: fileID } });
  data = autoMap(data, entityMap, true);
  return data;
};

exports.syncFile = async (edgeID, path, sourceFile) => {
  const url = `http://${config.SyncSystem.host}:${config.SyncSystem.port}/SyncSystem/syncFile`;
  const result = await requestPost(url, { edgeID, path, sourceFile });
  return result;
};

exports.addFileInfo = async (fileInfo) => {
  if (config.SyncSystem.useSync === false) {
    fileInfo.sync_time = fileInfo.update_time;
  }
  const data = autoMap(fileInfo, entityMap);
  await add({ tableName, mainData: data });
};

exports.updateFileInfo = async (fileInfo) => {
  if (config.SyncSystem.useSync === false) {
    fileInfo.sync_time = fileInfo.update_time;
  }
  const data = autoMap(fileInfo, entityMap);
  await update({
    tableName,
    mainData: data,
    wheres: { edge_id: data.edge_id, source_file: data.source_file },
  });
};
