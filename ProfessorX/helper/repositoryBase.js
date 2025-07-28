/* eslint-disable eqeqeq */
/* eslint-disable consistent-return */
/*!
 * repositoryBase It provides the basic query method of adding, deleting and modifying
 * nodeJs Ling https://www.cnblogs.com/laien/p/5610884.html
 * Copyright (c) 2020, 2024, jun bao.
 * Released under the MIT License.
 */

const {
  resSuccess,
  resError,
  updateModifyInfo,
  updateAddInfo,
} = require('./handle');

const {
  getOne,
  getList,
  add,
  update,
  del,
  getCount,
  execSql,
  transactingAction,
  addList,
  knexInstance,
} = require('../db/dbHelper');

const {
  getStr,
  setStr,
  getObject,
  setObject,
  deleteKey,
  getJson,
  setJson,
  getObjectProperty,
} = require('./cacheManager');

const config = require('../data/config/config');

const { getLogger } = require('./log4jsHelper');

const { dataTypeEnum } = require('./enum');

const {
  getDataType,
  copyObject,
  isUndefinedOrNull,
  getGUID,
  getFlakeId,
  getRandomRangeNum,
  isJSON,
  tryParseJson,
  readFile,
  getCurrentDate,
} = require('./common');

// 可以考虑使用 es6 的语法重构 反射这部分

// Object.keys：返回一个数组，成员是参数对象自身的（不含继承的）所有可遍历属性的键名

// Object.values：返回一个数组，成员是参数对象自身的（不含继承的）所有可遍历属性的键值

// Object.entries：返回一个数组，成员是参数对象自身的（不含继承的）所有可遍历属性的键值对

// https://blog.csdn.net/qq449245884/article/details/108333320?spm=1000.2115.3001.4277
// 高频 js 代码片段
// 处理下切分正式环境和开发环境js
// 导出数据库方法

// 备注：delete 是nodejs 关键字不能使用

// 针对http 接口添加删除修改查询 通用方法名称
// insert edit remove insertList removeList getPageData getDetail
// 针对数据库直接操作方法 定义 添加 删除 修改 查询 通用方法名称
// add  update  del  batchInsert   getList  getOne

exports.add = add;
exports.addList = addList;
exports.getOne = getOne;
exports.getList = getList;
exports.update = update;
exports.del = del;
exports.getCount = getCount;

exports.execSql = execSql;
exports.transactingAction = transactingAction;

exports.knex = knexInstance;

// 导出handle方法
exports.resSuccess = resSuccess;
exports.resError = resError;
exports.updateModifyInfo = updateModifyInfo;
exports.updateAddInfo = updateAddInfo;

// 导出缓存方法
exports.getStr = getStr;
exports.setStr = setStr;
exports.getObject = getObject;
exports.setObject = setObject;
exports.getJson = getJson;
exports.setJson = setJson;
exports.deleteKey = deleteKey;
exports.getObjectProperty = getObjectProperty;

// 导出config
exports.config = config;
exports.getLogger = getLogger;

// 通用方法
exports.dataTypeEnum = dataTypeEnum;
exports.getDataType = getDataType;
exports.isUndefinedOrNull = isUndefinedOrNull;
exports.getGUID = getGUID;
exports.getFlakeId = getFlakeId;
exports.getRandomRangeNum = getRandomRangeNum;
exports.isJSON = isJSON;
exports.tryParseJson = tryParseJson;
exports.readFile = readFile;
exports.getCurrentDate = getCurrentDate;

async function deleteCacheKeys(dicKeys) {
  if (typeof dicKeys !== 'undefined') {
    for (let i = 0; i < dicKeys.length; i++) {
      await deleteKey(dicKeys[i]);
    }
  }
}
/**
 *自动属性替换
 *
 * @param {*} req
 * @param {*} entityMap
 */
exports.autoMap = (data, entityMap, reverse = false) => {
  const index = reverse === true ? 1 : 0;
  const autoMapData = { ...data };
  const autoMapsAttribute = entityMap
    .filter((s) => typeof s === 'object')
    .filter((s) => Object.entries(s)[0][index] in autoMapData)
    .map((item) => {
      // const key = Object.entries(item)[0][0];
      // const value = Object.entries(item)[0][1];
      const [key, value] = Object.entries(item)[0];
      return { key, value };
    });
  autoMapsAttribute.forEach((item) => {
    // 如果需要反转key value 交换
    let { key, value } = item;
    if (reverse) {
      const temp = key;
      key = value;
      value = temp;
    }
    if (key in autoMapData) {
      autoMapData[value] = autoMapData[key];
      delete autoMapData[key];
    }
  });
  return autoMapData;
};
/**
 * 添加
 * @name insert
 * @alias insert
 * @param req `http请求对象`
 * @param reply `http回复对象`
 * @param tableName `表名称`
 * @param tableChineseName '表中文名称'
 * @param uniqueKey '字段唯一值去重验证'
 * @param dicKeys  '字典缓存Key'
 * @param unionKey '联合去重字段验证'
 * @param returnData '返回对象'
 * @param primaryKey '主键'
 * @param primaryKeyType '主键类型 int GUID'
 * @return {any}
 * @api public
 */
