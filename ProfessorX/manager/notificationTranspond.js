// eslint-disable-next-line import/no-extraneous-dependencies
const async = require('async');
const msgpack = require('msgpack-lite');
const _ = require('lodash');
const config = require('../data/config/config');
const { getClientConnByType } = require('./clientManager');
const { getLogger } = require('../helper/log4jsHelper');

const { getMessageHelper } = require('../helper/messageHelper');

const logger = getLogger('notificationTranspond');

const msgRead = (decodeMsg) => {
  try {
    // 数据分类后转发给前端
    if (decodeMsg && decodeMsg.result) {
      const sendData = _.cloneDeep(decodeMsg);
      sendData.result.dataCollection = [];
      let conns;
      decodeMsg.result.dataCollection.forEach((element) => {
        switch (element.type) {
          case 'gps':
          case 'compass':
          case 'moduleStateChange':
          case 'edgeStateChange':
          case 'environment':
          case 'switchState':
          case 'securityAlarm': {
            sendData.result.dataCollection.push(element);
            if (!conns) {
              conns = getClientConnByType('notify');
            }
            break;
          }

          case 'crondResult':
          case 'fileSaved':
          case 'taskList':
          case 'log':
          case 'heartBeat':
          case 'businessLog':
            break;

          default: {
            const connUndefined = getClientConnByType(element.type);
            if (connUndefined) {
              const undefinedData = _.cloneDeep(decodeMsg);
              undefinedData.result.dataCollection = [element];

              connUndefined.forEach((elementItem) => {
                elementItem.send(msgpack.encode(undefinedData));
              });
            }
          }
        }
      });
      if (conns) {
        conns.forEach((elementItem) => {
          elementItem.send(msgpack.encode(sendData));
        });
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
      `groupID_trans`,
      config.redisStream.streamsKey
    );
    async.forever(
      (next) => {
        readMessage(
          consumer,
          `groupID_trans`,
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
