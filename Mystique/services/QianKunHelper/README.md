# 安装

```bash
npm install skywaver-qiankun-helper
```

or

```bash
yarn add skywaver-qiankun-helper
```

# 引入

```javascript
import {
  MicroLoadManager,
  ActionType,
  LoadTypes,
  loadMicroByFeature,
  loadMicroWithEntry,
  } from "skywaver-qiankun-helper";
```

# 接口说明

> **1. loadMicroWithEntry(options)**

```javascript
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
const loadMicroWithEntry(options){}
```

> **2. loadMicroByFeature(options)**

```javascript
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
const loadMicroByFeature(options){}
```

# 枚举定义

```javascript
/**
 * 应用加载模式
 */
export const LoadTypes = {
  NORMAL: "normal",
  TRIGGER: "trigger",
  SWITCH: "switch",
  REPLAY: "replay",
};

/**
 * 子应用动作请求
 */
export const ActionType = {
  // 返回事件
  BACK: "back",
  // 返回Home事件
  BACKHOME: "backhome",
  // 弃用，显示后台任务
  SHOWBACKAPPS: "showbackapps",
  // 跳转弹窗
  TRIGGER: "trigger",
  // 快捷切换
  SWITCH: "switch",
  // 注册消息通道
  ONCHANNEL: "onchannel",
  // 弃用，设置标题文本信息
  CAPTION: "setcaption",
};
```