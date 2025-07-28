const config = require('../data/config/config');
const { resError } = require('../helper/handle');

const { whitelistedRoutes } = config;

async function jwtVerify(request) {
  await request.jwtVerify((err, decoded) => {
    if (!err) {
      request.authData = decoded;
      request.permissionAuth =
        request.permissionAuth || decoded.userId !== config.user.adminUserId;
    } else {
      resError({ message: err.message });
    }
  });
}
// 这个是由Hook执行的
exports.jwtAuth = async (request) => {
  if (!config.jwt.enable) {
    request.permissionAuth = false;
    return;
  }
  if (
    whitelistedRoutes.every((whitelistedRoute) => {
      return !request.raw.url.match(new RegExp(whitelistedRoute));
    })
  ) {
    await jwtVerify(request);
  } else {
    // 白名单 输入Token 同样验证token
    request.permissionAuth = false;
    if (
      typeof request.raw.headers.authorization !== 'undefined' &&
      request.raw.headers.authorization !== '' &&
      request.raw.headers.authorization.trim() !== 'Bearer'
    ) {
      await jwtVerify(request);
    }
  }
};
