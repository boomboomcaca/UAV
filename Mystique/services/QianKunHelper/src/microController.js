/**
 * 子应用动作请求
 */
export const ActionType = {
  // 返回事件
  BACK: 'back',
  // 返回Home事件
  BACKHOME: 'backhome',
  // 弃用，显示后台任务
  SHOWBACKAPPS: 'showbackapps',
  // 跳转弹窗
  TRIGGER: 'trigger',
  // 快捷切换
  SWITCH: 'switch',
  // 注册消息通道
  ONCHANNEL: 'onchannel',
  // 弃用，设置标题文本信息
  CAPTION: 'setcaption',
};

function MicroController(microId, callback) {
  // this.backing = false;
  this.microId = microId;
  this.callback = callback;
  // 是否正在返回，从表面解决加载慢多次点击问题
  // const isBacking = false;

  // onCallback = (e) => {
  //   if (callback) {
  //     // setTimeout 不一定能保全 —— 万一micro已经加载成功了，但是主应用的处理还没有完就触发事件了
  //     setTimeout(() => {
  //       callback(e);
  //     }, 250);
  //   }
  // };

  // /**
  //  * 返回
  //  */
  // back = () => {
  //   onCallback({ action: ActionType.BACK, microId: microId });
  // };

  // /**
  //  * 返回主页
  //  * @param {String} taskStatus
  //  */
  // backHome = (taskStatus) => {
  //   onCallback({
  //     action: ActionType.BACKHOME,
  //     microId: microId,
  //     taskStatus,
  //   });
  // };

  // /**
  //  * 显示后台任务
  //  */
  // showBackgroundApp = (taskStatus) => {
  //   onCallback({
  //     action: ActionType.SHOWBACKAPPS,
  //     microId: microId,
  //     taskStatus,
  //   });
  // };

  // /**
  //  * 跳转其它子应用
  //  * @param {String} moduleName 功能代号 如 ffm
  //  * @param {any} options       带入参数什么的
  //  */
  // microTrigger = (moduleName, options) => {
  //   onCallback({
  //     action: ActionType.TRIGGER,
  //     // 当前应用的id
  //     microId: microId,
  //     moduleName,
  //     options,
  //   });
  // };

  // /**
  //  * 功能切换快捷栏
  //  * @param {any} options  带入参数
  //  */
  // microSwitch = (options) => {
  //   onCallback({
  //     action: ActionType.SWITCH,
  //     microId: microId,
  //     options,
  //   });
  // };

  // /**
  //  * 设置子应用自己的显示文本，主要在主参数变更时更新 如：单频测量-99.5MHz
  //  * @param {String} caption
  //  */
  // setCaption = (caption) => {
  //   onCallback({
  //     action: ActionType.CAPTION,
  //     microId: microId,
  //     caption,
  //   });
  // };

  // /**
  //  * 注册消息通道，让主应用直接发送消息过去
  //  * @param {Function} channel
  //  */
  // onMessage = (channel) => {
  //   onCallback({
  //     action: ActionType.ONCHANNEL,
  //     microId: microId,
  //     channel,
  //   });
  // };
}

/**
 * 返回
 */
MicroController.prototype.back = function () {
  this.onCallback({ action: ActionType.BACK, microId: this.microId });
};

/**
 * 返回主页
 * @param {String} taskStatus
 */
MicroController.prototype.backHome = function (taskStatus) {
  this.onCallback({
    action: ActionType.BACKHOME,
    microId: this.microId,
    taskStatus,
  });
};

/**
 * 显示后台任务
 */
MicroController.prototype.showBackgroundApp = function (taskStatus) {
  this.onCallback({
    action: ActionType.SHOWBACKAPPS,
    microId: this.microId,
    taskStatus,
  });
};

/**
 * 跳转其它子应用
 * @param {String} moduleName 功能代号 如 ffm
 * @param {any} options       带入参数什么的
 */
MicroController.prototype.microTrigger = function (moduleName, options) {
  this.onCallback({
    action: ActionType.TRIGGER,
    // 当前应用的id
    microId: this.microId,
    moduleName,
    options,
  });
};

/**
 * 功能切换快捷栏
 * @param {any} options  带入参数
 */
MicroController.prototype.microSwitch = function (options) {
  this.onCallback({
    action: ActionType.SWITCH,
    microId: this.microId,
    options,
  });
};

/**
 * 设置子应用自己的显示文本，主要在主参数变更时更新 如：单频测量-99.5MHz
 * @param {String} caption
 */
MicroController.prototype.setCaption = function (caption) {
  this.onCallback({
    action: ActionType.CAPTION,
    microId: this.microId,
    caption,
  });
};

/**
 * 注册消息通道，让主应用直接发送消息过去
 * @param {Function} channel
 */
MicroController.prototype.onMessage = function (channel) {
  this.onCallback({
    action: ActionType.ONCHANNEL,
    microId: this.microId,
    channel,
  });
};

// 抽象的公共回调处理
MicroController.prototype.onCallback = function (e) {
  if (this.callback) {
    // setTimeout 不一定能保全 —— 万一micro已经加载成功了，但是主应用的处理还没有完就触发事件了
    const that = this;
    setTimeout(() => {
      that.callback(e);
    }, 250);
  }
};
// }

export default MicroController;
