const events = require('events');
const msgpack = require('msgpack-lite');
const ReplayService = require('../service/replayService');
const {
  requestReply,
  clientEmitter,
  getClientConnByType,
} = require('./clientManager');
const { getLogger } = require('../helper/log4jsHelper');
const logRepository = require('../db/logRepository');

const logger = getLogger('replayManager');
const replayIDServiceMap = {};
const requestSubject = '数据回放';
const syncEmitter = new events.EventEmitter();

syncEmitter.on('sync', (data) => {
  const resSync = {
    type: 'sync',
    syncCode: data.syncCode, // 200：成功 500：失败
    sourceFile: data.sourceFile,
  };

  if (resSync.syncCode === 200) {
    resSync.rate = data.rate;
  } else {
    resSync.message = data.message;
  }

  const syncMessage = {
    jsonrpc: '2.0',
    id: 0,
    result: {
      edgeID: data.edgeID,
      timestamp: new Date().getTime() * 1000000,
      dataCollection: [resSync],
    },
  };
  const connUndefined = getClientConnByType('notify');
  if (connUndefined) {
    connUndefined.forEach((elementItem) => {
      elementItem.send(msgpack.encode(syncMessage));
    });
  }
});

async function getReplayMessage(fileID) {
  const fileInfo = await logRepository.getReplayFileInfo(fileID);
  if (!fileInfo.id) {
    return '文件不存在';
  }
  if (fileInfo.sync_time && fileInfo.sync_time >= fileInfo.update_time) {
    return null;
  }
  try {
    await logRepository.syncFile(
      fileInfo.edgeID,
      fileInfo.path,
      fileInfo.sourceFile
    );
  } catch (err) {
    logger.error(err);
    return `文件同步请求失败:${err.message}`;
  }
  return '文件同步请求成功';
}

async function replayMessageHandler(clientConnID, decodeMsg) {
  if (decodeMsg.method === 'presetReplay') {
    const replayMsg = await getReplayMessage(decodeMsg.params.fileID);
    if (!replayMsg) {
      const replayService = new ReplayService();
      replayService.on('replayReply', (message) => {
        logger.debug(message);
        requestReply(clientConnID, message, requestSubject);
      });
      replayIDServiceMap[replayService.cloudReplayID] = replayService;
      clientEmitter.on(
        'data'.concat(replayService.cloudReplayID).concat('RequestClose'),
        () => {
          const innerReplayService =
            replayIDServiceMap[replayService.cloudReplayID];
          if (innerReplayService) {
            innerReplayService.clear();
            delete replayIDServiceMap[replayService.cloudReplayID];
          }
        }
      );
      clientEmitter.on(
        'data'.concat(replayService.cloudReplayID).concat('RequestError'),
        () => {
          const innerReplayService =
            replayIDServiceMap[replayService.cloudReplayID];
          if (innerReplayService) {
            innerReplayService.clear();
            delete replayIDServiceMap[replayService.cloudReplayID];
          }
        }
      );
      await replayService.preset(decodeMsg);
    } else {
      const toClientMessage = {
        jsonrpc: '2.0',
        id: decodeMsg.id,
        error: { code: 500, message: replayMsg },
      };
      logger.debug(toClientMessage);
      requestReply(clientConnID, toClientMessage, requestSubject);
    }
  } else {
    const replayService = replayIDServiceMap[decodeMsg.params.id];
    if (replayService) {
      if (!replayService.taskStopped()) {
        if (decodeMsg.method === 'startReplay') {
          await replayService.start(decodeMsg);
        } else if (decodeMsg.method === 'pauseReplay') {
          await replayService.pause(decodeMsg);
        } else if (decodeMsg.method === 'continueReplay') {
          await replayService.goOn(decodeMsg);
        } else if (decodeMsg.method === 'setReplayParameters') {
          await replayService.setParameters(decodeMsg);
        } else if (decodeMsg.method === 'singleReplay') {
          await replayService.singleReplay(decodeMsg);
        } else if (decodeMsg.method === 'stopReplay') {
          await replayService.stop(decodeMsg);
          delete replayIDServiceMap[replayService.cloudReplayID];
        } else {
          const toClientMessage = {
            jsonrpc: '2.0',
            id: decodeMsg.id,
            error: { code: 500, message: `请求方法错误:${decodeMsg.method}` },
          };
          requestReply(clientConnID, toClientMessage, requestSubject);
        }
      }
    } else {
      const toClientMessage = {
        jsonrpc: '2.0',
        id: decodeMsg.id,
        error: { code: 500, message: '回放服务未创建' },
      };
      logger.debug(toClientMessage);
      requestReply(clientConnID, toClientMessage, requestSubject);
    }
  }
}

module.exports = {
  replayMessageHandler,
  syncEmitter,
};
