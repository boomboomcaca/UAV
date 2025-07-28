const msgpack = require('msgpack-lite');
const events = require('events');
const WebSocket = require('ws');
const config = require('../data/config/config');
const { getGUID } = require('../helper/common');
const { getEdgeConn } = require('../manager/edgeManager');
const { getClientConnByType } = require('../manager/clientManager');
const { getLogger } = require('../helper/log4jsHelper');

const logger = getLogger('jointMeasure');

class jointMeasure extends events.EventEmitter {
  cloudTaskID = getGUID();

  datas = []; // 数据缓存

  edgeList = []; // 边缘端列表

  edgeDataConn = []; // 与边缘端数据通道

  edgeTaskIDList = []; // 边缘端任务ID列表

  async start(decodeMsg) {
    // 前端调用start后，云端分别调用边缘端的preset
    // 边缘端preset返回后云端自动调用start

    for (let i = 0; i < decodeMsg.params.edgeList.length; i++) {
      const edgeConn = await getEdgeConn(decodeMsg.params.edgeList[i].edgeID);
      if (!edgeConn) {
        const errorMessage = {
          jsonrpc: '2.0',
          id: decodeMsg.id,
          error: {
            code: 500,
            message: `与边缘端${decodeMsg.params.edgeList[i].edgeID}连接不存在，启动任务失败`,
          },
        };
        this.emit('taskReply', errorMessage);
        return false;
      }
    }
    const promises = decodeMsg.params.edgeList.map(async (element) => {
      this.edgeList.push(element);
      const edgeConn = await getEdgeConn(element.edgeID);
      if (edgeConn) {
        const toEdgeMessage = {
          jsonrpc: '2.0',
          id: decodeMsg.id,
          method: 'presetTask',
          params: {
            moduleID: element.moduleID,
            pluginID: element.pluginID,
            pluginName: element.pluginName,
            needHeart: false,
          },
        };
        edgeConn.send(msgpack.encode(toEdgeMessage));
      }
    });
    await Promise.all(promises);

    // 云端将preset调用完之后回复客户端
    const toClientMessage = {
      jsonrpc: '2.0',
      id: decodeMsg.id,
      result: {
        taskID: this.cloudTaskID,
        uri: `ws://${config.domainName}:${config.wsPort}/data/${this.cloudTaskID}`,
      },
    };
    this.emit('taskReply', toClientMessage);
    return true;
  }

  async stop(decodeMsg) {
    this.clear();
    const toClientMessage = {
      jsonrpc: '2.0',
      id: decodeMsg.id,
      result: null,
    };
    this.emit('taskReply', toClientMessage);
  }

  async addEdge(decodeMsg) {
    for (let i = 0; i < decodeMsg.params.edgeList.length; i++) {
      const edgeConn = await getEdgeConn(decodeMsg.params.edgeList[i].edgeID);
      if (!edgeConn) {
        const errorMessage = {
          jsonrpc: '2.0',
          id: decodeMsg.id,
          error: {
            code: 500,
            message: `与边缘端${decodeMsg.params.edgeList[i].edgeID}连接不存在，添加边缘端失败`,
          },
        };
        this.emit('taskReply', errorMessage);
        return;
      }
    }
    // 发送preset
    const promises = decodeMsg.params.edgeList.map(async (element) => {
      const edgeConn = await getEdgeConn(element.edgeID);
      if (edgeConn) {
        const toEdgeMessage = {
          jsonrpc: '2.0',
          id: decodeMsg.id,
          method: 'presetTask',
          params: {
            moduleID: element.moduleID,
            pluginID: element.pluginID,
            pluginName: element.pluginName,
          },
        };
        logger.debug(`addEdge:${JSON.stringify(toEdgeMessage)}`);
        edgeConn.send(msgpack.encode(toEdgeMessage));
      }
    });
    await Promise.all(promises);

    const toClientMessage = {
      jsonrpc: '2.0',
      id: decodeMsg.id,
      result: null,
    };
    this.emit('taskReply', toClientMessage);
  }

  async removeEdge(decodeMsg) {
    // 发送stop
    decodeMsg.params.edgeList.forEach(async (element) => {
      const edgeConn = await getEdgeConn(element.edgeID);
      if (edgeConn) {
        const edgeTaskID = this.edgeTaskIDList.find(
          (item) => item.edgeID === element.edgeID
        );
        if (edgeTaskID) {
          const toEdgeMessage = {
            jsonrpc: '2.0',
            id: decodeMsg.id,
            method: 'stopTask',
            params: { id: edgeTaskID.taskID },
          };
          logger.debug(`removeEdge:${JSON.stringify(toEdgeMessage)}`);
          edgeConn.send(msgpack.encode(toEdgeMessage));
          this.edgeTaskIDList.splice(
            this.edgeTaskIDList.findIndex(
              (item) => item.edgeID === element.edgeID,
              1
            )
          );
        }
      }
    });

    const toClientMessage = {
      jsonrpc: '2.0',
      id: decodeMsg.id,
      result: null,
    };
    this.emit('taskReply', toClientMessage);
  }

