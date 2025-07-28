// import { loadMicroApp } from "qiankun";
import { message } from 'dui';
import Logger from '@dc/logger';
import { ActionType } from './microController.js';
import load from './qiankunLoader.js';

/**
 * 应用加载模式
 */
export const LoadTypes = {
  NORMAL: 'normal',
  TRIGGER: 'trigger',
  SWITCH: 'switch',
  REPLAY: 'replay',
};

/**
 * 子应用控制接口类
 * 接口方法中调调出去的 this.states，是为了方便外部使用，每次有需要则更新states，以防止在回调中读取不到组件的state
 */
function MicroLoader(maxMicro, onMicroMessage) {
  this.maxMicro = maxMicro;
  // 已加载的任务
  this.loadedMicros = [];
  // 触发调起的任务
  this.triggerMicro = undefined;
  // 发起触发的父级应用
  this.triggerParent = undefined;
  // 是否正在加载，防止卡顿时重复点击
  this.loading = false;
  // 主子应用通信回调
  this.onMicroMessage = onMicroMessage;
}

/**
 * 存储最近使用功能
 * 最近的 5 个
 * @param {Object} app
 */
MicroLoader.prototype.saveLatestApps = function (app) {
  //  * @param {String} entryId          所属6大类的id
  const { entryId } = app;
  if (entryId === undefined) return;
  const latestUse = window.localStorage.getItem('MainRecentApps');

  let latestUseMicros = {};
  if (latestUse) {
    latestUseMicros = JSON.parse(latestUse);
  }

  const template = latestUseMicros[`latest_${entryId}`] || [];
  const targetIndex = template.findIndex((a) => {
    return a.name === app.name;
  });
  if (targetIndex >= 0) {
    template.splice(targetIndex, 1);
  }

  template.unshift(app);

  let newHistory = template;
  if (template.length > 5) {
    newHistory = template.slice(0, 5);
  }
  latestUseMicros[`latest_${entryId}`] = newHistory;
  window.localStorage.setItem(
    'MainRecentApps',
    JSON.stringify(latestUseMicros)
  );
};

/**
 * 主子应用通信——子应用回调事件过程处理函数
 * @param {Object} e
 */
MicroLoader.prototype.microMessage = function (e) {
  const def = this;
  const micro = def.loadedMicros.find((m) => m.id === e.microId);

  switch (e.action) {
    case ActionType.BACK:
      if (def.triggerMicro) {
        def.triggerMicro.unmount();
        // 通知父级应用刷新
        const parentMicro = def.loadedMicros.find(
          (m) => m.id === def.triggerParent
        );
        if (parentMicro && parentMicro.channel) {
          parentMicro.channel({ type: 'triggerBack' });
        }
        def.triggerMicro = undefined;
        def.triggerParent = undefined;
      }
      break;
    case ActionType.BACKHOME:
      // 先判断是否支持多开，万一它不支持多开，但未停止任务即返回主页
      // 再判断任务状态
      if (
        !micro.registers ||
        micro.registers < 2 ||
        !e.taskStatus ||
        e.taskStatus !== 'running'
      ) {
        // 释放
        micro.unmount();
        // 移除当前应用
        const exceptMicros = def.loadedMicros.filter((m) => m.id !== e.microId);
        def.loadedMicros = exceptMicros || [];
        // const updateMicro = e.states.filter((m) => m.id !== e.microId);
        // setLoadedMicros(updateMicro);
      }
      // 更新当前显示的子应用id
      // setFrontMicro(undefined);
      // 隐藏容器，显示主页
      document.querySelectorAll('.fullMicroContainer').forEach((div) => {
        div.style.left = '-100vw';
        div.style.display = 'none';
        div.style.zIndex = -1;
      });
      break;
    case ActionType.SHOWBACKAPPS:
      if (!e.taskStatus || e.taskStatus !== 'running') {
        // 释放
        micro.unmount();
        // 移除当前应用
        const exceptMicros = def.loadedMicros.filter((m) => m.id !== e.microId);
        def.loadedMicros = exceptMicros || [];
      }
      break;
    case ActionType.TRIGGER:
      // 触发测量
      // triggerToMicro(e);
      def.triggerParent = e.microId;
      break;
    case ActionType.ONCHANNEL:
      if (micro) micro.channel = e.channel;
      break;
    case ActionType.CAPTION:
      if (micro) micro.caption = e.caption;
      break;

    default:
      break;
  }
  def.onMicroMessage(e);
};

