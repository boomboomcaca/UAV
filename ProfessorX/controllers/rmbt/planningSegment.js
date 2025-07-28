// 基础操作 添加删除 修改 基于数据库二次封装
const {
  getPageData,
  update,
  remove,
  resSuccess,
  resError,
  getSingle,
  autoMap,
  getGUID,
} = require('../../helper/repositoryBase');
const { addList, getList } = require('../../db/dbHelper');

const tableName = 'rmbt_planning_segment';

const entityMap = [
  'id',
  { segmentTypeID: 'segment_type_id' },
  'name',
  { startFreq: 'start_freq' },
  { stopFreq: 'stop_freq' },
  { freqStep: 'freq_step' },
  'bandwidth',
  { isUserDefine: 'is_user_define' },
  'remark',
  { segmentID: 'segment_id' },
  'mode',
];

function getMethodProperties(method) {
  const properties = {
    segmentTypeID: { type: 'string', description: '频段类型ID' },
    name: { type: 'string', description: '规划频段名称' },
    startFreq: { type: 'number', description: '起始频率' },
    stopFreq: { type: 'number', description: '结束频率' },
    freqStep: { type: 'number', description: '步长' },
    bandwidth: { type: 'number', description: '带宽' },
    remark: { type: 'string', description: '描述' },
    segmentID: { type: 'string', description: '上行/下行关联的规划频段ID' },
    mode: { type: 'number', description: '未知/上行/下行' },
  };
  if (method === 'add') {
    delete properties.id;
  }
  return properties;
}

exports.addSchema = {
  description: `添加规划频段`,
  tags: [tableName],
  summary: `添加规划频段`,
  body: {
    type: 'array',
    items: {
      type: 'object',
      properties: getMethodProperties('add'),
    },
  },
};

exports.updateSchema = {
  description: `更新规划频段`,
  tags: [tableName],
  summary: `更新规划频段`,
  body: {
    type: 'object',
    properties: getMethodProperties('update'),
  },
};

exports.relationSchema = {
  description: `关联信道`,
  tags: [tableName],
  summary: `关联信道`,
  body: {
    type: 'object',
    properties: {
      relation: {
        type: 'boolean',
        description: '关联信道/取消关联',
      },
      id: {
        type: 'array',
        description: '关联ID',
        items: { type: 'string' },
      },
    },
  },
};

exports.deleteSchema = {
  description: `删除规划频段`,
  tags: [tableName],
  summary: `删除规划频段`,
  body: {
    type: 'object',
    properties: {
      id: { type: 'string', description: 'ID' },
    },
  },
};

exports.add = async (req, reply) => {
  let segmentData = req.body.map((d) => {
    d.id = getGUID();
    d.is_user_define = 1;
    return autoMap(d, entityMap);
  });

  await addList({ tableName, mainData: segmentData });

  segmentData = segmentData.map((d) => {
    return autoMap(d, entityMap, true);
  });
  resSuccess({ reply, result: segmentData });
};

exports.update = async (req, reply) => {
  if (req.body !== undefined) {
    req.body = autoMap(req.body, entityMap);
    delete req.body.is_user_define;
  }

  const segmentData = await getSingle({
    tableName,
    wheres: { id: req.body.id },
  });
  if (!segmentData) {
    resError({ message: '该条数据已被删除！' });
  }
  if (segmentData.is_user_define === 0) {
    resError({ message: '内置数据不允许修改！' });
  }

  const result = await update({
    tableName,
    mainData: req.body,
    wheres: { id: req.body.id },
  });

  resSuccess({ reply, result });
};

/* istanbul ignore next */
exports.relation = async (req, reply) => {
  // req.body: {
  //   "id" : ["4444","5555"],  // 有且仅有2个
  //   "relation": true  // true：关联上下行频段，false：取消关联
  // }

  if (
    req.body.id === undefined ||
    req.body.relation === undefined ||
    req.body.id.length !== 2
  ) {
    // todo:可以考虑jsonSchema 校验
    resError({ message: '输入参数有误！' });
  }
  const data = await getList({
    tableName,
    wheresIn: { key: 'id', value: req.body.id },
    queryColumn: entityMap,
  });
  if (data.length !== 2) {
    resError({ message: '部分数据不存在，请重新查询数据后重试！' });
  }

  if (req.body.relation === true) {
    // todo:可以考虑直接合并 4 个 update
    await update({
      tableName,
      mainData: { id: req.body.id[0], segment_type_id: req.body.id[1] },
      wheres: { id: req.body.id[0] },
    });
    await update({
      tableName,
      mainData: { id: req.body.id[1], segment_type_id: req.body.id[0] },
      wheres: { id: req.body.id[1] },
    });
    resSuccess({ reply });
  } else {
    await update({
      tableName,
      mainData: { id: req.body.id[0], segment_type_id: '' },
      wheres: { id: req.body.id[0] },
    });
    await update({
      tableName,
      mainData: { id: req.body.id[1], segment_type_id: '' },
      wheres: { id: req.body.id[1] },
    });
    resSuccess({ reply });
  }
};

exports.del = async (req, reply) => {
  const data = await getSingle({ tableName, wheres: { id: req.body.id } });
  if (!data) {
    resSuccess({ reply });
    return;
  }
  if (data.is_user_define === 0) {
    resError({ message: '内置数据不允许修改！' });
  }
  remove({ req, reply, tableName, tableChineseName: '' });
};

exports.getList = async (req, reply) => {
  getPageData({ req, reply, tableName, entityMap, isLimitMaxRow: false });
};

/**
 * 获取常用频段
 * @param {*} req
 * @param {*} reply
 */
exports.getCommonUse = async (req, reply) => {
  const segments = await getList({
    tableName,
    wheres: { segment_type_id: '718c1960-bdfb-11eb-a5ab-e91b13de939b' },
    queryColumn: entityMap,
  });
  resSuccess({ reply, result: segments });
};

exports.getCommonUseSchema = {
  description: `获取常用频段`,
  tags: [tableName],
  summary: `获取常用频段`,
  query: {},
};
