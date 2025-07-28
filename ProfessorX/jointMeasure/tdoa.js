const fs = require('fs');
const array = require('lodash/array');
const ChildProcess = require('child_process');
const { getLogger } = require('../helper/log4jsHelper');
const { getEdge } = require('../manager/edgeManager');
const jointMeasure = require('./jointMeasure');
const config = require('../data/config/config');

const logger = getLogger('tdoa');

class tdoa extends jointMeasure {
  receiveDataCache = [];

  edgeIDCollection = [];

  tdoaIntervalId;

  calculationProcess;

  calculation = false;

  constructor() {
    super();
    if (config.tdoa.recordIQData) {
      if (!fs.existsSync(config.tdoa.iqDataFolder)) {
        fs.mkdirSync(config.tdoa.iqDataFolder);
      }
    }
    this.tdoaIntervalId = setInterval(
      this.tdoaCalculation,
      config.tdoa.calculationInterval
    );
    if (fs.existsSync(__dirname.concat('/tdoaCalculation.js'))) {
      this.calculationProcess = ChildProcess.fork(
        __dirname.concat('/tdoaCalculation.js')
      );
    } else {
      this.calculationProcess = ChildProcess.fork(
        __dirname.concat('/tdoaCalculation.loader.js')
      );
    }
    this.registerMsgHandler();
  }

  registerMsgHandler() {
    this.calculationProcess.on('message', async (msg) => {
      if (msg.result) {
        msg.result.taskID = this.cloudTaskID;
        logger.debug('TDOA 计算成功。');
        this.sendDataToClient(msg);
      } else {
        logger.error('TDOA 计算进程出错，错误为：');
        logger.error(msg);
      }
    });
    this.calculationProcess.on('close', (code, signal) => {
      logger.debug(
        `收到 close 事件，TDOA 计算进程收到信号 ${signal} 而终止，退出码 ${code}`
      );
      this.calculationProcess.kill();
    });
  }

  async processData(data) {
    // data.result.dataCollection.forEach((item) => {
    //   logger.debug(`收到边缘端${data.result.edgeID}数据类型:${item.type}`);
    // });
    const iqDataItem = data.result.dataCollection.find((s) => s.type === 'iq');
    const newData = { ...data };
    if (iqDataItem) {
      if (config.tdoa.recordIQData) {
        try {
          fs.appendFile(
            `${config.tdoa.iqDataFolder}/iq_${this.cloudTaskID}_${data.result.edgeID}.txt`,
            JSON.stringify(iqDataItem).concat('\r'),
            { flags: 'w' },
            (err) => {
              if (err) {
                logger.error(err);
              }
            }
          );
        } catch (err) {
          logger.error(err);
        }
      }
      newData.result.dataCollection = data.result.dataCollection.filter(
        (dataItem) => dataItem.type !== 'iq'
      );
      // 将新的 edgeID 存入集合
      if (!this.edgeIDCollection.includes(data.result.edgeID)) {
        this.edgeIDCollection.push(data.result.edgeID);
      }
      // 去除时间戳为0的 IQ 数据
      if (iqDataItem.timestamp !== 0) {
        const receiveData = {};
        receiveData.edgeID = data.result.edgeID;
        receiveData.taskID = data.result.taskID;
        if (config.tdoa.virtualEdge) {
          if (iqDataItem.timestamp.toString().includes('000000000')) {
            receiveData.time =
              Math.floor(iqDataItem.timestamp / 1000000000) - 1;
          } else {
            receiveData.time = Math.floor(iqDataItem.timestamp / 1000000000);
          }
        } else {
          receiveData.time = Math.floor(iqDataItem.timestamp / 1000000000);
        }
        receiveData.lngLat = [];
        const edgeInfo = await getEdge(data.result.edgeID);
        receiveData.lngLat.push(edgeInfo.longitude);
        receiveData.lngLat.push(edgeInfo.latitude);
        receiveData.fs = iqDataItem.samplingRate * 1000;
        if (iqDataItem.data16 !== null) {
          receiveData.iq = iqDataItem.data16;
        } else if (iqDataItem.data32 !== null) {
          receiveData.iq = iqDataItem.data32;
        }
        if (
          this.receiveDataCache.length >
          this.edgeIDCollection.length * config.tdoa.edgeDataCacheCount
        ) {
          this.receiveDataCache.shift();
        }
        this.receiveDataCache.push(receiveData);
      }
    }
    if (newData.result.dataCollection.length !== 0) {
      this.sendDataToClient(newData);
    }
  }

  tdoaCalculation = async () => {
    if (this.calculation) {
      return;
    }
    this.calculation = true;
    if (this.receiveDataCache.length >= 3) {
      try {
        const receiveDataLength = this.receiveDataCache.length;
        for (let i = 0; i < receiveDataLength; i++) {
          const calculateDatas = [];
          // 模拟设备测试需考虑去除重复边缘端数据，真实设备理论上不存在重复数据
          if (config.tdoa.virtualEdge) {
            calculateDatas.push(this.receiveDataCache[i]);
            const calculateEdgeIDs = [];
            calculateEdgeIDs.push(this.receiveDataCache[i].edgeID);
            // const newCalculateDatas = this.receiveDataCache.filter((item) => {
            //   if (
            //     this.receiveDataCache[i].time === item.time &&
            //     !calculateEdgeIDs.includes(item.edgeID)
            //   ) {
            //     calculateEdgeIDs.push(item.edgeID);
            //     return true;
            //   }
            //   return false;
            // });
            // if (newCalculateDatas && newCalculateDatas.length > 0) {
            //   calculateDatas.push(...newCalculateDatas);
            // }
            for (let j = 0; j < receiveDataLength; j++) {
              if (
                this.receiveDataCache[i].time ===
                  this.receiveDataCache[j].time &&
                !calculateEdgeIDs.includes(this.receiveDataCache[j].edgeID)
              ) {
                calculateEdgeIDs.push(this.receiveDataCache[j].edgeID);
                calculateDatas.push(this.receiveDataCache[j]);
              }
            }
          } else {
            const newCalculateDatas = this.receiveDataCache.filter(
              (item) => this.receiveDataCache[i].time === item.time
            );
            if (newCalculateDatas && newCalculateDatas.length > 0) {
              calculateDatas.push(...newCalculateDatas);
            }
          }
          if (calculateDatas.length >= 3) {
            // 移除 IQ 数据
            const removeDatas = [...calculateDatas];
            // 移除比参与计算的数据的时间小的数据（真实设备适用）
            if (!config.tdoa.virtualEdge) {
              const calDataTime = calculateDatas[0].time;
              this.receiveDataCache.forEach((item) => {
                if (item.time < calDataTime) {
                  removeDatas.push(item);
                }
              });
            }
            array.pullAll(this.receiveDataCache, removeDatas);
            this.calculationProcess.send(calculateDatas);
            break;
          }
        }
      } catch (err) {
        logger.error('TDOA 计算出错，错误为：');
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

module.exports = tdoa;
