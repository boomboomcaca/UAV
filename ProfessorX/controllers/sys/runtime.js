const { updateRuntime } = require('../../manager/runtimeManager');

/**
 *获取系统运行时长
 *
 * @param {*} req
 * @param {*} reply
 */
exports.get = async (req, reply) => {
  const totalRuntime = await updateRuntime();
  reply.send({ result: totalRuntime });
};

exports.getSchema = {
  summary: '获取系统运行时长',
  description: '获取系统运行时长',
  tags: ['系统运行时长'],
  query: {},
};
