// 数据库上下文

const Context = require('./expressionVisitor');

// 使用New
const deviceContext = new Context('rmbt_device');
const userContext = new Context('sys_user');

function CloudDbContext() {
  this.user = userContext;
  this.device = deviceContext;
}

// todo:可以加一个初始化 保持一个全局单利
const cloudDbContext = new CloudDbContext();

// 这样维护了太多的实例，后面可以在进行按需加载优化
// 导出操作数据库的数据库上下文
module.exports = {
  deviceContext,
  userContext,
  cloudDbContext,
};
