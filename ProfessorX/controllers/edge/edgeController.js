const net = require('net');
const {
  config,
  resError,
  resSuccess,
  isUndefinedOrNull,
} = require('../../helper/repositoryBase');
const { requestPost } = require('../../helper/requestHelper');
const edgeRepository = require('../../db/edgeRepository');
const edgeManager = require('../../manager/edgeManager');
const controlManager = require('../../manager/controlManager');
const { getLogger } = require('../../helper/log4jsHelper');

const logger = getLogger('edgeController');

// 边缘端登录
exports.login = async (request, reply) => {
  const errInfo = await edgeManager.login(
    request.body.edgeID,
    request.ip,
    request.body.port
  );
  if (errInfo) {
    resError({ message: errInfo });
  }
  await edgeRepository.updateEdge({
    id: request.body.edgeID,
    ip: request.ip,
  });

  if (config.SyncSystem.useSync) {
    const edgeSchedules = await edgeRepository.getConfiguration({
      id: request.body.edgeID,
    });

    // 通知同步程序，边缘端同步配置已修改；这里不等待结果，执行失败，同步程序每天也会自动查询
    const url = `http://${config.SyncSystem.host}:${config.SyncSystem.port}/SyncSystem/changeConfig`;

    try {
      await requestPost(url, [edgeSchedules]);
    } catch (error) {
      logger.error(error);
    }
  }

  resSuccess({ reply, result: { ip: request.ip } });
};

// 边缘端登录
exports.loginControl = async (request, reply) => {
  const errInfo = await controlManager.login(
    request.ip, // 用第二个字段ip作为身份信息，该字段暂未使用，ip仅仅填充位置
    request.ip,
    request.body.port
  );
  if (errInfo) {
    resError({ message: errInfo });
  }
  resSuccess({ reply, result: { ip: request.ip } });
};

exports.loginSchema = {
  summary: '边缘端登录',
  description: '边缘端登录上线',
  tags: ['边缘端管理'],
  body: {
    type: 'object',
    properties: {
      edgeID: { type: 'string', description: '边缘端ID' },
      port: { type: 'number', description: '端口' },
    },
  },
};

exports.restartSchema = {
  summary: '边缘端重启',
  description: '边缘端重启',
  tags: ['边缘端管理'],
  body: {
    type: 'object',
    properties: {
      edgeID: {
        type: 'string',
        description: '边缘端ID，不重启边缘端时该字段为空',
      },
      deviceID: {
        type: 'string',
        description: '环境控制设备ID，不重启环境控制边缘端时该字段为空',
      },
    },
  },
};

// 边缘端重启
exports.restart = async (request, reply) => {
  if (!isUndefinedOrNull(request.body.edgeID)) {
    await edgeManager.restart(request.body.edgeID);
  }
  if (!isUndefinedOrNull(request.body.deviceID)) {
    await controlManager.restart(request.body.deviceID);
  }

  resSuccess({ reply });
};

// 获取边缘端列表
exports.getList = async (request, reply) => {
  const cacheEdges = await edgeManager.getEdges();

  let edges = [];
  if (request.query.supportedFeatures === undefined) {
    edges = cacheEdges;
  } else {
    // eslint-disable-next-line no-eval
    const supportedFeatures = eval(request.query.supportedFeatures);
    if (supportedFeatures === undefined || supportedFeatures.length === 0) {
      edges = cacheEdges;
    } else {
      cacheEdges.forEach((edge) => {
        for (let i = 0; i < edge.modules.length; i++) {
          if (edge.modules[i].moduleType === 'driver') {
            const feat = edge.modules[i].supportedFeatures.filter((value) =>
              new Set(supportedFeatures).has(value)
            );
            if (feat.length > 0) {
              edges.push(edge);
              break;
            }
          }
        }
      });
    }
  }
  if (request.query.isParam !== undefined && request.query.isParam === false) {
    edges.forEach((ed) => {
      ed.modules.forEach((mod) => {
        delete mod.parameters;
      });
    });
  }
  resSuccess({ reply, result: edges });
};

exports.getListSchema = {
  summary: '边缘端列表',
  description: '获取边缘端列表',
  tags: ['边缘端管理'],
  query: {
    supportedFeatures: {
      type: 'string',
      description: '功能列表，格式： [a, b]',
    },
    isParam: { description: '是否包含参数', type: 'boolean' },
  },
};

// exports.getListSchema = {
//   summary: '边缘端列表',
//   description: '获取边缘端列表',
//   tags: ['边缘端管理'],
//   body: {
//     supportedFeatures: {
//       type: 'array',
//       description: '功能',
//       items: { type: 'string' },
//     },
//     isParam: { description: '是否包含参数', type: 'boolean' },
//   },
// };

exports.getFuncParams = async (request, reply) => {
  const edge = await edgeManager.getEdge(request.query.edgeID);
  const func = edge.modules.filter((module) => {
    return module.id === request.query.id;
  });
  if (func.length === 0) {
    resSuccess({ reply, result: null });
  } else {
    resSuccess({ reply, result: func[0] });
  }
};

exports.getFuncParamsSchema = {
  summary: '获取功能详细信息',
  description: '获取功能详细信息',
  tags: ['边缘端管理'],
  query: {
    id: { description: '功能ID', type: 'string' },
    edgeID: { description: '边缘端ID', type: 'string' },
  },
};

