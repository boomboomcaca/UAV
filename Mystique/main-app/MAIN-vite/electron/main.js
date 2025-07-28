// 引入electron并创建一个Browserwindow
const { app, BrowserWindow, ipcMain } = require("electron");
const { Worker } = require("worker_threads");
const isDev = require("electron-is-dev");
const path = require("path");
const url = require("url");

// 保持window对象的全局引用,避免JavaScript对象被垃圾回收时,窗口被自动关闭.
/**
 * @type {BrowserWindow}
 */
let mainWindow;
/**
 * @type {Worker}
 */
let serverWorker;
function createWindow() {
  //创建浏览器窗口,宽高自定义具体大小你开心就好
  mainWindow = new BrowserWindow({
    width: 1024,
    height: 600,
    minWidth: 1024,
    minHeight: 600,
    title: "main",
    fullscreenable: true,
    webPreferences: {
      webSecurity: false,
      nodeIntegration: true,
      enableRemoteModule: true,
      nodeIntegrationInWorker: true,
      preload: path.join(__dirname, "./src/preload.js"),
      contextIsolation: false, // 必须加这个，preload生效？？
    },
  });
  mainWindow.maximize();

  // 加载应用----适用于 react 项目
  if (isDev) {
    // 打开开发者工具，默认不打开
    mainWindow.webContents.openDevTools();
    mainWindow.loadURL("http://127.0.0.1:3000/");
    // mainWindow.loadURL(
    //   url.format({
    //     pathname: path.join(__dirname, "./app/index.html"),
    //     protocol: "file:",
    //     slashes: true,
    //   })
    // );
  } else {
    mainWindow.loadURL(
      url.format({
        pathname: path.join(__dirname, "./app/index.html"),
        protocol: "file:",
        slashes: true,
      })
    );
  }

  // 关闭window时触发下列事件.
  mainWindow.on("closed", function () {
    mainWindow = null;
  });

  mainWindow.webContents.on("will-redirect", (e, i) => {
    console.log("will-redirect", e, i);
  });

  mainWindow.webContents.setWindowOpenHandler((e) => {
    console.log("open new window:::", e);
    return {
      action: "allow",
      overrideBrowserWindowOptions: {
        fullscreenable: true,
        fullscreen: true,
        title: "morscr",
        minWidth: 1280,
        minHeight: 700,
        webPreferences: {
          webSecurity: false,
          nodeIntegration: true,
          enableRemoteModule: true,
          nodeIntegrationInWorker: true,
          preload: path.join(__dirname, "./src/preload.js"),
          contextIsolation: false, // 必须加这个，preload生效？？
        },
      },
    };
  });

  // 监听鼠标移动事件
  mainWindow.on("mousemove", (event) => {
    console.log("mouse move:::");
  });
}

// 当 Electron 完成初始化并准备创建浏览器窗口时调用此方法
app.on("ready", () => {
  serverWorker = new Worker(path.join(__dirname, "./src/startTileServer.js"));
  createWindow();

  // 从渲染线程接收消息
  ipcMain.on("client", (e, data) => {
    // console.log("get message from render:::", data);
    const { name, value, hash } = data;

    switch (name) {
      case "opendevtools":
        mainWindow.webContents.openDevTools();
        break;
      default:
        break;
    }
  });
  mainWindow.webContents.openDevTools();
});

// 所有窗口关闭时退出应用.
app.on("window-all-closed", function () {
  if (serverWorker) {
    serverWorker.terminate();
  }
  // macOS中除非用户按下 `Cmd + Q` 显式退出,否则应用与菜单栏始终处于活动状态.
  if (process.platform !== "darwin") {
    app.quit();
  }
});

app.on("activate", function () {
  // macOS中点击Dock图标时没有已打开的其余应用窗口时,则通常在应用中重建一个窗口
  if (mainWindow === null) {
    createWindow();
  }
  mainWindow.webContents.openDevTools();
});

app.on("browser-window-created", function (e, window) {
  console.log("create new window:::00", window.id, window.title);
  // f11 不可用
  // window.setMenu(null);
  window.setMenuBarVisibility(false);
  const iconPath = path.join(__dirname, "assets/icon.png");
  window.setIcon(iconPath);
});
