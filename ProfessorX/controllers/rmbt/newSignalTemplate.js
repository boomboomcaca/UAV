// 基础操作 添加删除 修改 基于数据库二次封装
const {
  insert,
  getPageData,
  // removeList,
  getDetail,
  isJSON,
  resSuccess,
  // insertList,
  autoMap,
  transactingAction,
  getCurrentDate,
  getList,
  getGUID,
  tryParseJson,
} = require('../../helper/repositoryBase');

const { dateFormat, getDateString } = require('../../helper/common');

let param = {
  tableName: 'rmbt_new_signal_template',
  primaryKey: 'id',
  primaryKeyType: 'GUID',
  tableChineseName: '新信号模板',
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
const collectTimeTableName = 'rmbt_new_signal_collecttime';
const compareRecordTableName = 'rmbt_new_signal_compare_record';
const signalDataTableName = 'rmbt_new_signal_data';
const entityMap = [
  { edgeID: 'edgeID' },
  'id',
  'name',
  'coordinate',
  'segments',
  'remark',
  'parameters',
  { createTime: 'create_time' },
  { updateTime: 'update_time' },
];
const collectTimeEntityMap = [
  { templateId: 'template_id' },
  { startTime: 'start_time' },
  { endTime: 'end_time' },
  { createTime: 'create_time' },
  { updateTime: 'update_time' },
];

exports.addSchema = {
  description: `添加信号模板`,
  tags: [param.tableName],
  summary: `添加信号模板`,
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
// description: `更新信号模板`,
// tags: [param.tableName],
// summary: `更新信号模板`,
// body: {
//   type: 'object',
//   properties: {
//     id: { type: 'string', description: '模板Id', maxLength: 36 },
//     coordinate: { type: 'string', description: '坐标', maxLength: 36 },
//     template: { type: 'array', description: '模板数据' },
//     startTime: { type: 'string', description: '开始时间', maxLength: 136 },
//     endTime: { type: 'string', description: '结束时间' },
//   },
//   required: ['id', 'startTime', 'endTime', 'coordinate', 'template'],
// },
exports.updateSchema = {
  description: `更新信号模板`,
  tags: [param.tableName],
  summary: `更新信号模板`,
  body: {
    type: 'object',
    properties: {
      id: { type: 'string', description: '模板Id', maxLength: 36 },
      template: {
        type: 'array',
        description: '模板数据',
        maxLength: 36,
        items: {
          type: 'object',
        },
      },
      parameters: { type: 'object', description: '模板数据' },
      startTime: { type: 'string', description: '开始时间', maxLength: 136 },
      endTime: { type: 'string', description: '结束时间', maxLength: 36 },
      coordinate: { type: 'string', description: '坐标', maxLength: 36 },
    },
    required: ['id'],
  },
};

exports.deleteSchema = {
  description: `删除新信号模板`,
  tags: [param.tableName],
  summary: `删除新信号模板`,
  body: {
    type: 'object',
    properties: {
      id: { type: 'string', description: '模板id' },
    },
  },
};
exports.getListSchema = {
  description: `获取新信号模板列表`,
  tags: [param.tableName],
  summary: `获取新信号模板列表`,
  query: {
    id: { type: 'string', description: '模板id' },
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
              id: { type: 'string', description: '模板Id', maxLength: 36 },
              edgeID: {
                type: 'string',
                description: '边缘端ID',
                maxLength: 36,
              },
              name: { type: 'string', description: '模板名称', maxLength: 36 },
              remark: {
                type: 'string',
                description: '模板备注',
                maxLength: 36,
              },
              useTimes: {
                type: 'string',
                description: '使用次数',
                maxLength: 36,
              },
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
                      maxLength: 136,
                    },
                    endTime: { type: 'string', description: '结束时间' },
                  },
                },
              },
              compareRecord: {
                type: 'array',
                description: '采集时间',
                items: {
                  type: 'object',
                  description: '采集时间',
                  properties: {
                    startTime: {
                      type: 'string',
                      description: '开始时间',
                      maxLength: 136,
                    },
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
  description: `获取新信号模板`,
  tags: [param.tableName],
  summary: `获取新信号模板`,
  query: {
    id: { type: 'string', description: '模板id' },
  },
  response1: {
    200: {
      type: 'object',
      properties: {
        result: {
          type: 'object',
          description: '返回结果',
          properties: {
            id: { type: 'string', description: '模板Id', maxLength: 36 },
            edgeID: {
              type: 'string',
              description: '边缘端ID',
              maxLength: 36,
            },
            startStopFrequency: {
              type: 'string',
              description: '开始结束频率',
              maxLength: 255,
            },
            name: { type: 'string', description: '模板名称', maxLength: 36 },
            remark: {
              type: 'string',
              description: '模板备注',
              maxLength: 36,
            },
            useTimes: {
              type: 'string',
              description: '使用次数',
              maxLength: 36,
            },
            coordinate: { type: 'string', description: '坐标', maxLength: 36 },
            template: { type: 'array', description: '模板数据' },
            // object 需要列出来里面所有的信息
            parameters: { type: 'object', description: '参数信息' },
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
                    maxLength: 136,
                  },
                  endTime: { type: 'string', description: '结束时间' },
                },
              },
            },
            compareRecord: {
              type: 'array',
              description: '采集时间',
            },
          },
        },
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
  await insert(reqParam(req, reply));
};
// /**
//  *批量添加对象
//  *
//  * @param {*} req
//  * @param {*} reply
//  */
// exports.addList = async (req, reply) => {
//   await insertList(reqParam(req, reply));
// };
/**
 *更新对象
 *
 * @param {*} req
 * @param {*} reply
 */
exports.update = async (req, reply) => {
  // 更新模板
  // 修改可能会传入start_time 或者end_time
  // let data = autoMap(req.body, entityMap);
  const data = req.body;
  data.update_time = getCurrentDate();
  // data = autoMap(data, collectTimeEntityMap);
  const collectTimeEntity = {};
  collectTimeEntity.id = getGUID();
  collectTimeEntity.template_id = data.id;
  collectTimeEntity.start_time = getDateString(data.startTime, dateFormat);
  collectTimeEntity.end_time = getDateString(data.endTime, dateFormat);
  collectTimeEntity.create_time = getCurrentDate();
  const { startTime, ...tempData } = data;
  const { endTime, ...mainData } = tempData;
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

  // 我其实不需要存储
  // 要从data里面取出开始和结束频率然后进行json序列化存储
  // 直接通过事务进行提交
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
 *根据输入条件删除
 *
 * @param {*} req
 * @param {*} reply
 */
exports.del = async (req, reply) => {
  // await remove(reqParam(req, reply));
  // 删除模板同样删除;
  // 判断是否存在;
  await transactingAction(async (knex, trx) => {
    await knex(collectTimeTableName)
      .transacting(trx)
      .where('template_id', req.body.id)
      .del();
    await knex(compareRecordTableName)
      .transacting(trx)
      .where('template_id', req.body.id)
      .del();
    await knex(signalDataTableName)
      .transacting(trx)
      .where('template_id', req.body.id)
      .del();
    await knex(param.tableName).transacting(trx).where('id', req.body.id).del();
  });
  resSuccess({ reply });
};

// /**
//  *批量删除
//  *
//  * @param {*} req
//  * @param {*} reply
//  */
// exports.delList = async (req, reply) => {
//   await removeList(reqParam(req, reply));
// };

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
 *获取分页列表 通用封装getList
 *
 * @param {*} req
 * @param {*} reply
 */
exports.getList = async (req, reply) => {
  // 通过
  const listData = await getPageData({
    ...reqParam(req, reply),
    entityMap,
    isReply: false,
  });
  // 查询出来所有的Id
  const templateIds = listData.rows.map((s) => s.id);
  // 查询出来所有的模板Id
  const collectTimeDatas = await getList({
    tableName: collectTimeTableName,
    wheresIn: { key: 'template_id', value: templateIds },
    queryColumn: collectTimeEntityMap,
  });
  const compareRecordDatas = await getList({
    tableName: compareRecordTableName,
    wheresIn: { key: 'template_id', value: templateIds },
    queryColumn: collectTimeEntityMap,
  });

  // 查询出所有的模板数据
  for (let i = 0; i < listData.rows.length; i++) {
    const item = listData.rows[i];
    const { id } = item;
    item.segments = getSegments(item.segments);
    item.parameters = tryParseJson(item.parameters);
    // item['haveSignals']=;
    item.collectTime = collectTimeDatas.filter((s) => s.templateId === id);
    item.compareRecord = compareRecordDatas.filter(
      (s) => s.template_id === item.id
    );
  }
  resSuccess({ reply, result: listData.rows, total: listData.total });
};

/**
 *通过id获取详情 req.query.id
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
  let [item] = items;
  item = autoMap(item, entityMap, true);
  item.segments = getSegments(item.segments);
  // 获取所有的

  // 判断模板是否使用过
  item.template = JSON.parse(item.template);
  if (item.template == null) {
    item.template = [];
  }
  const collectTimeDatas = await getList({
    tableName: collectTimeTableName,
    wheres: { template_id: item.id },
    queryColumn: collectTimeEntityMap,
  });
  const compareRecordDatas = await getList({
    tableName: compareRecordTableName,
    wheres: { template_id: item.id },
    queryColumn: collectTimeEntityMap,
  });
  item.collectTime = collectTimeDatas;
  item.compareRecord = compareRecordDatas;
  item.parameters = tryParseJson(item.parameters);
  resSuccess({ reply, result: item });
};
