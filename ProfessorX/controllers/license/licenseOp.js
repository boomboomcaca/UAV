const fs = require('fs');
const path = require('path');
const {
  resSuccess,
  resError,
  getJson,
  config,
  isUndefinedOrNull,
} = require('../../helper/repositoryBase');

const {
  licPath,
  getLicenseLibName,
  licenseName,
  uploadLicense,
} = require('../../license/licenseHelper');

exports.uploadSchema = {
  description: '上传授权文件',
  tags: ['license'],
};

exports.downloadLibSchema = {
  description: '下载授权解密动态库',
  tags: ['license'],
  body: {
    type: 'object',
    properties: {
      osType: {
        type: 'integer',
        description: '操作系统类型：0-Windows，1-Linux',
      },
    },
  },
};

/**
 *导入数据
 *
 * @param {*} req
 * @param {*} reply
 */
exports.upload = async (req, reply) => {
  const sourcePath = req.file.path;
  await uploadLicense(sourcePath);
  resSuccess({ reply, result: '上传成功' });
};

exports.downloadLicense = async (req, reply) => {
  const licxPath = path.join(licPath, licenseName);
  if (!fs.existsSync(licxPath)) {
    resError({ message: '未找到授权文件' });
  }
  const stream = fs.createReadStream(licxPath);
  reply.header('Content-Disposition', `attachment; filename=license.licx`);
  reply.header('Content-Type', 'application/octet-stream');
  reply.send(stream);
};

exports.downloadLib = async (req, reply) => {
  const libName = getLicenseLibName(req.body.osType);
  if (isUndefinedOrNull(libName)) {
    resError({ message: '不支持该操作系统' });
  }
  const libPath = path.join(licPath, libName);
  if (!fs.existsSync(libPath)) {
    resError({ message: '未找到解密动态库' });
  }
  const stream = fs.createReadStream(libPath);
  reply.header('Content-Disposition', `attachment; filename=${libName}`);
  reply.header('Content-Type', 'application/octet-stream');
  reply.send(stream);
};

exports.getLicenseInfo = async (req, reply) => {
  const result = await getJson(config.license.statusResultKey);
  resSuccess({ reply, result });
};
