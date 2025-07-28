// 事件总线
const events = require('events');

let emitter;

// 我是不是应该封装成一个Class

// 初始化
const init = () => {
  emitter = new events.EventEmitter();
};
const getEmitter = () => {
  return emitter;
};

// 注册 输入的是一个方法
const register = (eventName, event) => {
  emitter.on(eventName, event);
};

const removeListener = (eventName, event) => {
  emitter.removeListener(eventName, event);
};
const send = (eventName, data) => {
  emitter.emit(eventName, data);
};

module.exports = {
  getEmitter,
  register,
  removeListener,
  send,
  init,
};
