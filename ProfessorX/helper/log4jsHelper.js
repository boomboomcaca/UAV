// eslint-disable-next-line import/no-extraneous-dependencies
const log4js = require('log4js');
const config = require('../data/config/config');

// https://log4js-node.github.io/log4js-node/layouts.html
// 'c': categoryName,
// 'd': formatAsDate,
// 'h': hostname,
// 'm': formatMessage,
// 'n': endOfLine,
// 'p': logLevel,
// 'r': startTime,
// '[': startColour,
// ']': endColour,
// '%': percent,
// 'x': userDefined
// 日志初始化

function getLogInfo(level, module, msg) {
  let logInfo = `level=${level} ts=${JSON.stringify(new Date()).replace(
    // eslint-disable-next-line no-useless-escape
    /\"/g,
    ''
  )} module=${module} `;
  if (typeof msg === 'object') {
    logInfo += JSON.stringify(msg);
  } else {
    logInfo += msg;
  }
  // todo:可以考虑异常换行符
  return logInfo;
}

exports.init = async () => {
  const log4jsconfig = config.log4jsconfigure;
  log4jsconfig.appenders.app.layout.tokens = {
    lokiStr(logEvent) {
      const logInfo = getLogInfo(
        logEvent.level.levelStr.toLowerCase(),
        logEvent.categoryName,
        logEvent.data[0]
      );
      // let logInfo = `level=${logEvent.level.levelStr.toLowerCase()} ts=${JSON.stringify(new Date()).replace(/\"/g, "")} module=${logEvent.categoryName} `
      return logInfo;
    },
  };
  log4js.configure(log4jsconfig);
};

exports.getLogger = (category) => {
  // 直接使用Log4Js的方法格式化
  return log4js.getLogger(category);
  // var logger = new Logger(category);
  // return logger;
};

// 如果有http 请求把当前信息写入到日志中
// 可以放到http 请求上一下文中 这样我就可以在http 请求上下文直接获取到请求用户
// 初步设计通过日志拦截的方式实现
// todo: 下面方法是使用fastify的log对象
// 写入的其实只有code,Message

// module.exports = BusinessLogger;
// fastify 框架默认的日志
// 也可以考虑直接用request.log 写日志
// todo:任然使用info error debug warn 进行日志打印 方便后续进行格式化
// Logger.prototype.info = (msg) => {
//   const logInfo = getLogInfo('info', this.category, msg);
//   this.logger.info(logInfo);
// };
// Logger.prototype.error = (msg) => {
//   const logInfo = getLogInfo('error', this.category, msg);
//   this.logger.error(logInfo);
// };
// Logger.prototype.debug = (msg) => {
//   const logInfo = getLogInfo('debug', this.category, msg);
//   this.logger.debug(logInfo);
// };
// Logger.prototype.warn = (msg) => {
//   const logInfo = getLogInfo('warn', this.category, msg);
//   this.logger.warn(logInfo);
// };
// Logger.prototype.fatal = (msg) => {
//   const logInfo = getLogInfo('fatal', this.category, msg);
//   this.logger.fatal(logInfo);
// };

// Logger.prototype.trace = (msg) => {
//   const logInfo = getLogInfo('trace', this.category, msg);
//   this.logger.trace(logInfo);
// };
