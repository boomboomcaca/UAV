const multer = require('fastify-multer');
const controllers = require('../controllers');
const defaultSchema = require('../schema/defaultSchema');

const upload = multer({ dest: 'public/uploads/' });
const routes = [
  {
    method: 'POST',
    url: '/License/upload',
    preHandler: upload.single('license'),
    handler: controllers.license.upload,
    schema: controllers.license.uploadSchema,
  },
  {
    method: 'GET',
    url: '/License/downloadLicense',
    handler: controllers.license.downloadLicense,
    schema: defaultSchema.getDefaultSchema({
      description: '下载License文件',
      tags: 'license',
    }),
  },
  {
    method: 'POST',
    url: '/License/downloadLib',
    handler: controllers.license.downloadLib,
    schema: controllers.license.downloadLibSchema,
  },
  {
    method: 'GET',
    url: '/License/getLicenseInfo',
    handler: controllers.license.getLicenseInfo,
    schema: defaultSchema.getDefaultSchema({
      description: '获取云端授权信息',
      tags: 'license',
    }),
  },
];

module.exports = routes;
