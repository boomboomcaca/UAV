const controller = require('../controllers');
const defaultSchema = require('../schema/defaultSchema');

// 可以考虑把所有路由从数据库读取
// 找个地方定义出所有的表
const routes = [
  // #region 边缘端
  {
    method: 'POST',
    url: '/Edge/add',
    handler: controller.rmbt_edge.add,
    schema: controller.rmbt_edge.addSchema,
  },
  {
    method: 'POST',
    url: '/Edge/update',
    handler: controller.rmbt_edge.update,
    schema: controller.rmbt_edge.updateSchema,
  },
  {
    method: 'POST',
    url: '/Edge/delete',
    handler: controller.rmbt_edge.del,
    schema: controller.rmbt_edge.deleteSchema,
  },
  {
    method: 'GET',
    url: '/Edge/getList',
    handler: controller.rmbt_edge.getList,
    schema: defaultSchema.getPageDataSchema({
      description: '获取分页数据',
      tags: 'rmbt_edge',
    }),
  },
  // #endregion
  // #region 设备
  {
    method: 'POST',
    url: '/Device/add',
    handler: controller.rmbt_device.add,
    schema: controller.rmbt_device.addSchema,
  },
  {
    method: 'POST',
    url: '/Device/update',
    handler: controller.rmbt_device.update,
    schema: controller.rmbt_device.updateSchema,
  },
  {
    method: 'POST',
    url: '/Device/delete',
    handler: controller.rmbt_device.del,
    schema: controller.rmbt_device.deleteSchema,
  },
  {
    method: 'GET',
    url: '/Device/getList',
    handler: controller.rmbt_device.getList,
    schema: defaultSchema.getPageDataSchema({
      description: '获取分页数据',
      tags: 'rmbt_device',
    }),
  },
  {
    method: 'GET',
    url: '/Device/getParam',
    handler: controller.rmbt_device.getOne,
    schema: controller.rmbt_device.getParamSchema,
  },
  // #endregion
  // #region模板
  {
    method: 'POST',
    url: '/Template/add',
    handler: controller.rmbt_template.add,
    schema: controller.rmbt_template.addSchema,
  },
  {
    method: 'POST',
    url: '/Template/update',
    handler: controller.rmbt_template.update,
    schema: controller.rmbt_template.updateSchema,
  },
  {
    method: 'POST',
    url: '/Template/delete',
    handler: controller.rmbt_template.del,
    schema: controller.rmbt_template.deleteSchema,
  },
  {
    method: 'GET',
    url: '/Template/getOne',
    handler: controller.rmbt_template.getOne,
    schema: controller.rmbt_template.getParamSchema,
  },
  {
    method: 'GET',
    url: '/Template/getList',
    handler: controller.rmbt_template.getList,
    schema: defaultSchema.getPageDataSchema({
      description: '获取分页数据',
      tags: 'rmbt_template',
    }),
  },
  // #endregion

  // #region
  {
    method: 'POST',
    url: '/PlanningBusiness/add',
    handler: controller.rmbt_planning_business.add,
    schema: controller.rmbt_planning_business.addSchema,
  },
  {
    method: 'POST',
    url: '/PlanningBusiness/update',
    handler: controller.rmbt_planning_business.update,
    schema: controller.rmbt_planning_business.updateSchema,
  },
  {
    method: 'POST',
    url: '/PlanningBusiness/delete',
    handler: controller.rmbt_planning_business.del,
    schema: controller.rmbt_planning_business.deleteSchema,
  },
  {
    method: 'GET',
    url: '/PlanningBusiness/getList',
    handler: controller.rmbt_planning_business.getList,
    schema: defaultSchema.getPageDataSchema({
      description: '获取分页数据',
      tags: 'rmbt_planning_business',
    }),
  },
  {
    method: 'GET',
    url: '/PlanningBusiness/getAllWithSegmentType',
    handler: controller.rmbt_planning_business.getAllWithSegmentType,
    schema: controller.rmbt_planning_business.getAllWithSegmentTypeSchema,
  },
  // #endregion

  // #region
  {
    method: 'POST',
    url: '/PlanningSegmentType/add',
    handler: controller.rmbt_planning_segment_type.add,
    schema: controller.rmbt_planning_segment_type.addSchema,
  },
  {
    method: 'POST',
    url: '/PlanningSegmentType/update',
    handler: controller.rmbt_planning_segment_type.update,
    schema: controller.rmbt_planning_segment_type.updateSchema,
  },
  {
    method: 'POST',
    url: '/PlanningSegmentType/delete',
    handler: controller.rmbt_planning_segment_type.del,
    schema: controller.rmbt_planning_segment_type.deleteSchema,
  },
  {
    method: 'GET',
    url: '/PlanningSegmentType/getList',
    handler: controller.rmbt_planning_segment_type.getList,
    schema: defaultSchema.getPageDataSchema({
      description: '获取分页数据',
      tags: 'rmbt_planning_segment_type',
    }),
  },
  // #endregion

  // #region
  {
    method: 'POST',
    url: '/PlanningSegment/add',
    handler: controller.rmbt_planning_segment.add,
    schema: controller.rmbt_planning_segment.addSchema,
  },
  {
    method: 'POST',
    url: '/PlanningSegment/update',
    handler: controller.rmbt_planning_segment.update,
    schema: controller.rmbt_planning_segment.updateSchema,
  },
  {
    method: 'POST',
    url: '/PlanningSegment/relation',
    handler: controller.rmbt_planning_segment.relation,
    schema: controller.rmbt_planning_segment.relationSchema,
  },
  {
    method: 'POST',
    url: '/PlanningSegment/delete',
    handler: controller.rmbt_planning_segment.del,
    schema: controller.rmbt_planning_segment.deleteSchema,
  },
  {
    method: 'GET',
    url: '/PlanningSegment/getList',
    handler: controller.rmbt_planning_segment.getList,
    schema: defaultSchema.getPageDataSchema({
      description: '获取分页数据',
      tags: 'rmbt_planning_segment',
    }),
  },
  {
    method: 'GET',
    url: '/PlanningSegment/getCommonUse',
    handler: controller.rmbt_planning_segment.getCommonUse,
    schema: controller.rmbt_planning_segment.getCommonUseSchema,
  },
  // #endregion

  // #region
  {
    method: 'POST',
    url: '/ChannelDivision/add',
    handler: controller.rmbt_channel_division.add,
    schema: controller.rmbt_channel_division.addSchema,
  },
  {
    method: 'POST',
    url: '/ChannelDivision/update',
    handler: controller.rmbt_channel_division.update,
    schema: controller.rmbt_channel_division.updateSchema,
  },
  {
    method: 'POST',
    url: '/ChannelDivision/relation',
    handler: controller.rmbt_channel_division.relation,
    schema: controller.rmbt_channel_division.relationSchema,
  },
  {
    method: 'POST',
    url: '/ChannelDivision/delete',
    handler: controller.rmbt_channel_division.del,
    schema: controller.rmbt_channel_division.deleteSchema,
  },
  {
    method: 'GET',
    url: '/ChannelDivision/getList',
    handler: controller.rmbt_channel_division.getList,
    schema: defaultSchema.getPageDataSchema({
      description: '获取分页数据',
      tags: 'rmbt_channel_division',
    }),
  },
  // {
  //   method: 'POST',
  //   url: '/ChannelDivision/addAutomaticFreqs',
  //   handler: controller.rmbt_channel_division.addAutomaticFreqs,
  //   schema: controller.rmbt_channel_division.addAutomaticFreqsSchema,
  // },
  {
    method: 'GET',
    url: '/ChannelDivision/getChannelPlanning',
    handler: controller.channelController.getChannelPlanning,
    schema: controller.channelController.getChannelPlanningSchema,
  },
  // #region 新信号
  {
    method: 'POST',
    url: '/newSignalTemplate/add',
    handler: controller.rmbt_new_signal_template.add,
    schema: controller.rmbt_new_signal_template.addSchema,
  },
  {
    method: 'POST',
    url: '/newSignalTemplate/update',
    handler: controller.rmbt_new_signal_template.update,
    schema: controller.rmbt_new_signal_template.updateSchema,
  },
  {
    method: 'POST',
    url: '/newSignalTemplate/delete',
    handler: controller.rmbt_new_signal_template.del,
    schema: controller.rmbt_new_signal_template.deleteSchema,
  },
  {
    method: 'GET',
    url: '/newSignalTemplate/get',
    handler: controller.rmbt_new_signal_template.get,
    schema: controller.rmbt_new_signal_template.getSchema,
  },
  {
    method: 'GET',
    url: '/newSignalTemplate/getList',
    handler: controller.rmbt_new_signal_template.getList,
    schema: controller.rmbt_new_signal_template.getListSchema,
  },
  // #endregion

  // #region 新信号数据
  {
    method: 'POST',
    url: '/newSignalData/add',
    handler: controller.rmbt_new_signal_data.add,
    schema: controller.rmbt_new_signal_data.addSchema,
  },
  // {
  //   method: 'POST',
  //   url: '/newSignalData/update',
  //   handler: controller.rmbt_new_signal_data.update,
  //   schema: defaultSchema.getUpdateSchema({
  //     description: '修改新信号数据',
  //     tags: 'rmbt_new_signal_data',
  //   }),
  // },
  // {
  //   method: 'POST',
  //   url: '/newSignalData/delete',
  //   handler: controller.rmbt_new_signal_data.del,
  //   schema: controller.rmbt_new_signal_data.deleteSchema,
  // },
  {
    method: 'GET',
    url: '/newSignalData/getList',
    handler: controller.rmbt_new_signal_data.getList,
    schema: controller.rmbt_new_signal_data.getListSchema,
  },
  // #endregion,

  // #region 忽略信号
  {
    method: 'POST',
    url: '/newSignalIgnore/add',
    handler: controller.rmbt_new_signal_ignore.add,
    schema: controller.rmbt_new_signal_ignore.addSchema,
  },
  {
    method: 'POST',
    url: '/newSignalIgnore/update',
    handler: controller.rmbt_new_signal_ignore.update,
    schema: defaultSchema.getUpdateSchema({
      description: '修改忽略信号数据',
      tags: 'rmbt_new_signal_ignore',
    }),
  },
  {
    method: 'POST',
    url: '/newSignalIgnore/delete',
    handler: controller.rmbt_new_signal_ignore.del,
    schema: controller.rmbt_new_signal_ignore.deleteSchema,
  },
  {
    method: 'GET',
    url: '/newSignalIgnore/getList',
    handler: controller.rmbt_new_signal_ignore.getList,
    schema: controller.rmbt_new_signal_ignore.getListSchema,
  },
  // {
  //   method: 'POST',
  //   url: '/newSignalIgnore/addList',
  //   handler: controller.rmbt_new_signal_ignore.addList,
  //   schema: defaultSchema.getDefaultSchema({
  //     description: '批量添加忽略信号数据',
  //     tags: 'rmbt_new_signal_ignore',
  //   }),
  // },
  {
    method: 'GET',
    url: '/ResourceAlarm/getList',
    handler: controller.rmbt_resource_alarm.getList,
    schema: defaultSchema.getPageDataSchema({
      description: '获取分页数据',
      tags: 'rmbt_resource_alarm',
    }),
  },
  {
    method: 'POST',
    url: '/ResourceThreshold/add',
    handler: controller.rmbt_resource_threshold.add,
    schema: controller.rmbt_resource_threshold.addSchema,
  },
  {
    method: 'POST',
    url: '/ResourceThreshold/delete',
    handler: controller.rmbt_resource_threshold.delete,
    schema: controller.rmbt_resource_threshold.deleteSchema,
  },
  {
    method: 'GET',
    url: '/ResourceThreshold/get',
    handler: controller.rmbt_resource_threshold.getCurrent,
    schema: controller.rmbt_resource_threshold.getCurrentSchema,
  },
  {
    method: 'POST',
    url: '/ResourceThreshold/getOne',
    handler: controller.rmbt_resource_threshold.getOne,
    schema: controller.rmbt_resource_threshold.getOneSchema,
  },
  {
    method: 'GET',
    url: '/Uav/getList',
    handler: controller.uav.getList,
    schema: controller.uav.getListSchema,
  },
  {
    method: 'GET',
    url: '/Uav/get',
    handler: controller.uav.get,
    schema: controller.uav.getSchema,
  },
  // #endregion,
];
module.exports = routes;
