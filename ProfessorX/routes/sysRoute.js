const controller = require('../controllers');
const defaultSchema = require('../schema/defaultSchema');

// 可以考虑把所有路由从数据库读取
// 找个地方定义出所有的表
const routes = [
  // #region 用户
  {
    method: 'POST',
    url: '/User/login',
    handler: controller.sys_user.login,
    schema: controller.sys_user.loginSchema,
  },
  {
    method: 'POST',
    url: '/User/loginOut',
    handler: controller.sys_user.loginOut,
    schema: defaultSchema.getDefaultSchema({
      description: '注销用户',
      tags: 'sys_user',
    }),
  },
  {
    method: 'POST',
    url: '/User/refreshToken',
    handler: controller.sys_user.refreshToken,
    schema: defaultSchema.getDefaultSchema({
      description: '刷新token',
      tags: 'sys_user',
    }),
  },
  {
    method: 'POST',
    url: '/User/modifyPassword',
    handler: controller.sys_user.modifyPassword,
    schema: controller.sys_user.modifyPasswordSchema,
  },
  {
    method: 'POST',
    url: '/User/resetPassword',
    handler: controller.sys_user.resetPassword,
    schema: controller.sys_user.resetPasswordSchema,
  },
  {
    method: 'POST',
    url: '/User/add',
    handler: controller.sys_user.add,
    schema: controller.sys_user.addSchema,
  },
  {
    method: 'POST',
    url: '/User/addList',
    handler: controller.sys_user.addList,
    schema: defaultSchema.getDefaultSchema({
      description: '批量添加用户',
      tags: 'sys_user',
    }),
  },
  {
    method: 'POST',
    url: '/User/update',
    handler: controller.sys_user.update,
    schema: defaultSchema.getUpdateSchema({
      description: '修改用户',
      tags: 'sys_user',
    }),
  },
  {
    method: 'POST',
    url: '/User/delete',
    handler: controller.sys_user.del,
    schema: defaultSchema.getDeleteSchema({
      description: '删除用户',
      tags: 'sys_user',
    }),
  },
  {
    method: 'POST',
    url: '/User/delList',
    handler: controller.sys_user.delList,
    schema: defaultSchema.getDeleteListSchema({
      description: '批量删除用户',
      tags: 'sys_user',
    }),
  },
  {
    method: 'GET',
    url: '/User/getUserPermission',
    handler: controller.sys_user.getUserPermission,
    schema: defaultSchema.getDefaultSchema({
      description: '获取用户权限',
      tags: 'sys_user',
    }),
  },
  // GET POST 都是同时支持可以任意选择
  // {
  //   method: 'POST',
  //   url: '/User/getList',
  //   handler: controller.sys_user.getList,
  //   // 可以考虑动态生成一些
  //   schema: defaultSchema.getPagePostDataSchema({
  //     description: '获取用户分页数据',
  //     tags: 'sys_user',
  //   }),
  // },
  // 其实可以考虑动态路由 但是方法不好处理？
  // 可以考虑动态路由，直接使用require
  {
    method: 'GET',
    url: '/User/getList',
    // require('./sys/user')
    handler: controller.sys_user.getList,
    // 可以考虑动态生成一些
    schema: defaultSchema.getPageDataSchema({
      description: '获取用户分页数据',
      tags: 'sys_user',
    }),
  },
  // #endregion

  // #region 权限
  {
    method: 'POST',
    url: '/Permission/add',
    handler: controller.sys_permission.add,
    schema: controller.sys_permission.addSchema,
  },
  {
    method: 'POST',
    url: '/Permission/addList',
    handler: controller.sys_permission.addList,
    schema: defaultSchema.getDefaultSchema({
      description: '批量添加权限',
      tags: 'sys_permission',
    }),
  },
  {
    method: 'POST',
    url: '/Permission/update',
    handler: controller.sys_permission.update,
    schema: defaultSchema.getUpdateSchema({
      description: '修改权限',
      tags: 'sys_permission',
    }),
  },
  {
    method: 'POST',
    url: '/Permission/delete',
    handler: controller.sys_permission.del,
    schema: defaultSchema.getDeleteSchema({
      description: '删除权限',
      tags: 'sys_permission',
    }),
  },
  {
    method: 'POST',
    url: '/Permission/delList',
    handler: controller.sys_permission.delList,
    schema: defaultSchema.getDeleteListSchema({
      description: '批量删除权限',
      tags: 'sys_permission',
    }),
  },
  {
    method: 'GET',
    url: '/Permission/getList',
    handler: controller.sys_permission.getList,
    // 可以考虑动态生成一些
    schema: defaultSchema.getPageDataSchema({
      description: '获取权限分页数据',
      tags: 'sys_permission',
    }),
  },
  // #endregion

  // #region 权限模块
  {
    method: 'POST',
    url: '/PermissionModule/add',
    handler: controller.sys_permission_module.add,
    schema: controller.sys_permission_module.addSchema,
  },
  {
    method: 'POST',
    url: '/PermissionModule/addList',
    handler: controller.sys_permission_module.addList,
    schema: defaultSchema.getDefaultSchema({
      description: '批量添加权限模块',
      tags: 'sys_permission_module',
    }),
  },
  {
    method: 'POST',
    url: '/PermissionModule/update',
    handler: controller.sys_permission_module.update,
    schema: defaultSchema.getUpdateSchema({
      description: '修改权限模块',
      tags: 'sys_permission_module',
    }),
  },
  {
    method: 'POST',
    url: '/PermissionModule/delete',
    handler: controller.sys_permission_module.del,
    schema: defaultSchema.getDeleteSchema({
      description: '删除权限模块',
      tags: 'sys_permission_module',
    }),
  },
  {
    method: 'POST',
    url: '/PermissionModule/delList',
    handler: controller.sys_permission_module.delList,
    schema: defaultSchema.getDeleteListSchema({
      description: '批量删除权限模块',
      tags: 'sys_permission_module',
    }),
  },
  {
    method: 'GET',
    url: '/PermissionModule/getList',
    handler: controller.sys_permission_module.getList,
    // 可以考虑动态生成一些
    schema: defaultSchema.getPageDataSchema({
      description: '获取权限模块分页数据',
      tags: 'sys_permission_module',
    }),
  },
  // #endregion

  // #region 角色
  {
    method: 'POST',
    url: '/Role/add',
    handler: controller.sys_role.add,
    schema: controller.sys_role.addSchema,
  },
  {
    method: 'POST',
    url: '/Role/addList',
    handler: controller.sys_role.addList,
    schema: defaultSchema.getDefaultSchema({
      description: '批量添加角色',
      tags: 'sys_role',
    }),
  },
  {
    method: 'POST',
    url: '/Role/update',
    handler: controller.sys_role.update,
    schema: defaultSchema.getUpdateSchema({
      description: '修改角色权限',
      tags: 'sys_role',
    }),
  },

  {
    method: 'POST',
    url: '/Role/delete',
    handler: controller.sys_role.del,
    schema: defaultSchema.getDeleteSchema({
      description: '删除角色权限',
      tags: 'sys_role',
    }),
  },
  {
    method: 'POST',
    url: '/Role/delList',
    handler: controller.sys_role.delList,
    schema: defaultSchema.getDeleteListSchema({
      description: '批量删除角色',
      tags: 'sys_role',
    }),
  },
  {
    method: 'GET',
    url: '/Role/getList',
    handler: controller.sys_role.getList,
    // 可以考虑动态生成一些
    schema: defaultSchema.getPageDataSchema({
      description: '获取分页数据',
      tags: 'sys_role',
    }),
  },
  // #endregion

  // #region 用户角色关系
  {
    method: 'POST',
    url: '/UserRole/add',
    handler: controller.sys_user_role.add,
    schema: controller.sys_user_role.addSchema,
  },
  {
    method: 'POST',
    url: '/UserRole/addList',
    handler: controller.sys_user_role.addList,
    schema: defaultSchema.getDefaultSchema({
      description: '用户角色',
      tags: 'sys_user_role',
    }),
  },
  {
    method: 'POST',
    url: '/UserRole/update',
    handler: controller.sys_user_role.update,
    schema: defaultSchema.getUpdateSchema({
      description: '修改用户角色关系',
      tags: 'sys_user_role',
    }),
  },
  {
    method: 'POST',
    url: '/UserRole/delete',
    handler: controller.sys_user_role.del,
    schema: defaultSchema.getDeleteSchema({
      description: '修改用户角色关系',
      tags: 'sys_user_role',
    }),
  },
  {
    method: 'POST',
    url: '/UserRole/delList',
    handler: controller.sys_user_role.delList,
    schema: defaultSchema.getDeleteListSchema({
      description: '批量删除用户角色关系',
      tags: 'sys_user_role',
    }),
  },
  {
    method: 'GET',
    url: '/UserRole/getList',
    handler: controller.sys_user_role.getList,
    // 可以考虑动态生成一些
    schema: defaultSchema.getPageDataSchema({
      description: '获取用户角色关系分页数据',
      tags: 'sys_user_role',
    }),
  },
  // #endregion

  // #region 角色权限
  {
    method: 'POST',
    url: '/RolePermission/add',
    handler: controller.sys_role_permission.add,
    schema: controller.sys_role_permission.addSchema,
  },
  {
    method: 'POST',
    url: '/RolePermission/addList',
    handler: controller.sys_role_permission.addList,
    schema: defaultSchema.getDefaultSchema({
      description: '批量添加角色权限',
      tags: 'sys_role_permission',
    }),
  },
  {
    method: 'POST',
    url: '/RolePermission/delList',
    handler: controller.sys_role_permission.delList,
    schema: defaultSchema.getDeleteListSchema({
      description: '批量删除角色权限',
      tags: 'sys_role_permission',
    }),
  },
  {
    method: 'POST',
    url: '/RolePermission/update',
    handler: controller.sys_role_permission.update,
    schema: defaultSchema.getUpdateSchema({
      description: '修改角色权限关系',
      tags: 'sys_role_permission',
    }),
  },
  {
    method: 'POST',
    url: '/RolePermission/delete',
    handler: controller.sys_role_permission.del,
    schema: defaultSchema.getDeleteSchema({
      description: '删除角色权限关系',
      tags: 'sys_role_permission',
    }),
  },
  {
    method: 'GET',
    url: '/RolePermission/getList',
    handler: controller.sys_role_permission.getList,
    // 可以考虑动态生成一些
    schema: defaultSchema.getPageDataSchema({
      description: '获取分页数据',
      tags: 'sys_role_permission',
    }),
  },
  {
    method: 'GET',
    url: '/RolePermission/getRolePermission',
    handler: controller.sys_role_permission.getRolePermission,
    // 可以考虑动态生成一些
    schema: controller.sys_role_permission.getRolePermissionSchema,
  },

  // #endregion

  // #region 用户组
  {
    method: 'POST',
    url: '/Group/add',
    handler: controller.sys_group.add,
    schema: controller.sys_group.addSchema,
  },
  {
    method: 'POST',
    url: '/Group/addList',
    handler: controller.sys_group.addList,
    schema: defaultSchema.getDefaultSchema({
      description: '批量添加用户组',
      tags: 'sys_group',
    }),
  },
  {
    method: 'POST',
    url: '/Group/update',
    handler: controller.sys_group.update,
    schema: defaultSchema.getUpdateSchema({
      description: '修改用户组',
      tags: 'sys_group',
    }),
  },
  {
    method: 'POST',
    url: '/Group/delete',
    handler: controller.sys_group.del,
    schema: defaultSchema.getDeleteSchema({
      description: '删除用户组',
      tags: 'sys_group',
    }),
  },
  {
    method: 'POST',
    url: '/Group/delList',
    handler: controller.sys_group.delList,
    schema: defaultSchema.getDeleteListSchema({
      description: '批量删除用户组',
      tags: 'sys_group',
    }),
  },
  {
    method: 'GET',
    url: '/Group/getList',
    handler: controller.sys_group.getList,
    // 可以考虑动态生成一些
    schema: defaultSchema.getPageDataSchema({
      description: '获取分页数据',
      tags: 'sys_group',
    }),
  },
  // #endregion

  // #region 用户用户组关系
  {
    method: 'POST',
    url: '/UserGroup/add',
    handler: controller.sys_user_group.add,
    schema: controller.sys_user_group.addSchema,
  },
  {
    method: 'POST',
    url: '/UserGroup/addList',
    handler: controller.sys_user_group.addList,
    schema: defaultSchema.getDefaultSchema({
      description: '批量添加用户和用户组关系',
      tags: 'sys_user_group',
    }),
  },
  {
    method: 'POST',
    url: '/UserGroup/update',
    handler: controller.sys_user_group.update,
    schema: defaultSchema.getUpdateSchema({
      description: '修改用户和用户组关系',
      tags: 'sys_user_group',
    }),
  },
  {
    method: 'POST',
    url: '/UserGroup/delete',
    handler: controller.sys_user_group.del,
    schema: defaultSchema.getDeleteSchema({
      description: '删除用户和用户组关系',
      tags: 'sys_user_group',
    }),
  },
  {
    method: 'POST',
    url: '/UserGroup/delList',
    handler: controller.sys_user_group.delList,
    schema: defaultSchema.getDeleteListSchema({
      description: '批量删除用户和用户组关系',
      tags: 'sys_user_group',
    }),
  },
  {
    method: 'GET',
    url: '/UserGroup/getList',
    handler: controller.sys_user_group.getList,
    // 可以考虑动态生成一些
    schema: defaultSchema.getPageDataSchema({
      description: '获取分页数据',
      tags: 'sys_user_group',
    }),
  },
  // #endregion

  // #region 用户组角色
  {
    method: 'POST',
    url: '/RoleGroup/add',
    handler: controller.sys_role_group.add,
    schema: controller.sys_role_group.addSchema,
  },
  {
    method: 'POST',
    url: '/RoleGroup/addList',
    handler: controller.sys_role_group.addList,
    schema: defaultSchema.getDefaultSchema({
      description: '用户组角色关系',
      tags: 'sys_role_group',
    }),
  },
  {
    method: 'POST',
    url: '/RoleGroup/update',
    handler: controller.sys_role_group.update,
    schema: defaultSchema.getUpdateSchema({
      description: '修改用户组角色关系',
      tags: 'sys_role_group',
    }),
  },
  {
    method: 'POST',
    url: '/RoleGroup/delete',
    handler: controller.sys_role_group.del,
    schema: defaultSchema.getDeleteSchema({
      description: '添加用户组角色关系',
      tags: 'sys_role_group',
    }),
  },
  {
    method: 'POST',
    url: '/RoleGroup/delList',
    handler: controller.sys_role_group.delList,
    schema: defaultSchema.getDeleteListSchema({
      description: '添加用户组角色关系',
      tags: 'sys_role_group',
    }),
  },
  {
    method: 'GET',
    url: '/RoleGroup/getList',
    handler: controller.sys_role_group.getList,
    // 可以考虑动态生成一些
    schema: defaultSchema.getPageDataSchema({
      description: '获取分页数据',
      tags: 'sys_role_group',
    }),
  },
  // #endregion

  // #region 字典
  {
    method: 'POST',
    url: '/Dictionary/add',
    handler: controller.sys_dic.add,
    schema: controller.sys_dic.addSchema,
  },
  {
    method: 'POST',
    url: '/Dictionary/update',
    handler: controller.sys_dic.update,
    schema: defaultSchema.getUpdateSchema({
      description: '更新字典',
      tags: 'sys_dictionary',
    }),
  },
  {
    method: 'POST',
    url: '/Dictionary/delete',
    handler: controller.sys_dic.del,
    schema: defaultSchema.getDeleteSchema({
      description: '添加字典',
      tags: 'sys_dictionary',
    }),
  },
  {
    method: 'POST',
    url: '/Dictionary/delList',
    handler: controller.sys_dic.delList,
    schema: defaultSchema.getDeleteListSchema({
      description: '批量删除字典',
      tags: 'sys_dictionary',
    }),
  },
  {
    method: 'POST',
    url: '/Dictionary/getdic',
    handler: controller.sys_dic.getDic,
    schema: controller.sys_dic.getDicSchema,
  },
  {
    method: 'GET',
    url: '/Dictionary/getList',
    handler: controller.sys_dic.getList,
    schema: defaultSchema.getPageDataSchema({
      description: '获取分页数据',
      tags: 'sys_dictionary',
    }),
  },
  // GET POST 方法暂无做兼容处理
  // {
  //   method: 'GET',
  //   url: '/Dictionary/getdic',
  //   handler: controller.sys_dic.getDic,
  //   schema: controller.sys_dic.getDicSchema,
  // },
  // #endregion

  // #region 字典明细
  {
    method: 'POST',
    url: '/DictionaryList/add',
    handler: controller.sys_dic_list.add,
    schema: controller.sys_dic_list.addSchema,
  },
  {
    method: 'POST',
    url: '/DictionaryList/update',
    handler: controller.sys_dic_list.update,
    schema: defaultSchema.getUpdateSchema({
      description: '更新字典明细',
      tags: 'sys_dictionarylist',
    }),
  },
  {
    method: 'POST',
    url: '/DictionaryList/delete',
    handler: controller.sys_dic_list.del,
    schema: defaultSchema.getDeleteSchema({
      description: '删除字典明细',
      tags: 'sys_dictionarylist',
    }),
  },
  {
    method: 'POST',
    url: '/DictionaryList/delList',
    handler: controller.sys_dic_list.delList,
    schema: defaultSchema.getDeleteListSchema({
      description: '批量删除字典明细',
      tags: 'sys_dictionarylist',
    }),
  },
  {
    method: 'GET',
    url: '/DictionaryList/getList',
    handler: controller.sys_dic_list.getList,
    schema: defaultSchema.getPageDataSchema({
      description: '获取分页数据',
      tags: 'sys_dictionarylist',
    }),
  },
  {
    method: 'POST',
    url: '/DictionaryList/addList',
    handler: controller.sys_dic_list.addList,
    schema: defaultSchema.getDefaultSchema({
      description: '批量添加字典列表',
      tags: 'sys_dictionarylist',
    }),
  },
  // #endregion
];
module.exports = routes;
