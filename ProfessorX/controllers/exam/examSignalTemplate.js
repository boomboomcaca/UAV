const {
  insert,
  getPageData,
  getDetail,
  isJSON,
  resSuccess,
  transactingAction,
  getCurrentDate,
  getList,
  getGUID,
  tryParseJson,
} = require('../../helper/repositoryBase');
const { dateFormat, getDateString } = require('../../helper/common');

let param = {
  tableName: 'rmbt_exam_signal_template',
  primaryKey: 'id',
  primaryKeyType: 'GUID',
  tableChineseName: '考试保障信号模板',
  uniqueKey: 'name',
  entityMap: [
    'id',
    'name',
    'coordinate',
    'segments',
    'parameters',
    'remark',
    'edgeID',
    'create_time',
    'update_time',
  ],
};
const collectTimeTableName = 'rmbt_exam_signal_collecttime';
const compareRecordTableName = 'rmbt_exam_signal_compare_record';
const signalDataTableName = 'rmbt_exam_signal_data';

exports.addSchema = {
  description: `添加考试保障信号模板`,
  tags: [param.tableName],
  summary: `添加考试保障信号模板`,
  body: {
    type: 'object',
    properties: {
      name: { type: 'string', description: '名称', maxLength: 36 },
      remark: { type: 'string', description: '备注', maxLength: 36 },
      edgeID: { type: 'string', description: '边缘端ID', maxLength: 36 },
    },
    required: ['name'],
  },
};

exports.updateSchema = {
  description: `更新考试保障信号模板`,
  tags: [param.tableName],
  summary: `更新考试保障信号模板`,
  body: {
    type: 'object',
    properties: {
      id: { type: 'string', description: '模板ID', maxLength: 36 },
      coordinate: { type: 'string', description: '坐标', maxLength: 36 },
      template: {
        type: 'array',
        description: '模板数据',
        items: {
          type: 'object',
        },
      },
      parameters: { type: 'object', description: '模板参数' },
      startTime: { type: 'string', description: '开始时间', maxLength: 36 },
      endTime: { type: 'string', description: '结束时间', maxLength: 36 },
    },
    required: ['id'],
  },
};

exports.deleteSchema = {
  description: `删除考试保障信号模板`,
  tags: [param.tableName],
  summary: `删除考试保障信号模板`,
  body: {
    type: 'object',
    properties: {
      id: { type: 'string', description: '模板ID' },
    },
  },
};

exports.getListSchema = {
  description: `获取考试保障信号模板列表`,
  tags: [param.tableName],
  summary: `获取考试保障信号模板列表`,
  query: {
    id: { type: 'string', description: '模板ID' },
    name: { type: 'string', description: '模板名称' },
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
              id: { type: 'string', description: '模板ID', maxLength: 36 },
              name: { type: 'string', description: '模板名称', maxLength: 36 },
              coordinate: {
                type: 'string',
                description: '坐标',
                maxLength: 36,
              },
              segments: {
                type: 'string',
                description: '频段',
                maxLength: 255,
              },
              template: { type: 'string', description: '模板数据' },
              parameters: { type: 'object', description: '参数信息' },
              remark: {
                type: 'string',
                description: '备注',
                maxLength: 36,
              },
              collectTime: {
                type: 'array',
                description: '采集时间',
                items: {
                  type: 'object',
                  description: '采集时间',
                  properties: {
                    startTime: { type: 'string', description: '开始时间' },
                    endTime: { type: 'string', description: '结束时间' },
                  },
                },
              },
              compareRecord: {
                type: 'array',
                description: '记录时间',
                items: {
                  type: 'object',
                  description: '记录时间',
                  properties: {
                    startTime: { type: 'string', description: '开始时间' },
                    endTime: { type: 'string', description: '结束时间' },
                  },
                },
              },
            },
          },
        },
        total: { type: 'number', description: '条数' },
      },
    },
  },
};

exports.getSchema = {
  description: `获取考试保障信号模板`,
  tags: [param.tableName],
  summary: `获取考试保障信号模板`,
  query: {
    id: { type: 'string', description: '模板ID' },
  },
  response1: {
    200: {
      type: 'object',
      properties: {
        result: {
          type: 'object',
          description: '返回结果',
          properties: {
            id: { type: 'string', description: '模板ID', maxLength: 36 },
            name: { type: 'string', description: '模板名称', maxLength: 36 },
            coordinate: { type: 'string', description: '坐标', maxLength: 36 },
            segments: {
              type: 'string',
              description: '频段',
              maxLength: 255,
            },
            template: { type: 'array', description: '模板数据' },
            parameters: { type: 'object', description: '参数信息' },
            remark: {
              type: 'string',
              description: '备注',
              maxLength: 36,
            },
            collectTime: {
              type: 'array',
              description: '采集时间',
              items: {
                type: 'object',
                description: '采集时间',
                properties: {
                  startTime: {
                    type: 'string',
                    description: '开始时间',
                  },
                  endTime: { type: 'string', description: '结束时间' },
                },
              },
            },
            compareRecord: {
              type: 'array',
              description: '记录时间',
              items: {
                type: 'object',
                description: '记录时间',
                properties: {
                  startTime: {
                    type: 'string',
                    description: '开始时间',
                  },
                  endTime: { type: 'string', description: '结束时间' },
                },
              },
            },
          },
        },
      },
    },
  },
};

