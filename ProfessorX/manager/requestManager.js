const msgpack = require('msgpack-lite');
const { clientEmitter, requestReply } = require('./clientManager');
const { sTaskInit, sTaskMessageHandler } = require('./sTaskManager');
const { mTaskInit, mTaskMessageHandler } = require('./mTaskManager');
const { replayMessageHandler } = require('./replayManager');
const { envInit, envMessageHandler } = require('./remoteControl');
const { getLogger } = require('../helper/log4jsHelper');
const { licenseWsAuth } = require('../license/licenseAuth');

const logger = getLogger('requestManager');

const init = async () => {
  await sTaskInit();
  await mTaskInit();
  await envInit();

  clientEmitter.on('controlRequest', async (clientConnID, encodeMsg) => {
    const decodeMsg = msgpack.decode(encodeMsg);
    try {
      await licenseWsAuth(decodeMsg.params, decodeMsg.method);
      logger.debug(`来自客户端${clientConnID}消息${JSON.stringify(decodeMsg)}`);

      if (decodeMsg.params.requestType === 'singleTask') {
        await sTaskMessageHandler(clientConnID, decodeMsg);
      } else if (decodeMsg.params.requestType === 'multipleTask') {
        await mTaskMessageHandler(clientConnID, decodeMsg);
      } else if (decodeMsg.params.requestType === 'replay') {
        await replayMessageHandler(clientConnID, decodeMsg);
      } else if (decodeMsg.params.requestType === 'environment') {
        await envMessageHandler(clientConnID, decodeMsg);
      } else {
        const toClientMessage = {
          jsonrpc: '2.0',
          id: decodeMsg.id,
          error: {
            code: 500,
            message: `请求类型错误:${decodeMsg.params.requestType}`,
          },
        };
        requestReply(clientConnID, toClientMessage, '客户端请求');
      }
    } catch (err) {
      const toClientMessage = {
        jsonrpc: '2.0',
        id: decodeMsg.id,
        error: {
          code: 500,
          message: `请求处理错误:${err.message}`,
        },
      };
      requestReply(clientConnID, toClientMessage, '客户端请求');
    }
  });
};

module.exports = {
  init,
};
