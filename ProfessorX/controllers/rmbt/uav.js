const uavInfos = require('../../data/config/uavInfo.json');

const propertiesItem = {
  machinetype: { type: 'string', description: '机型' },
  manufacturer: { type: 'string', description: '厂商' },
  description: { type: 'string', description: '描述' },
  imagePath: { type: 'string', description: '图片' },
};

exports.getSchema = {
  description: `查询指定型号无人机`,
  tags: ['uav_info'],
  summary: `查询指定型号无人机`,
  query: {
    machinetype: { type: 'string', description: '无人机型号' },
  },
  response1: {
    200: {
      type: 'object',
      properties: {
        result: {
          type: 'array',
          description: '返回结果',
          items: {
            type: 'object',
            description: '无人机信息',
            properties: propertiesItem,
          },
        },
      },
    },
  },
};

exports.getListSchema = {
  description: `查询无人机列表`,
  tags: ['uav_info'],
  summary: `查询无人机列表`,
  response1: {
    200: {
      type: 'object',
      properties: {
        result: {
          type: 'array',
          description: '返回结果',
          items: {
            type: 'object',
            description: '无人机信息',
            properties: propertiesItem,
          },
        },
        total: { type: 'number', description: '条数' },
      },
    },
  },
};

/**
 * 查询无人机列表
 *
 * @param {*} req
 * @param {*} reply
 */
exports.getList = async (req, reply) => {
  reply.send({ result: uavInfos });
};

/**
 * 查询指定型号无人机列表
 *
 * @param {*} req
 * @param {*} reply
 */
exports.get = async (req, reply) => {
  const newUavInfos = uavInfos.filter(
    (x) => x.machinetype === req.query.machinetype
  );
  reply.send({ result: newUavInfos });
};
