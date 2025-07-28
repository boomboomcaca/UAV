// 基础操作 添加删除 修改 基于数据库二次封装
// const Decimal = require('decimal');
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
const { getList, addList } = require('../../db/dbHelper');

const channelDivisionTableName = 'rmbt_channel_division';

const entityMap = [
  'id',
  { segmentID: 'segment_id' },
  { channelID: 'channel_id' },
  'name',
  'freq',
  'bandwidth',
  { isUserDefine: 'is_user_define' },
  'mode',
];

exports.addSchema = {
  description: `添加规划信道`,
  tags: [channelDivisionTableName],
  summary: `添加规划信道`,
  body: {
    type: 'array',
    items: {
      type: 'object',
      properties: {
        // id: { type: 'string', description: 'ID' },
        segmentID: { type: 'string', description: '规划频段ID' },
        channelID: { type: 'string', description: '上行/下行关联信道ID' },
        name: { type: 'string', description: '规划信道名称' },
        freq: { type: 'number', description: '中心频率' },
        bandwidth: { type: 'number', description: '带宽' },
        mode: { type: 'number', description: '未知/上行/下行' },
        // isUserDefine: { type: 'string', format: 'number', description: '是否用户定义', },
      },
    },
  },
};

// exports.addAutomaticFreqsSchema = {
//   description: `自动添加规划信道`,
//   tags: [channelDivisionTableName],
//   summary: `自动添加规划信道`,
//   body: {
//     type: 'object',
//     properties: {
//       segmentID: { type: 'string', description: '规划频段ID' },
//       startFreq: {
//         type: 'number',
//         description: '频段内信道中心频率最小值（包含）',
//       },
//       stopFreq: {
//         type: 'number',
//         description: '频段内信道中心频率最大值（不包含）',
//       },
//       bandwidth: { type: 'number', description: '信道带宽' },
//       step: { type: 'number', description: '相邻信道间隔' },
//       name: {
//         type: 'string',
//         description: '规划信道名称,与信道号拼接组成信道名称',
//       },
//       nameStartNumber: { type: 'number', description: '信道号' },
//       mode: { type: 'number', description: '未知/上行/下行' },
//     },
//   },
// };

exports.updateSchema = {
  description: `更新规划信道`,
  tags: [channelDivisionTableName],
  summary: `更新规划信道`,
  body: {
    type: 'object',
    properties: {
      id: { type: 'string', description: 'ID' },
      segmentID: { type: 'string', description: '规划频段ID' },
      channelID: { type: 'string', description: '上行/下行关联信道ID' },
      name: { type: 'string', description: '规划信道名称' },
      freq: { type: 'number', description: '中心频率' },
      bandwidth: { type: 'number', description: '带宽' },
      mode: { type: 'number', description: '未知/上行/下行' },
    },
    required: ['id'],
  },
};

exports.relationSchema = {
  description: `关联信道`,
  tags: [channelDivisionTableName],
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
  description: `删除规划信道`,
  tags: [channelDivisionTableName],
  summary: `删除规划信道`,
  body: {
    type: 'object',
    properties: {
      id: { type: 'string', description: 'ID' },
    },
  },
};

// exports.addAutomaticFreqs = async (req, reply) => {
//   const dataList = [];
//   const step = new Decimal(req.body.step).div(new Decimal(1000)).toNumber();
//   req.body.name = req.body.name || '';
//   let freq = req.body.startFreq;
//   do {
//     dataList.push({
//       id: getGUID(),
//       segment_id: req.body.segmentID,
//       // channelID: ,
//       name:
//         req.body.name +
//         (req.body.nameStartNumber === undefined
//           ? ''
//           : req.body.nameStartNumber),
//       freq,
//       bandwidth: req.body.bandwidth,
//       mode: req.body.mode,
//       is_user_define: 1,
//     });
//     freq = new Decimal(freq).add(new Decimal(step)).toNumber();
//     if (req.body.nameStartNumber !== undefined) {
//       req.body.nameStartNumber++;
//     }
//   } while (freq < req.body.stopFreq);
//   await addList({ tableName: channelDivisionTableName, mainData: dataList });

//   resSuccess({ reply, result: dataList });
// };

exports.add = async (req, reply) => {
  let data = req.body.map((d) => {
    d.id = getGUID();
    d.is_user_define = 1;
    return autoMap(d, entityMap);
  });
  await addList({ tableName: channelDivisionTableName, mainData: data });
  data = data.map((d) => {
    return autoMap(d, entityMap, true);
  });
  resSuccess({ reply, result: data });
};

exports.update = async (req, reply) => {
  req.body = autoMap(req.body, entityMap);
  delete req.body.is_user_define;

  const channelDivisiondata = await getSingle({
    tableName: channelDivisionTableName,
    wheres: { id: req.body.id },
  });
  if (!channelDivisiondata) {
    resError({ message: '该条数据已被删除！' });
  }
  if (channelDivisiondata.is_user_define === 0) {
    resError({ message: '内置数据不允许修改！' });
  }

  const result = await update({
    tableName: channelDivisionTableName,
    mainData: req.body,
    wheres: { id: req.body.id },
  });

  resSuccess({ reply, result });
};

exports.relation = async (req, reply) => {
  if (
    req.body.id === undefined ||
    req.body.relation === undefined ||
    req.body.id.length !== 2
  ) {
    resError({ message: '输入参数有误！' });
  }
  const data = await getList({
    tableName: channelDivisionTableName,
    wheresIn: { key: 'id', value: req.body.id },
    queryColumn: entityMap,
  });
  if (data.length !== 2) {
    resError({ message: '部分数据不存在，请重新查询数据后重试！' });
  }
  if (req.body.relation === true) {
    await update({
      tableName: channelDivisionTableName,
      mainData: { id: req.body.id[0], channel_id: req.body.id[1] },
      wheres: { id: req.body.id[0] },
    });
    await update({
      tableName: channelDivisionTableName,
      mainData: { id: req.body.id[1], channel_id: req.body.id[0] },
      wheres: { id: req.body.id[1] },
    });
    resSuccess({ reply });
  } else {
    await update({
      tableName: channelDivisionTableName,
      mainData: { id: req.body.id[0], channel_id: '' },
      wheres: { id: req.body.id[0] },
    });
    await update({
      tableName: channelDivisionTableName,
      mainData: { id: req.body.id[1], channel_id: '' },
      wheres: { id: req.body.id[1] },
    });
    resSuccess({ reply });
  }
};

exports.del = async (req, reply) => {
  const data = await getSingle({
    tableName: channelDivisionTableName,
    wheres: { id: req.body.id },
  });
  if (!data) {
    resSuccess({ reply });
    return;
  }
  if (data.is_user_define === 0) {
    resError({ message: '内置数据不允许修改！' });
  }
  await remove({
    req,
    reply,
    tableName: channelDivisionTableName,
    tableChineseName: '',
  });
};

exports.getList = async (req, reply) => {
  await getPageData({
    req,
    reply,
    tableName: channelDivisionTableName,
    entityMap,
    isLimitMaxRow: false,
  });
};
