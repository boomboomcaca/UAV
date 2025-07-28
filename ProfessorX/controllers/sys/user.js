/* eslint-disable camelcase */
/* istanbul ignore next */
/* eslint-disable no-unused-vars */
const md5 = require('md5-node');
const config = require('../../data/config/config');
const { cloudDbContext } = require('../../db/dbContext');

// 基础操作 添加删除 修改 基于数据库二次封装
const {
  insert,
  edit,
  remove,
  getPageData,
  removeList,
  insertList,
  getCurrentDate,
  dataTypeEnum,
} = require('../../helper/repositoryBase');
// 基础帮助方法 返回成功 失败  添加时间 更新时间赋值
const {
  resError,
  resSuccess,
  updateModifyInfo,
} = require('../../helper/repositoryBase');
// 缓存基本操作
const { getJson, setJson, deleteKey } = require('../../helper/repositoryBase');
// 数据库基本操作方法
const { sqlQuery, update, getSingle } = require('../../helper/repositoryBase');
const {
  isUndefinedOrNull,
  getRandomRangeNum,
  getDataType,
  dateFormat,
} = require('../../helper/common');
const { BusinessLogger } = require('../../helper/businessLogger');

// 这些其实可以考虑通过配置存到数据库！目前在代码里面写死吧！

const tableName = 'sys_user';
const primaryKey = 'id';
const tableChineseName = '用户';
const uniqueKey = 'account';

exports.loginSchema = {
  description: '登录',
  tags: ['sys_user'],
  summary: '登录',
  // 为了和http请求通用所有验证写到body里面
  // 默认取出body里面的属性进行jsonSchema验证
  body: {
    type: 'object',
    properties: {
      account: { type: 'string', description: '账户' },
      password: { type: 'string', description: '密码' },
    },
  },
  response: {
    200: {
      type: 'object',
      properties: {
        result: { type: 'string', description: 'token' },
      },
    },
  },
};

exports.login = async (request, reply) => {
  const newDate = getCurrentDate();
  const data = request.body;
  const wheres = {
    account: data.account,
  };
  const pwd = md5(data.password);

  // 采用链式语法 Lamda 表达式 查询 更加的优化 简单
  // 变量 查询 Bug // 变量查询Bug
  // 必须这么使用
  // 表达式的值必须要初始化传入
  // todo:查询方式优化
  // todo:并发可能会出错

  // 有可能大家都在同时对这个userContext.account 赋值
  cloudDbContext.user.account = data.account;
  // 不进行代码覆盖率检测
  // 简单方案 忽略代码覆盖率检测这一行
  // const user = await cloudDbContext.user
  //   .where((s) => s.account === data.account)
  //   .first()
  //   .toList();

  const user = await getSingle({ tableName, wheres });

  // 如果用户要注销功能?其实就是让 token失效
  // 用户登录成功后，更新token到数据库！
  if (!user || user.password !== pwd) {
    resError({ message: '用户名或者密码错误' });
  }
  const payLoad = {
    account: data.account,
    userId: user.id,
    date: newDate,
  };
  const token = await reply.jwtSign(payLoad);
  // 需要加8个小时
  const mainData = { token, last_login_time: newDate };
  // 更新token
  // 更新也直接生成sql
  await update({ tableName, mainData, wheres });
  await deleteKey(`${config.cacheKey.userPermission}${payLoad.account}`);
  // return { statusCode: 200, data: { token }, message: '用户登录成功' };
  resSuccess({ reply, result: token });
};
exports.modifyPasswordSchema = {
  description: '修改密码',
  tags: ['sys_user'],
  summary: '修改密码',
  body: {
    type: 'object',
    properties: {
      oldPassword: { type: 'string', minLength: 6, description: '旧密码' },
      newPassword: { type: 'string', minLength: 6, description: '新密码' },
    },
  },
};

exports.resetPasswordSchema = {
  description: '重置密码',
  tags: ['sys_user'],
  summary: '重置密码',
  body: {
    type: 'object',
    properties: {
      userId: { type: 'integer', description: '用户ID' },
      newPassword: { type: 'string', minLength: 6, description: '新密码' },
    },
  },
  response: {
    200: {
      type: 'object',
      properties: {
        result: {
          type: 'object',
          description: '返回结果',
          properties: {
            password: { type: 'string', description: '新密码' },
          },
        },
      },
    },
  },
};
exports.addSchema = {
  description: '添加用户',
  tags: ['sys_user'],
  summary: '添加用户',
  body: {
    type: 'object',
    properties: {
      account: {
        type: 'string',
        maxLength: 50,
        minLength: 1,
        description: '账户',
      },
      name: {
        type: 'string',
        maxLength: 50,
        minLength: 1,
        description: '姓名',
      },
      sex: { type: 'integer', description: '性别' },
      area_code: { type: 'string', description: '地区编码' },
      phone: {
        type: 'string',
        pattern: '^((13[0-9])|(14[5|7])|(15([0-3]|[5-9]))|(18[0,5-9]))\\d{8}$',
        description: '电话号码',
      },
      remark: { type: 'string', description: '描述' },
    },
  },
};