function reqParam(req, reply) {
  param = { ...param, req, reply };
  return param;
}

function getSegments(frequencyData) {
  if (isJSON(frequencyData)) {
    const item = tryParseJson(frequencyData);
    const startStopFrequency = item.map((s) => {
      return {
        frequencyInfo: `${s.startFrequency}_${s.stopFrequency}_${s.stepFrequency}`,
      };
    });
    const frequencyStr = startStopFrequency
      .select((s) => s.frequencyInfo)
      .join(',');
    return frequencyStr;
  }
  return '';
}

/**
 *添加对象
 *
 * @param {*} req
 * @param {*} reply
 */
exports.add = async (req, reply) => {
  await insert(reqParam(req, reply));
};

/**
 *更新对象
 *
 * @param {*} req
 * @param {*} reply
 */
exports.update = async (req, reply) => {
  const data = req.body;
  data.update_time = getCurrentDate();
  const collectTimeEntity = {};
  collectTimeEntity.id = getGUID();
  collectTimeEntity.templateID = data.id;
  collectTimeEntity.startTime = getDateString(data.startTime, dateFormat);
  collectTimeEntity.endTime = getDateString(data.endTime, dateFormat);
  collectTimeEntity.create_time = getCurrentDate();
  const { startTime, endTime, ...mainData } = data;
  // 更新的时候直接保存频率信息
  if (mainData.template) {
    const segments = mainData.template.map((s) => {
      return {
        startFrequency: s.startFrequency,
        stopFrequency: s.stopFrequency,
        stepFrequency: s.stepFrequency,
      };
    });
    mainData.segments = JSON.stringify(segments);
    mainData.parameters = JSON.stringify(mainData.parameters);
    mainData.template = JSON.stringify(mainData.template);
  }
  await transactingAction(async (knex, trx) => {
    await knex(param.tableName)
      .transacting(trx)
      .where('id', mainData.id)
      .update(mainData);
    await knex(collectTimeTableName).transacting(trx).insert(collectTimeEntity);
  });
  resSuccess({ reply, result: 1 });
};

/**
 *通过ID删除
 *
 * @param {*} req
 * @param {*} reply
 */
exports.del = async (req, reply) => {
  await transactingAction(async (knex, trx) => {
    await knex(collectTimeTableName)
      .transacting(trx)
      .where('templateID', req.body.id)
      .del();
    await knex(compareRecordTableName)
      .transacting(trx)
      .where('templateID', req.body.id)
      .del();
    await knex(signalDataTableName)
      .transacting(trx)
      .where('templateID', req.body.id)
      .del();
    await knex(param.tableName).transacting(trx).where('id', req.body.id).del();
  });
  resSuccess({ reply });
};

/**
 *分页查询
 *
 * @param {*} req
 * @param {*} reply
 */
exports.getList = async (req, reply) => {
  const listData = await getPageData({
    ...reqParam(req, reply),
    isReply: false,
  });
  // 查询所有的模板ID
  const templateIds = listData.rows.map((s) => s.id);
  const collectTimeDatas = await getList({
    tableName: collectTimeTableName,
    wheresIn: { key: 'templateID', value: templateIds },
  });
  const compareRecordDatas = await getList({
    tableName: compareRecordTableName,
    wheresIn: { key: 'templateID', value: templateIds },
  });
  // 查询所有的模板数据
  for (let i = 0; i < listData.rows.length; i++) {
    const item = listData.rows[i];
    const { id } = item;
    item.segments = getSegments(item.segments);
    item.template = tryParseJson(item.template);
    item.parameters = tryParseJson(item.parameters);
    item.collectTime = collectTimeDatas.filter((s) => s.templateID === id);
    item.compareRecord = compareRecordDatas.filter(
      (s) => s.templateID === item.id
    );
  }
  resSuccess({ reply, result: listData.rows, total: listData.total });
};

/**
 *通过ID查询
 *
 * @param {*} req
 * @param {*} reply
 */
exports.get = async (req, reply) => {
  const items = await getDetail({
    ...reqParam(req, reply),
    isReply: false,
  });
  if (items.length === 0) {
    resSuccess({ reply, result: {} });
    return;
  }
  const [item] = items;
  item.segments = getSegments(item.segments);
  item.template = JSON.parse(item.template);
  if (item.template == null) {
    item.template = [];
  }
  const collectTimeDatas = await getList({
    tableName: collectTimeTableName,
    wheres: { templateID: item.id },
  });
  const compareRecordDatas = await getList({
    tableName: compareRecordTableName,
    wheres: { templateID: item.id },
  });
  item.collectTime = collectTimeDatas;
  item.compareRecord = compareRecordDatas;
  item.parameters = tryParseJson(item.parameters);
  resSuccess({ reply, result: item });
};
