const config = require('../data/config/config');

const {
  setStr,
  getStr,
  setObject,
  getObject,
  deleteKey,
  getObjectProperty,
  init,
} = config.redisCache.useRedis
  ? require('./redisHelper')
  : require('./memoryCacheHelper');

exports.setStr = setStr;
exports.getStr = getStr;
exports.setObject = setObject;
exports.getObject = getObject;
exports.deleteKey = deleteKey;
exports.getObjectProperty = getObjectProperty;
exports.init = init;

/**
 * 添加 json 对象
 * @name setJson
 * @alias setJson
 * @param key `key`
 * @param value `value`
 * @api public
 */
exports.setJson = async (key, value, expire) => {
  setStr(key, JSON.stringify(value), expire);
};

/**
 *获取 json 对象
 * @name setJson
 * @alias setJson
 * @param key `key`
 * @param value `value`
 * @api public
 */
exports.getJson = async (key) => {
  const res = await this.getStr(key);
  if (res === '' || res == null) {
    return null;
  }

  return JSON.parse(res);
};
