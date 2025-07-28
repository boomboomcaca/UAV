const { getCurrentDate } = require('./common');
/**
 *http返回成功信息
 *@param reply  http请求回复对象
 * @param result 结果
 * @param total 分页查询特有 总条数
 * @returns
 */
exports.resSuccess = ({ reply, result, total }) => {
  if (total || total === 0) {
    return reply.send({ result, total });
  }
  if (result) {
    return reply.send({ result });
  }
  return reply.send({ result: null });
};
/**
 *http返回失败结果
 *
 * @param code 错误码
 * @param message 错误信息
 * @returns
 */
exports.resError = ({ code = 500, message = '请求失败' }) => {
  // 请求失败都是通过resError
  const data = { code, message };
  throw data;
};

/**
 *更新修改时间
 *
 * @param {*} data
 * @returns
 */
exports.updateModifyInfo = (data) => {
  // const modifyInfo = { update_time: new Date(new Date().getTime() + 28800000) };
  // return Object.assign(modifyInfo, data);
  // 判断是windows还是Linux
  return { ...data, update_time: getCurrentDate() };
};

// 到毫秒
/**
 *添加创建时间
 *
 * @param {*} data
 * @returns
 */
exports.updateAddInfo = (data) => {
  return { ...data, create_time: getCurrentDate() };
};
