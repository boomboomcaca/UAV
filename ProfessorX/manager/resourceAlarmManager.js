// eslint-disable-next-line import/no-extraneous-dependencies
const async = require('async');
const msgpack = require('msgpack-lite');
const _ = require('lodash');
const resourceAlarm = require('../controllers/rmbt/resourceAlarm');
const resourceMonitor = require('../controllers/rmbt/resourceMonitor');
const resourceThreshold = require('../controllers/rmbt/resourceThreshold');
const config = require('../data/config/config');
const { getClientConnByType } = require('./clientManager');
const { dateFormat, getDateString } = require('../helper/common');
const { getLogger } = require('../helper/log4jsHelper');
const { getMessageHelper } = require('../helper/messageHelper');

const {
  getGUID,
  getJson,
  isUndefinedOrNull,
} = require('../helper/repositoryBase');

const logger = getLogger('resourceAlarmManager');

let cpuCount = 0;
let memoryCount = 0;
let diskReadCount = 0;
let diskWriteCount = 0;
let diskRemainingCount = 0;
let networkUplinkCount = 0;
let networkDownlinkCount = 0;

const getThreshold = async () => {
  let data = await getJson(config.cacheKey.resourceThreshold);
  if (isUndefinedOrNull(data)) {
    data = await resourceThreshold.getLatestData();
  }
  return data;
};

const AlarmCpuUtilization = async (
  edgeId,
  hostName,
  thresholdId,
  time,
  value,
  condition
) => {
  if (isUndefinedOrNull(condition) || isUndefinedOrNull(value)) {
    return null;
  }

  if (value < condition.limit) {
    cpuCount = 0;
    return null;
  }
  cpuCount++;
  if (cpuCount < condition.count) {
    return null;
  }

  const alarmData = {
    id: getGUID(),
    threshold_id: thresholdId,
    edge_Id: edgeId,
    host_name: hostName,
    alarm_type: 'cpu_utilization',
    alarm_value: value,
    create_time: time,
  };

  await resourceAlarm.addData(alarmData);
  return alarmData;
};

const AlarmMemoryUtilization = async (
  edgeId,
  hostName,
  thresholdId,
  time,
  value,
  condition
) => {
  if (isUndefinedOrNull(condition) || isUndefinedOrNull(value)) {
    return null;
  }

  if (value < condition.limit) {
    memoryCount = 0;
    return null;
  }
  memoryCount++;
  if (memoryCount < condition.count) {
    return null;
  }

  const alarmData = {
    id: getGUID(),
    threshold_id: thresholdId,
    edge_Id: edgeId,
    host_name: hostName,
    alarm_type: 'memory_utilization',
    alarm_value: value,
    create_time: time,
  };

  await resourceAlarm.addData(alarmData);
  return alarmData;
};

const AlarmDiskReadSpeed = async (
  edgeId,
  hostName,
  thresholdId,
  time,
  value,
  condition
) => {
  if (isUndefinedOrNull(condition) || isUndefinedOrNull(value)) {
    return null;
  }
  if (value < condition.limit) {
    diskReadCount = 0;
    return null;
  }
  diskReadCount++;
  if (diskReadCount < condition.count) {
    return null;
  }

  const alarmData = {
    id: getGUID(),
    threshold_id: thresholdId,
    edge_Id: edgeId,
    host_name: hostName,
    alarm_type: 'diskReadCount_speed',
    alarm_value: value,
    create_time: time,
  };
  await resourceAlarm.addData(alarmData);
  return alarmData;
};

const AlarmDiskWriteSpeed = async (
  edgeId,
  hostName,
  thresholdId,
  time,
  value,
  condition
) => {
  if (isUndefinedOrNull(condition) || isUndefinedOrNull(value)) {
    return null;
  }

  if (value < condition.limit) {
    diskWriteCount = 0;
    return null;
  }
  diskWriteCount++;
  if (diskWriteCount < condition.count) {
    return null;
  }
  const alarmData = {
    id: getGUID(),
    threshold_id: thresholdId,
    edge_Id: edgeId,
    host_name: hostName,
    alarm_type: 'diskWriteCount_speed',
    alarm_value: value,
    create_time: time,
  };
  await resourceAlarm.addData(alarmData);
  return alarmData;
};

