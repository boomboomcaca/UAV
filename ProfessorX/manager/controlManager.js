const WebSocket = require('ws');
const events = require('events');
const msgpack = require('msgpack-lite');
const _ = require('lodash');
const config = require('../data/config/config');
const edgeRepository = require('../db/edgeRepository');
const { getMessageHelper } = require('../helper/messageHelper');
const { setJson, getJson, getGUID } = require('../helper/repositoryBase');
const { getLogger } = require('../helper/log4jsHelper');

const logger = getLogger('controlManager');
const edgeEmitter = new events.EventEmitter();
const messageHelper = getMessageHelper();
let producer;
const edgeStateChange = {
  jsonrpc: '2.0',
  id: 0,
  result: {
    edgeID: '',
    timestamp: 0,
    dataCollection: [],
  },
};
const controlID = config.edge.controlEdgeID[0];
const edgeConnList = {};
const heartbeatMsg = {
  jsonrpc: '2.0',
  id: 1,
  method: 'beat2',
};

/**
 * 重启环境控制边缘端
 */
const restart = async (deviceID) => {
  if (deviceID === undefined || deviceID === '') {
    return;
  }
  const edgeConn = edgeConnList[controlID];
  if (edgeConn) {
    edgeConn.send(
      msgpack.encode({
        jsonrpc: '2.0',
        id: getGUID(),
        method: 'restartApp',
        params: null,
      })
    );
  }
};

/**
 * 获取边缘端信息
 * @param {*} edgeID 边缘端ID
 * @return {*} 边缘端信息
 */
const getEdge = async (edgeID) => {
  let edge = await getJson(config.edge.controlsCacheKey + edgeID);
  if (!edge) {
    const edges = await edgeRepository.getControlEdges();
    [edge] = edges;
    if (edge) {
      await setJson(config.edge.controlsCacheKey + edgeID, edge);
    }
  }
  return edge;
};

/**
 * 更新边缘端缓存列表
 * @param {*} edges
 * @return {*}
 */
const setCacheEdges = async (edges) => {
  const promises = edges.map(async (edge) => {
    await setJson(config.edge.controlsCacheKey + edge.edgeID, edge);
  });
  return Promise.all(promises);
};

/**
 * 获取边缘端缓存列表
 * @param {*} edgeIds
 * @return {*}
 */
const getCacheEdges = async (edgeIDs) => {
  const promises = edgeIDs.map(async (edgeID) => {
    const edge = await getEdge(edgeID);
    return edge;
  });
  return Promise.all(promises);
};

const sendMessage = (edgeID, isActive) => {
  edgeStateChange.result.edgeID = edgeID;
  edgeStateChange.result.timestamp = Math.round(new Date().getTime() * 1000000);
  edgeStateChange.result.dataCollection = [];
  if (isActive) {
    edgeStateChange.result.dataCollection.push({
      type: 'edgeStateChange',
      state: 'online',
      content: '边缘端上线',
    });
  } else {
    edgeStateChange.result.dataCollection.push({
      type: 'edgeStateChange',
      state: 'offline',
      content: '边缘端离线',
    });
  }
  const payloads = [
    {
      topic: config.redisStream.streamsKey,
      messages: JSON.stringify(edgeStateChange),
      partition: 0,
    },
  ];
  messageHelper.sendData(producer, payloads);
  logger.debug(JSON.stringify(edgeStateChange));
};

/**
 * 获取边缘端内存列表
 * @return {*} 边缘端内存列表
 */
const getEdges = async () => {
  const edgeIDs = await getJson(config.edge.controlsCacheKey);
  const edges = await getCacheEdges(edgeIDs);
  return edges;
};

/**
 * 更新边缘端缓存信息
 *
 * @param {*} edge 边缘端信息
 */
const updateCache = async (edge) => {
  const newEdge = { ...edge };
  newEdge.isActive = true;
  newEdge.lastUpdateTime = new Date();

  // 刷新边缘端或添加边缘端到缓存（考虑运行中新增边缘端情况）
  const edgeIds = await getJson(config.edge.controlsCacheKey);
  if (!edgeIds.includes(newEdge.edgeID)) {
    edgeIds.push(newEdge.edgeID);
    await setJson(config.edge.controlsCacheKey, edgeIds);
  }
  await setJson(config.edge.controlsCacheKey + newEdge.edgeID, newEdge);

  // 发出上线通知
  sendMessage(newEdge.edgeID, newEdge.isActive);
};

/**
 * 与边缘端建立连接
 * @param {*} edgeID 边缘端ID
 * @param {*} ip 边缘端ip地址
 * @param {*} port 边缘端端口
 */
