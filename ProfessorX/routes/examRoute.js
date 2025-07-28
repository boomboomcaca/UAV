const controller = require('../controllers');
const defaultSchema = require('../schema/defaultSchema');

const routes = [
  // 考试保障信号模板
  {
    method: 'POST',
    url: '/ExamSignalTemplate/add',
    handler: controller.rmbt_exam_signal_template.add,
    schema: controller.rmbt_exam_signal_template.addSchema,
  },
  {
    method: 'POST',
    url: '/ExamSignalTemplate/update',
    handler: controller.rmbt_exam_signal_template.update,
    schema: controller.rmbt_exam_signal_template.updateSchema,
  },
  {
    method: 'POST',
    url: '/ExamSignalTemplate/delete',
    handler: controller.rmbt_exam_signal_template.del,
    schema: controller.rmbt_exam_signal_template.deleteSchema,
  },
  {
    method: 'GET',
    url: '/ExamSignalTemplate/get',
    handler: controller.rmbt_exam_signal_template.get,
    schema: controller.rmbt_exam_signal_template.getSchema,
  },
  {
    method: 'GET',
    url: '/ExamSignalTemplate/getList',
    handler: controller.rmbt_exam_signal_template.getList,
    schema: controller.rmbt_exam_signal_template.getListSchema,
  },

  // 考试保障信号数据
  {
    method: 'POST',
    url: '/ExamSignalData/add',
    handler: controller.rmbt_exam_signal_data.add,
    schema: controller.rmbt_exam_signal_data.addSchema,
  },
  {
    method: 'GET',
    url: '/ExamSignalData/getList',
    handler: controller.rmbt_exam_signal_data.getList,
    schema: controller.rmbt_exam_signal_data.getListSchema,
  },

  // 考试保障忽略信号
  {
    method: 'POST',
    url: '/ExamSignalIgnore/add',
    handler: controller.rmbt_exam_signal_ignore.add,
    schema: controller.rmbt_exam_signal_ignore.addSchema,
  },
  {
    method: 'POST',
    url: '/ExamSignalIgnore/update',
    handler: controller.rmbt_exam_signal_ignore.update,
    schema: defaultSchema.getUpdateSchema({
      description: '修改考试保障忽略信号',
      tags: 'rmbt_exam_signal_ignore',
    }),
  },
  {
    method: 'POST',
    url: '/ExamSignalIgnore/delete',
    handler: controller.rmbt_exam_signal_ignore.del,
    schema: controller.rmbt_exam_signal_ignore.deleteSchema,
  },
  {
    method: 'GET',
    url: '/ExamSignalIgnore/getList',
    handler: controller.rmbt_exam_signal_ignore.getList,
    schema: controller.rmbt_exam_signal_ignore.getListSchema,
  },
];
module.exports = routes;