const AlarmDiskRemaining = async (
  edgeId,
  hostName,
  thresholdId,
  time,
  value,
  condition
) => {
  if (isUndefinedOrNull(condition) || isUndefinedOrNull(value)) {
    return null;
  }

  if (value < condition.limit) {
    diskRemainingCount = 0;
    return null;
  }
  diskRemainingCount++;
  if (diskRemainingCount < condition.count) {
    return null;
  }
  const alarmData = {
    id: getGUID(),
    threshold_id: thresholdId,
    edge_Id: edgeId,
    host_name: hostName,
    alarm_type: 'diskRemainingCount',
    alarm_value: value,
    create_time: time,
  };
  await resourceAlarm.addData(alarmData);
  return alarmData;
};

const AlarmUplinkSpeed = async (
  edgeId,
  hostName,
  thresholdId,
  time,
  value,
  condition
) => {
  if (isUndefinedOrNull(condition) || isUndefinedOrNull(value)) {
    return null;
  }

  if (value < condition.limit) {
    networkUplinkCount = 0;
    return null;
  }
  networkUplinkCount++;
  if (networkUplinkCount < condition.count) {
    return null;
  }
  const alarmData = {
    id: getGUID(),
    threshold_id: thresholdId,
    edge_Id: edgeId,
    host_name: hostName,
    alarm_type: 'uplink_speed',
    alarm_value: value,
    create_time: time,
  };
  await resourceAlarm.addData(alarmData);
  return alarmData;
};

const AlarmDownlinkSpeed = async (
  edgeId,
  hostName,
  thresholdId,
  time,
  value,
  condition
) => {
  if (isUndefinedOrNull(condition) || isUndefinedOrNull(value)) {
    return null;
  }

  if (value < condition.limit) {
    networkDownlinkCount = 0;
    return null;
  }
  networkDownlinkCount++;
  if (networkDownlinkCount < condition.count) {
    return null;
  }
  const alarmData = {
    id: getGUID(),
    threshold_id: thresholdId,
    edge_Id: edgeId,
    host_name: hostName,
    alarm_type: 'downlink_speed',
    alarm_value: value,
    create_time: time,
  };
  await resourceAlarm.addData(alarmData);
  return alarmData;
};

function getCondition(cond) {
  if (isUndefinedOrNull(cond)) {
    return null;
  }

  const array = cond.split(',');
  if (array.length < 1) {
    return null;
  }

  const limit = parseFloat(array[0]);
  const count = array.length < 2 ? 0 : parseInt(array[1], 10);
  return {
    limit,
    count,
  };
}

