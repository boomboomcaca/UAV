// 基础操作 添加删除 修改 基于数据库二次封装

const {
  getPageData,
  insert,
  addList,
  resSuccess,
} = require('../../helper/repositoryBase');
const { getCurrentDate } = require('../../helper/common');

const tableName = 'log_envi_info';

exports.addSchema = {
  description: `添加动环环境信息`,
  tags: [tableName],
  summary: `动环`,
  body: {
    type: 'object',
    properties: {
      device_id: { type: 'string', description: '环境监控设备ID' },
      switchState: { type: 'string', description: '开关信息' },
      environment: { type: 'string', description: '环境信息' },
      securityAlarm: { type: 'string', description: '报警' },
      create_time: {
        type: 'string',
        description: '登记时间',
      },
    },
  },
};

/**
 *添加对象
 *
 * @param {*} req
 * @param {*} reply
 */
/* istanbul ignore next */
exports.add = async (req, reply) => {
  const log = req.body;
  log.create_time = getCurrentDate();
  await insert({ req, reply, tableName });
};

exports.addEnviInfos = async (mainData) => {
  await addList({ tableName, mainData });
};

exports.getList = async (req, reply) => {
  const listData = await getPageData({ req, reply, tableName, isReply: false });

  // 无边缘端信息，屏蔽边缘端名称查询
  // const edges = await sqlQuery('select id as edge_id, name from rmbt_edge');
  // listData.rows.forEach((element) => {
  //   const edge = edges.find((edgeItem) => edgeItem.edge_id === element.edge_id);
  //   if (edge) {
  //     element.edge_name = edge.name;
  //   } else {
  //     element.edge_name = '';
  //   }
  // });

  resSuccess({ reply, result: listData.rows, total: listData.total });
};
