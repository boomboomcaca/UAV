const msgpack = require('msgpack-lite');
const { getEdgeConn, getEdge, edgeEmitter } = require('./edgeManager');
const { requestReply } = require('./clientManager');
const { getLogger } = require('../helper/log4jsHelper');
const { getModule, addScanSegments } = require('./scanSegment');

const logger = getLogger('sTaskManager');
const errorMsg = {
  jsonrpc: '2.0',
  id: 0,
  error: { code: 500, message: '' },
};
const msgConn = {};
const driverModel = {};
const requestSubject = '单站任务';

/**
 * 处理单站任务请求
 * @param {*} clientConnID
 * @param {*} decodeMsg
 */
const sTaskMessageHandler = async (clientConnID, decodeMsg) => {
  const { edgeID } = decodeMsg.params;
  if (!edgeID) {
    errorMsg.id = decodeMsg.id;
    errorMsg.error.message = `edgeID参数错误:${edgeID}`;
    requestReply(clientConnID, errorMsg, requestSubject);
    return;
  }
  const edge = await getEdge(edgeID);
  if (!edge) {
    errorMsg.id = decodeMsg.id;
    errorMsg.error.message = `边缘端为“${edgeID}”不存在`;
    requestReply(clientConnID, errorMsg, requestSubject);
    return;
  }
  if (!edge.isActive) {
    errorMsg.id = decodeMsg.id;
    errorMsg.error.message = `边缘端“${edge.name}”离线，无法开启任务`;
    requestReply(clientConnID, errorMsg, requestSubject);
    return;
  }

  const edgeConn = await getEdgeConn(edgeID);
  if (edgeConn) {
    try {
      const newDecodeMsg = { ...decodeMsg };
      delete newDecodeMsg.params.requestType;
      delete newDecodeMsg.params.edgeID;
      edgeConn.send(msgpack.encode(newDecodeMsg));
      msgConn[decodeMsg.id] = clientConnID;
      // 拦截频段扫描设置频段参数，保存常用频段
      if (newDecodeMsg.method === 'presetTask') {
        const { moduleID } = newDecodeMsg.params;
        const module = await getModule(moduleID);
        if (module && module.module_type === 'driver') {
          driverModel[edgeID] = module.model;
        }
      } else if (newDecodeMsg.method === 'setTaskParameters') {
        if (driverModel[edgeID] === 'Scan') {
          let scanSegments;
          for (let i = 0; i < newDecodeMsg.params.parameters.length; i++) {
            if (newDecodeMsg.params.parameters[i].name === 'scanSegments') {
              scanSegments = newDecodeMsg.params.parameters[i].parameters;
              break;
            }
          }
          if (scanSegments) {
            await addScanSegments(scanSegments);
          }
        }
      } else if (newDecodeMsg.method === 'stopTask') {
        delete driverModel[edgeID];
      }
    } catch (error) {
      errorMsg.id = decodeMsg.id;
      errorMsg.error.message = `向边缘端${edge.name}发送数据失败：${error.message}`;
      requestReply(clientConnID, errorMsg, requestSubject);
    }
  } else {
    errorMsg.id = decodeMsg.id;
    errorMsg.error.message = `向边缘端${edge.name}发送数据失败：与边缘端连接不存在`;
    requestReply(clientConnID, errorMsg, requestSubject);
  }
};

const sTaskInit = async () => {
  edgeEmitter.on('taskReply', (edgeID, decodeMsg) => {
    // 单任务的消息才在这里处理
    const clientConnID = msgConn[decodeMsg.id];
    if (clientConnID) {
      logger.debug(`来自边缘端${edgeID}消息:${JSON.stringify(decodeMsg)}`);
      // 任务回复后删除消息ID与客户端ID关联关系
      delete msgConn[decodeMsg.id];
      requestReply(clientConnID, decodeMsg, requestSubject);
    }
  });
};

module.exports = {
  sTaskInit,
  sTaskMessageHandler,
};
