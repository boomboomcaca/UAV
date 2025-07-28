// eslint-disable-next-line import/no-extraneous-dependencies
const async = require('async');
const { addDeviceStatus } = require('../controllers/log/logDeviceStatus');
const { addEdgeStatus } = require('../controllers/log/logEdgeStatus');
// const { addEdgeTrack } = require('../controllers/log/logEdgeTrack');
const { addPlanRunning } = require('../controllers/log/logPlanRunning');
const {
  addTaskInfos,
  updateTaskInfo,
} = require('../controllers/log/logTaskInfo');
const { addFileInfo, updateFileInfo } = require('../db/logRepository');
const config = require('../data/config/config');
const { dateFormat, getDateString } = require('../helper/common');
const { getMessageHelper } = require('../helper/messageHelper');
const { getLogger } = require('../helper/log4jsHelper');
const { addEnviInfos } = require('../controllers/log/logEnviInfo');
const { getEdge } = require('./controlManager');

const logger = getLogger('notificationStorage');
const {
  businessLogger,
  businessLog,
  businessLogCodeEnum,
} = require('../helper/businessLogger');

const getEnviInfo = (enviInfos, element, createTime) => {
  let enviInfo = enviInfos.find((e) => e.device_id === element.moduleID);
  if (!enviInfo) {
    enviInfo = {
      device_id: element.moduleID,
      switchState: [],
      environment: [],
      securityAlarm: [],
      create_time: createTime,
    };
    enviInfos.push(enviInfo);
  }
  return enviInfo;
};

const msgRead = (decodeMsg) => {
  try {
    // 数据分类后转发给前端
    if (decodeMsg && decodeMsg.result) {
      const { edgeID } = decodeMsg.result;
      // 纳秒转毫秒，再转Date字符串
      const createTime = getDateString(decodeMsg.result.timestamp, dateFormat);
      const enviInfos = [];
      const tasks = [];
      decodeMsg.result.dataCollection.forEach(async (element) => {
        switch (element.type) {
          case 'gps':
          case 'compass': {
            // const edgeTrack = {
            //   edge_id: edgeID,
            //   Type: element.type,
            //   content: JSON.stringify(element),
            //   create_time: createTime,
            // };
            // addEdgeTrack(edgeTrack);
            break;
          }
          case 'moduleStateChange': {
            if (element.moduleType === 'device') {
              const deviceStatus = {
                edge_id: edgeID,
                device_id: element.id,
                // moduleType: element.moduleType,
                status: element.state,
                content: element.content,
                create_time: createTime,
              };
              const index = config.edge.controlEdgeID.indexOf(edgeID);
              if (index !== -1) {
                const controlEdge = await getEdge(
                  config.edge.controlEdgeID[index]
                );
                if (controlEdge) {
                  const module = controlEdge.modules.find(
                    (moduleItem) => moduleItem.id === element.id
                  );
                  if (module) {
                    deviceStatus.edge_id = module.edgeID;
                  }
                }
              }
              addDeviceStatus(deviceStatus);
            }
            break;
          }
          case 'edgeStateChange': {
            const edgeState = {
              edge_id: edgeID,
              status: element.state,
              content: element.content,
              create_time: createTime,
            };
            // 收到边缘端上线和离线消息
            let code = businessLogCodeEnum.edgeOffline;
            if (edgeState.status === 'online') {
              code = businessLogCodeEnum.edgeOnline;
            }
            const item = businessLog.filter((s) => s.code === code)[0];
            businessLogger.writeLog({ level: item.level, code, edgeID });

            addEdgeStatus(edgeState);
            break;
          }
          case 'environment': {
            const environment = getEnviInfo(enviInfos, element, createTime);
            environment.environment.push(element);
            break;
          }
          case 'switchState': {
            const switchState = getEnviInfo(enviInfos, element, createTime);
            switchState.switchState.push(element);
            break;
          }
          case 'securityAlarm': {
            const secuAlarm = getEnviInfo(enviInfos, element, createTime);
            secuAlarm.securityAlarm.push(element);
            break;
          }

          case 'crondResult': {
            const planRunning = {
              edge_id: edgeID,
              plan_id: element.crondID,
              description: element.description,
              result: element.result,
              create_time: createTime,
            };
            addPlanRunning(planRunning);
            break;
          }
          case 'fileSaved': {
            if (element.notificationType === 'create') {
              const fileCreate = {
                edgeID,
                taskID: element.taskId,
                params: element.parameters,
                path: element.rootPath,
                sourceFile: `${
                  element.relativePath.startsWith('/')
                    ? element.relativePath
                    : `/${element.relativePath}`
                }${
                  element.fileName.startsWith('/')
                    ? element.fileName
                    : `/${element.fileName}`
                }`,
                type: element.dataType,
                recordCount: element.recordCount,
                filesize: element.size,
                edge_func_id: element.driverId,
                front_func_id: element.pluginId,
                data_start_time: getDateString(
                  element.beginRecordTime,
                  dateFormat
                ),
                data_stop_time: getDateString(
                  element.endRecordTime,
                  dateFormat
                ),
                update_time: getDateString(
                  element.lastModifiedTime,
                  dateFormat
                ),
              };
              addFileInfo(fileCreate);
            } else if (element.notificationType === 'modified') {
              const fileUpdate = {
                edgeID,
                sourceFile: `${
                  element.relativePath.startsWith('/')
                    ? element.relativePath
                    : `/${element.relativePath}`
                }${
                  element.fileName.startsWith('/')
                    ? element.fileName
                    : `/${element.fileName}`
                }`,
              };

              // 同步程序nodejs触发的同步时间不用单位换算；边缘端.net core需要
              if (element.sync_time !== undefined) {
                fileUpdate.sync_time = getDateString(
                  element.sync_time,
                  dateFormat
                );
              }
              if (element.lastModifiedTime !== undefined) {
                fileUpdate.filesize = element.size;
                fileUpdate.recordCount = element.recordCount;

                fileUpdate.data_start_time = getDateString(
                  element.beginRecordTime,
                  dateFormat
                );
                fileUpdate.data_stop_time = getDateString(
                  element.endRecordTime,
                  dateFormat
                );
                fileUpdate.update_time = getDateString(
                  element.lastModifiedTime,
                  dateFormat
                );
              }

              updateFileInfo(fileUpdate);
            }
            break;
          }
          case 'taskList':
            // stopTime 不存在，任务创建，添加；存在，任务停止，更新
            element.taskInfo.forEach((task) => {
              const taskMsg = {
                id: task.id,
                edgeID,
                deviceID: task.deviceID,
                edgeFuncID: task.moduleID,
                params: JSON.stringify(task.majorParameters),
                name: task.name,
                startTime: getDateString(task.startTime, dateFormat),
                stopTime: '',
                workTime: 0,
                account: task.account,
              };

              taskMsg.planID =
                task.crondID === undefined || task.crondID === null
                  ? ''
                  : task.crondID;

              // 添加任务信息先缓存，后批量添加到数据库；更新单条更新
              if (task.stopTime === undefined || task.stopTime === null) {
                tasks.push(taskMsg);
              } else {
                // 计划任务启动时，可能未发送信息到云端（边缘端离线），故更新操作实际为数据存在更新，数据不存在添加
                taskMsg.stopTime = getDateString(task.stopTime, dateFormat);
                taskMsg.workTime = task.workTime;
                updateTaskInfo(taskMsg);
              }
            });
            break;

          case 'log':
          case 'heartBeat':
            break;

          default:
            // 未知数据类型
            break;
        }
      });

      // 包含任务信息，添加到数据库
      if (tasks.length > 0) {
        addTaskInfos(tasks);
      }

      // 包含环境控制信息，添加到数据库
      if (enviInfos.length > 0) {
        const infos = enviInfos.map((eInfo) => {
          const newInfo = {
            device_id: eInfo.device_id,
            switchState:
              eInfo.switchState.length === 0
                ? ''
                : JSON.stringify(eInfo.switchState),
            environment:
              eInfo.environment.length === 0
                ? ''
                : JSON.stringify(eInfo.environment),
            securityAlarm:
              eInfo.securityAlarm.length === 0
                ? ''
                : JSON.stringify(eInfo.securityAlarm),
            create_time: createTime,
          };
          return newInfo;
        });
        addEnviInfos(infos);
      }
    }
  } catch (error) {
    logger.error(error);
    logger.debug(decodeMsg);
  }
};