// 注销token
async function cancelToken(token) {
  let cancelTokens = await getJson(config.cacheKey.userCancelTokens);
  if (
    cancelTokens !== '' &&
    cancelTokens != null &&
    cancelTokens.tokens.length < config.user.cancelTokenLength
  ) {
    cancelTokens.tokens.push(token);
  } else {
    cancelTokens = {
      tokens: [token],
    };
  }
  setJson(config.cacheKey.userCancelTokens, cancelTokens, config.jwt.expiresIn);
}
// 必须要带token访问
exports.loginOut = async (request, reply) => {
  const token = request.raw.headers.authorization;
  await cancelToken(token);
  resSuccess({ reply });
};
// 刷新token
exports.refreshToken = async (request, reply) => {
  const newDate = getCurrentDate();
  const payLoad = {
    account: request.authData.account,
    userId: request.authData.userId,
    date: newDate,
  };
  const token = await reply.jwtSign(payLoad);
  resSuccess({ reply, result: token });
};
/**
 *修改密码
 *
 * @param {*} request
 * @param {*} reply
 */
exports.modifyPassword = async (request, reply) => {
  const data = request.body;
  const wheres = {
    id: request.authData.userId,
  };
  const user = await getSingle({ tableName, wheres });
  // 如果用户要注销功能?其实就是rang token失效
  // 用户登录成功后，更新token到数据库！
  // 登录的时候还要坚持 token是否和数据库一直
  if (!user || user.password !== md5(data.oldPassword)) {
    resError({ message: '密码错误' });
  }
  // 每次都会和缓存里面的token 进行检查
  const payLoad = {
    account: user.account,
    userId: user.id,
  };
  // 注销Token
  await cancelToken(user.token);
  const token = await reply.jwtSign(payLoad);
  // 更新修改时间
  let mainData = {
    token,
    password: md5(data.newPassword),
    last_login_time: new Date(),
  };
  mainData = updateModifyInfo(mainData);
  // 更新token
  await update({ tableName, mainData, wheres });
  // 数据加入缓存
  // setObject(data.account, payLoad);
  resSuccess({ reply, result: token });
};
// 重置密码 Post提交 userId 方法暂时不用
// exports.resetPassword1 = async (request, reply) => {
//   // 我都可以通过POST取
//   // 可以加一个Hook 拦截！ 如果是Get 方法 也可以通过Body 取数据 后面就不在需要判断是GET 还是POST
//   const data = request.body;
//   const wheres = {
//     id: data.userId,
//   };
//   const user = await getSingle({ tableName, wheres });
//   if (user == null) {
//     resError({ message: '用户不存在' });
//   }
//   await cancelToken(user.token);
//   // 密码长度最低6位
//   const newPassword = getRandomRangeNum(6);
//   // 重置密码 密码进行随机重置
//   let mainData = { password: md5(newPassword) };
//   mainData = updateModifyInfo(mainData);
//   // 更新token
//   await update({ tableName, mainData, wheres });
//   // 数据加入缓存
//   // setObject(data.account, payLoad);
//   resSuccess({
//     reply,
//     data: { password: newPassword },
//     message: '重置密码成功',
//   });
// };
// 重置密码要求输入新密码
exports.resetPassword = async (request, reply) => {
  // 我都可以通过POST取
  // 可以加一个Hook 拦截！ 如果是Get 方法 也可以通过Body 取数据 后面就不在需要判断是GET 还是POST
  const data = request.body;
  const wheres = {
    id: data.userId,
  };
  const user = await getSingle({ tableName, wheres });
  if (!user) {
    resError({ message: '用户不存在' });
  }
  await cancelToken(user.token);
  // 密码长度最低6位
  // let newPassword=getRandomRangeNum(6);
  const { newPassword } = data;
  // 重置密码 密码进行随机重置
  let mainData = { password: md5(newPassword) };
  mainData = updateModifyInfo(mainData);
  // 更新token
  await update({ tableName, mainData, wheres });
  // 数据加入缓存
  // setObject(data.account, payLoad);
  resSuccess({
    reply,
    result: { password: newPassword },
  });
};

