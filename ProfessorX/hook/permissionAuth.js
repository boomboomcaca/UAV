const { isUndefinedOrNull } = require('../helper/common');
const {
  resError,
  config,
  getJson,
  setJson,
  sqlQuery,
} = require('../helper/repositoryBase');

// map():返回一个新的Array，每个元素为调用func的结果。新数组的长度和原来的是一样的，他只不过是逐一对原来数据里的每个元素进行操作。

// filter():返回一个符合func条件的元素数组。筛选条件，把数组符合条件的放在新的数组里面返回。新数组和原来的数组长度不一定一样。

// some():返回一个boolean，判断是否有元素是否符合func条件。数组里面所有的元素有一个符合条件就返回true。

// every():返回一个boolean，判断每个元素是否符合func条件。数组里面所有的元素都符合才返回true。

// forEach():没有返回值，只是针对每个元素调用func   。循环数组。和for的用法一样的。

// 获取父角色
function getParentRoleId(roleId, allRole) {
  // todo:可以用Find
  const roles = allRole.filter((s) => {
    return s.id === roleId;
  });
  return roles.length > 0 ? roles[0].parentId : 0;
}
// 通过递归获取用户角色
function getParentRoles(userRole, allRole, resultRole) {
  const parentRole = [];
  for (let i = 0; i < userRole.length; i++) {
    const parentId = getParentRoleId(userRole[i].role_id, allRole);
    resultRole.push(userRole[i].role_id);
    if (parentId !== 0) {
      parentRole.push({ role_id: parentId });
      resultRole.push(parentId);
    }
  }
  if (parentRole.length > 0) {
    return getParentRoles(parentRole, allRole, resultRole);
  }
  return resultRole;
}
// 权限检查
function permissionCheck(urlPermission, req) {
  let success = false;
  for (let i = 0; i < urlPermission.length; i++) {
    if (req.raw.url.match(new RegExp([urlPermission[i].url]))) {
      success = true;
      req.urlPermission = urlPermission[i];
    }
  }
  if (!success) {
    resError({ message: '权限验证不通过' });
  }
  return success;
}
// token过期检查
async function tokenCancelCheck(req) {
  // 获取Json对象
  const cacheTokens = await getJson(config.cacheKey.userCancelTokens);
  if (cacheTokens != null) {
    const token = req.raw.headers.authorization;
    const cancel = cacheTokens.tokens.some((item) => {
      return item === token;
    });
    if (cancel) {
      resError({ statusCode: 500, message: 'token无效' });
    }
  }
}
// 获取用户所有角色
async function getUserAllRoles(userId) {
  // 这个没有考虑到角色继承
  let sql = `select b.role_id,b.object_id,a.object_name from sys_role a
     inner join 
     (select role_id,object_id from sys_user_role
          where user_id=${userId}
          union
          select  a.role_id,a.object_id from sys_role_group a inner join  sys_user_group b
          on a.group_id=b.group_id
          where  b.user_id=${userId}) as b
      on a.id=b.role_id`;
  let res = await sqlQuery(sql);
  if (res == null) {
    resError({ message: '未授权' });
  }
  const role = res;
  sql = 'select id,parent_id as parentId from sys_role';
  res = await sqlQuery(sql);
  const allRole = res.map((item) => {
    return { roleId: item.id, parentId: item.parentId };
  });
  // console.log('allRole')
  // console.log(allRole)
  // 用户的所有角色
  let userAllRoles = [];
  userAllRoles = getParentRoles(role, allRole, userAllRoles);
  // 去重
  userAllRoles = userAllRoles.uniq();
  return userAllRoles;
}
function getRolesWhereInSql(userAllRoles) {
  let whereInSql = '(';
  for (let i = 0; i < userAllRoles.length; i++) {
    whereInSql += `'${userAllRoles[i]}',`;
  }
  whereInSql = `${whereInSql.substring(0, whereInSql.length - 1)})`;
  return whereInSql;
}
// function getPermissionWhereInSql(userAllPermissions) {
//   let { permissionIds } = userAllPermissions;
//   // 判断第一位是否是逗号
//   if (permissionIds.substring(0, 1) === ',') {
//     permissionIds = permissionIds.substring(1, permissionIds.length - 1);
//   }
//   return `(${permissionIds})`;
//   // 移除第一位逗号
// }
// 获取角色数据权限
async function getRolesDataPermission(userAllRoles) {
  const whereInSql = getRolesWhereInSql(userAllRoles);
  const sql = ` select a.object_name,b.object_id from sys_role a
     inner join sys_user_role b
     on a.id=b.role_id
     and a.id in ${whereInSql}
     union
     select a.object_name,b.object_id from sys_role a
     inner join sys_role_group b
     on a.id=b.role_id
     and a.id in ${whereInSql} `;
  // let dataPermission = [];
  const res = await sqlQuery(sql);
  const dataPermission = res.map((item) => {
    return { object_name: item.object_name, object_id: item.object_id };
  });
  return dataPermission;
}
async function getDefaultPermission() {
  let sql = `select  b.id,b.name,b.url,b.parent_id as parentId,b.permission_ids as permissionIds from sys_permission_module  b
  where id=1`;
  const permissionModule = await sqlQuery(sql);
  const permissionIds = permissionModule[0].permissionIds.split(',');
  // 如果是多条
  sql = `
 select url,table_name as tableName,params from sys_permission
 where id in (${permissionIds.toString()})`;
  const res = await sqlQuery(sql);
  // let urlPermission = [];
  const urlPermission = res.map((item) => {
    return { url: item.url, tableName: item.tableName, params: item.params };
  });
  return urlPermission;
}
// linux
// 获取角色权限
async function getRolesPermission(userAllRoles) {
  const whereInSql = getRolesWhereInSql(userAllRoles);
  let sql = `		 
     select b.id,b.name,b.url,b.parent_id as parentId,b.permission_ids as permissionIds  from sys_role_permission a
inner join  sys_permission_module b
on  a.permission_module_id=b.id and  a.role_id in ${whereInSql}`;
  const permissionModule = await sqlQuery(sql);
  // 添加默认权限
  if (!permissionModule) {
    resError({ message: '未授权' });
  }
  let permissionIds = [];
  // todo:map 测试下
  permissionModule
    .filter((p) => !isUndefinedOrNull(p.permissionIds))
    .forEach((s) => {
      permissionIds = permissionIds.concat(s.permissionIds.split(','));
    });
  if (permissionIds.length === 0) {
    resError({ message: '未授权' });
  }
  // 如果是多条
  sql = `
   select url,table_name as tableName,params from sys_permission
   where id in (${permissionIds.toString()})`;
  const res = await sqlQuery(sql);
  // let urlPermission = [];
  const urlPermission = res.map((item) => {
    return { url: item.url, tableName: item.tableName, params: item.params };
  });
  return { urlPermission, permissionModule };
}
async function checkCachePermission(req, cacheKey) {
  const cachePermission = await getJson(cacheKey);
  if (cachePermission != null) {
    // console.log(cachePermission);
    permissionCheck(cachePermission.urlPermission, req);
    // console.log("缓存权限验证成功" + req.raw.url)
    req.dataPermission = cachePermission.dataPermission;
    return true;
  }
  return false;
}
async function checkDefaultPermission(req) {
  const urlPermission = await getDefaultPermission();
  return permissionCheck(urlPermission, req);
}

