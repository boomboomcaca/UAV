const controller = require('../controllers');
const defaultSchema = require('../schema/defaultSchema');

const routes = [
  {
    method: 'POST',
    url: '/Plan/add',
    handler: controller.rmbt_plan.add,
    schema: controller.rmbt_plan.addSchema,
  },
  {
    method: 'POST',
    url: '/Plan/update',
    handler: controller.rmbt_plan.update,
    schema: controller.rmbt_plan.updateSchema,
  },
  {
    method: 'POST',
    url: '/Plan/delete',
    handler: controller.rmbt_plan.del,
    schema: controller.rmbt_plan.deleteSchema,
  },
  {
    method: 'GET',
    url: '/Plan/getList',
    handler: controller.rmbt_plan.getList,
    schema: defaultSchema.getPageDataSchema({
      description: '获取计划列表',
      tags: 'rmbt_plan',
    }),
  },
  {
    method: 'GET',
    url: '/Plan/getExecutersList',
    handler: controller.rmbt_plan.getExecutersList,
    schema: defaultSchema.getPageDataSchema({
      description: '获取计划执行者列表',
      tags: 'rmbt_plan',
    }),
  },
];
module.exports = routes;
