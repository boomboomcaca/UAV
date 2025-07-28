/* eslint-disable */
import { Device } from "@capacitor/device";
import { Capacitor } from "@capacitor/core";
import { Filesystem, Directory, Encoding } from "@capacitor/filesystem";
import { Http } from "@capacitor-community/http";
import { Toast } from "@capacitor/toast";
import { StatusBar } from "@capacitor/status-bar";
import { Screenshot } from "capacitor-plugin-screenshot";
import { toJpeg } from "html2image";
import { downloadUseEleA } from "./publicFunc.js";

export const getPlatform = () => {
  let platform = Capacitor.getPlatform();
  if (navigator.userAgent.toLowerCase().includes("electron")) {
    platform = "electron";
  }
  return platform;
};

/**
 * 获取操作系统信息
 * @param {function} callback
 */
export const getSystemInfo = (callback) => {
  if (window.App) {
    callback();
    return;
  }
  Device.getInfo()
    .then((res) => {
      window.App = res;
      const { operatingSystem } = res;
      if (operatingSystem.toLocaleLowerCase().includes("android")) {
        window.App.platform = "android";
        console.log("init android platform:::");
        // 不生效
        // ScreenOrientation.lock({ orientation:"landscape" });
        StatusBar.hide();
      } else if (operatingSystem.toLocaleLowerCase().includes("ios")) {
        window.App.platform = "ios";
      }
      const usrAgent = navigator.userAgent;
      if (usrAgent.toLowerCase().includes("electron")) {
        window.App.platform = "electron";
      }
    })
    .catch((ex) => {
      window.console.log(ex);
    })
    .finally(() => {
      if (callback) callback();
    });
};

/**
 * 不同平台初始化文件存储
 */
export const initPlatform = () => {
  // 安卓处理
  if (window.App.platform === "android") {
    Filesystem.requestPermissions()
      .then(() => {
        // 1. 创建相册
        Filesystem.mkdir({
          path: "Pictures/uavdefense",
          directory: Directory.ExternalStorage,
        }).catch((ex) => window.console.log("Create album failed", ex));
        // 2. 创建Uavdefense文件夹
        Filesystem.mkdir({
          path: "Uavdefense/Reports",
          directory: Directory.ExternalStorage,
          recursive: true,
        }).catch((ex) => window.console.log("Create folder failed", ex));
        Filesystem.mkdir({
          path: "Uavdefense/Pictures",
          directory: Directory.ExternalStorage,
          recursive: true,
        }).catch((ex) =>
          window.console.log("Create folder Pictures failed", ex)
        );
      })
      .catch((ex) => window.console.log("Request permissions failed", ex));
  }

  // IOS 处理
  if (window.App.platform === "ios") {
    // 2. 创建Reports文件夹
    Filesystem.mkdir({ path: "Reports", directory: Directory.Data }).catch(
      (ex) => window.console.log("Create folder failed", ex)
    );
    Filesystem.mkdir({ path: "Pictures", directory: Directory.Data }).catch(
      (ex) => window.console.log("Create folder Pictures  failed", ex)
    );
  }
  console.log("window.App.screenshot = screenshot");
  // 挂载方法
  window.App.screenshot = screenshot;
  window.App.saveFile = saveFile;
  window.App.downloadAndSaveFile = downloadAndSaveFile;
  // 获取base64 配置
  // 移动端使用 http下载文件，这个就不需要了
  // fetch('base64Ext.json')
  //   .then((re) => {
  //     re.json()
  //       .then((data) => {
  //         window.base64Ext = data;
  //       })
  //       .catch((ex) => console.log(ex));
  //   })
  //   .catch((e) => console.console.log(e));
};

/**
 * 挂载端截图方法
 * @param {HTMLElement} dom
 * @returns {PromiseConstructor}
 */
