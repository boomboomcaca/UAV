// 基础操作 添加删除 修改 基于数据库二次封装
const { addList } = require('../../db/dbHelper');
const {
  autoMap,
  getPageData,
  update,
  remove,
  getList,
  resSuccess,
  resError,
  getSingle,
  getGUID,
} = require('../../helper/repositoryBase');

const tableName = 'rmbt_planning_business';

const entityMap = ['id', 'name', { isUserDefine: 'is_user_define' }];

const tableNameType = 'rmbt_planning_segment_type';
const entityTypeMap = [
  'id',
  { businessID: 'business_id' },
  'name',
  'level',
  { isUserDefine: 'is_user_define' },
];

exports.addSchema = {
  description: `添加规划业务`,
  tags: [tableName],
  summary: `添加规划业务`,
  body: {
    type: 'array',
    items: {
      type: 'object',
      properties: {
        // id: { type: 'string', description: 'ID' },
        name: { type: 'string', description: '业务名称' },
        // isUserDefine: { type: 'string', format: 'number', description: '是否用户定义', },
      },
    },
  },
};

exports.updateSchema = {
  description: `更新规划业务`,
  tags: [tableName],
  summary: `更新规划业务`,
  body: {
    type: 'object',
    properties: {
      id: { type: 'string', description: 'ID' },
      name: { type: 'string', description: '业务名称' },
      // isUserDefine: { type: 'string', format: 'number', description: '是否用户定义', },
    },
  },
};

exports.deleteSchema = {
  description: `删除规划业务`,
  tags: [tableName],
  summary: `删除规划业务`,
  body: {
    type: 'object',
    properties: {
      id: { type: 'string', description: 'ID' },
      // name: { type: 'string', description: '业务名称' },
      // isUserDefine: { type: 'string', format: 'number', description: '是否用户定义', },
    },
  },
};

exports.getAllWithSegmentTypeSchema = {
  description: `获取全部业务信息，包含频段类型`,
  tags: [tableName],
  summary: `获取全部业务信息，包含频段类型`,
};

exports.add = async (req, reply) => {
  const data = req.body;
  data.forEach((d) => {
    d.id = getGUID();
    d.is_user_define = 1;
  });
  await addList({ tableName, mainData: data });
  data.forEach((d) => {
    d.isUserDefine = d.is_user_define;
    delete d.is_user_define;
  });
  resSuccess({ reply, result: data });
};

exports.update = async (req, reply) => {
  const data = await getSingle({ tableName, wheres: { id: req.body.id } });
  if (!data) {
    resError({ message: '该条数据已被删除！' });
  }
  if (data.is_user_define === 0) {
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
  const data = await getSingle({ tableName, wheres: { id: req.body.id } });
  if (!data) {
    resSuccess({ reply });
    return;
  }
  if (data.is_user_define === 0) {
    resError({ message: '内置数据不允许修改！' });
  }
  await remove({ req, reply, tableName, tableChineseName: '' });
};

exports.getList = async (req, reply) => {
  await getPageData({ req, reply, tableName, entityMap, isLimitMaxRow: false });
};

exports.getAllWithSegmentType = async (req, reply) => {
  let segmentTypeData = await getList({ tableName: tableNameType });
  let businessData = await getList({ tableName });
  segmentTypeData = segmentTypeData.map((data) => {
    return autoMap(data, entityTypeMap, true);
  });
  businessData = businessData.map((data) => {
    return autoMap(data, entityMap, true);
  });
  if (
    businessData !== undefined &&
    businessData.length > 0 &&
    segmentTypeData !== undefined &&
    segmentTypeData.length > 0
  ) {
    businessData.forEach((business) => {
      const types = segmentTypeData.filter((s) => s.businessID === business.id);
      business.segmentType = types;
      return business;
    });
  }
  resSuccess({ reply, result: businessData });
};
