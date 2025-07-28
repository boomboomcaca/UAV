// 基础操作 添加删除 修改 基于数据库二次封装
const {
  getPageData,
  insert,
  resSuccess,
  sqlQuery,
  isUndefinedOrNull,
  autoMap,
} = require('../../helper/repositoryBase');

const { businessLog } = require('../../helper/businessLogger');
// 基础帮助方法 返回成功 失败  添加时间 更新时间赋值
// 完全可以通过动态配置
let param = {
  tableName: 'log_business',
  primaryKey: 'id',
  tableChineseName: '业务日志',
  entityMap: [
    'id',
    { edgeID: 'edge_id' },
    'type',
    'code',
    'level',
    { userId: 'user_id' },
    'ip',
    'create_time',
    'message',
    'parameter1',
    'parameter2',
    'parameter3',
    { userName: 'user_name' },
  ],
};
/**
 *获取请求参数
 *
 * @param {*} req
 * @param {*} reply
 */
function reqParam(req, reply) {
  param = { ...param, req, reply };
  return param;
}
exports.addSchema = {
  description: `添加业务日志`,
  tags: [param.tableName],
  summary: `添加业务日志`,
  body: {
    type: 'object',
    properties: {
      code: { type: 'string', description: '日志Code' },
      level: { type: 'string', description: '日志等级' },
      edgeID: { type: 'string', description: '边缘端ID' },
      parameter1: { type: 'string', description: '参数1' },
      parameter2: { type: 'string', description: '参数2' },
      parameter3: { type: 'string', description: '参数3' },
    },
    required: ['code', 'level'],
  },
};

exports.getTypeCountListSchema = {
  description: `查询日志指定类型各个等级的数量`,
  tags: [param.tableName],
  summary: `查询日志指定类型各个等级的数量`,
  body: {
    type: 'object',
    properties: {
      type: {
        type: 'string',
        description: '日志类型，可为空，为空则查询全部日志类型',
      },
    },
  },
};

exports.add = async (req, reply) => {
  let data = req.body;
  data.type = `${data.code.toString().substring(0, 1)}00000`;
  data.userId = req.user.userId;
  data.userName = req.user.account;
  data = autoMap(data, param.entityMap, false);
  req.body = data;
  await insert(reqParam(req, reply));
};

function getLogMessage(log) {
  const logItem = businessLog.filter((s) => s.code === log.code)[0];
  log.message = logItem.message.format(
    log.parameter1,
    log.parameter2,
    log.parameter3
  );
  return log.message;
}
/**
 * 获取分页列表
 *
 * @param {*} req
 * @param {*} reply
 */
exports.getList = async (req, reply) => {
  const listData = await getPageData({
    ...reqParam(req, reply),
    isReply: false,
  });
  for (let i = 0; i < listData.rows.length; i++) {
    listData.rows[i].message = getLogMessage(listData.rows[i]);
  }
  resSuccess({ reply, result: listData.rows, total: listData.total });
};

/**
 * 获取日志各个类型的数量
 *
 * @param {*} req
 * @param {*} reply
 */
exports.getTypeCountList = async (req, reply) => {
  let sWhere = '';
  if (!isUndefinedOrNull(req.body.type)) {
    sWhere = ` WHERE type = '${req.body.type}'`;
  }
  const strSql = `SELECT type, level,COUNT(id) as 'value' FROM log_business${sWhere} GROUP BY type,level`;
  const result = await sqlQuery(strSql);
  resSuccess({ reply, result });
};
