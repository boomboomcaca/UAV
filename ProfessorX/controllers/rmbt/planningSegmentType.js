// 基础操作 添加删除 修改 基于数据库二次封装
const {
  getPageData,
  update,
  remove,
  resSuccess,
  resError,
  autoMap,
  getSingle,
  getGUID,
} = require('../../helper/repositoryBase');
const { addList } = require('../../db/dbHelper');

const tableName = 'rmbt_planning_segment_type';

const entityMap = [
  'id',
  { businessID: 'business_id' },
  'name',
  'level',
  { isUserDefine: 'is_user_define' },
];

exports.addSchema = {
  description: `添加频段类型`,
  tags: [tableName],
  summary: `添加频段类型`,
  body: {
    type: 'array',
    items: {
      type: 'object',
      properties: {
        // id: { type: 'string', description: 'ID' },
        businessID: { type: 'string', description: '规划业务ID' },
        name: { type: 'string', description: '频段类型名称' },
        level: { type: 'number', description: '频段主次业务类型' },
        // isUserDefine: { type: 'string', format: 'number', description: '是否用户定义', },
      },
    },
  },
};

exports.updateSchema = {
  description: `更新频段类型`,
  tags: [tableName],
  summary: `更新频段类型`,
  body: {
    type: 'object',
    properties: {
      id: { type: 'string', description: 'ID' },
      businessID: { type: 'string', description: '规划业务ID' },
      name: { type: 'string', description: '频段类型名称' },
      level: { type: 'number', description: '频段主次业务类型' },
    },
  },
};

exports.deleteSchema = {
  description: `删除频段类型`,
  tags: [tableName],
  summary: `删除频段类型`,
  body: {
    type: 'object',
    properties: {
      id: { type: 'string', description: 'ID' },
    },
  },
};

exports.add = async (req, reply) => {
  let segmentTypeData = req.body.map((d) => {
    d.id = getGUID();
    d.is_user_define = 1;
    return autoMap(d, entityMap);
  });

  await addList({ tableName, mainData: segmentTypeData });

  segmentTypeData = segmentTypeData.map((d) => {
    return autoMap(d, entityMap, true);
  });
  resSuccess({ reply, result: segmentTypeData });
};

exports.update = async (req, reply) => {
  if (req.body) {
    req.body = autoMap(req.body, entityMap);
    delete req.body.is_user_define;
  }

  const segmentTypeData = await getSingle({
    tableName,
    wheres: { id: req.body.id },
  });
  if (!segmentTypeData) {
    resError({ message: '该条数据已被删除！' });
  }
  if (segmentTypeData.is_user_define === 0) {
    resError({ message: '内置数据不允许修改！' });
  }

  const result = await update({
    tableName,
    mainData: req.body,
    wheres: { id: req.body.id },
  });

  resSuccess({ reply, result });
};

exports.del = async (req, reply) => {
  const segmentTypeData = await getSingle({
    tableName,
    wheres: { id: req.body.id },
  });
  if (!segmentTypeData) {
    resError({ message: '该条数据已被删除！' });
  }
  if (segmentTypeData.is_user_define === 0) {
    resError({ message: '内置数据不允许修改！' });
  }
  await remove({ req, reply, tableName, tableChineseName: '' });
};

exports.getList = async (req, reply) => {
  await getPageData({ req, reply, tableName, entityMap, isLimitMaxRow: false });
};