/**
 * 正常加载子应用
 * @param {*} options
 * const { name, entry, mTitle, registers, onMicroLoaded } = options;
 * @returns
 */
MicroLoader.prototype.loadMicroOnNormal = function (options) {
  const { parameters } = options;
  this.loadMicro1(options, parameters ? LoadTypes.SWITCH : LoadTypes.NORMAL);
};

/**
 * 触发跳转
 */
MicroLoader.prototype.loadMicroOnTrigger = function (options) {
  this.loadMicro1(options, LoadTypes.TRIGGER);
};

MicroLoader.prototype.unmountMicro = function (microId) {
  const micro = this.loadedMicros.find((m) => m.id === microId);
  if (micro) {
    micro.unmount();
    // 移除当前应用
    const exceptMicros = this.loadedMicros.filter((m) => m.id !== microId);
    this.loadedMicros = exceptMicros || [];
  }
};

// /**
//  * 通过指定应用发送消息与主应用通信
//  * @param {String} microId
//  * @param {String} actionType
//  * @param {*} tag
//  */
// MicroLoader.prototype.sendMicroMessage = function (microId, actionType, tag) {
//   const micro = this.loadedMicros.find((m) => m.id === microId);
//   if (micro) {

//   }
// };

/**
 * 往指定应用的消息通道发送消息
 * @param {String} microId
 * @param {*} message
 */
MicroLoader.prototype.microChannel = function (microId, message) {
  const micro = this.loadedMicros.find((m) => m.id === microId);
  if (micro && micro.channel) {
    micro.channel(message);
  }
};

MicroLoader.prototype.loadMicro1 = function (options, loadType) {
  if (this.loading) return;
  this.loading = true;
  const {
    name,
    entry,
    mTitle,
    registers,
    container,
    onMicroLoaded,
    parameters, // 带入参数
  } = options;
  if (this.loadedMicros.length >= 8) {
    message.warning('最多只能同时运行8个应用');
    return;
  }
  if (registers && registers > 1) {
    // 这种情况不可以多开
    const exists = this.loadedMicros.filter((m) => m.moduleName === name);
    if (exists.length >= registers) {
      message.warning(`最多只能同时运行${registers}个[${mTitle}]`);
      return;
    }
  }
  let isLoadSuccess = false;
  const that = this;
  const micorIndex = this.loadedMicros.length || 0;
  const microCon = container || `micro${micorIndex}`;
  const microMsgPort = (e) => {
    this.microMessage(e);
  };
  load({
    name,
    entry,
    container: microCon,
    loadType,
    more: parameters, // 带入参数
    messagePort: microMsgPort,
  })
    .then((res) => {
      // {Promise<{id: String, controller:MicroController, unmount: Function, unmountPromise: Promise}>}
      const micro = res;
      micro.moduleName = name;
      micro.name = mTitle;
      micro.registers = registers;
      micro.container = microCon;

      if (loadType === LoadTypes.NORMAL || loadType === LoadTypes.SWITCH) {
        document.getElementById(microCon).style.zIndex = 5;
        document.getElementById(microCon).style.left = 0;
        document.getElementById(microCon).style.display = 'unset';
        this.loadedMicros.push(micro);
        // 最近使用处理
        this.saveLatestApps(options);
      } else if (loadType === LoadTypes.TRIGGER) {
        this.triggerMicro = micro;
      } else {
        // 其它，如回放
      }
      that.loading = false;
      // 加载完成，通知外部
      if (onMicroLoaded) {
        onMicroLoaded({ loadType, microId: micro.id });
      }
      isLoadSuccess = true;
      Logger.addInfo('main.microLoaded', name);
    })
    .catch((ex) => {
      Logger.addError('mian.microload', String(ex));
      console.log('!!!!!!!!!!!==== load error =========!!!!!', ex);
      ex && message.error('应用加载失败');
      setTimeout(() => {
        if (onMicroLoaded && !isLoadSuccess) {
          that.loading = false;
          // loader: this  把自己传出去，方便外部处理
          onMicroLoaded({ error: 'failed' });
        }
      }, 200);
    });
};

export { ActionType };
export default MicroLoader;
