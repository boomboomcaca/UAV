// 基础操作 添加删除 修改 基于数据库二次封装
const { isUndefinedOrNull } = require('../../helper/common');
const {
  edit,
  remove,
  getPageData,
  removeList,
  config,
  resError,
  resSuccess,
  updateAddInfo,
  sqlQuery,
} = require('../../helper/repositoryBase');
// 基础帮助方法 返回成功 失败  添加时间 更新时间赋值
// 缓存基本操作
const { setJson, getJson, deleteKey } = require('../../helper/repositoryBase');
// 数据库基本操作方法
const { getCount, transactingAction } = require('../../helper/repositoryBase');

const dicListTableName = 'sys_dictionarylist';
const tableName = 'sys_dictionary';
let param = {
  tableName,
  primaryKey: 'id',
  tableChineseName: '字典',
  dicKeys: [config.cacheKey.dic],
  uniqueKey: 'dic_no',
};

// 字典初步思路 用到什么加载什么但是sql不缓存！
// 直接全部查询出来在内存缓存！所有的通过RedisHash对象去查询 速度快！
// 如果没有的直接从数据库查询！如果包含sql不存储！
// 每次更新删除所有字典缓存！
// 通过Hash

exports.addSchema = {
  description: `添加${param.tableChineseName}`,
  tags: [param.tableName],
  summary: `添加${param.tableChineseName}`,
  body: {
    type: 'object',
    properties: {
      dic_no: { type: 'string' },
      name: { type: 'string' },
      db_sql: { type: 'string' },
      remark: { type: 'string' },
      dicList: {
        type: 'array',
        description: '字典明细',
        items: {
          type: 'object',
          properties: {
            dic_id: { type: 'integer' },
            order_no: { type: 'integer' },
            key: { type: 'string' },
            value: { type: 'string' },
            enable: { type: 'integer' },
            remark: { type: 'string' },
          },
        },
      },
    },
  },
};
exports.getDicSchema = {
  description: `获取字典数据`,
  tags: [tableName],
  summary: `获取字典数据`,
  body: {
    type: 'array',
    items: [
      {
        type: 'string',
      },
    ],
  },
};
/**
 *添加字典 主表和明细表同步添加 使用事务
 *
 * @param {*} req
 * @param {*} reply
 */
exports.add = async (req, reply) => {
  const data = req.body;
  // 移除dicList 属性
  // eslint-disable-next-line prefer-const
  let { dicList, ...mainData } = data;
  mainData = updateAddInfo(mainData);
  const wheres = { dic_no: mainData.dic_no };
  const count = await getCount({ tableName, wheres });
  if (count[0].count >= 1) {
    resError({ message: `字典编号:${mainData.dic_no}已存在` });
  }
  // todo: mysql并发 事务 脏读 事务进行了修改操作,还没有提交到数据库 后面的查询又来了
  await transactingAction(async (knex, trx) => {
    const resp = await knex(tableName).transacting(trx).insert(mainData);
    const [id] = resp;
    if (!isUndefinedOrNull(dicList) && dicList.length >= 1) {
      // todo:map 特可以实现
      for (let i = 0; i < dicList.length; i++) {
        dicList[i].dic_id = id;
        dicList[i] = updateAddInfo(dicList[i]);
      }
      await knex(dicListTableName).transacting(trx).insert(dicList);
    }
  });
  deleteKey(config.cacheKey.dic);
  resSuccess({ reply, result: [] });
  // 把事务封装下外面可以无感调用
};
async function getDicCache() {
  const sql = `select a.dic_no,b.key,b.value,a.db_sql from sys_dictionary a 
  left join sys_dictionarylist b
  on a.id=b.dic_id and b.enable=1`;
  const res = await sqlQuery(sql);
  const dicCache = res.map((item) => {
    return {
      dic_no: item.dic_no,
      key: item.key,
      value: item.value,
      db_sql: item.db_sql,
    };
  });
  await setJson(config.cacheKey.dic, dicCache);
  return dicCache;
}
exports.getDic = async (req, reply) => {
  // 输入的字典编号
  const dicNoArray = req.body;
  const dicArray = [];
  let dicCache = await getJson(config.cacheKey.dic);
  if (dicCache == null) {
    // 从字典里面读取缓存
    dicCache = await getDicCache();
  } else {
    // 判断当前Key是否在字典缓存中，如果不在字典缓存中,则重新初始化最新缓存
    dicCache = dicCache.filter((s) => dicNoArray.indexOf(s) > -1);
    if (dicCache.length === 0) {
      // 如果没有重新初始化缓存
      dicCache = await getDicCache();
    }
  }
  for (let i = 0; i < dicNoArray.length; i++) {
    const dicNo = dicNoArray[i];
    const dicData = {
      dicNo,
      data: [],
    };
    const dics = dicCache.filter((s) => {
      return s.dic_no === dicNo;
    });
    if (dics.length >= 1 && isUndefinedOrNull(dics[0].db_sql)) {
      dicData.data = dics.map((item) => {
        return { key: item.key, value: item.value };
      });
    } else if (dics.length === 1 && dics[0].db_sql !== '') {
      const dicList = await sqlQuery(dics[0].db_sql);
      // dicData.data = dicList[0];
      dicData.data = dicList.map((item) => {
        return { key: item.key, value: item.value };
      });
    }
    dicArray.push(dicData);
  }
  resSuccess({ reply, result: dicArray });
};
function reqParam(req, reply) {
  param = { ...param, req, reply };
  return param;
}
/**
 *修改字典 不包括 修改明细数据
 *
 * @param {*} req
 * @param {*} reply
 */
exports.update = async (req, reply) => {
  await edit(reqParam(req, reply));
};
/**
 *根据输入条件删除
 *
 * @param {*} req
 * @param {*} reply
 */
exports.del = async (req, reply) => {
  // 删除批量删除
  await remove(reqParam(req, reply));
};

/**
 *批量删除
 *
 * @param {*} req
 * @param {*} reply
 */
exports.delList = async (req, reply) => {
  // 删除批量删除
  await removeList(reqParam(req, reply));
};
/**
 *获取列表
 *
 * @param {*} req
 * @param {*} reply
 */
exports.getList = async (req, reply) => {
  await getPageData(reqParam(req, reply));
};
