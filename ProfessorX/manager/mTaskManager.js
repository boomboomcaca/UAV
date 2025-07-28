const IntersectionPositioning = require('../jointMeasure/intersectionPositioning');
const TDOA = require('../jointMeasure/tdoa');
const POA = require('../jointMeasure/poa');
const { edgeEmitter } = require('./edgeManager');
const { requestReply, clientEmitter } = require('./clientManager');
const { getLogger } = require('../helper/log4jsHelper');
const { getSingle } = require('../helper/repositoryBase');

const logger = getLogger('mTaskManager');
const msgIDCloudTaskIDMap = {};
const cloudTaskIDTaskMap = {};
const clientConnIDTaskMap = {};
const requestSubject = '多站任务';

/**
 * 清理任务数据
 * @param {string} clientConnID
 * @param {string} cloudTaskID
 */
function clearTaskData(clientConnID, cloudTaskID) {
  delete cloudTaskIDTaskMap[cloudTaskID];
  if (clientConnIDTaskMap[clientConnID] === cloudTaskID) {
    delete clientConnIDTaskMap[clientConnID];
  }
  const keys = Object.keys(msgIDCloudTaskIDMap);
  keys.forEach((element) => {
    if (msgIDCloudTaskIDMap[element] === cloudTaskID) {
      delete msgIDCloudTaskIDMap[element];
    }
  });
}

/**
 * 自动停止任务
 * @param {string} clientConnID
 * @param {string} cloudTaskID
 */
function autoStopTask(clientConnID, cloudTaskID) {
  if (cloudTaskIDTaskMap[cloudTaskID]) {
    cloudTaskIDTaskMap[cloudTaskID].clear();
    clearTaskData(clientConnID, cloudTaskID);
  }
}

/**
 * 获取驱动模块
 * @param {*} moduleID 驱动模块ID
 * @returns 驱动模块
 */
async function getModule(moduleID) {
  const module = await getSingle({
    tableName: 'rmbt_device',
    wheres: { id: moduleID, module_type: 'driver' },
  });
  return module;
}

/**
 * 校验模块ID
 * @param {*} decodeMsg 解码消息
 * @returns 校验结果
 */
async function validateModule(decodeMsg) {
  for (let i = 0; i < decodeMsg.params.edgeList.length; i++) {
    const module = await getModule(decodeMsg.params.edgeList[i].moduleID);
    if (!module) {
      return `moduleID: ${decodeMsg.params.edgeList[i].moduleID} 错误`;
    }
    const supportedFeatures = JSON.stringify(module.supported_features);
    switch (decodeMsg.params.feature) {
      case 'tdoa': {
        if (!supportedFeatures.includes('tdoa')) {
          return `moduleID: ${decodeMsg.params.edgeList[i].moduleID} 错误`;
        }
        break;
      }
      case 'poa': {
        if (!supportedFeatures.includes('ffm')) {
          return `moduleID: ${decodeMsg.params.edgeList[i].moduleID} 错误`;
        }
        break;
      }
      case 'intersectionPositioning': {
        if (!supportedFeatures.includes('ffdf')) {
          return `moduleID: ${decodeMsg.params.edgeList[i].moduleID} 错误`;
        }
        break;
      }
      default:
        break;
    }
  }
  return null;
}

/**
 * 处理多站任务请求
 * @param {*} clientConnID
 * @param {*} decodeMsg
 */
