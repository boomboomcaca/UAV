const controllers = require('../controllers');

const routes = [
  {
    method: 'POST',
    url: '/demo',
    handler: controllers.demo.login,
    defaultSchema: controllers.demo.loginSchema,
  },
  {
    method: 'GET',
    url: '/demo',
    handler: controllers.demo.user,
    defaultSchema: controllers.demo.userSchema,
  },
];

module.exports = routes;
