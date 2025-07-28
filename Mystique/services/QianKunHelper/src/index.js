import MicroLoadManager, { LoadTypes, ActionType } from "./microLoadManager.js";
import loadMicro from "./qiankunLoader.js";

/**
 * messagePort 回调函数定义
 * @callback onMessagePort
 * @type Function
 * @param {{action:ActionType,microId:String,tag:Object,taskStatus:String,channel:Function}} e
 */

/**
 * 通过url加载子应用
 * name: 名称
 * entry：入口地址
 * container：容器id
 * loadType：加载类型  normal || trigger || replay
 * parameters：带入参数
 * messagePort：主子应用通信处理函数
 * tag：其它附加信息，一般用于跨域字段访问
 * @param {{name:String,entry:String, container:String ,loadType:String,parameters:Object, messagePort:onMessagePort,tag:object}} options
 * @returns {Promise<{id: String, controller:MicroController, unmount: Function, unmountPromise: Promise}>}
 */
const loadMicroWithEntry = (options) => {
  return loadMicroByFeature(options);
};

/**
 * 通过feature加载子应用
 * feature：子应用代号 如 ffm
 * container：容器id
 * loadType：加载类型  normal || trigger || replay
 * parameters：带入参数
 * messagePort：主子应用通信处理函数
 * tag：其它附加信息，一般用于跨域字段访问
 * @param {{feature:String,container:String ,loadType:String,parameters:Object, messagePort:onMessagePort,tag:object}} options
 * @returns {Promise<{id: String, controller:MicroController, unmount: Function, unmountPromise: Promise}>}
 */
const loadMicroByFeature = (options) => {
  let entry;
  let name;
  const { feature, container, loadType, parameters, messagePort, tag } =
    options;
  if (feature) {
    // 通过功能名称加载
    if (
      window.projConfig &&
      window.projConfig.main &&
      window.projConfig.main.microApps
    ) {
      // 查找feature
      const microApps = window.projConfig.main.microApps;
      const module = microApps[feature];
      if (module) {
        name = module.name;
        entry = module.entry;
      }
    }
  } else {
    // 通过地址加载
    name = options.name;
    entry = options.entry;
  }
  if (name && entry && container) {
    // 挂载子应用
    return loadMicro({
      name,
      entry,
      container,
      loadType, // 加载类型
      more: parameters, // 带入参数
      messagePort: messagePort,
      tag, // tag 给进去是为了接近作用域无法获取值的问题
    });
  }
  return new Promise((resolve, reject) => {
    reject("应用加载失败");
  });
};

export {
  MicroLoadManager,
  ActionType,
  LoadTypes,
  loadMicroByFeature,
  loadMicroWithEntry,
};
