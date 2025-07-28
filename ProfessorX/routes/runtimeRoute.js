const controllers = require('../controllers');

const routes = [
  {
    method: 'GET',
    url: '/Runtime/get',
    handler: controllers.sys_runtime.get,
    schema: controllers.sys_runtime.getSchema,
  },
];

module.exports = routes;
