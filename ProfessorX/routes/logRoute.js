const controller = require('../controllers');
const defaultSchema = require('../schema/defaultSchema');

const routes = [
  // {
  //   method: 'POST',
  //   url: '/LogDeviceStatus/add',
  //   handler: controller.log_device_status.add,
  //   schema: controller.log_device_status.addSchema,
  // },
  {
    method: 'GET',
    url: '/LogDeviceStatus/getList',
    handler: controller.log_device_status.getList,
    schema: defaultSchema.getPageDataSchema({
      description: '查询日志',
      tags: 'log_device_status',
    }),
  },

  // {
  //   method: 'POST',
  //   url: '/LogEdgeStatus/add',
  //   handler: controller.log_edge_status.add,
  //   schema: controller.log_edge_status.addSchema,
  // },
  {
    method: 'GET',
    url: '/LogEdgeStatus/getList',
    handler: controller.log_edge_status.getList,
    schema: defaultSchema.getPageDataSchema({
      description: '查询日志',
      tags: 'log_edge_status',
    }),
  },

  // {
  //   method: 'POST',
  //   url: '/LogEdgeTrack/add',
  //   handler: controller.log_edge_track.add,
  //   schema: controller.log_edge_track.addSchema,
  // },
  {
    method: 'GET',
    url: '/LogEdgeTrack/getList',
    handler: controller.log_edge_track.getList,
    schema: defaultSchema.getPageDataSchema({
      description: '查询日志',
      tags: 'log_edge_track',
    }),
  },

  // {
  //   method: 'POST',
  //   url: '/LogPlanRunning/add',
  //   handler: controller.log_plan_running.add,
  //   schema: controller.log_plan_running.addSchema,
  // },
  {
    method: 'GET',
    url: '/LogPlanRunning/getList',
    handler: controller.log_plan_running.getList,
    schema: defaultSchema.getPageDataSchema({
      description: '查询日志',
      tags: 'log_plan_running',
    }),
  },

  // {
  //   method: 'POST',
  //   url: '/LogRemoteControl/add',
  //   handler: controller.log_remote_control.add,
  //   schema: controller.log_remote_control.addSchema,
  // },
  {
    method: 'GET',
    url: '/LogRemoteControl/getList',
    handler: controller.log_remote_control.getList,
    schema: defaultSchema.getPageDataSchema({
      description: '查询日志',
      tags: 'log_remote_control',
    }),
  },

  // {
  //   method: 'POST',
  //   url: '/LogEnviInfo/add',
  //   handler: controller.log_envi_info.add,
  //   schema: controller.log_envi_info.addSchema,
  // },
  {
    method: 'GET',
    url: '/LogEnviInfo/getList',
    handler: controller.log_envi_info.getList,
    schema: defaultSchema.getPageDataSchema({
      description: '查询日志',
      tags: 'log_envi_info',
    }),
  },

  // {
  //   method: 'POST',
  //   url: '/LogFileInfo/add',
  //   handler: controller.log_file_info.add,
  //   schema: controller.log_file_info.addSchema,
  // },
  // {
  //   method: 'POST',
  //   url: '/LogFileInfo/update',
  //   handler: controller.log_file_info.update,
  //   schema: controller.log_file_info.updateSchema,
  // },
  {
    method: 'POST',
    url: '/LogFileInfo/delete',
    handler: controller.log_file_info.del,
    schema: controller.log_file_info.deleteSchema,
  },
  {
    method: 'GET',
    url: '/LogFileInfo/get',
    handler: controller.log_file_info.get,
    schema: controller.log_file_info.getSchema,
  },
  {
    method: 'GET',
    url: '/LogFileInfo/getList',
    handler: controller.log_file_info.getList,
    schema: defaultSchema.getPageDataSchema({
      description: '查询文件信息',
      tags: 'log_file_info',
    }),
  },
  {
    method: 'POST',
    url: '/LogFileInfo/syncFile',
    handler: controller.log_file_info.syncFile,
    schema: controller.log_file_info.syncFileSchema,
  },
  {
    method: 'POST',
    url: '/LogFileInfo/delList',
    handler: controller.log_file_info.delList,
    schema: defaultSchema.getDeleteListSchema({
      description: '批量删除文件信息',
      tags: 'log_file_info',
    }),
  },

  {
    method: 'POST',
    url: '/TaskInfo/delete',
    handler: controller.log_task_info.del,
    schema: controller.log_task_info.deleteSchema,
  },
  {
    method: 'GET',
    url: '/TaskInfo/get',
    handler: controller.log_task_info.get,
    schema: controller.log_task_info.getSchema,
  },
  {
    method: 'GET',
    url: '/TaskInfo/getList',
    handler: controller.log_task_info.getList,
    schema: defaultSchema.getPageDataSchema({
      description: '查询文件信息',
      tags: 'log_task_info',
    }),
  },
  {
    method: 'GET',
    url: '/Task/get',
    handler: controller.task_controller.get,
    schema: controller.task_controller.getSchema,
  },
  {
    method: 'GET',
    url: '/Task/getList',
    handler: controller.task_controller.getList,
    schema: controller.task_controller.getListSchema,
  },
  {
    method: 'POST',
    url: '/LogBusiness/add',
    handler: controller.log_business.add,
    schema: controller.log_business.addSchema,
  },
  {
    method: 'GET',
    url: '/LogBusiness/getList',
    handler: controller.log_business.getList,
    schema: defaultSchema.getPageDataSchema({
      description: '获取分页数据',
      tags: 'log_business',
    }),
  },
  {
    method: 'POST',
    url: '/LogBusiness/getTypeCountList',
    handler: controller.log_business.getTypeCountList,
    schema: controller.log_business.getTypeCountListSchema,
  },
];

module.exports = routes;
