// 基础操作 添加删除 修改 基于数据库二次封装
const {
  insert,
  edit,
  remove,
  getPageData,
  removeList,
} = require('../../helper/repositoryBase');
// 基础帮助方法 返回成功 失败  添加时间 更新时间赋值
const { resSuccess } = require('../../helper/repositoryBase');

// 这些其实可以考虑通过配置存到数据库！目前在代码里面写死吧！
const tableName = 'test_device';
const primaryKey = 'id';
const tableChineseName = '设备';
const uniqueKey = 'name';

exports.queryTest = async (req, reply) => {
  // get请求测试
  resSuccess({ reply, message: '查询成功' });
};
/**
 *添加对象
 *
 * @param {*} req
 * @param {*} reply
 */
exports.add = async (req, reply) => {
  insert({ req, reply, tableName, tableChineseName, primaryKey, uniqueKey });
};
/**
 *更新对象
 *
 * @param {*} req
 * @param {*} reply
 */
exports.update = async (req, reply) => {
  await edit({
    req,
    reply,
    tableName,
    tableChineseName,
    primaryKey,
    uniqueKey,
  });
};
/**
 *根据输入条件删除
 *
 * @param {*} req
 * @param {*} reply
 */
exports.del = async (req, reply) => {
  // 删除批量删除
  await remove({
    req,
    reply,
    tableName,
    tableChineseName,
    primaryKey,
    uniqueKey,
  });
};

/**
 *批量删除
 *
 * @param {*} req
 * @param {*} reply
 */
exports.delList = async (req, reply) => {
  // 删除批量删除
  await removeList({
    req,
    reply,
    tableName,
    tableChineseName,
    primaryKey,
    uniqueKey,
  });
};
/**
 *获取列表 智能获取自己被授予权限的资源
 *
 * @param {*} req
 * @param {*} reply
 */
exports.getList = async (req, reply) => {
  getPageData({
    req,
    reply,
    tableName,
    tableChineseName,
    primaryKey,
    uniqueKey,
  });
};
