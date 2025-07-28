// 基础操作 添加删除 修改 基于数据库二次封装
const { getPageData, add, insert } = require('../../helper/repositoryBase');
const { getCurrentDate } = require('../../helper/common');

const tableName = 'log_remote_control';

exports.addSchema = {
  description: `添加动环操作`,
  tags: [tableName],
  summary: `动环`,
  body: {
    type: 'object',
    properties: {
      device_id: { type: 'string', description: '设备ID' },
      switch_name: { type: 'string', description: '开关名称' },
      switch_display_name: { type: 'string', description: '开关显示名称' },
      switch_state: { type: 'string', description: '开关状态' },
      account: { type: 'string', description: '操作用户账户' },
      create_time: {
        type: 'string',
        description: '操作时间',
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

exports.addRemoteControl = async (mainData) => {
  await add({ tableName, mainData });
};

exports.getList = async (req, reply) => {
  await getPageData({ req, reply, tableName });
};
