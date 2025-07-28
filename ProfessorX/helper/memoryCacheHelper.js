// eslint-disable-next-line import/no-extraneous-dependencies
const cache = require('memory-cache');

exports.init = async () => {};
// client.auth(123456);
/*
 * 添加字符串 string 类型
 * @name addStr
 * @alias addStr
 * @param key `key`
 * @param value `value`
 * @api public
 */
// eslint-disable-next-line no-unused-vars
exports.setStr = async (key, value, expire = 0) => {
  cache.put(key, value);
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
  return cache.get(key);
};
/**
 * 添加对象
 * @name addObject
 * @alias addObject
 * @param key `key`
 * @param key `value`
 * @api public
 */
// eslint-disable-next-line no-unused-vars
exports.setObject = async (key, value, expire) => {
  // 把对象转成json
  cache.put(key, JSON.stringify(value));
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
  const data = cache.get(key);

  return JSON.parse(data);
  // return data;
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
  const res = cache.get(key);
  if (res === '' || res == null) {
    return null;
  }
  const data = JSON.parse(res);
  return data[property];
};
/**
 * 删除缓存
 * @name deleteKey
 * @alias deleteKey
 * @param key `key`
 * @api public
 */
exports.deleteKey = async (key) => {
  cache.del(key);
};
