const { resError } = require('../../helper/handle');
const {
  remove,
  add,
  getSingle,
  getGUID,
  resSuccess,
  getJson,
  setJson,
  deleteKey,
  knex,
  config,
  isUndefinedOrNull,
} = require('../../helper/repositoryBase');

const tableName = 'rmbt_resource_threshold';

const thresholdProperties = {
  id: {
    type: 'string',
    description: 'ID',
  },
  cpu_utilization: {
    type: 'string',
    description: 'CPU使用率',
  },
  memory_utilization: {
    type: 'string',
    description: '内存占用量',
  },
  disk_read_speed: {
    type: 'string',
    description: '磁盘读速度',
  },
  disk_write_speed: {
    type: 'string',
    description: '磁盘写速度',
  },
  disk_remaining: {
    type: 'string',
    description: '磁盘剩余空间',
  },
  network_uplink_speed: {
    type: 'string',
    description: '网络上行速度',
  },
  network_downlink_speed: {
    type: 'string',
    description: '网络下行速度',
  },
  create_time: {
    type: 'string',
    description: '设置时间',
  },
};

exports.addSchema = {
  description: `添加设施监控告警门限`,
  tags: [tableName],
  summary: `设施监控`,
  body: {
    type: 'object',
    properties: thresholdProperties,
  },
};

exports.getOneSchema = {
  description: `获取指定ID设施监控告警门限`,
  tags: [tableName],
  summary: `设施监控`,
  body: {
    type: 'object',
    properties: {
      id: {
        type: 'string',
        description: 'ID',
      },
    },
  },
};

exports.getCurrentSchema = {
  description: `获取当前设施监控告警门限`,
  tags: [tableName],
  summary: `设施监控`,
};

exports.deleteSchema = {
  description: `删除设施监控告警门限`,
  tags: [tableName],
  summary: `设施监控`,
  body: {
    type: 'object',
    properties: {
      id: {
        type: 'string',
        description: 'ID',
      },
    },
  },
};

exports.add = async (req, reply) => {
  const data = req.body;
  data.id = getGUID();
  await add({
    tableName,
    mainData: data,
  });
  await this.getLatestData();
  resSuccess({
    reply,
    result: data,
  });
};

exports.delete = async (req, reply) => {
  const data = await getSingle({
    tableName,
    wheres: {
      id: req.body.id,
    },
  });
  const cache = await getJson(config.cacheKey.resourceThreshold);
  if (isUndefinedOrNull(cache) || req.body.id === cache.id) {
    // 缓存为空或缓存门限ID和删除ID一致时 需要重新设置缓存
    await this.getCurrent(req, reply);
  }

  if (data == null) {
    resSuccess({
      reply,
    });
    return;
  }
  await remove({
    req,
    reply,
    tableName,
    tableChineseName: '',
  });
};

exports.getOne = async (req, reply) => {
  const data = await getSingle({
    tableName,
    wheres: {
      id: req.body.id,
    },
  });
  if (data == null) {
    resError({
      message: '不存在数据！',
    });
  }
  resSuccess({
    reply,
    result: data,
  });
};

exports.getCurrent = async (req, reply) => {
  const data = await this.getLatestData();
  resSuccess({
    reply,
    result: data,
  });
};

exports.getLatestData = async () => {
  const data = await knex(tableName).orderBy('create_time', 'desc').limit(1);
  if (!isUndefinedOrNull(data) && data.length > 0) {
    await setJson(config.cacheKey.resourceThreshold, data[0]);
    return data[0];
  }
  await deleteKey(config.cacheKey.resourceThreshold);
  return null;
};
