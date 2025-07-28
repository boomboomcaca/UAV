import React from "react";
import ReactDOM from "react-dom/client";
import "@dc/theme";
// import Theme, { Mode } from '@dc/theme/dist/tools.js';
import App from "./App";
// import * as serviceWorker from './serviceWorker';
import { setViewport } from "./utils/publicFunc";
import { getSystemInfo, getPlatform } from "./utils/capacitorUtils";
import fetchConfig, { initConfig } from "./utils/configManager";
import "./styles/video-react.css";

// 拉取子应用配置
// fetchConfig();
const platform = getPlatform();
console.log("platform ::::", platform);
function createMicroDoms() {
  // const mircoAppLen = config.microApps.length;
  // 最多能打开8个子应用
  for (let i = 0; i < 8; i += 1) {
    const container = document.createElement("div");
    container.className = "fullMicroContainer";
    container.id = `micro${i}`;
    document.body.appendChild(container);
  }
}

createMicroDoms();
// 加载本地配置
initConfig();
// 获取系统信息
getSystemInfo(() => {
  const mobiles = ["android", "ios"];
  if (mobiles.includes(platform)) {
    // 移动端
    setViewport(document);
  } else if (platform === "electron") {
    window.projConfig.syncconfiguration.webMapUrl = "http://127.0.0.1:8182";
  }

  console.log("platform ::::", window.App.platform, window.projConfig);
  // 初始化挂载信息
  // initPlatform();
});

window.onresize = () => {
  // TODO
};

window.onload = () => {
  // TODO
};

ReactDOM.createRoot(document.getElementById("root")).render(
  // StrictMode 会导致 useEffect 多次
  // <React.StrictMode>
  <App />
  // </React.StrictMode>
);
