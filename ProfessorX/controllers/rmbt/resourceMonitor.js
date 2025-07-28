// 基础操作 添加删除 修改 基于数据库二次封装
const {
  // remove,
  add,
  // getSingle,
  // getPageData,
  // resSuccess,
} = require('../../helper/repositoryBase');

const tableName = 'rmbt_resource_monitor';

const thresholdProperties = {
  id: {
    type: 'string',
    description: 'ID',
  },
  edge_id: {
    type: 'string',
    description: '边缘端ID',
  },
  host_name: {
    type: 'string',
    description: '主机名',
  },
  memory_utilization: {
    type: 'string',
    description: '内存占用量',
  },
  cpu_utilization: {
    type: 'string',
    description: 'CPU使用率',
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
    description: '监控时间',
  },
};

exports.addSchema = {
  description: `添加设施监控数据`,
  tags: [tableName],
  summary: `设施监控`,
  body: {
    type: 'object',
    properties: thresholdProperties,
  },
};

exports.deleteSchema = {
  description: `删除设施监控数据`,
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

// exports.add = async (req, reply) => {
//   const data = req.body;
//   await add({
//     tableName,
//     mainData: data,
//   });
//   resSuccess({
//     reply,
//     result: data,
//   });
// };

// exports.del = async (req, reply) => {
//   const data = await getSingle({
//     tableName,
//     wheres: {
//       id: req.body.id,
//     },
//   });
//   if (data == null) {
//     resSuccess({
//       reply,
//     });
//     return;
//   }
//   await remove({
//     req,
//     reply,
//     tableName,
//     tableChineseName: '',
//   });
// };

// exports.getList = async (req, reply) => {
//   await getPageData({
//     req,
//     reply,
//     tableName,
//     isLimitMaxRow: false,
//   });
// };

exports.addData = async (data) => {
  await add({
    tableName,
    mainData: data,
  });
};
