const msgpack = require('msgpack-lite');
const { getEdgeConn, getEdge, edgeEmitter } = require('./controlManager');
const { addRemoteControl } = require('../controllers/log/logRemoteControl');
const { getLogger } = require('../helper/log4jsHelper');
const { config } = require('../helper/repositoryBase');
const { requestReply } = require('./clientManager');

const logger = getLogger('remoteControl');
const msgConn = {};
const requestSubject = '环境控制';

const errorMsg = {
  jsonrpc: '2.0',
  id: 258,
  error: { message: '' },
};

/**
 * 处理环境控制请求
 * @param {*} clientConnID
 * @param {*} decodeMsg
 * @return {*}
 */
const envMessageHandler = async (clientConnID, decodeMsg) => {
  const mainData = [];
  if (decodeMsg && decodeMsg.params) {
    decodeMsg.params.parameters.forEach((element) => {
      const data = {
        device_id: decodeMsg.params.moduleID,
        switch_name: element.name,
        switch_display_name: element.displayName,
        switch_state: element.value,
        account: '',
        create_time: new Date(),
      };
      mainData.push(data);
    });
    addRemoteControl(mainData);
  }

  const edgeID = config.edge.controlEdgeID[0];
  const edge = await getEdge(edgeID);
  // if (!edge) {
  //   errorMsg.id = decodeMsg.id;
  //   errorMsg.error.message = `边缘端为“${edgeID}”不存在`;
  //   requestReply(clientConnID, errorMsg, requestSubject);
  //   return;
  // }

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
      edgeConn.send(msgpack.encode(newDecodeMsg));
      msgConn[decodeMsg.id] = clientConnID;
    } catch (error) {
      errorMsg.error.message = `向边缘端${edge.name}发送数据失败：${error.message}`;
      requestReply(clientConnID, errorMsg, requestSubject);
    }
  } else {
    errorMsg.error.message = `向边缘端${edge.name}发送数据失败：与边缘端连接不存在`;
    requestReply(clientConnID, errorMsg, requestSubject);
  }
};

const envInit = async () => {
  edgeEmitter.on('controlReply', (edgeID, decodeMsg) => {
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
  envInit,
  envMessageHandler,
};