// add
exports.add = async (req, reply) => {
  // 所有的对象直接存Body
  // req.body.password = md5(config.user.initPwd);
  await insert({
    req,
    reply,
    tableName,
    tableChineseName,
    primaryKey,
    uniqueKey,
  });
};
exports.addList = async (req, reply) => {
  const data = req.body;
  for (let i = 0; i < data.length; i++) {
    data[i].password = md5(config.user.initPwd);
  }
  await insertList({
    req,
    reply,
    tableName,
    tableChineseName,
    primaryKey,
    uniqueKey,
  });
};
/**
 *更新 默认是按照主键更新
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
// 删除
exports.del = async (req, reply) => {
  // eslint-disable-next-line eqeqeq
  if (req.body.id == config.user.adminUserId) {
    resError({ message: '超级管理员不能删除' });
  }
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
 *查询用户列表
 *
 * @param {*} req
 * @param {*} reply
 */

function getSex(sex) {
  if (isUndefinedOrNull(sex)) {
    return null;
  }
  // eslint-disable-next-line eqeqeq
  if (sex == 1) {
    return '男';
  }
  return '女';
}
/**
 *批量删除
 *
 * @param {*} req
 * @param {*} reply
 */
exports.getList = async (req, reply) => {
  // 对角色ID做一个处理
  const sql = `select c.*,d.name as role_name from 
  (select a.*,b.role_id,b.id as user_role_id from sys_user a
  left join  sys_user_role b
  on a.id=b.user_id order by a.create_time desc) as c
 left join sys_role d
  on c.role_id=d.id`;
  let users = await sqlQuery(sql);
  users = users.map((s) => {
    return {
      ...s,
      sex: getSex(s.sex),
    };
  });
  const contains = (s, keyword) => {
    if (!isUndefinedOrNull(s) && s.indexOf(keyword) >= 0) {
      return true;
    }
    return false;
  };
  const { keyword, role_id, sex } = req.body;
  if (keyword) {
    users = users.filter(
      (s) =>
        contains(s.name, keyword) ||
        contains(s.account, keyword) ||
        contains(s.remark, keyword) ||
        contains(s.sex, keyword) ||
        contains(s.role_name, keyword) ||
        contains(s.phone, keyword)
    );
  }
  if (sex) {
    users = users.filter((s) => contains(s.sex, sex));
  }
  if (role_id) {
    if (getDataType(role_id) === dataTypeEnum.array) {
      users = users.filter((s) => role_id.indexOf(s.role_id) >= 0);
    } else {
      // 忽略数字和支付比较
      // eslint-disable-next-line eqeqeq
      users = users.filter((s) => s.role_id == role_id);
    }
  }
  resSuccess({ reply, result: users, total: users.length });
};

// 获取子权限
function getChildren(parentId, modules) {
  const nodes = [];
  for (let i = modules.length - 1; i >= 0; i--) {
    const module = modules[i];
    if (parentId !== module.parent_id) {
      continue;
    }
    modules.splice(i, 1);
    nodes.push({
      id: module.id,
      name: module.name,
      children: [],
    });
  }
  Object.keys(nodes).forEach((index) => {
    const node = nodes[index];
    node.children = getChildren(node.id, modules);
  });
  return nodes;
}
// 处理权限
function handlerPermission(modules) {
  const permission = [];
  // 从后往前 减少循环次数
  for (let i = modules.length - 1; i > 0; i--) {
    const item = modules[i];
    item.children = getChildren(item.id, modules);
    permission.push(item);
  }
  return permission;
}

// 获取用户需要token
// 获取用户权限
// 但是这个没有角色 名称
// 这个接口不能是白名单 白名单就不通过后面的权限
exports.getUserPermission = async (req, reply) => {
  // 用户账户 直接从缓存里面取出权限
  // 简单一点添加角色名称 和用户名称
  const userAccount = req.authData.account;

  let roleName = '';

  let userName = '';

  let sql = '';
  // 获取用户角色
  let permissionModule = [];
  if (req.authData.userId === config.user.adminUserId) {
    sql = `		 
      select b.id,b.name,b.url,b.parent_id  from sys_permission_module b where b.id>1`;
    permissionModule = await sqlQuery(sql);

    // 渲染成2为数组
    roleName = '超级管理员';
    userName = '管理员';
  } else {
    const cacheKey = `${config.cacheKey.userPermission}${userAccount}`;
    permissionModule = await getJson(cacheKey).permissionModule;
    sql = `select c.account,d.name as roleName,c.name,c.id from 
    (select a.*,b.role_id from sys_user a
    inner join  sys_user_role b
    on a.id=b.user_id) as c
    inner join sys_role d
    on c.role_id=d.id and c.id=${userAccount}`;
    const userInfo = await sqlQuery(sql);
    if (userInfo) {
      roleName = userInfo.roleName || '无';
      userName = userInfo.userName || '无';
    }
  }
  permissionModule = handlerPermission(permissionModule);
  permissionModule = permissionModule.map((s) => {
    return {
      name: s.name,
      children: s.children,
      id: s.id,
    };
  });
  // 返回用户的权限信息
  resSuccess({
    reply,
    result: {
      permission: permissionModule,
      roleName,
      userName,
    },
  });
};
