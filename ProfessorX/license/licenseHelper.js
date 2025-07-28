const os = require('os');
const fs = require('fs');
const path = require('path');
const ffi = require('ffi-napi');
const pump = require('pump');
const compressing = require('compressing');
const { execSync } = require('child_process');
const log4Manager = require('../helper/log4jsHelper');
const { isJSON, isUndefinedOrNull } = require('../helper/common');
const config = require('../data/config/config');
const moduleInfoConfig = require('../data/config/moduleInfo.json');

const logger = log4Manager.getLogger('license');

const licenseState = {
  success: 0,
  failed: 1,
  none: 2,
  expired: 3,
};

const licenseName = 'license.licx';
const uploadPath = path.join(process.cwd(), 'public/uploads/license');
const licPath = path.join(process.cwd(), 'data/license');

const execShellCmd = (cmdStr) => {
  const data = execSync(cmdStr, { encoding: 'utf-8' });
  logger.debug(data);
};

const getOsType = () => {
  let osType;
  if (os.type() === 'Windows_NT') {
    osType = 0;
    // windows
  } else if (os.type() === 'Linux') {
    osType = 1;
    // Linux
  } else if (os.type() === 'Darwin') {
    osType = 2;
    // mac
  } else {
    osType = -1;
    // 不支持提示
  }
  return osType;
};

const getLicenseLibName = (osType, rand = null) => {
  let libName = 'libLicenseDecrypt';
  if (!isUndefinedOrNull(rand)) {
    libName += rand;
  }
  if (osType === 0) {
    libName += '.dll';
  } else if (osType === 1) {
    libName += '.so';
  } else {
    libName = null;
  }
  return libName;
};

const unzip = async (sourcePath, destPath) => {
  await compressing.zip.uncompress(sourcePath, destPath);
};

const delSingleDir = (sPath) => {
  const files = fs.readdirSync(sPath);
  files.forEach((fi) => {
    const curr = `${sPath}/${fi}`;
    if (fs.statSync(curr).isDirectory()) {
      delSingleDir(curr);
    } else {
      fs.unlinkSync(curr);
    }
  });
  fs.rmdirSync(sPath);
};

const delDir = (sPath) => {
  if (!fs.existsSync(sPath)) {
    return;
  }
  delSingleDir(sPath);
};

const decryptLicense = (licensePath, libPath) => {
  logger.info('解密授权信息');
  try {
    const myLibrary = new ffi.Library(libPath, {
      decodeLicense: ['string', ['string']],
    });
    const result = myLibrary.decodeLicense(escape(licensePath));
    const real = unescape(result);
    return real;
  } catch (err) {
    logger.error(err);
  }
  return '';
};

function getVerifyResult(code, data = null) {
  switch (code) {
    case licenseState.none:
      return { code, message: '未授权', data };
    case licenseState.expired:
      return { code, message: '授权已过期', data };
    case licenseState.failed:
      return { code, message: '授权失败', data };
    case licenseState.success:
      return { code, message: '授权成功', data };
    default:
      return { code, message: '授权异常', data };
  }
}

const verifyLicense = (context, machineCode) => {
  if (isUndefinedOrNull(context)) {
    return getVerifyResult(licenseState.none);
  }

  if (!isJSON(context)) {
    return getVerifyResult(licenseState.failed);
  }
  const licenseObj = JSON.parse(context);
  if (!licenseObj) {
    return getVerifyResult(licenseState.failed);
  }
  logger.info(`当前机器码：${machineCode}`);
  const { modules, expirationDate, machineCodes, productCode, userId } =
    licenseObj;
  if (
    isUndefinedOrNull(expirationDate) ||
    isUndefinedOrNull(modules) ||
    isUndefinedOrNull(userId) ||
    isUndefinedOrNull(productCode)
  ) {
    return getVerifyResult(licenseState.failed);
  }

  // 校验是否可用
  if (
    isUndefinedOrNull(config.license.checkMachineCode) ||
    config.license.checkMachineCode
  ) {
    if (
      isUndefinedOrNull(machineCodes) ||
      machineCodes.indexOf(machineCode) < 0
    ) {
      return getVerifyResult(licenseState.failed);
    }
  }

  // 校验是否过期
  if (
    isUndefinedOrNull(config.license.checkExpirationDate) ||
    config.license.checkExpirationDate
  ) {
    const now = new Date();
    const expirationTime = new Date(expirationDate);
    if (now >= expirationTime) {
      return getVerifyResult(licenseState.expired, {
        productCode,
        expirationDate,
      });
    }
  }

  const result = getVerifyResult(licenseState.success, {
    productCode,
    expirationDate,
    modules,
  });
  return result;
};

