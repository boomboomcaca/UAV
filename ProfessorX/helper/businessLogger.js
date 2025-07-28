/* eslint-disable no-param-reassign */
/* eslint-disable func-names */
const { getCurrentDate, getDataType } = require('./common');
// const { add } = require('../db/dbHelper');
const { dataTypeEnum } = require('./enum');
const { getLogger } = require('./log4jsHelper');

const logger = getLogger('businessLogger');

const businessLog = require('../data/config/bussinessLog.json');
const { businessLogCodeEnum } = require('./enum');
const { addLog } = require('../manager/logRecorder');

exports.businessLog = businessLog;
exports.businessLogCodeEnum = businessLogCodeEnum;

function BusinessLogger() {
  // this.info = async function (code, msg, req = null) {
  //   await this.writeLog('info', code, msg, req);
  // };
  // this.warn = async function (code, msg, req = null) {
  //   await this.writeLog('warn', code, msg, req);
  // };
  // this.error = async function (code, msg, req = null) {
  //   await this.writeLog('error', code, msg, req);
  // };
  this.writeLog = async function ({
    level,
    code,
    parameter1 = null,
    parameter2 = null,
    parameter3 = null,
    req = null,
    edgeID = null,
  }) {
    try {
      const logEntity = {};
      logEntity.level = level;
      logEntity.code = code;
      logEntity.parameter1 = parameter1;
      logEntity.parameter2 = parameter2;
      logEntity.parameter3 = parameter3;
      logEntity.edge_id = edgeID;
      if (req != null) {
        if (req.user) {
          logEntity.user_id = req.user.userId;
          logEntity.user_name = req.user.account;
        }
        logEntity.ip = req.ip || '';
      }
      logEntity.create_time = getCurrentDate();
      logEntity.type = `${code.toString().substring(0, 1)}00000`;
      await this.insertLog(logEntity);
    } catch (error) {
      logger.error(error);
    }
    // 直接调用dbHelper 里面的方法直接写入日志
  };
  this.insertLog = async function (logEntity) {
    // 如果是数组则对create_time 赋值
    // todo:暂时没有批量插入需求
    if (getDataType(logEntity) === dataTypeEnum.array) {
      for (let i = 0; i < logEntity.length; i++) {
        logEntity[i].type = `${logEntity[i].code
          .toString()
          .substring(0, 1)}00000`;
        logEntity[i].create_time = getCurrentDate();
        const { logType, ...data } = logEntity[i];
        logEntity[i] = data;
      }
    } else {
      logEntity.type = `${logEntity.code.toString().substring(0, 1)}00000`;
      logEntity.create_time = getCurrentDate();
      const { logType, ...data } = logEntity;
      logEntity = data;
    }
    // await add({ tableName: 'log_business', mainData: logEntity });
    await addLog(logEntity);
  };
}
exports.businessLogger = new BusinessLogger();