exports.insert = async ({
  req,
  reply,
  tableName,
  tableChineseName,
  dicKeys,
  returnData,
  failMessage,
  primaryKey = 'id',
  primaryKeyType = 'int',
  isReply = true,
}) => {
  await deleteCacheKeys(dicKeys);
  let data = req.body;
  let res = '';
  if (primaryKeyType === 'GUID' && isUndefinedOrNull(data[primaryKey])) {
    data[primaryKey] = getGUID();
  }
  data = updateAddInfo(data);
  try {
    res = await add({ tableName, mainData: data });
  } catch (error) {
    if (error.code === 'ER_DUP_ENTRY') {
      resError({ message: `添加${tableChineseName}失败` });
    }
    throw error;
  }

  if (res[0] < 0) {
    const message = failMessage || `添加${tableChineseName}失败`;
    resError({ message });
  }
  if (res[0] == 0 || primaryKeyType === 'GUID') {
    res = [];
    res.push(data[primaryKey]);
  }
  if (!isReply) return res;
  resSuccess({ reply, result: returnData || res });
  // resSuccess({ reply, message, data: returnData || res });
};
/**
 * 批量添加
 * @name insertList
 * @alias insertList
 * @param req `http请求对象`
 * @param reply `http回复对象`
 * @param tableName `表名称`
 * @param tableChineseName '表中文名称'
 * @param uniqueKey '字段唯一值去重验证'
 * @param dicKeys  '字典缓存Key'
 * @param unionKey '联合去重字段验证'
 * @return {any}
 * @api public
 */
exports.insertList = async ({
  req,
  reply,
  tableName,
  tableChineseName,
  dicKeys,
  failMessage,
  isReply = true,
}) => {
  await deleteCacheKeys(dicKeys);
  let data = req.body;
  let res = '';
  data = data.map((s) => updateAddInfo(s));
  try {
    res = await add({ tableName, mainData: data });
  } catch (error) {
    if (error.code === 'ER_DUP_ENTRY') {
      resError({ message: `批量添加${tableChineseName}失败` });
    } else {
      throw error;
    }
  }
  let message = '';
  if (res[0] < 0) {
    message = failMessage || `批量添加${tableChineseName}失败`;
    resError({ message });
  }
  // todo: 需要测试 添加 失败判断
  // res = await knexInstance.batchInsert(tableName, data, 1000).returning('id');
  // res = await add({ tableName, mainData: data });
  if (!isReply) return res;

  resSuccess({ reply, result: res });
};
/**
 * 修改
 * @name edit
 * @alias edit
 * @param req `http请求对象`
 * @param reply `http回复对象`
 * @param tableName `表名称`
 * @param tableChineseName '表中文名称'
 * @param uniqueKey '字段唯一值去重验证'
 * @param dicKeys  '字典缓存Key'
 * @param unionKey '联合去重字段验证'
 * @param returnData '返回对象'
 * @param primaryKey '主键'
 * @return {any}
 * @api public
 */
exports.edit = async ({
  req,
  reply,
  tableName,
  tableChineseName,
  dicKeys,
  primaryKey,
  returnData,
  failMessage,
  isReply = true,
}) => {
  await deleteCacheKeys(dicKeys);

  const data = req.body;
  const wheres = {};
  let mainData = {};
  let res = '';
  mainData = updateModifyInfo(copyObject(data));
  delete mainData[primaryKey];
  wheres[primaryKey] = data[primaryKey];
  // mainData = updateModifyInfo(data);
  try {
    res = await update({ tableName, mainData, wheres });
  } catch (error) {
    if (error.code === 'ER_DUP_ENTRY') {
      resError({ message: `更新${tableChineseName}失败` });
    } else {
      throw error;
    }
  }
  // todo:更新待测试 int 和 guid 主键不存在的情况
  if (res <= 0) {
    const message = failMessage || `更新${tableChineseName}失败`;
    resError({ message });
  }

  if (!isReply) return res;
  // 改为result
  resSuccess({ reply, result: returnData || res });
  // resSuccess({ reply, message, data: returnData || res });
};
/**
 * 删除(根据条件删除)
 * @name remove
 * @alias remove
 * @param req `http请求对象`
 * @param reply `http回复对象`
 * @param tableName `表名称`
 * @param tableChineseName '表中文名称'
 * @param dicKeys  '字典缓存Key'
 * @param returnData '返回对象'
 * @return {any}
 * @api public
 */