const dataProcess = async (time, edgeId, data) => {
  if (isUndefinedOrNull(data)) {
    return null;
  }
  const hostName = data.host_name;
  const cpuUtilization = data.cpu.usage;
  const memoryUtilization = data.memory.usage;
  const diskReadSpeed = data.disk.readSpeed;
  const diskWriteSpeed = data.disk.writeSpeed;
  const diskRemaining = data.disk.freeSpace;
  const networkUplinkSpeed = data.network.upstream;
  const networkDownlinkSpeed = data.network.downstream;

  const monitorData = {
    id: 0,
    edge_Id: edgeId,
    host_name: hostName,
    cpu_utilization: cpuUtilization,
    memory_utilization: memoryUtilization,
    disk_read_speed: diskReadSpeed,
    disk_write_speed: diskWriteSpeed,
    disk_remaining: diskRemaining,
    network_uplink_speed: networkUplinkSpeed,
    network_downlink_speed: networkDownlinkSpeed,
    create_time: time,
  };
  await resourceMonitor.addData(monitorData);

  const thresholdData = await getThreshold();
  if (isUndefinedOrNull(thresholdData)) {
    return null;
  }

  const thresholdId = thresholdData.id;
  const cpuUtilizationCond = getCondition(thresholdData.cpu_utilization);
  const memoryUtilizationCond = getCondition(thresholdData.memory_utilization);
  const diskReadSpeedCond = getCondition(thresholdData.disk_read_speed);
  const diskWriteSpeedCond = getCondition(thresholdData.disk_write_speed);
  const diskRemainingCond = getCondition(thresholdData.disk_remaining);
  const networkUplinkSpeedCond = getCondition(
    thresholdData.network_uplink_speed
  );
  const networkDownlinkSpeedCond = getCondition(
    thresholdData.networkDownlinkCount_speed
  );

  const cpuAlarmData = await AlarmCpuUtilization(
    edgeId,
    hostName,
    thresholdId,
    time,
    cpuUtilization,
    cpuUtilizationCond
  );
  const memoryAlarmData = await AlarmMemoryUtilization(
    edgeId,
    hostName,
    thresholdId,
    time,
    memoryUtilization,
    memoryUtilizationCond
  );
  const diskReadSpeedAlarmData = await AlarmDiskReadSpeed(
    edgeId,
    hostName,
    thresholdId,
    time,
    diskReadSpeed,
    diskReadSpeedCond
  );
  const diskWriteSpeedAlarmData = await AlarmDiskWriteSpeed(
    edgeId,
    hostName,
    thresholdId,
    time,
    diskWriteSpeed,
    diskWriteSpeedCond
  );
  const diskRemainingAlarmData = await AlarmDiskRemaining(
    edgeId,
    hostName,
    thresholdId,
    time,
    diskRemaining,
    diskRemainingCond
  );
  const uplinkSpeedAlarmData = await AlarmUplinkSpeed(
    edgeId,
    hostName,
    thresholdId,
    time,
    networkUplinkSpeed,
    networkUplinkSpeedCond
  );
  const downlinkSpeedAlarmData = await AlarmDownlinkSpeed(
    edgeId,
    hostName,
    thresholdId,
    time,
    networkDownlinkSpeed,
    networkDownlinkSpeedCond
  );

  const dataArray = [
    cpuAlarmData,
    memoryAlarmData,
    diskReadSpeedAlarmData,
    diskWriteSpeedAlarmData,
    diskRemainingAlarmData,
    uplinkSpeedAlarmData,
    downlinkSpeedAlarmData,
  ];
  const newArray = dataArray.filter(
    (value) =>
      value !== null && value !== undefined && Object.keys(value).length !== 0
  );

  const alarmData = {
    id: getGUID(),
    type: 'resourceAlarm',
    data: newArray,
  };
  return alarmData;
};

const msgRead = async (decodeMsg) => {
  try {
    // 数据分类后转发给前端
    if (decodeMsg && decodeMsg.result) {
      const { edgeID } = decodeMsg.result;
      // 纳秒转毫秒，再转Date字符串
      const createTime = getDateString(decodeMsg.result.timestamp, dateFormat);

      // "type": "resourceMonitor",
      const elementData = decodeMsg.result.dataCollection.firstOrDefault(
        (element) => element.type === 'resourceMonitor'
      );
      const alarmData = await dataProcess(createTime, edgeID, elementData);
      if (alarmData) {
        const conns = getClientConnByType('notify');
        if (conns) {
          const sendData = _.cloneDeep(decodeMsg);
          sendData.result.dataCollection = [];
          sendData.result.dataCollection.push(alarmData);
          conns.forEach((elementItem) => {
            elementItem.send(msgpack.encode(sendData));
          });
        }
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
        next(err);
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
      `groupID_resource`,
      config.redisStream.streamsKey
    );
    async.forever(
      (next) => {
        readMessage(
          consumer,
          `groupID_resource`,
          'consumerId',
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