const mTaskMessageHandler = async (clientConnID, decodeMsg) => {
  if (decodeMsg.method === 'startTask') {
    if (clientConnIDTaskMap[clientConnID]) {
      const message = `不允许启动多个多站任务。`;
      logger.error(message);
      const toClientMessage = {
        jsonrpc: '2.0',
        id: decodeMsg.id,
        error: { code: 500, message },
      };
      requestReply(clientConnID, toClientMessage, requestSubject);
      return;
    }
    const validateResult = await validateModule(decodeMsg);
    if (validateResult) {
      logger.error(validateResult);
      const toClientMessage = {
        jsonrpc: '2.0',
        id: decodeMsg.id,
        error: { code: 500, message: validateResult },
      };
      requestReply(clientConnID, toClientMessage, requestSubject);
      return;
    }
    let jMeasure;
    switch (decodeMsg.params.feature) {
      case 'tdoa':
        jMeasure = new TDOA();
        break;
      case 'poa':
        jMeasure = new POA();
        break;
      case 'intersectionPositioning':
        jMeasure = new IntersectionPositioning();
        break;
      default:
        break;
    }
    if (jMeasure) {
      jMeasure.on('taskReply', (message) => {
        logger.debug(message);
        // 任务回复
        requestReply(clientConnID, message, requestSubject);
      });
      const success = await jMeasure.start(decodeMsg);
      if (success) {
        msgIDCloudTaskIDMap[decodeMsg.id] = jMeasure.cloudTaskID;
        cloudTaskIDTaskMap[jMeasure.cloudTaskID] = jMeasure;
        clientConnIDTaskMap[clientConnID] = jMeasure.cloudTaskID;
        clientEmitter.on(`${clientConnID}controlRequestClose`, () => {
          autoStopTask(clientConnID, jMeasure.cloudTaskID);
        });
        clientEmitter.on(
          'data'.concat(jMeasure.cloudTaskID).concat('RequestClose'),
          () => {
            autoStopTask(clientConnID, jMeasure.cloudTaskID);
          }
        );
      }
    } else {
      const message = `请求功能错误:${decodeMsg.params.feature}`;
      logger.error(message);
      const toClientMessage = {
        jsonrpc: '2.0',
        id: decodeMsg.id,
        error: { code: 500, message },
      };
      requestReply(clientConnID, toClientMessage, requestSubject);
    }
  } else {
    const jMeasure = cloudTaskIDTaskMap[decodeMsg.params.id];
    if (jMeasure) {
      if (decodeMsg.method === 'stopTask') {
        await jMeasure.stop(decodeMsg);
        clearTaskData(clientConnID, jMeasure.cloudTaskID);
      } else if (decodeMsg.method === 'addEdge') {
        msgIDCloudTaskIDMap[decodeMsg.id] = jMeasure.cloudTaskID;
        await jMeasure.addEdge(decodeMsg);
      } else if (decodeMsg.method === 'removeEdge') {
        msgIDCloudTaskIDMap[decodeMsg.id] = jMeasure.cloudTaskID;
        await jMeasure.removeEdge(decodeMsg);
      } else if (decodeMsg.method === 'setTaskParameters') {
        msgIDCloudTaskIDMap[decodeMsg.id] = jMeasure.cloudTaskID;
        await jMeasure.setParameters(decodeMsg);
      } else {
        const toClientMessage = {
          jsonrpc: '2.0',
          id: decodeMsg.id,
          error: { code: 500, message: `请求方法错误:${decodeMsg.method}` },
        };
        requestReply(clientConnID, toClientMessage, requestSubject);
      }
    } else {
      const toClientMessage = {
        jsonrpc: '2.0',
        id: decodeMsg.id,
        error: { code: 500, message: '任务未创建' },
      };
      requestReply(clientConnID, toClientMessage, requestSubject);
    }
  }
};

const mTaskInit = async () => {
  edgeEmitter.on('taskReply', async (edgeID, decodeMsg) => {
    // 边缘端回复的消息（任务控制回复）
    const cloudTaskID = msgIDCloudTaskIDMap[decodeMsg.id];
    if (cloudTaskID) {
      const jMeasure = cloudTaskIDTaskMap[cloudTaskID];
      if (jMeasure) {
        await jMeasure.receiveFromEdgeMessage(edgeID, decodeMsg);
      }
    }
  });
};

module.exports = {
  mTaskInit,
  mTaskMessageHandler,
};
