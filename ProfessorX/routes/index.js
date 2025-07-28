/* eslint-disable import/no-dynamic-require */
/* eslint-disable no-return-assign */
const fastifySwagger = require('fastify-swagger');
const swagger = require('../data/config/swagger');
const { jwtAuth } = require('../hook/jwtAuth');
const { licenseHttpAuth } = require('../license/licenseAuth');
const { permissionAuth } = require('../hook/permissionAuth');
const { dataPermissionAuth } = require('../hook/dataPermissionAuth');
const { paramConvert } = require('../hook/paramConvert');
const { paramPermissionAuth } = require('../hook/paramPermissionAuth');
const { corsHandler } = require('../hook/corsHandler');
const { routeFiles } = require('./routeFiles');
const { reqLogRecord, resLogRecord } = require('../hook/logRecord');
const { businessLogRecord } = require('../hook/businessLogRecord');
const { config } = require('../helper/repositoryBase');
const { isUndefinedOrNull } = require('../helper/common');
// const defaultSchema = require('../schema/defaultSchema');
// const updateDeleteResponse = {
//   200: {
//     type: 'object',
//     properties: {
//       result: { type: 'number', description: '影响行数' },
//     },
//   },
// };
// const insertResponse = {
//   200: {
//     type: 'object',
//     description: '返回结果',
//     properties: {
//       result: {
//         type: 'array',
//         description: '主键',
//         items: { type: 'string' },
//       },
//     },
//   },
// };
function getAllRoutes() {
  let tempRoutes = [];
  // 如果我在这里直接添加如何？
  // todo:可以考虑动态路由，以后基本的添加删除修改都不在需要写代码,只需要进行配置文件配置就可以了
  routeFiles.map(
    // eslint-disable-next-line global-require
    (routeFile) => (tempRoutes = tempRoutes.concat(require(`./${routeFile}`)))
  );
  // 完全可以进行动态路由组合
  config.routes = tempRoutes;
  // 所有的接口都统一添加  security: config.security,
  // todo:也可以考虑使用注解形式 然后通过反射自己加载
  // decorator 可以使用修饰器 修饰器只能用于类和类的方法，不能用于函数，因为存在函数提升 面向切面编程 拦截器
  tempRoutes.map((s) => {
    const route = s;
    const defaultSchema = {
      ...route.schema,
      security: config.security,
    };
    route.schema = defaultSchema;
    // 添加返回数据格式定义
    // todo:暂不约定返回数据格式，约定了就必须要强制
    // if (route.url.indexOf('delete') > 0) {
    //   if (!route.schema.response) {
    //     route.schema.response = updateDeleteResponse;
    //   }
    // }
    // if (route.url.indexOf('add') > 0) {
    //   if (!route.schema.response) {
    //     route.schema.response = insertResponse;
    //   }
    // }
    // update 路由自动添加json schema
    // 可以考虑动态的添加查询的POST 请求,这样GET 方法能同时支持GET 和 POST
    if (route.url.includes('update') && isUndefinedOrNull(route.schema.body)) {
      const addUrl = route.url.replace('update', 'add');
      const deleteUrl = route.url.replace('update', 'delete');
      const addListUrl = route.url.replace('update', 'addList');
      const deleteItem = tempRoutes.find((r) => r.url === deleteUrl);
      const addItem = tempRoutes.find((r) => r.url === addUrl);
      const addListItem = tempRoutes.find((r) => r.url === addListUrl);

      // 动态处理update路由默认和add一样加上主键
      if (route.url.includes('update')) {
        if (deleteItem && addItem) {
          const updateItemProperties = {
            ...deleteItem.schema.body.properties,
            ...addItem.schema.body.properties,
          };
          route.schema.body.type = 'object';
          route.schema.body.properties = updateItemProperties;
          route.schema.body.required = Object.keys(
            deleteItem.schema.body.properties
          );
          // 动态处理response
          // if (!route.schema.response) {
          //   route.schema.response = updateDeleteResponse;
          // }
        }
      }
      if (
        addListItem &&
        addItem &&
        isUndefinedOrNull(addListItem.schema.body)
      ) {
        // 设置swagger 上授权显示
        addListItem.security = config.security;
        addListItem.schema.body = {
          type: 'array',
          description: '',
          items: {
            type: 'object',
            properties: addItem.schema.body.properties,
          },
        };
      }
    }
    return route;
  });

  // 考虑考虑把批量添加给处理了？
  const getListRoute = tempRoutes.filter(
    (s) => s.url.includes('getList') && s.method === 'GET' && s.schema.post
  );

  getListRoute.forEach((r) => {
    // 这段代码修改了Get 请求 有Bug 不致命
    // 这段代码为什么修改了地址？
    // todo:解构赋值bug
    const item = {};
    item.handler = r.handler;
    item.url = r.url;
    item.security = config.security;
    item.schema = {};
    item.schema.tags = r.schema.tags;
    item.schema.description = r.schema.description;
    item.schema.summary = r.schema.summary;
    item.schema.body = {
      type: 'object',
      properties: {
        page: { description: '页数', type: 'integer' },
        rows: { description: '条数', type: 'integer' },
        sort: { description: '排序', type: 'string' },
        order: { description: '升序降序', type: 'string' },
      },
    };
    item.method = 'POST';
    // delete item.schema.query;
    tempRoutes.push(item);
  });

  return tempRoutes;
}

function regRoutes(fastify) {
  const tempRoutes = getAllRoutes();
  // 统一处理更新Schema
  // 通过req然后找到对应的schema
  // 可以考虑把所有字段的中英文名称都存起来?
  const promises = tempRoutes.map((route) => {
    // eslint-disable-next-line no-unused-vars
    return new Promise((resolve, reject) => {
      fastify.route(route);
      resolve();
    });
  });
  return Promise.all(promises);
}

// eslint-disable-next-line no-unused-vars
const routes = async (fastify, options) => {
  fastify.register(fastifySwagger, swagger.options);
  // 注册跨域处理
  fastify.addHook('onRequest', async (request, reply) => {
    await corsHandler(request, reply);
  });
  fastify.addHook('onRequest', (request, reply, next) => {
    // before jwt 不带异步方法需要next 带异步方法不需要next
    next();
  });
  // jwt 验证以及权限验证
  fastify.addHook('onRequest', async (request, reply) => {
    // 后续都统一使用Body 不在使用Query GET 请求Body 覆盖
    request.body = request.method === 'GET' ? request.query : request.body;
    await licenseHttpAuth(request, reply);
    await jwtAuth(request, reply);
    if (request.permissionAuth) {
      await permissionAuth(request);
    }
  });
  // fastify.addHook('onRequest', async (request, reply) => {
  //   // after jwt
  // });
  // 在 onRequest 钩子中，request.body 的值总是 null，这是因为 body 的解析发生在 preValidation 钩子之前。
  // eslint-disable-next-line no-unused-vars
  fastify.addHook('preHandler', async (request, reply) => {
    // 所有验证通过的请求进行日志记录
    // 方法执行前进行业务日志记录
    await businessLogRecord(request);
    await reqLogRecord(request);
    await paramConvert(request);
    if (request.permissionAuth) {
      await dataPermissionAuth(request);
      await paramPermissionAuth(request);
    }
  });
  fastify.addHook('onSend', async (request, reply, payload) => {
    await resLogRecord(request, reply, payload);
  });
  await regRoutes(fastify);
};

module.exports = {
  routes,
  getAllRoutes,
};
