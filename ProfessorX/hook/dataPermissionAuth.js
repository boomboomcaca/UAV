/* eslint-disable eqeqeq */
/* eslint-disable no-param-reassign */
const {
  resError,
  sqlQuery,
  config,
  getDataType,
  dataTypeEnum,
  isUndefinedOrNull,
} = require('../helper/repositoryBase');

// map():返回一个新的Array，每个元素为调用func的结果。新数组的长度和原来的是一样的，他只不过是逐一对原来数据里的每个元素进行操作。

// filter():返回一个符合func条件的元素数组。筛选条件，把数组符合条件的放在新的数组里面返回。新数组和原来的数组长度不一定一样。

// some():返回一个boolean，判断是否有元素是否符合func条件。数组里面所有的元素有一个符合条件就返回true。

// every():返回一个boolean，判断每个元素是否符合func条件。数组里面所有的元素都符合才返回true。

// forEach():没有返回值，只是针对每个元素调用func   。循环数组。和for的用法一样的。

// 范围涉及，连续范围，离散范围。(连续范围要考虑边缘问题，大于小于或者小于等于)

function checkCreateUserAccess(req) {
  if (
    config.permissionRole.createUserAccess.every((whitelistedRoute) => {
      return !req.raw.url.match(new RegExp(whitelistedRoute));
    })
  ) {
    return false;
  }
  return true;
}
// 检查对象访问权限
async function checkObjectParamsAccess(
  req,
  reqData,
  dataPermission,
  tableName,
  createUserAccess
) {
  let res = dataPermission.some((x) => {
    return x.object_id == reqData.id;
  });
  if (res === false && createUserAccess === false) {
    resError({ message: '没有权限操作该条数据' });
  }
  // 表示创建人可以访问
  else if (res == false && createUserAccess == true) {
    // 我需要去查询这条数据的Create_id
    const sql = `select create_id from ${tableName} where id=${reqData.id}`;
    res = await sqlQuery(sql);
    if (res == null || res[0].create_id != req.user.user_id) {
      resError({ message: `没有权限操作该条数据${req.raw.url}` });
    }
  }
}
// 检查关联对象参数权限
async function checkAssociatedObjectParamsAccess(
  req,
  reqData,
  dataPermission,
  tableName,
  createUserAccess
) {
  let isContainObjectField = false;
  // eslint-disable-next-line no-restricted-syntax
  for (const p in reqData) {
    if (
      getDataType(reqData[p]) == dataTypeEnum.object ||
      getDataType(reqData[p]) == dataTypeEnum.array
    ) {
      continue;
    }
    // 如果p的名称等于对象名称加 id
    // 可以考虑数组也不进行验证
    let res = dataPermission.filter((x) => {
      return p == `${x.object_name}_id`;
    });
    if (res.length >= 1) {
      isContainObjectField = true;
      res = res.some((x) => {
        // 不需要严格等于 数字字符串都认
        // eslint-disable-next-line eqeqeq
        return x.object_id == reqData[p];
      });
      // 如果Project_id的值没有在允许的值中
      if (res == false && createUserAccess == false) {
        resError({ message: '没有权限操作该条数据' });
      } else if (res == false && createUserAccess == true) {
        // 我需要去查询这条数据的create_id
        const sql = `select create_id from ${tableName} where ${p}=${reqData[p]}`;
        res = await sqlQuery(sql);
        if (res == null || res[0].create_id != req.user.user_id) {
          resError({ message: `没有权限操作该条数据${req.raw.url}` });
        }
      }
    }
    // 检查p的名称是否在可以操作的
  }
  return isContainObjectField;
}
// 检查关联对象数据库数据访问权限
async function checkAssociatedObjectDbDataAccess(
  req,
  reqData,
  dataPermission,
  tableName,
  createUserAccess
) {
  // 我是否可以操作Project_id为5 的数据？
  // 如果没有id 则不进行检查
  // 没有输入id则不进行管理对象检查
  if (isUndefinedOrNull(reqData.id)) {
    return;
  }
  const sql = `select * from ${tableName} where id=${reqData.id}`;
  let res = await sqlQuery(sql);
  // 数据库里面没有该条数据
  if (res == null) {
    return;
  }
  const [data] = res;
  // eslint-disable-next-line no-restricted-syntax
  for (const p in data) {
    if (data[p] !== null && getDataType(data[p]) === dataTypeEnum.object) {
      continue;
    }
    res = dataPermission.filter((x) => {
      return p === `${x.object_name}_id`;
    });
    // 如果检查到这个字段就要去判断这个字段的值
    if (res.length >= 1) {
      res = res.some((x) => {
        return x.object_id == data[p];
      });
      if (res === false && createUserAccess === false) {
        resError({ message: '没有权限操作该条数据' });
      }
      if (
        res === false &&
        createUserAccess === true &&
        data.create_id != req.user.user_id
      ) {
        resError({ message: `没有权限操作该条数据${req.raw.url}` });
      }
    }
  }
}
// 检查是否是对象本身
function checkIsObjectItself(dataPermission, tableName) {
  // Is the object itself
  // 检查 project 和 test_project 相等
  // 何必这么复杂 我直接拆除下划线//我直接移除前缀
  // tableName 移除前缀
  for (let i = 0; i <= config.tablePrefix.length; i++) {
    if (tableName.includes(config.tablePrefix[i])) {
      tableName = tableName.replace(config.tablePrefix[i], '');
    }
  }
  if (dataPermission.find((item) => item.object_name === tableName)) {
    return true;
  }
  return false;
}
// 是否检查数据权限
function checkDataPermission(req, dataPermission) {
  // 如果不包含id 数据权限验证
  if (
    isUndefinedOrNull(req.body) ||
    isUndefinedOrNull(req.urlPermission.tableName)
  ) {
    return false;
  }
  if (dataPermission.find((s) => s.object_name === '*')) {
    return false;
  }
  return true;
}

exports.dataPermissionAuth = async (req) => {
  const { dataPermission } = req;
  const isCheckDataPermission = checkDataPermission(req, dataPermission);
  if (!isCheckDataPermission) {
    // consoleLogger.trace("用户不需要数据权限验证")
    return;
  }
  const { tableName } = req.urlPermission;
  // 其实我同样要看
  // console.log("用户需要数据权限验证");
  const reqData = req.body;
  // 判断该Url是否是允许自己访问
  const createUserAccess = checkCreateUserAccess(req);
  // todo:可能有Bug
  const isObjectItSelf = checkIsObjectItself(dataPermission, tableName);
  // 检查对象访问权限
  if (isObjectItSelf) {
    await checkObjectParamsAccess(
      req,
      reqData,
      dataPermission,
      tableName,
      createUserAccess
    );
  }
  // 检查关联对象访问权限
  else {
    const canAccess = await checkAssociatedObjectParamsAccess(
      req,
      reqData,
      dataPermission,
      tableName,
      createUserAccess
    );
    if (!canAccess) {
      await checkAssociatedObjectDbDataAccess(
        req,
        reqData,
        dataPermission,
        tableName,
        createUserAccess
      );
    }
  }
};
