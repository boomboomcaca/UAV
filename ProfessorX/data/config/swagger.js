const config = require('./config');

exports.options = {
  routePrefix: '/doc',
  exposeRoute: true,
  swagger: {
    info: {
      title: 'Fastify API',
      description:
        'Building a blazing fast REST API with Node.js, MongoDB, Fastify and Swagger',
      version: '1.0.0',
    },
    externalDocs: {
      url: 'https://swagger.io',
      description: 'Find more info here',
    },
    host: `${config.domainName}:${config.port}`,
    schemes: ['http', 'https'],
    consumes: ['application/json'],
    produces: ['application/json', 'application/xml'],
    securityDefinitions: {
      authorization: {
        description:
          'JWT授权token前面需要加上字段Bearer与一个空格,如Bearer token',
        type: 'apiKey', // 不能修改报错 源码看不懂
        name: 'authorization',
        in: 'header',
        bearerFormat: 'JWT',
        scheme: 'Bearer',
      },
    },

    basedir: __dirname, // app absolute path
    files: ['./controller*.js'], // Path to the API handle folder
  },
};
