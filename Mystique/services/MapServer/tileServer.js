const express = require("express");
var cors = require("cors");

const { parentPort } = require("worker_threads");
const exp = express();

const { layerTypes, mapProviders } =
  require("./utils/mapSource").initProviders();
const { database } = require("./utils/dbHelper1");

// process.env.NODE_ENV = "production";

const {
  startDownload,
  stopDownload,
  getTileWidthHttp1,
  getTileWidthHttps,
  getTileWidthHttps1,
  getVectorPbfWidthHttps,
} = require("./utils/tileDownloader");
const { render2MainMsg } = require("./utils/contracts");

let browseOnly = false;
// 是否仅使用离线数据，默认为false，先查数据库，再请求http
let onlyOffline = false;
let downloadRegion;

/**
 * @type {database}
 */
let db;

const getTileOnline = (x, y, z, ms, mt, callback) => {
  let url = mapProviders[ms][mt];
  if (url) {
    url = String(url).replace("{x}", x).replace("{y}", y).replace("{z}", z);
    // console.log(url);
    if (mt === "buildings") {
      getVectorPbfWidthHttps(url, (e, headers) => {
        callback(e, headers);
      });
    } else if (url.startsWith("https")) {
      getTileWidthHttps1(url, (e) => {
        callback(e);
      });
    } else {
      getTileWidthHttp1(url, (e) => {
        callback(e);
      });
    }
  }
};

const api_get = () => {
  exp.get("/tile", (req, res) => {
    if (req.query && req.query !== null) {
      let x = parseInt(req.query.x);
      let y = parseInt(req.query.y);
      let z = parseInt(req.query.z);
      let ms = req.query.ms;
      let mt = req.query.mt;
      if (onlyOffline) {
        db.getTile(x, y, z, ms, mt, (tile) => {
          if (tile.er) console.log("get tile error in db:::", tile.er);
          if (tile.res && tile.res.remark) {
            // 设置header
            const headers = JSON.parse(tile.res.remark);
            const keys = Object.keys(headers);
            keys.forEach((key) => {
              res.setHeader(key, headers[key]);
            });
          }
          res.end(tile.res ? tile.res.tile : "no data");
        });
      } else {
        db.getTile(x, y, z, ms, mt, (tile) => {
          if (tile.er || !tile.res || !tile.res.tile) {
            getTileOnline(x, y, z, ms, mt, (e, headers) => {
              if (headers) {
                const keys = Object.keys(headers);
                keys.forEach((key) => {
                  res.setHeader(key, headers[key]);
                });
              }
              res.end(e);
              // 保存
              if (e && !browseOnly)
                db.addByTransaction(
                  x,
                  y,
                  z,
                  e,
                  ms,
                  mt,
                  headers ? JSON.stringify(headers) : ""
                );
            });
          } else {
            if (tile.res.remark) {
              // 设置header
              const headers = JSON.parse(tile.res.remark);
              const keys = Object.keys(headers);
              keys.forEach((key) => {
                res.setHeader(key, headers[key]);
              });
            }
            res.end(tile.res.tile);
          }
        });
      }
    } else {
      res.end();
    }
  });
};

let progresInfo = { p: "test" };
const api_download = () => {
  exp.post("/download", (req, res) => {
    let data = "";
    req.on("data", (chunk) => {
      data += chunk;
    });
    req.on("end", () => {
      // 将JSON字符串解析成对象
      data = JSON.parse(data);
      downloadRegion = data.coordinates;
      console.log("get download request::", data);
      if (progresInfo.level && !progresInfo.over) {
        // stopDownload();
        // progresInfo = {};
        res.end({
          error: "Already has task,you can stop it first",
        });
      } else {
        res.end();
        progresInfo = {};
        startDownload(data, db, (e) => {
          if (e.over) {
            downloadRegion = undefined;
          }
          progresInfo = e;
        });
      }
    });
  });
};

const api_downloadProgress = () => {
  exp.get("/progress", (req, res) => {
    res.end(JSON.stringify(progresInfo));
  });
};

function api_getProviders() {
  exp.get("/providers", (req, res) => {
    res.end(JSON.stringify({ mapProviders }));
  });
}

// onlyoffline browseOnly
function api_setOptions() {
  exp.post("/setoptions", (req, res) => {
    let data = "";
    req.on("data", (chunk) => {
      data += chunk;
    });
    req.on("end", () => {
      // 将JSON字符串解析成对象
      data = JSON.parse(data);
      onlyOffline = data.onlyOffline;
      browseOnly = data.browseOnly;
      res.end();
    });
    req.on("error", (er) => {
      res.end({
        error: er,
      });
    });

    // if (req.query && req.query !== null) {
    //   console.log("set options:::".req.query);
    //   onlyOffline = req.query.onlyOffline;
    //   browseOnly = req.query.browseOnly;
    //   res.end();
    // } else {
    //   res.end({
    //     error: "invalid parameters",
    //   });
    // }
  });
}

// onlyoffline browseOnly downloadRegion
function api_getOptions() {
  exp.get("/getoptions", (req, res) => {
    res.end(
      JSON.stringify({
        onlyOffline,
        browseOnly,
        downloadRegion,
      })
    );
  });
}

function initServer(params) {
  const { onMessage } = params;
  exp.use(cors());
  console.log(layerTypes);
  // 初始化数据
  db = new database(mapProviders, layerTypes);
  api_get();
  api_download();
  api_downloadProgress();
  api_getProviders();
  api_getOptions();
  api_setOptions();
  exp.use(express.static("public"));
  const server = exp.listen(8182, () => {
    const host = server.address().address;
    const port = server.address().port;

    console.log(`应用实例，访问地址为 http://${host}:${port}`);
    if (onMessage) {
      onMessage(`应用实例，访问地址为 http://${host}:${port}`);
    }
    return server;
  });
}

if (parentPort) {
  parentPort.on("message", (msg) => {
    console.log(`worker: receive ${msg}`);
    const { type, data } = msg;
    if (type === render2MainMsg.setMode) {
      setMode(data);
    }
    if (type === render2MainMsg.setOffline) {
      onlyOffline = data;
    }
    // if (msg === 5) { process.exit(); }
    // parentPort.postMessage(msg);
  });
}
setTimeout(() => {
  initServer((e) => {
    // sendToRender(main2RenderMessages.info, e);
  });
}, 200);

// module.exports = { initServer, setMode };