const readMessage = (client, groupID, consumerId, streamsKey, next) => {
  client.xreadgroup(
    'GROUP',
    groupID,
    consumerId,
    // 'BLOCK',
    // 1000,
    'COUNT',
    100,
    'NOACK',
    'STREAMS',
    streamsKey,
    '>',
    (err, stream) => {
      if (err) {
        logger.error(err);
        next();
      }
      if (stream) {
        const messages = stream[0][1];
        messages.forEach((message) => {
          // convert the message into a JSON Object
          const id = message[0];
          const values = message[1];
          const messageObject = { id };
          for (let i = 0; i < values.length; i += 2) {
            messageObject[values[i]] = values[i + 1];
          }

          const decodeMsg = JSON.parse(messageObject.message);
          msgRead(decodeMsg);
        });
      }
      next();
    }
  );
};

const init = async () => {
  const messageHelper = getMessageHelper();
  const consumer = await messageHelper.getConsumer();

  if (config.redisStream.useRedis) {
    messageHelper.createGroup(
      consumer,
      `groupID_stor`,
      config.redisStream.streamsKey
    );
    async.forever(
      (next) => {
        readMessage(
          consumer,
          `groupID_stor`,
          'consumer',
          config.redisStream.streamsKey,
          next
        );
      },
      (err) => {
        logger.error(err);
      }
    );
  } else {
    consumer.on('message', (message) => {
      const decodeMsg = JSON.parse(message.value);
      msgRead(decodeMsg);
    });
  }
};

module.exports = {
  init,
};
