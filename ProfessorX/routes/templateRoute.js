const multer = require('fastify-multer');
const controller = require('../controllers');
const defaultSchema = require('../schema/defaultSchema');

const upload = multer({ dest: 'public/uploads/' });

// 可以考虑把所有路由从数据库读取
// 找个地方定义出所有的表
const routes = [
  // 频率模板
  {
    method: 'POST',
    url: '/FrequencyTemplate/add',
    handler: controller.template_frequency.add,
    schema: controller.template_frequency.addSchema,
  },
  {
    method: 'POST',
    url: '/FrequencyTemplate/delete',
    handler: controller.template_frequency.del,
    schema: controller.template_frequency.deleteSchema,
  },
  {
    method: 'POST',
    url: '/FrequencyTemplate/update',
    handler: controller.template_frequency.update,
    schema: controller.template_frequency.updateSchema,
  },
  {
    method: 'GET',
    url: '/FrequencyTemplate/get',
    handler: controller.template_frequency.get,
    schema: controller.template_frequency.getSchema,
  },
  {
    method: 'GET',
    url: '/FrequencyTemplate/getList',
    handler: controller.template_frequency.getList,
    schema: defaultSchema.getPageDataSchema({
      description: '获取分页数据',
      tags: 'rmbt_template_frequency',
    }),
  },
  {
    method: 'POST',
    url: '/FrequencyTemplate/import',
    preHandler: upload.single('templatefile'),
    handler: controller.template_frequency.import,
    schema: controller.template_frequency.importSchema,
  },
  {
    method: 'GET',
    url: '/FrequencyTemplate/export',
    handler: controller.template_frequency.export,
    schema: controller.template_frequency.exportSchema,
  },
  // 门限模板
  {
    method: 'POST',
    url: '/ThresholdTemplate/add',
    handler: controller.template_threshold.add,
    schema: controller.template_threshold.addSchema,
  },
  {
    method: 'POST',
    url: '/ThresholdTemplate/delete',
    handler: controller.template_threshold.del,
    schema: controller.template_threshold.deleteSchema,
  },
  {
    method: 'POST',
    url: '/ThresholdTemplate/update',
    handler: controller.template_threshold.update,
    schema: controller.template_threshold.updateSchema,
  },
  {
    method: 'GET',
    url: '/ThresholdTemplate/get',
    handler: controller.template_threshold.get,
    schema: controller.template_threshold.getSchema,
  },
  {
    method: 'GET',
    url: '/ThresholdTemplate/getList',
    handler: controller.template_threshold.getList,
    schema: defaultSchema.getPageDataSchema({
      description: '获取分页数据',
      tags: 'rmbt_template_threshold',
    }),
  },
  {
    method: 'POST',
    url: '/ThresholdTemplate/import',
    preHandler: upload.single('templatefile'),
    handler: controller.template_threshold.import,
    schema: controller.template_threshold.importSchema,
  },
  {
    method: 'GET',
    url: '/ThresholdTemplate/export',
    handler: controller.template_threshold.export,
    schema: controller.template_threshold.exportSchema,
  },
];
module.exports = routes;
