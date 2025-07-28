// 基础操作 添加删除 修改 基于数据库二次封装
const {
  // edit,
  getPageData,
  // removeList,
  resSuccess,
  // insertList,
  // remove,
  autoMap,
  transactingAction,
  getCurrentDate,
  getGUID,
} = require('../../helper/repositoryBase');

const { dateFormat, getDateString } = require('../../helper/common');

let param = {
  tableName: 'rmbt_new_signal_data',
  primaryKey: 'id',
  primaryKeyType: 'GUID',
  tableChineseName: '新信号数据',
  entityMap: [
    'id',
    { templateId: 'template_id' },
    { recordId: 'record_id' },
    { segmentIndex: 'segment_index' },
    'frequency',
    'bandwidth',
    { freqIndex: 'freq_index' },
    { maxLevel: 'max_level' },
    { avgLevel: 'avg_level' },
    { firstTime: 'first_time' },
    { lastTime: 'last_time' },
    'name',
    'result',
    { isActive: 'is_active' },
    { createTime: 'create_time' },
    { updateTime: 'update_time' },
  ],
};
const compareRecordEntityMap = [
  { templateId: 'template_id' },
  { startTime: 'start_time' },
  { endTime: 'end_time' },
  { createTime: 'create_time' },
  { updateTime: 'update_time' },
];

const compareRecordTableName = 'rmbt_new_signal_compare_record';
const propertiesItem = {
  frequency: { type: 'string', description: '频率', maxLength: 36 },
  freqIndex: { type: 'string', description: '频段索引', maxLength: 36 },
  maxLevel: { type: 'string', description: '最大值', maxLength: 36 },
  avgLevel: { type: 'string', description: '平均值', maxLength: 36 },
  bandwidth: { type: 'string', description: '带宽', maxLength: 36 },
  firstTime: {
    type: 'string',
    description: '第一次出现时间',
    maxLength: 36,
  },
  lastTime: {
    type: 'string',
    description: '最后一次出现时间',
    maxLength: 36,
  },
  result: { type: 'string', description: '结果', maxLength: 36 },
  name: { type: 'string', description: '名字', maxLength: 36 },
  isActive: { type: 'string', description: '是否活跃', maxLength: 36 },
};
exports.addSchema = {
  description: `添加新信号数据`,
  tags: [param.tableName],
  summary: `添加新信号数据`,
  body: {
    type: 'object',
    properties: {
      templateId: { type: 'string', description: '模板Id' },
      startTime: { type: 'string', description: '开始时间' },
      endTime: { type: 'string', description: '结束时间' },
      data: {
        type: 'array',
        description: '信号数据',
        items: {
          type: 'object',
          description: '模板对象',
          properties: {
            segmentIndex: { type: 'string', description: '频段索引' },
            segmentInfo: {
              type: 'array',
              description: '频段信息集合',
              items: {
                type: 'object',
                description: '频段信息',
                properties: propertiesItem,
              },
            },
          },
        },
      },
    },
    required: ['templateId'],
  },
};
exports.deleteSchema = {
  description: `删除新信号数据`,
  tags: [param.tableName],
  summary: `删除新信号数据`,
  body: {
    type: 'object',
    properties: {
      id: { type: 'string', description: '主键' },
    },
  },
};
exports.getListSchema = {
  description: `获取新信号数据列表`,
  tags: [param.tableName],
  summary: `获取新信号数据列表`,
  query: {
    templateId: { type: 'string', description: '模板ID' },
  },
  post: true,
  response1: {
    200: {
      type: 'object',
      properties: {
        result: {
          type: 'array',
          description: '返回结果',
          items: {
            type: 'object',
            description: '模板对象',
            properties: {
              ...propertiesItem,
              id: { type: 'string', description: '主键' },
            },
          },
        },
        total: { type: 'number', description: '条数' },
      },
    },
  },
};

/**
 *获取请求参数
 *
 * @param {*} req
 * @param {*} reply
 */
function reqParam(req, reply) {
  // 获取请求参数的时候不要带上autoMap的？
  param = { ...param, req, reply };
  return param;
}

/**
 *添加对象
 *
 * @param {*} req
 * @param {*} reply
 */
exports.add = async (req, reply) => {
  // 第一步获取模板Id
  // eslint-disable-next-line prefer-const
  let { data, ...recordData } = req.body;
  const signalData = req.body.data;
  recordData.createTime = getCurrentDate();
  recordData.id = getGUID();
  recordData.startTime = getDateString(recordData.startTime, dateFormat);
  recordData.endTime = getDateString(recordData.endTime, dateFormat);
  recordData = autoMap(recordData, compareRecordEntityMap, false);

  let signals = [];

  signalData.forEach((item) => {
    // eslint-disable-next-line prefer-destructuring
    const segmentIndex = item.segmentIndex;
    const segmentInfo = item.segmentInfo.map((s) => {
      return {
        ...s,
        firstTime: getDateString(s.firstTime, dateFormat),
        lastTime: getDateString(s.lastTime, dateFormat),
        segmentIndex,
        createTime: getCurrentDate(),
      };
    });

    signals = signals.concat(segmentInfo);
  });
  await transactingAction(async (knex, trx) => {
    await knex(compareRecordTableName).transacting(trx).insert(recordData);
    signals = signals.map((s) => {
      return {
        ...autoMap(s, param.entityMap, false),
        record_id: recordData.id,
        id: getGUID(),
        template_id: recordData.template_id,
      };
    });
    await knex(param.tableName).transacting(trx).insert(signals);
  });
  // 第二步拆解获取信号记录数据
  resSuccess({ reply, result: signals.select((s) => s.id) });
};

// /**
//  *批量添加对象
//  *
//  * @param {*} req
//  * @param {*} reply
//  */
// exports.addList = async (req, reply) => {
//   // 暂时不支持批量添加
//   await insertList(reqParam(req, reply));
// };

// /**
//  *更新对象
//  *
//  * @param {*} req
//  * @param {*} reply
//  */
// exports.update = async (req, reply) => {
//   await edit(reqParam(req, reply));
// };

// /**
//  *根据输入条件删除
//  *
//  * @param {*} req
//  * @param {*} reply
//  */
// exports.del = async (req, reply) => {
//   await remove(reqParam(req, reply));
// };

// /**
//  *批量删除
//  *
//  * @param {*} req
//  * @param {*} reply
//  */
// exports.delList = async (req, reply) => {
//   await removeList(reqParam(req, reply));
// };

/**
 *获取分页列表 通用封装getList
 *
 * @param {*} req
 * @param {*} reply
 */
exports.getList = async (req, reply) => {
  // 通过
  req.body = autoMap(req.body, param.entityMap);
  await getPageData(reqParam(req, reply));
};
