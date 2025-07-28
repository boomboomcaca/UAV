const controllers = require('../controllers');

const routes = [
  {
    method: 'POST',
    url: '/Edge/login',
    handler: controllers.edge.login,
    schema: controllers.edge.loginSchema,
  },
  {
    method: 'GET',
    url: '/Edge',
    handler: controllers.edge.getList,
    schema: controllers.edge.getListSchema,
  },
  {
    method: 'GET',
    url: '/Edge/getFuncParams',
    handler: controllers.edge.getFuncParams,
    schema: controllers.edge.getFuncParamsSchema,
  },

  {
    method: 'GET',
    url: '/Edge/getConfigurationVersion',
    handler: controllers.edge.getConfigurationVersion,
    schema: controllers.edge.getConfigurationVersionSchema,
  },
  {
    method: 'GET',
    url: '/Edge/getConfiguration',
    handler: controllers.edge.getEdgeConfiguration,
    schema: controllers.edge.getEdgeConfigurationSchema,
  },
  {
    method: 'GET',
    url: '/SyncConfiguration/getList',
    handler: controllers.edge.getEdgeSchedules,
    schema: controllers.edge.getEdgeSchedulesSchema,
  },
  {
    method: 'POST',
    url: '/SyncConfiguration/getConfiguration',
    handler: controllers.edge.getConfiguration,
    schema: controllers.edge.getConfigurationSchema,
  },
  {
    method: 'POST',
    url: '/SyncConfiguration/updateSchedule',
    handler: controllers.edge.updateEdgeSchedules,
    schema: controllers.edge.updateEdgeSchedulesSchema,
  },

  {
    method: 'POST',
    url: '/Edge/loginControl',
    handler: controllers.edge.loginControl,
    schema: controllers.edge.loginSchema,
  },
  {
    method: 'POST',
    url: '/Edge/restart',
    handler: controllers.edge.restart,
    schema: controllers.edge.restartSchema,
  },
];

module.exports = routes;
