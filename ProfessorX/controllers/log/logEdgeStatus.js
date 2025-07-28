// 基础操作 添加删除 修改 基于数据库二次封装
const {
  getPageData,
  insert,
  add,
  sqlQuery,
  resSuccess,
} = require('../../helper/repositoryBase');
const { getCurrentDate } = require('../../helper/common');

const tableName = 'log_edge_status';

exports.addSchema = {
  description: `添加日志`,
  tags: [tableName],
  summary: `添加日志`,
  body: {
    type: 'object',
    properties: {
      edge_id: { type: 'string', description: '边缘端ID' },
      status: { type: 'string', description: '边缘端状态' },
      content: { type: 'string', description: '日志内容' },
      create_time: {
        type: 'string',
        description: '创建时间',
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

exports.addEdgeStatus = async (mainData) => {
  // mainData.create_time = moment(mainData.create_time).format(
  //   'YYYY-MM-DD HH:mm:ss.SSS'
  // );
  await add({ tableName, mainData });
};

exports.getList = async (req, reply) => {
  const listData = await getPageData({ req, reply, tableName, isReply: false });

  const edges = await sqlQuery('select id as edge_id, name from rmbt_edge');

  listData.rows.forEach((element) => {
    const edge = edges.find((edgeItem) => edgeItem.edge_id === element.edge_id);
    if (edge) {
      element.edge_name = edge.name;
    } else {
      element.edge_name = '';
    }
  });

  resSuccess({ reply, result: listData.rows, total: listData.total });
};
