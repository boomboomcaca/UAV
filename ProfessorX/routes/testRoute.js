const controller = require('../controllers');
const defaultSchema = require('../schema/defaultSchema');

const routes = [
  // #region 设备
  // 通过反射可以直接生成
  {
    method: 'POST',
    url: '/testdevice/add',
    handler: controller.test_device.add,
    schema: defaultSchema.getAddSchema({
      description: '添加设备',
      tags: 'test_device',
    }),
  },
  {
    method: 'POST',
    url: '/testdevice/update',
    handler: controller.test_device.update,
    schema: defaultSchema.getUpdateSchema({
      description: '修改设备',
      tags: 'test_device',
    }),
  },
  {
    method: 'POST',
    url: '/testdevice/delete',
    handler: controller.test_device.del,
    schema: defaultSchema.getDeleteSchema({
      description: '删除设备',
      tags: 'test_device',
    }),
  },
  {
    method: 'GET',
    url: '/testdevice/getList',
    handler: controller.test_device.getList,
    // 可以考虑动态生成一些
    schema: defaultSchema.getPageDataSchema({
      description: '获取分页设备',
      tags: 'test_device',
    }),
  },
  {
    method: 'GET',
    url: '/testdevice/queryTest',
    handler: controller.test_device.queryTest,
    // 可以考虑动态生成一些
    schema: defaultSchema.getDefaultSchema({
      description: '查询',
      tags: 'test_device',
    }),
  },
  // #endregion

  // #region 项目
  {
    method: 'POST',
    url: '/testproject/add',
    handler: controller.test_project.add,
    schema: defaultSchema.getAddSchema({
      description: '添加项目',
      tags: 'test_project',
    }),
  },
  {
    method: 'POST',
    url: '/testproject/update',
    handler: controller.test_project.update,
    schema: defaultSchema.getUpdateSchema({
      description: '修改项目',
      tags: 'test_project',
    }),
  },
  {
    method: 'POST',
    url: '/testproject/delete',
    handler: controller.test_project.del,
    schema: defaultSchema.getDeleteSchema({
      description: '删除项目',
      tags: 'test_project',
    }),
  },
  {
    method: 'GET',
    url: '/testproject/getList',
    handler: controller.test_project.getList,
    // 可以考虑动态生成一些
    schema: defaultSchema.getPageDataSchema({
      description: '获取分页项目',
      tags: 'test_project',
      post: false,
    }),
  },
  {
    method: 'GET',
    url: '/testproject/queryTest',
    handler: controller.test_project.queryTest,
    // 可以考虑动态生成一些
    schema: defaultSchema.getDefaultSchema({
      description: '查询',
      tags: 'test_project',
    }),
  },
  // #endregion
];
module.exports = routes;
