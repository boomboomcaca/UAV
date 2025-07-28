const WebSocket = require('ws');
const events = require('events');
const msgpack = require('msgpack-lite');
const _ = require('lodash');
const config = require('../data/config/config');
const { getLogger } = require('../helper/log4jsHelper');
const { getMessageHelper } = require('../helper/messageHelper');
const { setJson, getJson, getGUID } = require('../helper/repositoryBase');
const edgeRepository = require('../db/edgeRepository');

const logger = getLogger('edgeManager');
const edgeEmitter = new events.EventEmitter();
const messageHelper = getMessageHelper();
const edgeStateChange = {
  jsonrpc: '2.0',
  id: 0,
  result: {
    edgeID: '',
    timestamp: 0,
    dataCollection: [],
  },
};
const edgeConnList = {};
let producer;
const heartbeatMsg = {
  jsonrpc: '2.0',
  id: 1,
  method: 'beat2',
};

/**
 * 边缘端重启或边缘端同步服务配置更新
 * @param { 边缘端ID } edgeID
 * @param { true：重启rsync daemon，false：重启边缘端 } rsync
 */
const restart = async (edgeID, rsync = false) => {
  if (edgeID === undefined || edgeID === '') {
    return;
  }
  const edgeConn = edgeConnList[edgeID];
  if (edgeConn) {
    let jsonrpc;
    if (rsync === true) {
      jsonrpc = {
        jsonrpc: '2.0',
        id: getGUID(),
        method: 'updateRsync',
        params: {
          configType: 'update',
        },
      };
    } else {
      jsonrpc = {
        jsonrpc: '2.0',
        id: getGUID(),
        method: 'restartApp',
        params: null,
      };
    }
    edgeConn.send(msgpack.encode(jsonrpc));
  }
};

/**
 * 获取指定的边缘端连接
 * @param {*} edgeID 边缘端ID
 * @return {*} 边缘端连接
 */
const getEdgeConn = async (edgeID) => {
  return edgeConnList[edgeID];
};

/**
 * 获取边缘端信息
 * @param {*} edgeID 边缘端ID
 * @return {*} 边缘端信息
 */