function getValidModules(modules) {
  let validModules = [];
  if (!modules || modules.length < 1) {
    return validModules;
  }
  Object.keys(modules).forEach((index) => {
    const module = modules[index];
    if (isUndefinedOrNull(module.children)) {
      validModules.push(module);
    } else {
      const validChildren = getValidModules(module.children);
      validModules = validModules.concat(validChildren);
    }
  });

  return validModules;
}

const getHttpRoutes = (modules) => {
  const routes = [];
  const { httpWhiteList, httpRoutes } = moduleInfoConfig;
  Object.keys(httpWhiteList).forEach((index) => {
    routes.push(httpWhiteList[index]);
  });
  if (isUndefinedOrNull(modules)) {
    return routes;
  }
  const validModules = getValidModules(modules);
  if (validModules) {
    Object.keys(httpRoutes).forEach((index) => {
      const route = httpRoutes[index];
      if (
        validModules.some((validModule) => {
          return (
            validModule.name.toLowerCase() === route.moduleCode.toLowerCase() ||
            (!isUndefinedOrNull(route.parentCode) &&
              validModule.name.toLowerCase() === route.parentCode.toLowerCase())
          );
        })
      ) {
        routes.push(route.url);
      }
    });
  }

  return routes;
};

const getWsFeatures = (modules) => {
  const features = [];
  if (isUndefinedOrNull(modules)) {
    return features;
  }
  const { wsFeatures } = moduleInfoConfig;
  const validModules = getValidModules(modules);

  if (validModules) {
    Object.keys(wsFeatures).forEach((index) => {
      const feat = wsFeatures[index];
      if (
        validModules.some((validModule) => {
          return (
            validModule.name.toLowerCase() === feat.moduleCode.toLowerCase() ||
            (!isUndefinedOrNull(feat.parentCode) &&
              validModule.name.toLowerCase() === feat.parentCode.toLowerCase())
          );
        })
      ) {
        features.push(feat.feature);
      }
    });
  }

  return features;
};

const updateLicensePackage = () => {
  if (!fs.existsSync(uploadPath)) {
    return;
  }

  if (!fs.existsSync(licPath)) {
    fs.mkdirSync(licPath);
  }

  const osType = getOsType();
  const libName = getLicenseLibName(osType);
  if (isUndefinedOrNull(libName)) {
    return;
  }
  const srcLicensePath = path.join(uploadPath, licenseName);
  const srcLibPath = path.join(uploadPath, libName);

  const destLicensePath = path.join(licPath, licenseName);
  const destLibPath = path.join(licPath, libName);

  if (!fs.existsSync(srcLicensePath) || !fs.existsSync(srcLibPath)) {
    return;
  }

  if (!fs.existsSync(destLicensePath) || !fs.existsSync(destLibPath)) {
    try {
      fs.copyFileSync(srcLibPath, destLibPath);
      fs.copyFileSync(srcLicensePath, destLicensePath);
    } catch (err) {
      logger.warn('复制License文件异常');
    }
    return;
  }

  const srcLicxStat = fs.statSync(srcLicensePath);
  const srcLibStat = fs.statSync(srcLibPath);
  const destLicxStat = fs.statSync(destLicensePath);
  const destLibStat = fs.statSync(destLibPath);

  if (
    srcLibStat.ctime <= destLibStat.ctime ||
    srcLicxStat.ctime <= destLicxStat.ctime
  ) {
    return;
  }
  try {
    const tempLibName = path.join(
      licPath,
      getLicenseLibName(osType, `_${new Date().getTime()}`)
    );
    fs.renameSync(destLibPath, tempLibName);
    fs.copyFileSync(srcLibPath, destLibPath);
    fs.copyFileSync(srcLicensePath, destLicensePath);
  } catch (err) {
    logger.warn('复制License文件异常');
  }
};

const uploadLicense = async (filePath) => {
  if (fs.existsSync(uploadPath)) {
    delDir(uploadPath);
  }
  fs.mkdirSync(uploadPath);
  const destPath = path.join(uploadPath, 'license.zip');
  const sourceStream = fs.createReadStream(filePath);
  const destStream = fs.createWriteStream(destPath);
  pump(sourceStream, destStream);
  await unzip(destPath, uploadPath);
  updateLicensePackage();

  setTimeout(() => {
    logger.info('马上重启.....');
    execShellCmd(config.rebootCmd);
  }, 1000);

  return true;
};

module.exports = {
  licenseState,
  licenseName,
  licPath,
  getOsType,
  uploadLicense,
  decryptLicense,
  verifyLicense,
  getHttpRoutes,
  getLicenseLibName,
  updateLicensePackage,
  getWsFeatures,
};
