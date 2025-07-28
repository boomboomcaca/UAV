import { name as projectName } from '../../package.json';

const ip = '192.168.1.103';
const port1 = '12001';
const port2 = '12001';

// const ip = '192.168.102.191';
// const port1 = '19101';
// const port2 = '19101';

export const microConfig = {
  projectName,
  appid: '064a326c-9d8e-4f62-bd2b-41229054a6ad',

  apiBaseUrl: `http://${ip}:${port1}`, // 应用api服务器地址
  wsTaskUrl: `ws://${ip}:${port2}/control`, // 应用webSocket服务器地址，单站任务
  wsNotiUrl: `ws://${ip}:${port2}/notify`, // 消息通知、GPS、罗盘等通知信息webSocket服务器地址
  wsDataUrl: `ws://${ip}:${port2}/data`, // 原始数据回放webSocket服务器地址

  mapType: 'amap',
  webMapUrl: 'http://192.168.1.191:6088', // 应用map服务器地址
  webMapFontUrl: 'http://192.168.102.103:6066/public',

  videoServerUrl: 'http://192.168.102.191:7001',
};

/**
 * 业务中获取配置请使用getConfig，不要使用microConfig！！！
 *
 * @returns
 */
function getConfig() {
  return window.projConfig[projectName];
}

export default getConfig;
