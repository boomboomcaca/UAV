const fs = require('fs');
const events = require('events');
const config = require('../data/config/config');
const { getGUID } = require('../helper/common');
const { getLogger } = require('../helper/log4jsHelper');
const logRepository = require('../db/logRepository');
const {
  getFileDescriptor,
  getIndexData,
  getContentData,
} = require('../helper/readFileHelper');
const { getClientConnByType } = require('../manager/clientManager');

const logger = getLogger('replayService');

const UNSTART = 'unstart'; // 未启动
const PAUSED = 'paused'; // 已暂停
const RUNNING = 'running'; // 运行中
const STOPPED = 'stopped'; // 已停止
const successMsg = {
  result: null,
};
const failedMsg = {
  error: { message: '请求参数错误' },
};
const stoppedMsg = {
  error: { message: '服务已停止' },
};

class ReplayServie extends events.EventEmitter {
  cloudReplayID = getGUID();

  serviceStatus = UNSTART;

  playIndex = 0;

  playTimeSpeed = 1;

  interval;

  fileInfo;

  fd;

  indexDatas;

  contentTotalLength;

  playInterval = 100;

  sendMessage(decodeMsg, contentMsg) {
    const toClientMessage = {
      jsonrpc: '2.0',
      id: decodeMsg.id,
    };
    Object.assign(toClientMessage, contentMsg);
    this.emit('replayReply', toClientMessage);
  }

  getValidationResult(playIndex, playTimeSpeed) {
    let result;
    if (playIndex >= 1 && playIndex <= this.indexDatas.length) {
      if ([1, 2, 3, 4].includes(playTimeSpeed)) {
        result = true;
      } else {
        result = false;
      }
    } else {
      result = false;
    }
    return result;
  }

  startSendData() {
    this.stopSendData();
    this.interval = setInterval(
      this.sendDataContinuous,
      this.playInterval / this.playTimeSpeed
    );
  }

  stopSendData() {
    if (this.interval) {
      clearInterval(this.interval);
    }
    this.interval = null;
  }

  closeFileDescriptor() {
    if (this.fd) {
      fs.close(this.fd, () => {});
    }
    this.fd = null;
  }

  sendDataContinuous = async () => {
    if (this.playIndex < this.indexDatas.length && this.playIndex >= 0) {
      await this.sendData();
      this.playIndex++;
    } else {
      this.stopSendData();
      this.serviceStatus = PAUSED;
    }
  };

  getContentLength() {
    let contentLength;
    if (this.playIndex < this.indexDatas.length - 1) {
      contentLength =
        this.indexDatas[this.playIndex + 1].offset -
        this.indexDatas[this.playIndex].offset;
    } else {
      contentLength =
        this.contentTotalLength - this.indexDatas[this.playIndex].offset;
    }
    return contentLength;
  }

  calculatePlayInterval() {
    const totalInterval =
      Date.parse(this.fileInfo.data_stop_time) -
      Date.parse(this.fileInfo.data_start_time);
    const interval = Math.floor(totalInterval / this.fileInfo.recordCount);
    if (interval >= 10 && interval <= 1000) {
      this.playInterval = interval;
    }
  }

  async sendData() {
    // 获取文件名称
    const contentLength = this.getContentLength();
    const contentData = await getContentData(
      this.fd,
      this.indexDatas[this.playIndex],
      contentLength
    );
    const conns = getClientConnByType(this.cloudReplayID);
    if (conns && conns.length > 0) {
      conns.forEach((element) => {
        element.send(contentData);
      });
    }
  }

  async preset(decodeMsg) {
    this.fileInfo = await logRepository.getReplayFileInfo(
      decodeMsg.params.fileID
    );
    const indexFileName = config.SyncSystem.path
      .concat(this.fileInfo.sourceFile)
      .concat('.idx');
    const contentFileName = config.SyncSystem.path
      .concat(this.fileInfo.sourceFile)
      .concat('.dat');
    let contentMsg;
    try {
      this.indexDatas = await getIndexData(indexFileName);
      this.fd = await getFileDescriptor(contentFileName);
      this.contentTotalLength = await new Promise((resolve) => {
        fs.stat(contentFileName, (err, stats) => {
          resolve(stats.size);
        });
      });
      this.calculatePlayInterval();
      contentMsg = {
        result: {
          replayID: this.cloudReplayID,
          uri: `ws://${config.domainName}:${config.wsPort}/data/${this.cloudReplayID}`,
        },
      };
    } catch (err) {
      logger.error(err);
      contentMsg = {
        error: {
          code: 500,
          message: err.message,
        },
      };
    }
    this.sendMessage(decodeMsg, contentMsg);
  }

  async start(decodeMsg) {
    if (
      this.getValidationResult(
        decodeMsg.params.playIndex,
        decodeMsg.params.playTimeSpeed
      )
    ) {
      this.playIndex = decodeMsg.params.playIndex - 1;
      this.playTimeSpeed = decodeMsg.params.playTimeSpeed;
      this.startSendData();
      this.serviceStatus = RUNNING;
      this.sendMessage(decodeMsg, successMsg);
    } else {
      this.sendMessage(decodeMsg, failedMsg);
    }
  }

  async pause(decodeMsg) {
    this.stopSendData();
    this.serviceStatus = PAUSED;
    this.sendMessage(decodeMsg, successMsg);
  }

  async goOn(decodeMsg) {
    this.startSendData();
    this.serviceStatus = RUNNING;
    this.sendMessage(decodeMsg, successMsg);
  }

  async setParameters(decodeMsg) {
    if (
      this.getValidationResult(
        decodeMsg.params.playIndex,
        decodeMsg.params.playTimeSpeed
      )
    ) {
      this.playIndex = decodeMsg.params.playIndex - 1;
      this.playTimeSpeed = decodeMsg.params.playTimeSpeed;
      if (this.serviceStatus === RUNNING) {
        this.startSendData();
      }
      this.sendMessage(decodeMsg, successMsg);
    } else {
      this.sendMessage(decodeMsg, failedMsg);
    }
  }

  async singleReplay(decodeMsg) {
    if (
      decodeMsg.params.playIndex <= this.indexDatas.length &&
      decodeMsg.params.playIndex >= 1
    ) {
      this.stopSendData();
      this.serviceStatus = PAUSED;
      this.playIndex = decodeMsg.params.playIndex - 1;
      this.sendMessage(decodeMsg, successMsg);
      await this.sendData();
    } else {
      this.sendMessage(decodeMsg, failedMsg);
    }
  }

  async stop(decodeMsg) {
    this.clear();
    this.serviceStatus = STOPPED;
    this.sendMessage(decodeMsg, successMsg);
  }

  clear() {
    this.stopSendData();
    this.closeFileDescriptor();
  }

  taskStopped(decodeMsg) {
    if (this.fd) {
      return false;
    }
    this.sendMessage(decodeMsg, stoppedMsg);
    return true;
  }
}

module.exports = ReplayServie;