  async setParameters(decodeMsg) {
    for (let i = 0; i < decodeMsg.params.edgeList.length; i++) {
      const edgeConn = await getEdgeConn(decodeMsg.params.edgeList[i].edgeID);
      if (!edgeConn) {
        const errorMessage = {
          jsonrpc: '2.0',
          id: decodeMsg.id,
          error: {
            code: 500,
            message: `与边缘端${decodeMsg.params.edgeList[i].edgeID}连接不存在，设置边缘端参数失败`,
          },
        };
        this.emit('taskReply', errorMessage);
        return;
      }
    }
    // 发送setParameter
    decodeMsg.params.edgeList.forEach(async (element) => {
      const edgeConn = await getEdgeConn(element.edgeID);
      if (edgeConn) {
        const edgeTaskID = this.edgeTaskIDList.find(
          (item) => item.edgeID === element.edgeID
        );
        if (edgeTaskID) {
          const toEdgeMessage = {
            jsonrpc: '2.0',
            id: decodeMsg.id,
            method: 'setTaskParameters',
            params: {
              id: edgeTaskID.taskID,
              parameters: element.parameters,
            },
          };
          logger.debug(`setTaskParameters:${JSON.stringify(toEdgeMessage)}`);
          edgeConn.send(msgpack.encode(toEdgeMessage));
        }
      }
    });

    const toClientMessage = {
      jsonrpc: '2.0',
      id: decodeMsg.id,
      result: null,
    };
    this.emit('taskReply', toClientMessage);
  }

  async receiveFromEdgeMessage(edgeID, decodeMsg) {
    if (decodeMsg.result) {
      // preset返回值
      if (decodeMsg.result.taskID) {
        this.edgeTaskIDList.push({
          edgeID,
          taskID: decodeMsg.result.taskID,
        });

        this.createEdgeDataChannel(decodeMsg.result.uri);

        // preset返回成功后就可以调用start了
        const edgeConn = await getEdgeConn(edgeID);
        if (edgeConn) {
          const toEdgeMessage = {
            jsonrpc: '2.0',
            id: decodeMsg.id,
            method: 'startTask',
            params: { id: decodeMsg.result.taskID },
          };
          edgeConn.send(msgpack.encode(toEdgeMessage));
        }
      } else {
        logger.debug(JSON.stringify(decodeMsg));
      }
    }
  }

  createEdgeDataChannel(dataEndpoint) {
    const edgeDataChannelSocket = new WebSocket(dataEndpoint);
    edgeDataChannelSocket.on('open', () => {
      logger.debug(`${dataEndpoint}:open`);
    });

    edgeDataChannelSocket.on('error', (err) => {
      logger.debug(`${dataEndpoint}error:${err}`);
    });

    edgeDataChannelSocket.on('close', (code, reason) => {
      logger.debug(`${dataEndpoint}(close${code}:${reason})`);
    });

    edgeDataChannelSocket.on('message', async (data) => {
      const decodeData = msgpack.decode(data);
      await this.processData(decodeData);
    });
    this.edgeDataConn.push(edgeDataChannelSocket);
  }

  /**
   * 监测数据处理，子类重写该方法实现数据处理逻辑
   * 如：
   * 1.TDOA功能，IQ数据放入缓冲区，电平、频谱数据转发给前端
   * 2.交会定位，测向数据放入缓冲区，测向、电平、频谱数据转发给前端
   *
   * @param {*} data
   * @memberof jointMeasure
   */
  // eslint-disable-next-line class-methods-use-this
  async processData(data) {
    logger.debug('processData');
    logger.debug(data);
  }

  sendDataToClient(data) {
    // 获取指定客户端对该任务的连接
    const conns = getClientConnByType(this.cloudTaskID);
    if (conns && conns.length > 0) {
      conns.forEach((element) => {
        element.send(msgpack.encode(data));
      });
    }
  }

  clear() {
    // 停止边缘端
    this.edgeTaskIDList.forEach(async (element) => {
      const edgeConn = await getEdgeConn(element.edgeID);
      if (edgeConn) {
        const toEdgeMessage = {
          jsonrpc: '2.0',
          id: getGUID(),
          method: 'stopTask',
          params: { id: element.taskID },
        };
        edgeConn.send(msgpack.encode(toEdgeMessage));
      }
    });
    // 关闭边缘端数据通道
    this.edgeDataConn.forEach((item) => {
      item.close();
    });
    // 关闭云端数据通道
    const conns = getClientConnByType(this.cloudTaskID);
    if (conns && conns.length > 0) {
      conns.forEach((element) => {
        element.close();
      });
    }
    // 手动GC回收内存
    try {
      global.gc();
      // eslint-disable-next-line no-empty
    } catch {}
  }
}

module.exports = jointMeasure;
