const { getDataType } = require('./common');
const { dataTypeEnum } = require('./enum');
const { config, isUndefinedOrNull, getLogger } = require('./repositoryBase');

//
const logger = getLogger('errorHandle');

function getFiledDesc(filed, request) {
  let { url } = request.raw;
  if (request.raw.url.includes('?')) {
    url = url.substring(0, url.indexOf('?'));
  }
  const route = config.routes.where((s) => s.url === url).first();

  let properties =
    route.schema.body.properties || route.schema.query.properties;
  // 主要是处理数组问题 todo：有可能输入的是数组 简单点处理
  if (!properties) {
    properties = route.schema.body.items.properties;
  }
  if (getDataType(properties) === dataTypeEnum.array) {
    // eslint-disable-next-line no-const-assign
    // eslint-disable-next-line prefer-destructuring
    properties = properties[0];
  }
  return properties[filed].description || filed;
}

function replyError(reply, code, message) {
  const data = {
    error: {
      code,
      message,
    },
  };
  // 以600错误码返回
  reply.status(600).send(data);
}
exports.errorHandler = (error, request, reply) => {
  logger.error(error.message);
  // 通用错误处理在加上错误码
  // 处理jwt错误
  let errorMessage = '';
  try {
    errorMessage = error.message || '';
    request.logLevel = 'warn';
    // 处理 必填 最大最小 正则 三种验证
    if (!isUndefinedOrNull(error.validation)) {
      const validation = error.validation[0];
      let filed =
        validation.dataPath.split('.')[1] || validation.params.missingProperty;
      // 如果字段不为空
      // [0].account;
      if (!isUndefinedOrNull(filed)) {
        filed = getFiledDesc(filed, request);
        if (validation.keyword === 'required') {
          errorMessage = `${filed}不能为空`;
        } else if (
          validation.keyword === 'maxLength' ||
          validation.keyword === 'minLength'
        ) {
          errorMessage = `${filed}${
            validation.keyword === 'maxLength' ? '最大' : '最小'
          }长度${validation.params.limit}`;
        } else if (validation.keyword === 'pattern') {
          errorMessage = `${filed} 错误`;
        }
      }
    }
    // 处理程序异常
    if (typeof error.stack !== 'undefined') {
      request.logLevel = 'error';
      logger.error(error);
      // 400请求前端自行处理
      replyError(reply, error.code || 500, errorMessage);
    } else {
      replyError(reply, error.code, errorMessage);
    }
  } catch (err) {
    logger.error(err.message);
    replyError(reply, 500, err.message);
  }
};
