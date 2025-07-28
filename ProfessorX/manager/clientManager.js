const webSocket = require('ws');
const events = require('events');
const msgpack = require('msgpack-lite');
const _ = require('lodash');
const { getLogger } = require('../helper/log4jsHelper');
const config = require('../data/config/config');
const { getGUID } = require('../helper/common');

const logger = getLogger('clientManager');
const clientEmitter = new events.EventEmitter();
const clientConnList = {};
const clientServiceList = {};
const clientActiveTimeList = {};
const heartbeatMsg = {
  jsonrpc: '2.0',
  id: 1,
  result: 1,
};

const getClientConnByType = (type) => {
  const conns = [];
  _.keys(clientConnList).forEach((key) => {
    if (_.includes(key, type)) {
      conns.push(clientConnList[key]);
    }
  });
  return conns;
};

function clearClientConnection(clientConnID) {
  if (clientServiceList[clientConnID]) {
    const clientWSClient = clientConnList[clientConnID];
    if (clientConnID.includes('control')) {
      clientEmitter.emit(
        `${clientConnID}${clientServiceList[clientConnID]}Close`
      );
    } else if (clientConnID.includes('data')) {
      clientEmitter.emit(`${clientServiceList[clientConnID]}Close`);
    }
    delete clientConnList[clientConnID];
    delete clientActiveTimeList[clientConnID];
    delete clientServiceList[clientConnID];
    clientWSClient.close();
  }
}

const init = async () => {
  const wss = new webSocket.Server({
    port: config.wsPort,
  });
  wss.on('connection', (clientWS, request) => {
    let clientConnID;
    if (request.headers.token) {
      clientConnID = request.url.concat('/') + request.headers.token;
    } else if (request.headers['sec-websocket-protocol'] === undefined) {
      if (request.url.includes('notify')) {
        clientConnID = request.url.concat('/') + getGUID();
      } else {
        clientConnID = request.url;
      }
    } else {
      clientConnID =
        request.url.concat('/') + request.headers['sec-websocket-protocol'];
    }
    logger.debug(`前端输入 clientConnID ${clientConnID}`);
    // const service = `${request.url.replace('/', '')}Request`; // 此方法只能替换第一个匹配的字符
    const service = `${request.url.replace(new RegExp('/', 'g'), '')}Request`; // 替换所有匹配的字符
    logger.debug(`前端 ${clientConnID} 连接成功:${service}`);
    clientWS.on('message', async (message) => {
      const decodeMsg = msgpack.decode(message);
      clientActiveTimeList[clientConnID] = new Date();
      // eslint-disable-next-line eqeqeq
      if (decodeMsg.id == 1) {
        // logger.debug(JSON.stringify(decodeMsg));
        if (decodeMsg.method === 'beat2') {
          // 回复心跳包
          clientWS.send(msgpack.encode(heartbeatMsg));
        }
        // eslint-disable-next-line eqeqeq
      } else if (decodeMsg.method.includes('beat') && decodeMsg.id != 1) {
        const toClientMessage = {
          jsonrpc: '2.0',
          id: decodeMsg.id,
          error: {
            code: 500,
            message: `心跳包格式错误`,
          },
        };
        clientWS.send(msgpack.encode(toClientMessage));
      } else {
        clientEmitter.emit(service, clientConnID, message);
      }
    });
    clientWS.on('close', (code, reason) => {
      logger.debug(`与前端 ${clientConnID} 连接中断(${code}:${reason})`);
      clearClientConnection(clientConnID);
    });
    clientWS.on('error', (error) => {
      logger.debug(`与前端 ${clientConnID} 连接出错:${error}`);
      clearClientConnection(clientConnID);
    });
    // 前端重连云端控制通道
    if (clientConnList[clientConnID]) {
      logger.debug(`收到前端 ${clientConnID} 重连请求`);
      clientConnList[clientConnID].close();
    }
    clientConnList[clientConnID] = clientWS;
    clientServiceList[clientConnID] = service;
    clientActiveTimeList[clientConnID] = new Date();
  });
  wss.on('error', (error) => {
    logger.error(error);
  });
};

/**
 * 前端请求回复
 * @param {string} clientConnID 前端连接ID
 * @param {object} decodeMsg 未打包消息
 * @param {string} requestSubject 请求主题
 */
const requestReply = (clientConnID, decodeMsg, requestSubject) => {
  const clientConn = clientConnList[clientConnID];
  if (clientConn) {
    clientConn.send(msgpack.encode(decodeMsg));
    logger.debug(`${requestSubject}回复成功:${JSON.stringify(decodeMsg)}`);
  } else {
    logger.info(
      `${requestSubject}回复失败，未找到前端 ${clientConnID} 的连接通道${JSON.stringify(
        decodeMsg
      )}`
    );
  }
};

/**
 * 监控前端状态（监控控制通道的前端连接）
 */
setInterval(async () => {
  try {
    const now = new Date();
    _.keys(clientConnList).forEach((clientConnID) => {
      if (
        clientConnID.includes('control') &&
        now - clientActiveTimeList[clientConnID] > 15000
      ) {
        // 判定前端离线
        logger.debug(`前端 ${clientConnID} 离线，关闭连接并清理资源`);
        clearClientConnection(clientConnID);
      }
    });
  } catch (err) {
    logger.error(`监控前端状态出错:${err}`);
  }
}, 2000);

module.exports = {
  init,
  getClientConnByType,
  requestReply,
  clientEmitter,
};
