const redis = require('redis');
const config = require('../data/config/config');
const { getLogger } = require('./log4jsHelper');

const logger = getLogger('redisStreamHelper');

// docker 容器之间通信有问题，需要特别关注处理
async function auth(client, redisAuth) {
  await new Promise((resolve, reject) => {
    client.auth(redisAuth, (err, res) => {
      if (err) {
        // throw err;
        reject(err);
      } else {
        resolve(res);
      }
    });
  });
}

const createGroup = (clent, groupID, streamsKey) => {
  clent.xgroup('CREATE', streamsKey, groupID, '$', 'MKSTREAM', (err) => {
    if (err) {
      if (err.code === 'BUSYGROUP') {
        logger.info(
          `Redis-stream Group ${groupID} already exists at stream ${streamsKey}`
        );
      } else {
        logger.info(err);
      }
    }
  });
};

const getClient = async () => {
  const client = redis.createClient(
    config.redisStream.redisPort,
    config.redisStream.redisHost
  );
  // docker 容器之间通信有问题，需要特别关注处理
  if (config.redisStream.redisAuth !== '') {
    // 要等待授权成功后 在进行操作
    await auth(client, config.redisStream.redisAuth);
  }
  return client;
};

const getProducer = async () => {
  const client = await getClient();
  return client;
};

const getConsumer = async () => {
  const client = await getClient();
  return client;
};

const init = () => {};

const sendData = (producer, payloads) => {
  if (payloads !== undefined) {
    payloads.forEach((element) => {
      producer.xadd(element.topic, '*', 'message', element.messages, (err) => {
        if (err) {
          logger.error(`发送到消息队列出错:${err}`);
        }
      });
    });
  }
};

module.exports = {
  getProducer,
  getConsumer,
  sendData,
  init,
  createGroup,
};