exports.remove = async ({
  req,
  reply,
  tableName,
  tableChineseName,
  dicKeys,
  returnData,
  failMessage,
  isReply = true,
}) => {
  await deleteCacheKeys(dicKeys);
  // 删除批量删除
  const wheres = req.body;
  const res = await del({ tableName, wheres });
  // 其实就是判断下
  if (res <= 0) {
    const message = failMessage || `删除${tableChineseName}失败`;
    resError({ message });
  }
  if (!isReply) return res;
  resSuccess({ reply, result: returnData || res });
};
/**
 * 批量删除
 * @name removeList
 * @alias removeList
 * @param req `http请求对象`
 * @param reply `http回复对象`
 * @param tableName `表名称`
 * @param tableChineseName '表中文名称'
 * @param dicKeys  '字典缓存Key'
 * @return {any}
 * @api public
 */
exports.removeList = async ({
  req,
  reply,
  tableName,
  tableChineseName,
  dicKeys,
  failMessage,
  isReply = true,
}) => {
  await deleteCacheKeys(dicKeys);
  const data = req.body;
  const dataAttribute = Object.entries(data);
  const [key, value] = dataAttribute[0];
  const wheresIn = {
    key,
    value,
  };
  if (wheresIn.value.length <= 0) {
    resError({ reply, message: '参数错误' });
  }
  const res = await del({ tableName, wheresIn });
  if (res <= 0) {
    const message = failMessage || `删除${tableChineseName}失败`;
    resError({ message });
  }
  if (!isReply) return res;
  resSuccess({ reply, result: res });
};
/**
 * 获取分页数据
 * @name getPageData
 * @alias getPageData
 * @param req `http请求对象`
 * @param reply `http回复对象`
 * @param tableName `表名称`
 * @param entityMap '实体映射Map'
 * @param isLimitMaxRow '是否限制分页最大行数 默认限制  true 限制 false 不限制'
 * @param isReply   '是否reply Http请求 默认true 直接reply Http请求'
 * @return {any}
 * @api public
 */
exports.getPageData = async ({
  req,
  reply,
  tableName,
  entityMap,
  isLimitMaxRow = true,
  isReply = true,
}) => {
  // await deleteCacheKeys(dicKeys);
  // 不在区分GET POST 都是一样的数据格式回传
  let data = { wheres: {}, wheresIn: {} };
  const { pageDefault } = config;
  // Object.entries //这个是数组
  // 去掉反射 使用ES6 新特性
  Object.entries(req.body).forEach((item) => {
    // const key = item.firstOrDefault();
    // const value = item.lastOrDefault();
    const [key, value] = item;
    if (!Object.keys(pageDefault).some((s) => s === key)) {
      if (getDataType(value) !== dataTypeEnum.array) data.wheres[key] = value;
      else data.wheresIn = { key, value };
    } else {
      data[key] = value;
    }
  });
  data.page = data.page || pageDefault.page;
  data.rows = data.rows || pageDefault.rows;
  data.rows = data.rows > pageDefault.maxRows ? pageDefault.maxRows : data.rows;
  data.tableName = tableName;
  // 通过实体映射 指定查询列
  data = { ...data, queryColumn: entityMap };
  // 如果不限制最大行数
  if (!isLimitMaxRow && !req.body.rows) {
    delete data.page;
  }

  // getCount 和 getList 可以一起发送到数据库执行
  const count = await getCount({
    tableName,
    wheres: data.wheres,
    wheresIn: data.wheresIn,
  });
  const res = await getList(data);
  const listData = {
    rows: res,
    total: count[0].count,
  };
  if (!isReply) {
    return listData;
  }
  // 其实也就是对象属性合并 result[] total 其实也就是对象属性合并
  resSuccess({ reply, result: res, total: count[0].count });
};
/**
 * 获取明细数据
 * @name getDetail
 * @alias getDetail
 * @param req `http请求对象`
 * @param reply `http回复对象`
 * @param tableName `表名称`
 * @param isReply   '是否reply Http请求 默认true 直接reply Http请求'
 * @return {any}
 * @api public
 */
exports.getDetail = async ({ req, reply, tableName, isReply = true }) => {
  const wheres = { id: req.body.id };
  const res = await getOne({ tableName, wheres });
  if (!isReply) {
    return res;
  }
  // 其实我就可以这段可以不用修改！
  // 当然成功默认是返回Result的！
  resSuccess({ reply, result: res });
};
/**
 * 获取单条数据
 * @name getSingle
 * @alias getSingle
 * @param tableName `表名称`
 * @param wheres  `查询条件`
 * @return {any}
 * @api public
 */
exports.getSingle = async ({ tableName, wheres }) => {
  const res = await getOne({ tableName, wheres });
  const [data] = res;
  return data || null;
};
/**
 * Sql查询
 * @name sqlQuery
 * @alias sqlQuery
 * @param sql `sql`
 * @return {any}
 * @api public
 */
exports.sqlQuery = async (sql) => {
  // 有可能查询的一条 有可能查询的是一个对象出来
  const res = await execSql(sql);

  // ！！！sqlite与mysql查询结果有区别，sqlite直接返回，mysql返回res[0]
  if (config.DB === 'sqlite3') {
    return res;
  }
  const [data] = res;
  return data || null;
};
