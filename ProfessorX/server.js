/* eslint-disable */
// 公共参数配置
require('./helper/extend');
const jwt = require('fastify-jwt');
const fastify = require('fastify')();
const multer = require('fastify-multer');
const config = require('./data/config/config');
const log4Manager = require('./helper/log4jsHelper');
const edgeManager = require('./manager/edgeManager');
const requestManager = require('./manager/requestManager');
const clientManager = require('./manager/clientManager');
const licenseManager = require('./license/licenseManager');
const notificationTranspond = require('./manager/notificationTranspond');
const notificationStorage = require('./manager/notificationStorage');
const resourceAlarmManager = require('./manager/resourceAlarmManager');
const { errorHandler } = require('./helper/errorHandle');
const { routes } = require('./routes');
const { regGlobalHandler } = require('./helper/routeHelper');
const controlManager = require('./manager/controlManager');
const cacheManger = require('./helper/cacheManager');
const logRecorder = require('./manager/logRecorder');
const logger = log4Manager.getLogger('server');
const runtimeManager = require('./manager/runtimeManager');
const serveStatic = require('fastify-static');
const path = require('path');
async function Initialize() {
  // fastify.register(serveStatic, {
  //   root: path.join(__dirname, 'public'),
  // });
  fastify.register(serveStatic, {
    root: path.join(process.cwd(), 'public'),
    prefix: '/public/', // optional: default '/'
  });
  fastify.register(multer.contentParser);
  fastify.register(routes); // 注入路由
  fastify.register(jwt, config.jwt);
  fastify.setErrorHandler(errorHandler);
  // 注入全局路由处理 不会被写入swagger
  fastify.all('*', regGlobalHandler);
  //fastify.use('/', serveStatic(path.resolve(__dirname, 'public')));

  // 初始化边缘端列表

  await log4Manager.init();
  logger.info(
    `port:${config.port} wsPort:${config.wsPort} domainName:${config.domainName} database:${config.DB}`
  );
  await notificationTranspond.init();
  await notificationStorage.init();
  await resourceAlarmManager.init();
  await cacheManger.init();
  await clientManager.init();
  await edgeManager.init();
  await licenseManager.init();
  await controlManager.init();
  await requestManager.init();
  await runtimeManager.init();
  logRecorder.init();

  return fastify;
}
// app 启动
Initialize()
  .then((s) => s.listen(config.port, '0.0.0.0'))
  .catch((err) => logger.error(err));

// 监听未捕获的异常
process.on('uncaughtException', (err) => {
  logger.error('uncaughtException');
  logger.error(err);
});

// 监听Promise没有被捕获的失败函数
process.on('unhandledRejection', (err, p) => {
  logger.error('Unhandled Rejection at:', p, 'reason:', err);
});
module.exports = fastify;