const getEdge = async (edgeID) => {
  let edge = await getJson(config.edge.edgesCacheKey + edgeID);
  if (!edge) {
    edge = await edgeRepository.getEdge(edgeID);
    if (edge) {
      await setJson(config.edge.edgesCacheKey + edgeID, edge);
    }
  }
  return edge;
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

const sendMessage = (edgeID, message) => {
  edgeStateChange.result.edgeID = edgeID;
  edgeStateChange.result.timestamp = Math.round(new Date().getTime() * 1000000);
  edgeStateChange.result.dataCollection = [];
  edgeStateChange.result.dataCollection.push(message);
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
 * 更新边缘端缓存信息
 *
 * @param {*} edge 边缘端信息
 */
const updateCache = async (edge) => {
  const newEdge = { ...edge };
  newEdge.isActive = true;
  newEdge.lastUpdateTime = new Date();
  // 刷新边缘端或添加边缘端到缓存
  const edgeIds = await getJson(config.edge.edgesCacheKey);
  if (!edgeIds.includes(newEdge.edgeID)) {
    edgeIds.push(newEdge.edgeID);
    await setJson(config.edge.edgesCacheKey, edgeIds);
  }
  await setJson(config.edge.edgesCacheKey + newEdge.edgeID, newEdge);
  // 发出上线通知
  const onlineMsg = {
    type: 'edgeStateChange',
    state: 'online',
    content: '边缘端上线',
  };
  sendMessage(newEdge.edgeID, onlineMsg);
};

/**
 * 边缘端离线时，移除该边缘端任务信息
 * @param {string} edgeID
 */
const deleteTask = async (edgeID) => {
  let tasks = await getJson(config.edge.taskCacheKey);
  if (!tasks) {
    tasks = [];
  } else {
    tasks.forEach((t) => {
      if (t.edgeID === edgeID) {
        tasks.remove(t);
      }
    });
  }
  await setJson(config.edge.taskCacheKey, tasks);
};

/**
 * 查询任务信息缓存
 */
const getTasks = async () => {
  let tasks = await getJson(config.edge.taskCacheKey);
  if (tasks === null) {
    tasks = [];
  }
  return tasks;
};

/**
 * 任务信息缓存管理
 * @param {*} taskList
 */
const taskManager = async (taskList) => {
  let tasks = await getJson(config.edge.taskCacheKey);
  if (!tasks) {
    tasks = [];
  }
  taskList.taskInfo.forEach((task) => {
    if (task.stopTime === undefined || task.stopTime === null) {
      const t = {
        id: task.id,
        edgeID: taskList.edgeID,
        deviceID: task.deviceID,
        edgeFuncID: task.moduleID,
        params: task.majorParameters,
        planID: task.crondID,
        name: task.name,
        startTime: task.startTime,
        account: task.account,

        parameters: task.parameters,
        frontFuncID: task.pluginID,
        dataEndpoint: task.uri,
      };

      tasks.push(t);
    } else {
      tasks.forEach((t) => {
        if (t.id === task.id) {
          tasks.remove(t);
        }
      });
    }
  });
  await setJson(config.edge.taskCacheKey, tasks);
};

/**
 * 与边缘端建立连接
 * @param {string} edgeID 边缘端ID
 * @param {string} url 边缘端ws地址
 */
const connToEdge = async (edgeID, url) => {
  // 与边缘端建立WS连接
  const edgeConn = new WebSocket(url);
  const msg = await new Promise((resolve) => {
    edgeConn.on('open', () => {
      edgeConnList[edgeID] = edgeConn;
      logger.debug(`与边缘端 ${edgeID} 建立连接成功：${url}`);
      resolve();
    });
    edgeConn.on('message', async (message) => {
      const edge = await getEdge(edgeID);
      // 更新边缘端最后活动时间
      edge.lastUpdateTime = new Date();
      // 解析数据，分发数据
      const decodeMsg = msgpack.decode(message);
      if (decodeMsg.id === 0) {
        // 通知类走消息队列
        const payloads = [
          {
            topic: config.redisStream.streamsKey,
            messages: JSON.stringify(decodeMsg),
            partition: 0,
          },
        ];
        messageHelper.sendData(producer, payloads);
        // 更新边缘端经纬度
        const gpsDataItem = decodeMsg.result.dataCollection.find(
          (s) => s.type === 'gps'
        );
        if (gpsDataItem) {
          edge.longitude = gpsDataItem.longitude;
          edge.latitude = gpsDataItem.latitude;
        }
        // 任务信息管理
        const taskList = decodeMsg.result.dataCollection.find(
          (s) => s.type === 'taskList'
        );
        if (taskList) {
          taskList.edgeID = decodeMsg.result.edgeID;
          await taskManager(taskList);
        }
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
      } else if (decodeMsg.id !== 1) {
        logger.debug(JSON.stringify(decodeMsg));
        // 任务回复不走消息队列
        edgeEmitter.emit('taskReply', edgeID, decodeMsg);
      }
      await setJson(config.edge.edgesCacheKey + edge.edgeID, edge);
    });
    edgeConn.on('close', (code, reason) => {
      logger.debug(`与边缘端 ${edgeID} 连接断开(${code}:${reason})`);
    });
    edgeConn.on('error', (error) => {
      logger.debug(`与边缘端 ${edgeID} 连接出错:${error}`);
      resolve(`与边缘端 ${edgeID} 建立连接失败!`);
    });
  });
  return msg;
};

/**
 * 获取边缘端缓存列表
 * @return {*} 边缘端缓存列表
 */
const getEdges = async () => {
  const edgeIDs = await getJson(config.edge.edgesCacheKey);
  const edges = await getCacheEdges(edgeIDs);
  return edges;
};

/**
 * 更新边缘端缓存列表
 * @param {*} edges
 * @return {*}
 */
const setCacheEdges = async (edges) => {
  const promises = edges.map(async (edge) => {
    await setJson(config.edge.edgesCacheKey + edge.edgeID, edge);
  });
  return Promise.all(promises);
};

/**
 * 边缘端管理器初始化
 */
const init = async () => {
  producer = await messageHelper.getProducer();
  try {
    await setJson(config.edge.taskCacheKey, []);
    const edges = await edgeRepository.getEdges();
    const edgeIds = edges.map((edge) => edge.edgeID);
    await setCacheEdges(edges);
    await setJson(config.edge.edgesCacheKey, edgeIds);
  } catch (err) {
    logger.error(err);
  }
};

/**
 * 边缘端登录
 * @param {string} edgeID 边缘端ID
 * @param {string} ip 边缘端ip地址
 * @param {number} port 边缘端端口
 * @return {string} 异常信息
 */
const login = async (edgeID, ip, port) => {
  let message;
  const edge = await getEdge(edgeID);
  if (edge) {
    if (edge.isActive) {
      message = `地址为 ${ip} 的边缘端登录失败，ID 为 ${edgeID} 的边缘端已登录`;
    } else {
      const url = `ws://${ip}:${port}/control`;
      message = await connToEdge(edgeID, url);
      if (!message) {
        await deleteTask(edgeID);
        edge.ip = ip;
        await updateCache(edge);
      }
    }
  } else {
    message = `云端无 ID 为 ${edgeID} 的边缘端`;
  }
  if (message) {
    logger.debug(message);
  }
  return message;
};

async function monitorEdge(edgeID) {
  const edge = await getJson(config.edge.edgesCacheKey + edgeID);
  if (edge && edge.isActive) {
    const activeInterval = new Date() - Date.parse(edge.lastUpdateTime);
    if (activeInterval > config.edge.timeLimit * 1000) {
      if (edgeConnList[edgeID]) {
        await edgeConnList[edgeID].close();
        delete edgeConnList[edgeID];
        // 更新缓存信息
        edge.isActive = false;
        edge.modules.forEach((module) => {
          module.moduleState =
            module.moduleState === 'disabled' ? 'disabled' : 'offline';
        });
        await setJson(config.edge.edgesCacheKey + edge.edgeID, edge);
        await deleteTask(edgeID);
        // 发出离线通知
        const offlineMsg = {
          type: 'edgeStateChange',
          state: 'offline',
          content: '边缘端离线',
        };
        sendMessage(edge.edgeID, offlineMsg);
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

/**
 * 监控边缘端状态
 */
setInterval(async () => {
  try {
    const edgeIDs = await getJson(config.edge.edgesCacheKey);
    if (edgeIDs) {
      const promises = edgeIDs.map(async (edgeID) => {
        await monitorEdge(edgeID);
      });
      await Promise.all(promises);
    }
  } catch (err) {
    logger.error(`监控边缘端状态出错:${err}`);
  }
}, config.edge.interval * 1000);

module.exports = {
  init,
  login,
  getEdge,
  getEdges,
  getEdgeConn,
  edgeEmitter,
  getTasks,
  restart,
};
