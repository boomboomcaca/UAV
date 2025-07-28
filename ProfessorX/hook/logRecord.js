const { getLogger } = require('../helper/log4jsHelper');
const { isUndefinedOrNull } = require('../helper/common');
// todo:日志记录加入配置 配置是否要记录请求参数和返回参数
const logger = getLogger('requestLog');

// todo:优化日志模块 整理记录
function getReqLog(req) {
  const log = {
    reqId: req.id,
    req: req.method === 'POST' ? req.body : '',
    url: req.url,
    method: req.method,
    hostname: req.hostname,
    ip: req.ip,
    startTime: new Date(new Date().getTime() + 28800000),
    endTime: '',
    resTime: '',
    res: '',
    userId: '',
    account: '',
    loginTime: '',
  };
  if (!isUndefinedOrNull(req.user)) {
    log.userId = req.user.userId;
    log.account = req.user.account;
    log.loginTime = req.user.date;
  }
  return log;
}
exports.resLogRecord = async (req, reply, payLoad) => {
  const newDate = new Date(new Date().getTime() + 28800000);
  const log = req.reqLog || getReqLog(req);
  const level = req.logLevel || 'info';
  // 把message直接写入
  // 其实要判断payLoad
  log.res = level === 'info' ? '' : payLoad;
  log.req = level === 'info' ? '' : log.req;
  log.resTime = newDate - log.startTime;
  log.endTime = newDate;
  let logInfo = '';
  // let logInfo = `level=${level} ts=${JSON.stringify(new Date()).replace(/\"/g, "")} module=${logger.category}`;
  const logData = Object.entries(log);
  logData.forEach((s) => {
    const [key, value] = s;
    logInfo += ` ${key}=${JSON.stringify(value)}`;
  });
  if (level === 'warn') logger.warn(logInfo);
  else if (level === 'error') logger.error(logInfo);
  else logger.info(logInfo);
};
exports.reqLogRecord = async (req) => {
  // 请求日志放到req中
  req.reqLog = getReqLog(req);
};
