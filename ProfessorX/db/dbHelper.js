/* eslint-disable import/order */
/* eslint-disable no-param-reassign */
/*!
 * dbHelper Based on the basic database query method
 * nodeJs Ling https://www.cnblogs.com/laien/p/5610884.html
 * Copyright (c) 2020, 2024, jun bao.
 * Released under the MIT License.
 */
// eslint-disable-next-line no-param-reassign
const config = require('../data/config/config');
const { isUndefinedOrNull, getDataType } = require('../helper/common');
const { dataTypeEnum } = require('../helper/enum');

const { getLogger } = require('../helper/log4jsHelper');

const logger = getLogger('dbHelper');

// todo:这个会多次执行 这样写其实不好
(() => {
  if (config.DB === 'sqlite3') {
    config[config.DB].filename = `${process.cwd()}/data/config/${
      config[config.DB].filename
    }`;
    // __dirname.replace('db', 'config/') + config[config.DB].filename;
  }
})();

// 可以导出一个对象 tableInstance
//  然后我再解析where，拼接成sql
// userInstance.where(s=>s.userName=='张三');
// where 是一个方法
// 引入log A.where(s=>s.userName=='33)
// 如果这样可以查出数据是不是更快了
// eslint-disable-next-line import/order
// todo:可以进行修改 进行读写分离改造
const knex = require('knex')({
  client: config.DB,
  connection: config[config.DB],
  pool: config.pool,
  useNullAsDefault: true,
});

// 导出knex
exports.knexInstance = knex;

/**
 * 事务执行方法
 * @name transactingAction
 * @alias transactingAction
 * @param action `action(knex,trx)`
 * @return {any}
 * @api public
 */
exports.transactingAction = async (action) => {
  const trx = await knex.transaction();
  try {
    await action(knex, trx);
    trx.commit();
  } catch (e) {
    trx.rollback();
    throw e;
  }
};

// 初始化 SDK
// 将基础配置和 sdk.config 合并传入 SDK 并导出初始化完成的 SDK
/**
 * getOne
 * @name getOne
 * @alias getOne
 * @param {tableName,wheres} `PageDataOptions`
 * @return {any}
 * @api public
 */
exports.getOne = async ({ tableName, wheres }) => {
  let query = knex.select().from(tableName);
  // 集成条件判断查询
  if (!isUndefinedOrNull(wheres)) {
    query = query.andWhere(wheres);
  }
  query = query.limit(1);
  const querySql = query.toString();
  if (config.isPrintSql) logger.debug(querySql);
  return query;
};
// 得到knex的查询组合 支持like //有需要可以添加更多的操作符
function getKnexQuery({ wheres, wheresIn, query }) {
  // todo:如果有需要可以考虑集成or
  const wheresLike = [];
  // get knex query
  /* 
   where:{
     create_time:{
       value:[1,2],
       dbOperator:['>','<']
     }
  } */
  if (!isUndefinedOrNull(wheresIn)) {
    // or在没有索引的情况下呈指数增长，in则是正常递增。
    // or的效率为O(n)，而in的效率为O(logn), 当n越大的时候效率相差越明显，or会慢很多。
    query = query.whereIn(wheresIn.key, wheresIn.value);
  }
  if (!isUndefinedOrNull(wheres)) {
    const dataAttribute = Object.entries(wheres);
    dataAttribute.forEach((item) => {
      if (getDataType(item[1]) === dataTypeEnum.object) wheresLike.push(item);
      else query = query.andWhere(item[0], item[1]);
    });
  }
  if (wheresLike.length > 0) {
    wheresLike.forEach((item) => {
      // 可以考虑采用mongodb 定义的查询方式
      // db.userinfo.find({age:{$in;[1,2]}})
      // .where('id', '>', 10)
      // 其实Operator 是一个素组
      for (let i = 0; i < item[1].dbOperator.length; i++) {
        const name = item[0];
        const dbOperator = item[1].dbOperator[i];
        const value = item[1].value[i];
        if (dbOperator === 'like') {
          query = query.andWhere(name, 'like', `%${value}%`);
        } else {
          query = query.andWhere(name, dbOperator, value);
        }
      }
    });
  }
  // 集成In查询

  return query;
}
/**
 * getList
 * @name getList
 * @alias getList
 * @param { page, rows, sort, order, wheres, tableName, wheresIn,queryColumn } `PageDataOptions`
 * @return {any}
 * @api public
 */
