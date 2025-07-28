import { message } from "dui";

const fetchConfig = (callback) => {
  // 从服务器拉去子应用配置
  loadConfig(callback);
};

/**
 * 从前端部署的服务器拉去配置
 * @param {*} serverIP
 */
const loadConfig = (callback) => {
  const serverIP = window.baseConf.server;

  const configUrl =
    process.env.NODE_ENV === "production"
      ? `http://${serverIP}/microAppsConfig.json`
      : "microAppsConfig.json";
  fetch(configUrl, {
    method: "GET",
    // headers: {
    //   'Content-Type': 'application/json',
    // },
    // mode: 'no-cors', // 允许发送跨域请求
    // credentials: 'include',
  })
    .then((response) => {
      const { baseConf } = window;
      // 打印返回的json数据
      response.json().then((data) => {
        console.log(window.App.platform);
        let micros;
        // if (window.App.platform === "electron") {
        //   micros = { ...data.microApps };
        //   Object.keys(micros).forEach((key) => {
        //     const mic = data.microApps[key];
        //     mic.entry = `file://${window.basedir}/micros/${mic.name}/index.html`;
        //   });
        // } else {
        micros = formatConf(JSON.stringify(data.microApps), baseConf);
        // }
        console.log("fetch micro configg:::", micros);
        const indexEntry = formatConf(
          JSON.stringify(data.indexEntry),
          baseConf
        );
        // 更新配置
        window.projConfig.main.microApps = micros;
        window.projConfig.main.indexEntry = indexEntry;

        if (callback)
          callback({
            microApps: micros,
            indexEntry,
          });
      });
    })
    .catch(() => {
      message.info("无法连接服务器");
    });
};

const formatConf = (str, obj) => {
  let s = str;
  if (typeof obj === "object") {
    Object.keys(obj).forEach((key) => {
      s = s.replace(new RegExp(`\\$${key}\\$`, "g"), obj[key]);
    });
  }
  return JSON.parse(s);
};

/**
 * 初始化本地配置
 */
export const initConfig = () => {
  // 加载本地配置
  const baseConf = localStorage.getItem("baseConf");
  if (baseConf) {
    try {
      const parsedConf = JSON.parse(baseConf);
      // TODO 配置
      window.baseConf = parsedConf;
      // baseConf更新后 更新公共配置
      const loadConf = {
        // 云端接口地址
        apiBaseUrl: `http://${window.baseConf.cloud}`,
        apiBaseUrl1: `http://${window.baseConf.cloud1}`,
        // 单站任务、多站任务、原始数据回放ws地址
        wsTaskUrl: `ws://${window.baseConf.cloud}/control`,
        wsMTaskUrl: `ws://${window.baseConf.cloud}/control`,
        // 消息通知ws地址
        wsNotiUrl: `ws://${window.baseConf.cloud}/notify`,
        wsReplayUrl: `ws://${window.baseConf.cloud}/control`,
        videoServerUrl: `ws://${window.baseConf.video}/ws/`,
        // 地图字体请求地址
        // webMapFontUrl: `http://${window.baseConf.server}/public`,
        // 地图瓦片数据请求地址（注意大屏geojson数据请求后缀）
        webMapUrl: `http://${window.baseConf.map}`,
        // 地图瓦片类型（amap：高德，）
        mapType: window.baseConf.mapType || "amap",
        province: window.baseConf.province || "510000",
        procinceName: window.baseConf.procinceName || "四川省",
      };
      window.projConfig.syncconfiguration = {
        ...window.projConfig.syncconfiguration,
        ...loadConf,
      };
    } catch (e) {
      window.console.log("localStorage.getItem('baseConf')", e);
      // 清楚
      localStorage.clear();
      // 刷新拉取新配置
      window.location.reload();
    }
  }
};

export default fetchConfig;
