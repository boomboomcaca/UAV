/* eslint-disable no-restricted-properties */
/* eslint-disable import/no-extraneous-dependencies */
const FlakeId = require('flake-idgen');
const intformat = require('biguint-format');
const fs = require('fs');
const UUID = require('uuid');
const moment = require('moment');

const flakeIdGen = new FlakeId({ epoch: 1300000000000 });
const { dataTypeEnum } = require('./enum');

exports.getDataType = (param) => {
  if (typeof param === 'string') {
    return dataTypeEnum.string;
  }
  if (
    typeof param === 'object' &&
    // eslint-disable-next-line eqeqeq
    typeof param.constructor !== 'undefined' &&
    // eslint-disable-next-line eqeqeq
    param.constructor == Array
  ) {
    return dataTypeEnum.array;
  }
  if (typeof param === 'object') {
    return dataTypeEnum.object;
  }
  if (typeof param === 'number') {
    return dataTypeEnum.number;
  }
  return dataTypeEnum.string;
};
exports.isUndefinedOrNull = (param) => {
  // 可以考虑只用最后一个

  if (param == null) {
    return true;
  }
  // 经过测试可以不要这个分支
  // if (typeof param === 'undefined') {
  //   return true;
  // }
  if (typeof param === 'string' && param.replace(/\s/g, '').length === 0) {
    return true;
  }
  if (typeof param === 'object' && Object.keys(param).length === 0) {
    return true;
  }
  return false;
};
exports.copyObject = (origin) => {
  return { ...origin };
};

exports.getGUID = () => {
  return UUID.v1();
};

exports.getFlakeId = () => {
  return intformat(flakeIdGen.next(), 'dec');
};
// 获取随机数
exports.getRandomNum = (Min, Max) => {
  const Range = Max - Min;
  const Rand = Math.random();
  return Min + Math.round(Rand * Range);
};
// 获取指定长度随机数
exports.getRandomRangeNum = (length) => {
  // 1000000
  // 99999999
  const minNumber = Math.pow(10, length - 1);
  const maxNumber = Math.pow(10, length) - 1;
  const range = maxNumber - minNumber; // 取值范围的差
  const random = Math.random(); // 小于1的随机数
  return minNumber + Math.round(random * range); // 最小数与随机数和取值范围求和，返回想要的随机数字
};
exports.isJSON = (str) => {
  // 使用loaddash 方法判断
  if (typeof str === 'string') {
    try {
      const obj = JSON.parse(str);
      if (typeof obj === 'object' && obj) {
        return true;
      }
      return false;
    } catch (e) {
      return false;
    }
  }
  return false;
};
exports.tryParseJson = (str) => {
  // 使用loaddash 方法判断
  if (typeof str === 'string') {
    try {
      const obj = JSON.parse(str);
      if (typeof obj === 'object' && obj) {
        return obj;
      }
      return null;
    } catch (e) {
      return null;
    }
  }
  return null;
};
exports.readFile = async (filename) => {
  return new Promise((resolve, reject) => {
    fs.readFile(filename, (err, res) => {
      if (err) {
        reject(err);
      } else {
        resolve(res);
      }
    });
  });
};
// 相对目录
exports.writeImage = async (canvas, imageName) => {
  return new Promise((resolve, reject) => {
    fs.writeFile(`${imageName}`, canvas.toBuffer(), (err) => {
      if (err) {
        reject(err);
      }
      resolve();
    });
  });
};

exports.dateFormat = 'YYYY-MM-DD HH:mm:ss';
exports.dateSecondFormat = 'YYYY-MM-DD HH:mm:ss.SSS';

/**
 * 获取当前时间字符串
 * @param {*} format 时间格式
 */
exports.getCurrentDate = (format = 'YYYY-MM-DD HH:mm:ss') => {
  const time = moment(new Date()).format(format);
  return time;
};

/**
 * 时间戳转换为 format 格式的字符串
 * @param {Date} date  1970-1-1 00:00:00 UTC 起 时间戳，单位：纳秒
 * @param {string} format 时间格式
 */
exports.getDateString = (date, format) => {
  return moment(new Date(date / 1000000)).format(format);
};
