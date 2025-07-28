/* eslint-disable prefer-destructuring */
const { getLogger } = require('../helper/log4jsHelper');
// todo:日志记录加入配置 配置是否要记录请求参数和返回参数
const logger = getLogger('businessLog');
const { businessLogger, businessLog } = require('../helper/businessLogger');
const { getSingle } = require('../helper/repositoryBase');

// 初始化一个字典配置每一个Url的Code
// 主要是记录操作日志所有的配了Url的日志均由这个hook 拦截产生
exports.businessLogRecord = async (req) => {
  try {
    // eslint-disable-next-line prefer-destructuring
    const url = req.raw.url;
    const data = req.body;
    // 忽略字符串和数字
    // eslint-disable-next-line eqeqeq
    if (url === '/plan/update' && data && data.status != 2) {
      return;
    }
    for (let i = 0; i < businessLog.length; i++) {
      const whitelistedRoute = [];
      let item = businessLog[i];
      // 如果没有配置Url的不再进行检查
      if (!item.url) {
        continue;
      }
      whitelistedRoute.push(item.url);
      if (url === item.url) {
        let parameter1 = '';
        let parameter2 = '';
        // 记录设备和功能操作日志
        // 添加设备
        if (item.url.indexOf('/Device/') >= 0) {
          let edgeId = '';
          let moduleType = '';
          if (data.id) {
            const device = await getSingle({
              tableName: 'rmbt_device',
              wheres: { id: data.id },
            });
            if (device) {
              moduleType = device.module_type || data.module_type;
              edgeId = device.edge_id || data.edgeid;
              parameter2 = device.name || data.name;
            }
          } else {
            moduleType = data.moduleType || data.module_type;
            edgeId = data.edgeid || data.edge_id;
            parameter2 = data.name;
          }
          const edge = await getSingle({
            tableName: 'rmbt_edge',
            wheres: { id: edgeId },
          });
          if (edge) {
            // 如果已经匹配过了一次则不再继续匹配
            parameter1 = edge.name;
            const code = item[moduleType];
            item = businessLog.filter((s) => s.code === code.concat(''))[0];
          }
        } else if (item.url === 'User/loginOut') {
          parameter1 = req.user.account;
        } else if (data[item.field]) {
          parameter1 = data[item.field];
        } else {
          const primaryKey = item.primaryKey || 'id';
          const tableName = item.tableName;
          const wheres = {};
          wheres[primaryKey] = data[primaryKey];
          const res = await getSingle({ tableName, wheres });
          if (res && res[item.field]) {
            parameter1 = res[item.field];
          }
        }
        // if (item) {
        await businessLogger.writeLog({
          level: item.level,
          code: item.code,
          parameter1,
          parameter2,
          req,
        });
        // 匹配过了就结束循环
        return;
      }
    }
  } catch (error) {
    logger.error(error);
  }
  // 其实要判断当前请求的Url和那个Url 相同不管是否带参数
};