const screenshot = (dom) => {
  let shotElement = document.body;
  if (dom instanceof HTMLElement) {
    shotElement = dom;
  }
  console.log("screenshot main entry");
  return new Promise((resolve, reject) => {
    window.console.log(`The current platform is ${window.App.platform}`);
    let isOver = false; // 双保险
    const tmrTimeout = setTimeout(() => {
      if (!isOver) reject(new Error("截图超时"));
    }, 10000);
    if (window.App.platform === "ios") {
      // IOS
      screenShotOnIOS(shotElement, resolve, reject);
    }
    if (window.App.platform === "android") {
      screenshotOnAndroid(shotElement, resolve, reject);
    }
    if (window.App.platform === "electron") {
      // electronF
      screenshotOnElectron(resolve, reject);
    }
    if (window.App.platform === "web") {
      screenshotOnWeb(shotElement, resolve, reject);
    }

    isOver = true;
    clearTimeout(tmrTimeout);
  });
};

/**
 * 挂载文件下载和存储方法
 * url 文件下载地址
 * type 仅Android&IOS生效，screenshot：存储到截图目录；reports：存储到报表目录；其它情况：存储到根目录；
 * name 文件名  可选
 * @param {{url:String,type:String,name:String}} params
 */
const downloadAndSaveFile = (url, type, name) => {
  return new Promise((resolve, reject) => {
    if (!url.trim().startsWith("http")) {
      reject(new Error("Invalid URL"));
    } else {
      const lastPoint = url.lastIndexOf(".");
      const fileExt = url.substring(lastPoint);
      const lastSymbol = url.lastIndexOf("/");
      const fileName =
        name || url.substring(lastSymbol + 1).replace(fileExt, "");
      if (window.App.platform === "android" || window.App.platform === "ios") {
        // const base64Des = window.base64Ext ? window.base64Ext[fileExt] : undefined;
        const savePath = fileSavePathOnMobile(fileName, type);
        // 判断文件是否存在
        Filesystem.stat({
          path: `${savePath.filePath}${fileExt}`,
          directory: savePath.directory,
        })
          .then((res) => {
            // 文件已存在
            reject(new Error(`File "${savePath.filePath}${fileExt}" existed!`));
            Toast.show({
              text: "文件已存在",
              position: "center",
            });
          })
          .catch(() => {
            // 不存在再下载
            const options = {
              url: encodeURI(url),
              filePath: `${savePath.filePath}${fileExt}`, // `${fileName}${fileExt}`
              fileDirectory: savePath.directory,
              method: "GET",
            };
            // Writes to local filesystem
            Http.downloadFile(options)
              .then((res) => {
                resolve();
              })
              .catch((er) => {
                reject(er);
              });
          });
      } else {
        // a 标签下载
        saveFileOnWeb(`${fileName}${fileExt}`, url)
          .then((res) => {
            resolve();
          })
          .catch((er) => {
            reject(er);
          });
      }
    }
  });
};

const fileSavePathOnMobile = (fileName, type) => {
  let filePath = "";
  let dir = Directory.ExternalStorage;
  if (window.App.platform === "android") {
    filePath = `Uavdefense/${fileName}`;
    if (type === "report") {
      filePath = `Uavdefense/Reports/${fileName}`;
    } else if (type === "screenshot") {
      filePath = `Uavdefense/Pictures/${fileName}`;
    }
  }
  if (window.App.platform === "ios") {
    filePath = fileName;
    dir = Directory.Data;
    if (type === "report") {
      filePath = `Reports/${fileName}`;
    } else if (type === "screenshot") {
      filePath = `Pictures/${fileName}`;
    }
  }

  return { filePath, directory: dir };
};

/**
 * 挂载文件存储方法
 * fileName 文件名，不含后缀
 * dataString url || base64 || 文本
 * utf8 当保存（dataString）文本时 设置true
 * type 保存类型
 * @param {{fileName:String,dataString:String,utf8:boolean,type:String}} params
 */
