import {
  addGlobalUncaughtErrorHandler,
  runAfterFirstMounted,
  // setDefaultMountApp,
  start,
  initGlobalState,
} from 'qiankun';

const { setGlobalState, onGlobalStateChange } = initGlobalState({});

setGlobalState({
  token: 'master',
  user: 'microfrontend',
  updatedApp: {},
  modalChildProps: {},
  runningApps: [], // 缓存有任务运行的子应用
  stopedAppId: '',
  stopedAppTaskId: '',
  stopedAppTaskParam: {},
  modalChildName: '',
  modalChildEntry: '',
  microCount: 0,
});

/**
 * Step3 设置默认进入的子应用, 会影响页面F5刷新后展示页面
 */
// setDefaultMountApp('/sub_test_react/');

/**
 * 添加全局的未捕获异常处理器
 */
addGlobalUncaughtErrorHandler((event) => {
  window.console.error('addGlobalUncaughtErrorHandler', event);
  const { message: msg } = event;
  // 加载失败时提示
  if (msg && msg.includes('died in status LOADING_SOURCE_CODE')) {
    // console.log('子应用加载失败，请检查应用是否可运行', msg);
  }
});

runAfterFirstMounted(() => {
  // console.log('[MainApp] first app mounted');
});

export { setGlobalState, onGlobalStateChange };

// 导出 qiankun 的启动函数
export default start;
