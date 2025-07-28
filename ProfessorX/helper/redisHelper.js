// var Memcached = require('memcached');
// var memcached = new Memcached('127.0.0.1:12111');
// eslint-disable-next-line import/no-extraneous-dependencies
const redis = require('redis');
const config = require('../data/config/config');

// todo:有可能会重复连接redis
let client;

// docker 容器之间通信有问题，需要特别关注处理
async function auth(redisAuth) {
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

// todo:可以考虑直接参数传递进来不在依赖配置文件
exports.init = async () => {
  client = redis.createClient(
    config.redisCache.redisPort,
    config.redisCache.redisHost
  );
  // docker 容器之间通信有问题，需要特别关注处理
  if (config.redisCache.redisAuth !== '') {
    // 要等待授权成功后 在进行操作
    await auth(config.redisCache.redisAuth);
  }
};

function checkRedisConnect() {
  if (!client.connected) {
    // 如果正式环境可以考虑输出日志，但是还是返回null让程序继续执行！
    const err = { statusCode: 500, message: 'Redis 连接失败' };
    throw err;
  }
}
/**
 * 添加字符串 string 类型
 * @name setStr
 * @alias setStr
 * @param key `key`
 * @param value `value`
 * @api public
 */
exports.setStr = async (key, value, expire = 0) => {
  checkRedisConnect();
  // eslint-disable-next-line no-unused-vars
  await new Promise((resolve, reject) => {
    client.set(key, value, (err, res) => {
      if (err) {
        throw err;
        // reject(err)
      } else {
        resolve(res);
      }
    });
  });
  if (expire !== 0) {
    client.expire(key, expire);
  }
};
/**
 * 获取字符串 string 类型
 * @name getStr
 * @alias getStr
 * @param key `key`
 * @api public
 * @return {string}
 */
exports.getStr = async (key) => {
  checkRedisConnect();
  return new Promise((resolve, reject) => {
    client.get(key, (err, res) => {
      if (err) {
        reject(err);
      } else {
        resolve(res);
      }
    });
  });
};
/**
 * 添加对象
 * @name addObject
 * @alias addObject
 * @param key `key`
 * @param key `value`
 * @param key `expire`
 * @api public
 */
exports.setObject = async (key, value, expire = 0) => {
  checkRedisConnect();
  await new Promise((resolve, reject) => {
    client.hmset(key, value, (err, res) => {
      if (err) {
        // throw err;
        reject(err);
      } else {
        resolve(res);
      }
    });
  });
  if (expire !== 0) {
    client.expire(key, expire);
  }
};
/**
 * 获取对象
 * @name getObject
 * @alias getObject
 * @param key `key`
 * @param key `value`
 * @api public
 */
exports.getObject = async (key) => {
  checkRedisConnect();
  return new Promise((resolve, reject) => {
    client.hgetall(key, (err, value) => {
      if (err) {
        reject(err);
      } else {
        resolve(value);
      }
    });
  });
};
/**
 * 获取对象属性
 * @name getObjectProperty
 * @alias getObjectProperty
 * @param key `key`
 * @param key `value`
 * @api public
 */
exports.getObjectProperty = async (key, property) => {
  checkRedisConnect();
  return new Promise((resolve, reject) => {
    client.hget(key, property, (err, value) => {
      if (err) {
        reject(err);
      } else {
        resolve(value);
      }
    });
  });
};
/**
 * 删除缓存
 * @name deleteKey
 * @alias deleteKey
 * @param key `key`
 * @api public
 */
exports.deleteKey = async (key) => {
  checkRedisConnect();
  return new Promise((resolve, reject) => {
    client.del(key, (err, value) => {
      if (err) {
        reject(err);
      } else {
        resolve(value);
      }
    });
  });
};