const saveFile = (params) => {
  const { fileName, dataString, utf8, type } = params;
  if (window.App.platform === "android" || window.App.platform === "ios") {
    const savePath = fileSavePathOnMobile(fileName, type);
    const encode = utf8 ? Encoding.UTF8 : undefined;
    return Filesystem.writeFile({
      directory: savePath.directory,
      path: savePath.filePath,
      data: dataString,
      encoding: encode,
    });
    // .then(() => {
    //   Toast.show({
    //     text: '保存成功',
    //     position: 'center',
    //   });
    // })
    // .catch((e) => {
    //   Toast.show({
    //     text: '保存失败',
    //     position: 'center',
    //   });
    // });
  } else {
    return saveFileOnWeb(fileName, dataString, utf8);
  }
};

/**
 *
 * @param {String} fileName
 * @param {String} dataString
 * @param {*} utf8
 */
const saveFileOnWeb = (fileName, dataString, utf8) => {
  return new Promise((resolve, reject) => {
    if (utf8) {
      try {
        // 对字符串进行编码
        const encode = encodeURI(dataString);
        // 对编码的字符串转化base64
        const base64Str = `data:text/plain;base64,${window.btoa(encode)}`;
        downloadUseEleA(base64Str, fileName);
        resolve();
      } catch {
        reject(new Error(""));
      }
    } else {
      // fetch 一手，避免跨域无法直接下载
      fetch(dataString)
        .then((res) => {
          res.blob().then((b) => {
            downloadUseEleA(URL.createObjectURL(b), fileName);
            resolve();
          });
        })
        .catch((er) => {
          console.log("download error", er);
          reject(er);
        });
    }
  });
};

/**
 * 根据dom节点查找含有webgl特性的canvas
 * @param {HTMLElement} dom 父级节点
 * @returns
 */
const screenshotWebgl = (dom) => {
  return new Promise((resolve, reject) => {
    // const domElement = dom || document.body;
    if (dom instanceof HTMLElement) {
      const canvas = dom.getElementsByTagName("canvas");
      const webglCanvas = [];
      if (canvas && canvas.length > 0) {
        for (let i = 0; i < canvas.length; i += 1) {
          // 判断是不是webgl的canvas
          const isWebgl = canvas[i].parentElement.getAttribute("webgl"); // canvas[i].parentElement.className.includes('Spectrum');
          if (isWebgl && isWebgl !== null) {
            const screenRect = canvas[i].getBoundingClientRect();
            // 屏幕内的才获取
            if (screenRect.x > 1 && screenRect.y > 1) {
              const url = canvas[i].toDataURL("image/png", 0.95);
              webglCanvas.push({
                x: screenRect.x,
                y: screenRect.y,
                width: screenRect.width,
                height: screenRect.height,
                url,
              });
            }
          }
        }
      }
      resolve(webglCanvas);
    }
    resolve([]);
  });
};

/**
 * html2vanvas 无法截图video视频，这里单独覆盖
 * @param {HTMLElement} dom 父级节点
 * @returns
 */
const screenshotVideo = (dom) => {
  return new Promise((resolve, reject) => {
    // const domElement = dom || document.body;
    if (dom instanceof HTMLElement) {
      const video = dom.getElementsByTagName("video");
      let imgUrl = "";
      if (video && video.length > 0) {
        const canvas = document.createElement("canvas");
        const rect = video[0].getBoundingClientRect();
        if (rect.x >= 0 && rect.y >= 0) {
          canvas.width = rect.width;
          canvas.height = rect.height;
          canvas
            .getContext("2d")
            .drawImage(video[0], 0, 0, canvas.width, canvas.height);
          imgUrl = canvas.toDataURL("image/png", 1.0);
          resolve({
            x: rect.x,
            y: rect.y,
            width: rect.width,
            height: rect.height,
            url: imgUrl,
          });
        }
      }
      resolve(undefined);
    }
    resolve([]);
  });
};

/**
 * 处理IOS截屏
 * @param {HTMLElement} resolve
 * @param {void} resolve
 * @param {void} reject
 */
