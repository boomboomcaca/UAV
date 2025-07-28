const fs = require('fs');
const path = require('path');
const config = require('../data/config/config');
const { setJson } = require('../helper/cacheManager');

const { getMachineCode } = require('./machineCodeHelper');
const {
  licenseState,
  decryptLicense,
  verifyLicense,
  getLicenseLibName,
  getOsType,
  licPath,
  licenseName,
  updateLicensePackage,
} = require('./licenseHelper');
const {
  knex,
  isUndefinedOrNull,
  getCurrentDate,
} = require('../helper/repositoryBase');

let isUsing = false;

function getValidModules(modules) {
  let validModules = [];
  if (!modules || modules.length < 1) {
    return validModules;
  }
  Object.keys(modules).forEach((index) => {
    const module = modules[index];
    validModules.push(module.name);
    if (!isUndefinedOrNull(module.children)) {
      const validChildren = getValidModules(module.children);
      validModules = validModules.concat(validChildren);
    }
  });
  return validModules;
}

const init = async () => {
  const osType = getOsType();
  const libName = getLicenseLibName(osType);
  await setJson(config.license.statusResultKey, {
    code: licenseState.none,
    message: '未授权',
  });
  await knex('sys_permission_module')
    .where('need_auth', 1)
    .update({ enable: 0, update_time: getCurrentDate() });

  if (isUndefinedOrNull(libName)) {
    return;
  }
  const libPath = path.join(licPath, libName);
  const licensePath = path.join(licPath, licenseName);

  if (!isUsing) {
    isUsing = true;
    updateLicensePackage();
    if (!fs.existsSync(libPath) || !fs.existsSync(licensePath)) {
      await setJson(config.license.statusResultKey, {
        code: licenseState.failed,
        message: '授权失败',
      });
      isUsing = false;
      return;
    }
    const context = decryptLicense(licensePath, libPath);
    const machineCode = getMachineCode();
    const verifyResult = verifyLicense(context, machineCode);
    if (verifyResult.code === licenseState.success) {
      const modules = getValidModules(verifyResult.data.modules);
      if (!isUndefinedOrNull(modules)) {
        await knex('sys_permission_module')
          .whereIn('code', modules)
          .andWhere('need_auth', 1)
          .update({ enable: 1, update_time: getCurrentDate() });
      }
    }
    await setJson(config.license.statusResultKey, verifyResult);
    isUsing = false;
  }
};

module.exports = {
  init,
};
