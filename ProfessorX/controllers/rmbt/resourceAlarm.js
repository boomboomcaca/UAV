// 基础操作 添加删除 修改 基于数据库二次封装
const {
  // remove,
  add,
  // getSingle,
  getPageData,
  getGUID,
  // resSuccess,
} = require('../../helper/repositoryBase');

const tableName = 'rmbt_resource_alarm';

exports.addSchema = {
  description: `添加设施监控告警信息`,
  tags: [tableName],
  summary: `设施监控`,
  body: {
    type: 'object',
    properties: {
      id: {
        type: 'string',
        description: 'ID',
      },
      threshold_id: {
        type: 'string',
        description: '设施告警门限表ID',
      },
      edge_id: {
        type: 'string',
        description: '边缘端ID',
      },
      host_name: {
        type: 'string',
        description: '主机名',
      },
      alarm_type: {
        type: 'string',
        description: '告警类型',
      },
      alarm_value: {
        type: 'string',
        description: '告警值',
      },
      create_time: {
        type: 'string',
        description: '记录时间',
      },
    },
  },
};

// exports.delete = async (req, reply) => {
//   const data = await getSingle({
//     tableName,
//     wheres: {
//       id: req.body.id,
//     },
//   });
//   if (data == null) {
//     resSuccess({ reply });
//     return;
//   }
//   await remove({
//     req,
//     reply,
//     tableName,
//     tableChineseName: '',
//   });
// };

exports.getList = async (req, reply) => {
  await getPageData({
    req,
    reply,
    tableName,
    isLimitMaxRow: false,
  });
};

exports.addData = async (data) => {
  data.id = getGUID();
  await add({
    tableName,
    mainData: data,
  });
};