// 获取边缘端配置版本
exports.getConfigurationVersion = async (request, reply) => {
  // edgeid为IP地址，该方法获取环境控制边缘端信息；
  if (net.isIPv4(request.query.edgeID)) {
    const controlEdge = await controlManager.getEdge(
      config.edge.controlEdgeID[0]
    );
    if (controlEdge && controlEdge.ip === request.query.edgeID) {
      const arrTime = [];
      controlEdge.modules.forEach((element) => {
        const time =
          element.updateTime == null ? element.createTime : element.updateTime;
        arrTime.push(new Date(time).getTime());
      });
      const version = Math.max.apply(null, arrTime);
      resSuccess({ reply, result: version.toString() });
    } else {
      resError({ message: '边缘端未登录' });
    }
  } else {
    const cacheEdge = await edgeManager.getEdge(request.query.edgeID);
    if (cacheEdge) {
      resSuccess({ reply, result: cacheEdge.version });
    } else {
      resError({ message: '边缘端未登录' });
    }
  }
};

exports.getConfigurationVersionSchema = {
  summary: '边缘端配置版本',
  description: '获取边缘端配置版本',
  tags: ['边缘端管理'],
  query: {
    edgeID: { description: '边缘端ID', type: 'string' },
  },
};

// 获取边缘端配置
exports.getEdgeConfiguration = async (request, reply) => {
  // edgeid为IP地址，该方法获取环境控制边缘端信息；
  if (net.isIPv4(request.query.edgeID)) {
    const controlEdge = await controlManager.getEdge(
      config.edge.controlEdgeID[0]
    );
    if (controlEdge && controlEdge.ip === request.query.edgeID) {
      controlEdge.modules.forEach((item, index, arr) => {
        if (
          item.moduleType === 'device' &&
          item.moduleCategory.indexOf('control') === -1
        ) {
          arr.splice(index, 1);
        }
      });
      resSuccess({ reply, result: controlEdge });
    } else {
      resError({ message: '边缘端未登录' });
    }
  } else {
    const cacheEdge = await edgeManager.getEdge(request.query.edgeID);
    if (cacheEdge) {
      if (request.query.rmControl) {
        cacheEdge.modules.forEach((item, index, arr) => {
          if (
            item.moduleType === 'device' &&
            item.moduleCategory.indexOf('control') !== -1
          ) {
            arr.splice(index, 1);
          }
        });
      }
      resSuccess({ reply, result: cacheEdge });
    } else {
      resError({ message: '边缘端未登录' });
    }
  }
};

exports.getEdgeConfigurationSchema = {
  summary: '边缘端配置',
  description: '获取边缘端配置',
  tags: ['边缘端管理'],
  query: {
    edgeID: { description: '边缘端ID/环境控制工控机IP', type: 'string' },
    rmControl: { description: '是否移除环境控制设备', type: 'boolean' },
  },
};

// 获取所有边缘端同步规则
exports.getEdgeSchedules = async (request, reply) => {
  // 缓存同步程序所属工控机通信IP地址（同步程序启动时调用）
  config.SyncSystem.host = request.ip;

  const edgeSchedules = await edgeRepository.getEdgeSchedules();
  resSuccess({ reply, result: edgeSchedules });
};

exports.getEdgeSchedulesSchema = {
  summary: '边缘端同步规则',
  description: '获取边缘端同步规则',
  tags: ['边缘端管理'],
};

// 获取边缘端同步规则
exports.getConfiguration = async (request, reply) => {
  const edgeSchedules = await edgeRepository.getConfiguration(request.body);
  resSuccess({ reply, result: edgeSchedules });
};

exports.getConfigurationSchema = {
  summary: '边缘端同步规则',
  description: '获取边缘端同步规则',
  tags: ['边缘端管理'],
  body: {
    type: 'object',
    properties: {
      id: { type: 'string' },
    },
    required: ['id'],
  },
};

// 更新边缘端同步规则
exports.updateEdgeSchedules = async (request, reply) => {
  // todo 是否更新时间
  await edgeRepository.updateEdgeSchedules(request.body);
  if (config.SyncSystem.useSync) {
    // 通知同步程序，边缘端同步配置已修改；这里不等待结果，执行失败，同步程序每天也会自动查询
    const url = `http://${config.SyncSystem.host}:${config.SyncSystem.port}/SyncSystem/changeConfig`;
    for (let i = 0; i < request.body.length; i++) {
      const edgeSchedules = await edgeRepository.getConfiguration({
        id: request.body[i].id,
      });
      if (edgeSchedules.ip && edgeSchedules.ip !== '') {
        try {
          await requestPost(url, edgeSchedules);
        } catch (error) {
          logger.error(error);
        }
      }
    }

    // 通知边缘端更细同步配置，并重启
    request.body.forEach((edgeValue) => {
      edgeManager.restart(edgeValue.id, true);
    });
  }
  resSuccess({ reply });
};

exports.updateEdgeSchedulesSchema = {
  summary: '边缘端同步规则',
  description: '更新边缘端同步规则',
  tags: ['边缘端管理'],
  body: {
    type: 'array',
    description: '边缘端同步规则列表',
    items: {
      type: 'object',
      properties: {
        id: { type: 'string', description: '边缘端ID' },
        syncSchedule: {
          type: 'array',
          description: '边缘端同步规则',
          items: { type: 'object' },
        },
      },
    },
  },
};
