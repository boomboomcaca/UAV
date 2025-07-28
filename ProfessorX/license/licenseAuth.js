const config = require('../data/config/config');
const { getJson } = require('../helper/cacheManager');
const { isUndefinedOrNull } = require('../helper/common');
const { resError } = require('../helper/handle');
const { getEdge } = require('../manager/edgeManager');
const {
  licenseState,
  getHttpRoutes,
  getWsFeatures,
} = require('./licenseHelper');

const log4Manager = require('../helper/log4jsHelper');

const logger = log4Manager.getLogger('licenseAuth');

const errorMsg = { code: 500, message: '权限受限' };

// 这个是由Hook执行的
const licenseHttpAuth = async (request) => {
  if (!isUndefinedOrNull(config.license.enable) && !config.license.enable) {
    return;
  }
  const licenseStatusResult = await getJson(config.license.statusResultKey);
  let modules = null;
  if (
    licenseStatusResult &&
    licenseStatusResult.code === licenseState.success &&
    licenseStatusResult.data
  ) {
    modules = licenseStatusResult.data.modules;
  }
  const routes = getHttpRoutes(modules);
  const isLegal = routes.some((route) => {
    const reg = `${route.replace('*', '\\S*$')}$`;
    return request.raw.url.match(reg);
  });
  if (isLegal) {
    return;
  }
  logger.error(`${request.raw.url}权限受限`);
  resError({ message: '权限受限' });
};

function authByFeature(feature, features) {
  const isLegal = features.some((feat) => {
    return feat.toLowerCase() === feature.toLowerCase();
  });
  if (isLegal) {
    return;
  }
  logger.error(`多站任务${feature}权限受限`);
  throw errorMsg;
}

async function authByModuleID(moduleID, edgeID, features) {
  const edge = await getEdge(edgeID);
  if (isUndefinedOrNull(edge) || isUndefinedOrNull(edge.modules)) {
    logger.error(`边缘端${edgeID}不可用`);
    resError({ code: 500, message: '边缘端不可用' });
  }
  const edgeModule = edge.modules.find(
    (element) => element.id === moduleID && element.moduleType === 'driver'
  );

  if (
    isUndefinedOrNull(edgeModule) ||
    isUndefinedOrNull(edgeModule.supportedFeatures)
  ) {
    logger.error(`边缘端${edgeID}不支持ID为${moduleID}的功能`);
    resError({ code: 500, message: '边缘端不支持该功能' });
  }

  const isLegal = features.some((feat) => {
    return (
      edgeModule.supportedFeatures.findIndex(
        (p) => p.toLowerCase() === feat.toLowerCase()
      ) >= 0
    );
  });
  if (isLegal) {
    return;
  }
  logger.error(`权限受限【边缘端：${edgeID}；功能：${moduleID}】`);
  throw errorMsg;
}

const licenseWsAuth = async (msgParams, methodName) => {
  if (!isUndefinedOrNull(config.license.enable) && !config.license.enable) {
    return;
  }
  const { feature, requestType, edgeID, moduleID } = msgParams;
  if (requestType !== 'singleTask' && requestType !== 'multipleTask') {
    return;
  }

  const licenseStatusResult = await getJson(config.license.statusResultKey);

  let modules = null;
  if (
    licenseStatusResult &&
    licenseStatusResult.code === licenseState.success &&
    licenseStatusResult.data
  ) {
    modules = licenseStatusResult.data.modules;
  }

  const features = getWsFeatures(modules);
  if (requestType === 'multipleTask') {
    authByFeature(feature, features);
  } else if (!isUndefinedOrNull(moduleID) && methodName === 'presetTask') {
    await authByModuleID(moduleID, edgeID, features);
  }
};

module.exports = {
  licenseHttpAuth,
  licenseWsAuth,
};
