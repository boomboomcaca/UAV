const controller = require('../controllers');
const defaultSchema = require('../schema/defaultSchema');

const routes = [
  {
    method: 'POST',
    url: '/ScanSignal/add',
    handler: controller.rmbt_scan_signal.add,
    schema: controller.rmbt_scan_signal.addSchema,
  },
  {
    method: 'POST',
    url: '/ScanSignal/addList',
    handler: controller.rmbt_scan_signal.addList,
    schema: defaultSchema.getDefaultSchema({
      description: '批量添加频段扫描信号',
      tags: 'rmbt_scan_signal',
    }),
  },
  {
    method: 'POST',
    url: '/ScanSignal/update',
    handler: controller.rmbt_scan_signal.update,
    schema: defaultSchema.getUpdateSchema({
      description: '修改频段扫描信号',
      tags: 'rmbt_scan_signal',
    }),
  },
  {
    method: 'POST',
    url: '/ScanSignal/delete',
    handler: controller.rmbt_scan_signal.del,
    schema: controller.rmbt_scan_signal.deleteSchema,
  },
  {
    method: 'POST',
    url: '/ScanSignal/deleteList',
    handler: controller.rmbt_scan_signal.del,
    schema: controller.rmbt_scan_signal.deleteListSchema,
  },
  {
    method: 'GET',
    url: '/ScanSignal/getList',
    handler: controller.rmbt_scan_signal.getList,
    schema: controller.rmbt_scan_signal.getListSchema,
  },
];
module.exports = routes;
