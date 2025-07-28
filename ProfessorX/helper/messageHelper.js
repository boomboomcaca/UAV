const redisStreamHelper = require('./redisStreamHelper');
const eventHelper = require('./eventHelper');
const config = require('../data/config/config');

let helper;

const getMessageHelper = () => {
  if (helper) {
    return helper;
  }
  if (config.redisStream.useRedis) {
    helper = redisStreamHelper;
  } else {
    helper = eventHelper;
  }
  helper.init();
  return helper;
};

module.exports = {
  getMessageHelper,
  // init,
  // getProducer,
  // getConsumer,
  // sendData,
};