// 这个是由Hook验证完成jwt之后执行
// 中间件里面暂时无法读取出 jwt解密之后的信息！ 如果可以读取可以把这段代码移动到中间件执行
// 同样要加白名单过滤
// 缓存权限的时候 同时缓存当前用户token
async function permissionAuth(req) {
  try {
    // 简单的权限管理 // 只有管理员有权访问
    // 用户管理、角色管理
    // 管理员其他用户不进行授权
    const { authData } = req;
    const { userId } = authData;
    await tokenCancelCheck(req);
    const cacheKey = `${config.cacheKey.userPermission}${authData.account}`;
    let canAccess = await checkDefaultPermission(req);
    // 默认权限不进行后续检查
    if (canAccess) {
      req.dataPermission = [];
      return;
    }
    // 从缓存读取数据 JSON.parse
    canAccess = await checkCachePermission(req, cacheKey);
    if (canAccess) {
      return;
    }
    const userAllRoles = await getUserAllRoles(userId);
    if (userAllRoles.length === 0) {
      resError({ message: '未授权1' });
    }
    const dataPermission = await getRolesDataPermission(userAllRoles);
    const rolesPermission = await getRolesPermission(userAllRoles);
    // 用户拥有的权限模块写入缓存
    const userPermission = {
      urlPermission: rolesPermission.urlPermission,
      dataPermission,
      permissionModule: rolesPermission.permissionModule,
    };
    await setJson(cacheKey, userPermission, config.jwt.expiresIn);
    permissionCheck(userPermission.urlPermission, req);
    req.dataPermission = dataPermission;
  } catch (error) {
    resError({ message: error.message });
  }
}
module.exports = {
  getParentRoles,
  permissionAuth,
};