const screenShotOnIOS = (dom, resolve, reject) => {
  Screenshot.echo(90)
    .then((res) => {
      // 合并
      const domElement = dom || document.body;
      const shotCanvas = document.createElement("canvas");
      shotCanvas.width = domElement.clientWidth;
      shotCanvas.height = domElement.clientHeight;
      const ctx = shotCanvas.getContext("2d");

      const shot = new Image();
      shot.crossOrigin = "Anonymous";
      shot.onload = () => {
        setTimeout(() => {
          ctx.drawImage(
            shot,
            0,
            0,
            domElement.clientWidth,
            domElement.clientHeight
          );
          screenshotWebgl(domElement)
            .then((cs) => {
              // 绘制到现有图片
              if (cs.length > 0) {
                cs.forEach((item, index) => {
                  const { width, height, url, x, y } = item;
                  const img = new Image();
                  img.onload = () => {
                    setTimeout(() => {
                      ctx.drawImage(img, x, y, width, height);
                      if (index === cs.length - 1) {
                        const str = shotCanvas.toDataURL("image/jpeg");
                        resolve({ uri: str });
                      }
                    }, 10);
                  };
                  img.crossOrigin = "Anonymous";
                  img.src = `${url}`;
                });
              }
            })
            .catch((er) => reject(new Error(er)));
        }, 10);
      };
      shot.src = `${res.value}`; // `${res.URI}`;
    })
    .catch((er) => reject(new Error(er)));
};

/**
 * 处理electron截图
 * @param {void} resolve
 * @param {void} reject
 */
const screenshotOnElectron = (resolve, reject) => {
  const electron = window.require("electron");
  const { ipcRenderer } = electron;

  // 向主进程发送消息
  ipcRenderer.send("screenshot", "1");
  const tmr = setTimeout(() => {
    reject(new Error("timeout"));
  }, 3000);
  // 监听主进程返回的消息
  ipcRenderer.on("screenshot", (e, arg) => {
    const bytes = new Uint8Array(arg);
    let binary = "";
    for (let len = bytes.byteLength, i = 0; i < len; i += 1) {
      binary += String.fromCharCode(bytes[i]);
    }
    const base64Str = window.btoa(binary);
    clearTimeout(tmr);
    resolve({ uri: `data:image/jpeg;base64,${base64Str}` });
  });
};

/**
 *
 * @param {HTMLElement} dom
 * @returns
 */
const fetchScreenshot = (dom) => {
  let shotElement = document.body;
  if (dom instanceof HTMLElement) {
    shotElement = dom;
    console.log("set dom on screenshot:::", dom);
  }
  const rect = shotElement.getBoundingClientRect();
  console.log("screentshot rect :::", rect);
  const ratio = window.devicePixelRatio;
  // const x = Math.ceil((window.screenLeft + rect.x) * ratio);
  // const y = Math.ceil((window.screenTop + window.outerHeight) * ratio - window.innerHeight * ratio);
  // const w = Math.floor(rect.width * ratio);
  // const h = Math.floor(rect.height * ratio - 2);

  const outTop = window.screenTop;
  const outLeft = window.screenLeft;
  const outHeight = window.outerHeight;
  const inHeight = window.innerHeight;
  // const eleTop = Math.ceil(rect.y * ratio);
  // const eleLeft = Math.ceil(rect.x * ratio);
  // const eleWidth = Math.floor(rect.width * ratio);
  // const eleHeight = Math.floor(rect.height * ratio - 2);

  // ox=${outLeft}&
  // oy=${outTop}&
  // oh=${outHeight}&
  // ih=${inHeight}&
  // ex=${eleLeft}&
  // ey=${eleTop}&
  // ew=${eleWidth}&
  // eh=${eleHeight}&
  // rt=${ratio}

  return Promise.race([
    fetch(
      `http://127.0.0.1:8481/shot?ox=${outLeft}&oy=${outTop}&oh=${outHeight}&ih=${inHeight}&ex=${rect.x}&ey=${rect.y}&ew=${rect.width}&eh=${rect.height}&rt=${ratio}`
    ),
    new Promise(function (resl, rejt) {
      setTimeout(() => {
        rejt({ code: 10086 });
      }, 1200);
    }),
  ]);

  // return Promise.race([
  //   fetch(`http://127.0.0.1:8481/shot?x=${x}&y=${y}&w=${w}&h=${h}`),
  //   new Promise(function (resl, rejt) {
  //     setTimeout(() => {
  //       rejt({ code: 10086 });
  //     }, 1200);
  //   }),
  // ]);
};

