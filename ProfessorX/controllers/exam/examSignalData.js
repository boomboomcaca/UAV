const {
  getPageData,
  resSuccess,
  transactingAction,
  getCurrentDate,
  getGUID,
} = require('../../helper/repositoryBase');

const { dateFormat, getDateString } = require('../../helper/common');

let param = {
  tableName: 'rmbt_exam_signal_data',
  primaryKey: 'id',
  primaryKeyType: 'GUID',
  tableChineseName: '考试保障信号数据',
};

const compareRecordTableName = 'rmbt_exam_signal_compare_record';

const propertiesItem = {
  frequency: { type: 'number', description: '频率' },
  freqIndex: { type: 'number', description: '频段索引' },
  maxLevel: { type: 'number', description: '最大值' },
  avgLevel: { type: 'number', description: '平均值' },
  bandwidth: { type: 'number', description: '带宽' },
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
  decoder: { type: 'string', description: '解码结果' },
};

exports.addSchema = {
  description: `添加考试保障信号数据`,
  tags: [param.tableName],
  summary: `添加考试保障信号数据`,
  body: {
    type: 'object',
    properties: {
      templateID: { type: 'string', description: '模板ID' },
      startTime: { type: 'string', description: '开始时间' },
      endTime: { type: 'string', description: '结束时间' },
      data: {
        type: 'array',
        description: '信号数据',
        items: {
          type: 'object',
          description: '模板对象',
          properties: {
            segmentIndex: { type: 'number', description: '频段索引' },
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
    required: ['templateID'],
  },
};

exports.deleteSchema = {
  description: `删除考试保障信号数据`,
  tags: [param.tableName],
  summary: `删除考试保障信号数据`,
  body: {
    type: 'object',
    properties: {
      id: { type: 'string', description: '主键' },
    },
  },
};

exports.getListSchema = {
  description: `获取考试保障信号数据列表`,
  tags: [param.tableName],
  summary: `获取考试保障信号数据列表`,
  query: {
    templateID: { type: 'string', description: '模板ID' },
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
  const { data, ...recordData } = req.body;
  recordData.create_time = getCurrentDate();
  recordData.id = getGUID();
  recordData.startTime = getDateString(recordData.startTime, dateFormat);
  recordData.endTime = getDateString(recordData.endTime, dateFormat);

  let signals = [];
  data.forEach((item) => {
    const { segmentIndex } = item;
    const segmentInfo = item.segmentInfo.map((s) => {
      return {
        ...s,
        firstTime: getDateString(s.firstTime, dateFormat),
        lastTime: getDateString(s.lastTime, dateFormat),
        segmentIndex,
        create_time: getCurrentDate(),
      };
    });

    signals = signals.concat(segmentInfo);
  });
  await transactingAction(async (knex, trx) => {
    await knex(compareRecordTableName).transacting(trx).insert(recordData);
    signals = signals.map((s) => {
      return {
        ...s,
        recordID: recordData.id,
        id: getGUID(),
        templateID: recordData.templateID,
      };
    });
    await knex(param.tableName).transacting(trx).insert(signals);
  });
  resSuccess({ reply, result: signals.select((s) => s.id) });
};

/**
 *分页查询
 *
 * @param {*} req
 * @param {*} reply
 */
exports.getList = async (req, reply) => {
  await getPageData(reqParam(req, reply));
};