const connToEdge = async (edgeID, ip, port) => {
  // 与边缘端建立连接
  const url = `ws://${ip}:${port}/environment`;
  const edgeConn = new WebSocket(url);
  // todo: 建一个WEBSocket基类 进行再次封装 使用的时候对WebSocker message on close error 无感

  const msg = await new Promise((resolve) => {
    edgeConn.on('open', () => {
      edgeConnList[edgeID] = edgeConn;
      logger.debug(`与边缘端${edgeID}建立连接成功：${url}`);
      resolve();
    });
    edgeConn.on('message', async (message) => {
      // 更新边缘端最后活动时间
      const edge = await getEdge(edgeID);
      edge.lastUpdateTime = new Date();
      // 解析数据，分发数据
      const decodeMsg = msgpack.decode(message);
      if (decodeMsg.id === 0) {
        logger.debug(JSON.stringify(decodeMsg));
        // 更新设备及功能模块状态
        const moduleStateItems = decodeMsg.result.dataCollection.filter(
          (s) => s.type === 'moduleStateChange'
        );
        if (moduleStateItems.length > 0) {
          moduleStateItems.forEach((moduleStateItem) => {
            edge.modules.forEach((module) => {
              if (module.id === moduleStateItem.id) {
                module.moduleState = moduleStateItem.state;
              }
            });
          });
        }
        // 通知类走消息队列
        const payloads = [
          {
            topic: config.redisStream.streamsKey,
            messages: JSON.stringify(decodeMsg),
            partition: 0,
          },
        ];
        messageHelper.sendData(producer, payloads);
      } else if (decodeMsg.id !== 1) {
        logger.debug(JSON.stringify(decodeMsg));
        // 远程开关机回复
        edgeEmitter.emit('controlReply', edgeID, decodeMsg);
      }
      await setJson(config.edge.controlsCacheKey + edge.edgeID, edge);
    });
    edgeConn.on('close', (code, reason) => {
      logger.debug(`与边缘端 ${edgeID} 连接中断(${code}:${reason})`);
    });
    edgeConn.on('error', (error) => {
      logger.debug(`与边缘端 ${edgeID} 连接出错:${error}`);
      resolve(`与边缘端 ${edgeID} 建立连接失败!`);
    });
  });
  return msg;
};

/**
 * 边缘端管理器初始化
 */
const init = async () => {
  producer = await messageHelper.getProducer();
  try {
    const edges = await edgeRepository.getControlEdges();
    const edgeIds = edges.map((edge) => edge.edgeID);
    await setCacheEdges(edges);
    await setJson(config.edge.controlsCacheKey, edgeIds);
  } catch (err) {
    logger.error(err);
  }
};

/**
 * 边缘端登录
 * @param {*} edgeID 边缘端ID，暂未使用
 * @param {*} ip 边缘端ip地址
 * @param {*} port 边缘端端口
 * @return {*} 异常信息
 */
const login = async (edgeID, ip, port) => {
  // 环境控制无真实边缘端，用登录ip作为身份信息，edgeID使用配置的controlID
  // eslint-disable-next-line no-param-reassign
  edgeID = controlID;
  let message;
  const edge = await getEdge(edgeID);
  if (edge) {
    if (edge.isActive && edge.ip !== '') {
      message = `地址为${ip}的环境监控服务登录失败，环境监控服务${edge.ip}已登录`;
    } else {
      message = await connToEdge(edgeID, ip, port);
      if (!message) {
        edge.ip = ip;
        await updateCache(edge);
      }
    }
  } else {
    // 理论上，不应进入该方法
    message = `云端无ID为 ${controlID} 的边缘端`;
  }
  if (message) {
    logger.info(message);
  }
  return message;
};

/**
 * 获取指定的边缘端连接
 * @param {*} edgeID 边缘端ID
 * @return {*} 边缘端连接
 */
const getEdgeConn = async (edgeID) => {
  return edgeConnList[edgeID];
};

async function monitorEdge(edgeID) {
  const edge = await getJson(config.edge.controlsCacheKey + edgeID);
  if (edge && edge.isActive) {
    const activeInterval = new Date() - Date.parse(edge.lastUpdateTime);
    if (activeInterval > config.edge.timeLimit * 1000) {
      if (edgeConnList[edgeID]) {
        edgeConnList[edgeID].close();
        delete edgeConnList[edgeID];
        // 更新缓存信息
        edge.isActive = false;
        edge.modules.forEach((module) => {
          module.moduleState =
            module.moduleState === 'disabled' ? 'disabled' : 'offline';
        });
        edge.ip = '';
        await setJson(config.edge.controlsCacheKey + edgeID, edge);
        // 发出离线通知
        sendMessage(edge.edgeID, edge.isActive);
      }
    }
  }
}

/**
 * 向边缘端发送心跳包
 */
setInterval(async () => {
  try {
    const encodeMsg = msgpack.encode(heartbeatMsg);
    _.keys(edgeConnList).forEach((key) => {
      edgeConnList[key].send(encodeMsg);
    });
  } catch (err) {
    logger.error(`向边缘端发送心跳包出错:${err}`);
  }
}, 1000);

// 监控边缘端状态
setInterval(async () => {
  try {
    const edgeIDs = await getJson(config.edge.controlsCacheKey);
    if (edgeIDs) {
      const promises = edgeIDs.map(async (edgeID) => {
        await monitorEdge(edgeID);
      });
      await Promise.all(promises);
    }
  } catch (err) {
    logger.error(`监控边缘端状态出错${err}`);
  }
}, config.edge.interval * 1000);

module.exports = {
  init,
  login,
  getEdge,
  getEdges,
  getEdgeConn,
  edgeEmitter,
  restart,
};
