const fs = require('fs');
const ChildProcess = require('child_process');
const array = require('lodash/array');
const { getLogger } = require('../helper/log4jsHelper');
const { getEdge } = require('../manager/edgeManager');
const jointMeasure = require('./jointMeasure');
const config = require('../data/config/config');

const logger = getLogger('poa');

class poa extends jointMeasure {
  receiveDataCache = [];

  edgeIDCollection = [];

  tdoaIntervalId;

  calculationProcess;

  calculation = false;

  constructor() {
    super();

    this.tdoaIntervalId = setInterval(
      this.poaCalculation,
      config.poa.calculationInterval
    );
    if (fs.existsSync(__dirname.concat('/poaCalculation.js'))) {
      this.calculationProcess = ChildProcess.fork(
        __dirname.concat('/poaCalculation.js')
      );
    } else {
      this.calculationProcess = ChildProcess.fork(
        __dirname.concat('/poaCalculation.loader.js')
      );
    }
    this.registerMsgHandler();
  }

  registerMsgHandler() {
    this.calculationProcess.on('message', async (msg) => {
      if (msg.result) {
        msg.result.taskID = this.cloudTaskID;
        logger.debug('POA 计算成功。');
        this.sendDataToClient(msg);
      } else {
        logger.error('POA 计算进程出错，错误为：');
        logger.error(msg);
      }
    });
    this.calculationProcess.on('close', (code, signal) => {
      logger.debug(
        `收到 close 事件，POA 计算进程收到信号 ${signal} 而终止，退出码 ${code}`
      );
      this.calculationProcess.kill();
    });
  }

  async processData(data) {
    // data.result.dataCollection.forEach((item) => {
    //   logger.debug(`收到边缘端${data.result.edgeID}数据类型:${item.type}`);
    // });
    const levelItem = data.result.dataCollection.find(
      (s) => s.type === 'level'
    );
    if (levelItem) {
      // 将新的 edgeID 存入集合
      if (!this.edgeIDCollection.includes(data.result.edgeID)) {
        this.edgeIDCollection.push(data.result.edgeID);
      }
      const receiveData = {};
      receiveData.edgeID = data.result.edgeID;
      receiveData.taskID = data.result.taskID;
      receiveData.lngLat = [];
      const edgeInfo = await getEdge(data.result.edgeID);
      receiveData.lngLat.push(edgeInfo.longitude);
      receiveData.lngLat.push(edgeInfo.latitude);
      receiveData.frequency = levelItem.frequency;
      receiveData.bandwidth = levelItem.bandwidth;
      receiveData.data = levelItem.data;
      if (
        this.receiveDataCache.length >
        this.edgeIDCollection.length * config.poa.edgeDataCacheCount
      ) {
        this.receiveDataCache.shift();
      }
      this.receiveDataCache.push(receiveData);
    }
    this.sendDataToClient(data);
  }

  poaCalculation = async () => {
    if (this.calculation) {
      return;
    }
    this.calculation = true;
    if (this.receiveDataCache.length >= 3) {
      try {
        const receiveDataLength = this.receiveDataCache.length;
        const calculateDatas = [];
        const existDataEdgeIDs = [];
        existDataEdgeIDs.push(this.receiveDataCache[0].edgeID);
        calculateDatas.push(this.receiveDataCache[0]);
        for (let i = 1; i < receiveDataLength; i++) {
          if (existDataEdgeIDs.length === this.edgeIDCollection.length) {
            break;
          }
          if (!existDataEdgeIDs.includes(this.receiveDataCache[i].edgeID)) {
            existDataEdgeIDs.push(this.receiveDataCache[i].edgeID);
            calculateDatas.push(this.receiveDataCache[i]);
          }
        }
        if (calculateDatas.length >= 3) {
          array.pullAll(this.receiveDataCache, calculateDatas);
          this.calculationProcess.send(calculateDatas);
        }
      } catch (err) {
        logger.error('POA 计算出错，错误为：');
        logger.error(err);
      }
    }
    this.calculation = false;
  };

  clear() {
    if (this.tdoaIntervalId) {
      clearInterval(this.tdoaIntervalId);
      this.tdoaIntervalId = null;
    }
    this.calculationProcess.kill();
    super.clear();
  }
}

module.exports = poa;