const useHtml2ImageOnWeb = (dom, resolve, reject) => {
  toJpeg(dom, { quality: 0.95, pixelRatio: 1 })
    .then((res) => {
      // 合并
      const domElement = dom || document.body;
      screenshotVideo(domElement)
        .then((videoImg) => {
          // 绘制到现有图片
          if (videoImg) {
            const shotCanvas = document.createElement("canvas");
            shotCanvas.width = domElement.clientWidth;
            shotCanvas.height = domElement.clientHeight;
            const ctx = shotCanvas.getContext("2d");
            const shot = new Image();
            shot.crossOrigin = "Anonymous";
            shot.onload = () => {
              // 绘制主图
              ctx.drawImage(
                shot,
                0,
                0,
                domElement.clientWidth,
                domElement.clientHeight
              );
            };
            shot.src = `${res}`;

            // 绘制附加图
            const { width, height, url, x, y } = videoImg;
            const img = new Image();
            img.onload = () => {
              setTimeout(() => {
                // 绘制附加图
                ctx.drawImage(img, x, y, width, height);
                const str = shotCanvas.toDataURL("image/jpeg");
                resolve({ uri: str });
              }, 15);
            };
            img.crossOrigin = "Anonymous";
            img.src = `${url}`;
          } else {
            resolve({ uri: res });
          }
        })
        .catch((er) => reject(new Error(er)));
    })
    .catch((er) => {
      reject(new Error(er));
    });
};

const screenshotOnWeb = (dom, resolve, reject) => {
  const usable = localStorage.getItem("browser-screenshot-plugin");
  // null 没有下载过，1或者曾经可以用      0 未知状态  2 不可用 3 拒绝下载
  if (window.App.operatingSystem === "windows") {
    // windows操作系统
    if (!usable) {
      useHtml2ImageOnWeb(dom, resolve, reject);
      // 提升下载
      const confirm = window.confirm(
        "为了给您提供更好的截图体验，请下载并安装截图插件"
      );

      if (confirm) {
        localStorage.setItem("browser-screenshot-plugin", "1");
        const origin =
          process.env.NODE_ENV === "production"
            ? window.location.origin
            : "http://192.168.102.167:8888/cdn";
        downloadUseEleA(
          `${origin}/public/ScreenCapturePlugin/Uavdefense.screenshot.plugin.exe`,
          "Uavdefense.screenshot.plugin.exe"
        );
      } else {
        localStorage.setItem("browser-screenshot-plugin", "3");
      }
    } else if (usable === "2") {
      // 插件不可用
      useHtml2ImageOnWeb(dom, resolve, reject);
    } else {
      // 走插件下载
      fetchScreenshot(dom)
        .then((res) => {
          localStorage.setItem("browser-screenshot-plugin", "1");
          res
            .json()
            .then((json) => {
              resolve({ uri: json.data });
            })
            .catch((er) => {
              reject(new Error(er));
            });
        })
        .catch((er) => {
          // 容错1次，第二次出现问题才不走插件
          localStorage.setItem(
            "browser-screenshot-plugin",
            usable === "0" ? "2" : "0"
          );
          useHtml2ImageOnWeb(dom, resolve, reject);
          // reject(new Error(er));
        });
    }
  } else {
    // 其它操作系统
    useHtml2ImageOnWeb(dom, resolve, reject);
  }
  // if (usable === '2' && window.App.operatingSystem === 'windows') {
  // } else {
  //   useHtml2ImageOnWeb(dom, resolve, reject);
  // }
};

const screenshotOnAndroid = (dom, resolve, reject) => {
  toJpeg(dom, { quality: 0.95, pixelRatio: 1 })
    .then((res) => {
      resolve({ uri: res });
    })
    .catch((er) => {
      reject(new Error(er));
    });
};