exports.getList = async ({
  page,
  rows,
  sort,
  order,
  wheres,
  tableName,
  wheresIn,
  queryColumn,
}) => {
  let query = knex.select().from(tableName);
  if (!isUndefinedOrNull(queryColumn)) {
    query = knex.column(queryColumn).select().from(tableName);
  }
  query = getKnexQuery({ wheres, wheresIn, query });
  // 集成分页
  if (typeof page !== 'undefined') {
    query = query.limit(rows).offset((page - 1) * rows);
  }
  // 集成排序
  if (!isUndefinedOrNull(sort)) {
    query.orderBy(sort, order || config.pageDefault.order);
  }

  const querySql = query.toString();
  if (config.isPrintSql) logger.debug(querySql);
  return query;
};

/**
 *获取总数 count
 *
 * @param {*} {wheres, tableName, wheresIn }
 * @returns
 */
exports.getCount = async ({ wheres, tableName, wheresIn }) => {
  let query = knex(tableName).count({ count: '*' });
  query = getKnexQuery({ wheres, wheresIn, query });
  // 集成条件判断查询
  const querySql = query.toString();
  if (config.isPrintSql) logger.debug(querySql);
  return query;
};

/**
 * add
 * @name add
 * @alias Insert
 * @param {tableName,mainData} `saveModel`
 * @return {any}
 * @api public
 */
exports.add = async ({ tableName, mainData }) => {
  const result = knex(tableName).insert(mainData);
  if (config.isPrintSql) logger.debug(result.toString());
  return result;
};

/**
 * add
 * @name addList
 * @alias addList
 * @param {tableName,mainData,chunkSize} `saveModel`
 * @return {any}
 * @api public
 */
exports.addList = async ({ tableName, mainData, chunkSize = 1000 }) => {
  // 速度要稍微快一点
  const result = await knex.batchInsert(tableName, mainData, chunkSize);
  return result;
};
/**
 * update
 * @name update
 * @alias edit
 * @param {tableName,mainData,wheres} `saveModel`
 * @return {any}
 * @api public
 */
exports.update = async ({ tableName, mainData, wheres }) => {
  let result = knex(tableName).update(mainData);
  if (!isUndefinedOrNull(wheres)) {
    result = result.andWhere(wheres);
  } else {
    // 不允许不带条件更新 抛出异常
    // todo:自定义异常和系统异常分开 统一异常处理个更好处理
    throw new Error('没有更新条件');
  }
  if (config.isPrintSql) logger.debug(result.toString());

  return result;
};
/**
 * Delete
 * @name  del
 * @alias delete
 * @param {tableName,wheres} `deleteModel`
 * @return {any}
 * @api public
 */
exports.del = async ({ tableName, wheres, wheresIn }) => {
  if (isUndefinedOrNull(wheres) && isUndefinedOrNull(wheresIn)) {
    throw new Error('没有删除条件');
  }
  let result = knex(tableName);

  if (!isUndefinedOrNull(wheres)) {
    result = result.where(wheres);
  }
  // 目前knex 暂时没有发现支持 批量删除，可以考虑使用原生的mysql包操作
  if (!isUndefinedOrNull(wheresIn)) {
    result = result.whereIn(wheresIn.key, wheresIn.value);
  }
  result = result.del();
  if (config.isPrintSql) logger.debug(result.toString());
  return result;
};

/**
 * execSql
 * @name query
 * @alias exec sql
 * @param sql `sql`
 * @return {object} `queryResult`
 * @api public
 */
exports.execSql = async (sql) => {
  if (config.isPrintSql) logger.debug(sql);
  return knex.raw(sql);
};
