// 事件总线
const events = require('events');

// 消息事件
let eventEmitter;

// server.js 中初始化
const init = () => {
  if (eventEmitter === undefined) eventEmitter = new events.EventEmitter();
};

// eslint-disable-next-line no-unused-vars
const createGroup = (clent, groupID, streamsKey) => {};

const getProducer = async () => {
  return eventEmitter;
};

const getConsumer = async () => {
  return eventEmitter;
};

/**
 *
 * @param {发送消息的事件} emitter
 * @param {格式消息} payloads
 */
const sendData = (emitter, payloads) => {
  payloads.forEach((element) => {
    const message = { value: element.messages };
    emitter.emit('message', message);
  });
};

module.exports = {
  getProducer,
  getConsumer,
  sendData,
  init,
  createGroup,
};
