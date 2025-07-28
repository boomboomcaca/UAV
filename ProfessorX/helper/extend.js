/* eslint-disable */
/*!
 * extend Based on the basic linq js
 * ling js https://github.com/mihaifm/linq/blob/master/linq.js
 * Copyright (c) 2020, 2024, jun bao.
 * Released under the MIT License.
 */
// lodash js 库  predicate
Enumerable = require('linq');
const _ = require('lodash');
const { stringify } = require('uuid');

Array.prototype.findLastIndex = function (predicateOrObject) {
  return _.findLastIndex(this, predicateOrObject);
};
Array.prototype.findIndex = function (predicateOrObject) {
  return _.findIndex(this, predicateOrObject);
};
Array.prototype.uniq = function () {
  return _.uniq(this);
};
Array.prototype.first = function () {
  return _.first(this);
};
Array.prototype.last = function () {
  return _.last(this);
};
Array.prototype.where = function (predicate) {
  return this.filter(predicate);
  // return Enumerable.from(this).where(predicate);
};
Array.prototype.select = function (predicate) {
  return this.map(predicate);
  // return Enumerable.from(this).select(predicate);
};

String.prototype.format = function (args) {
  var result = this;
  if (arguments.length > 0) {
    if (arguments.length == 1 && typeof args == 'object') {
      for (var key in args) {
        if (args[key] != undefined) {
          const reg = new RegExp('({' + key + '})', 'g');
          result = result.replace(reg, args[key]);
        }
      }
    } else {
      for (var i = 0; i < arguments.length; i++) {
        if (arguments[i] != undefined) {
          //var reg = new RegExp("({[" + i + "]})", "g");//这个在索引大于9时会有问题
          const reg = new RegExp('({)' + i + '(})', 'g');
          result = result.replace(reg, arguments[i]);
        }
      }
    }
  }
  return result;
};
String.prototype.trim = function () {
  return _.trim(this);
};
// 使用Find
// Array.prototype.isExist = function (predicate) {
//   return Enumerable.from(this).where(predicate).toArray().length > 0;
// };
Array.prototype.firstOrDefault = function (predicate) {
  if (predicate == undefined) return Enumerable.from(this).firstOrDefault();
  return Enumerable.from(this).firstOrDefault(predicate);
};

Array.prototype.lastOrDefault = function (predicate) {
  if (predicate == undefined) return Enumerable.from(this).lastOrDefault();
  return Enumerable.from(this).lastOrDefault(predicate);
};
// Array.prototype.objectEntriesKey = function () {
//   return this[0][0];
// };
// Array.prototype.objectEntriesValue = function () {
//   return this[0][1];
// };
// 判断字符串是否是对象
// String.prototype.isObject =function () {
//     return _.isObject(this);
// };
Array.prototype.indexOf = function (val) {
  for (let i = 0; i < this.length; i++) {
    if (this[i] == val) return i;
  }
  return -1;
};
Array.prototype.remove = function (val) {
  const index = this.indexOf(val);
  if (index > -1) {
    this.splice(index, 1);
  }
};
