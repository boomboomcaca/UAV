const https = require("https");
const http = require("http");

const { database } = require("./dbHelper1");
const { mapProviders } = require("./mapSource").initProviders();

var TileLnglatTransform = require("tile-lnglat-transform");

const getTileWidthHttps = (host, path, callback) => {
  https
    .get(
      {
        host,
        path,
        headers: {
          "User-Agent":
            "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:11.0) Gecko/20180401 Firefox/11.0.9",
          // Accept: "*/*",
          // Referer: "http://map.baidu.com",
        },
      },
      (resp) => {
        // `${host}${path}`
        const tileData = [];
        let dataLen = 0;
        // res.setEncoding("binary");
        // A chunk of data has been recieved.
        resp.on("data", (chunk) => {
          tileData.push(chunk);
          dataLen += chunk.length;
        });
        // The whole response has been received. Print out the result.
        resp.on("end", (res) => {
          // console.log("====================", res);
          const allDataBuffer = Buffer.concat(tileData, dataLen);
          // console.log(allDataBuffer, allDataBuffer.length);
          console.log("end  ====================", allDataBuffer);
          callback(allDataBuffer);
        });
      }
    )
    .on("error", (err) => {
      console.log("Error: ", err);
      callback();
    });
};

const getVectorPbfWidthHttps = (url, callback) => {
  https
    .get(url, (resp) => {
      const tileData = [];
      let dataLen = 0;
      resp.on("data", (chunk) => {
        tileData.push(chunk);
        dataLen += chunk.length;
      });
      resp.on("end", (res) => {
        const allDataBuffer = Buffer.concat(tileData, dataLen);
        callback(allDataBuffer, resp.headers);
        //   // 解码 protobuf 数据
        // const decodedData = protobuf.decoder() protobuf.Root.fromJSON(protobufJson).lookupType('MessageType').decode(Buffer.from(data, 'binary'));
        // // 将 protobuf 数据发送到前端
        // res.send(decodedData);
      });
    })
    .on("error", (err) => {
      console.log("Error: ", err);
      callback();
    });
};

const getTileWidthHttps1 = (url, callback) => {
  https
    .get(url, (resp) => {
      // `${host}${path}`
      const tileData = [];
      let dataLen = 0;
      // res.setEncoding("binary");
      // A chunk of data has been recieved.
      resp.on("data", (chunk) => {
        tileData.push(chunk);
        dataLen += chunk.length;
      });
      // The whole response has been received. Print out the result.
      resp.on("end", (res) => {
        const allDataBuffer = Buffer.concat(tileData, dataLen);
        callback(allDataBuffer);
      });
    })
    .on("error", (err) => {
      console.log("Error: ", err);
      callback();
    });
};

const getTileWidthHttp1 = (url, callback) => {
  http
    .get(url, (resp) => {
      const tileData = [];
      let dataLen = 0;
      // res.setEncoding("binary");
      // A chunk of data has been recieved.
      resp.on("data", (chunk) => {
        tileData.push(chunk);
        dataLen += chunk.length;
      });
      // The whole response has been received. Print out the result.
      resp.on("end", (res) => {
        const allDataBuffer = Buffer.concat(tileData, dataLen);
        // console.log(allDataBuffer, allDataBuffer.length);
        //    console.log("end  ====================", allDataBuffer);
        callback(allDataBuffer);
      });
    })
    .on("error", (err) => {
      console.log("Error: ", err);
      callback();
    });
};

/**
 *
 * @param {{x:Number, y:Number, z:Number, ms: String, mt:String}} tasks
 * @param {database} db
 * @param {*} cb
 */
function downloadTask(tasks, db, cb) {
  if (tasks.length === 0) {
    cb();
    return;
  }
  let over = 0;
  const temp = [];
  tasks.forEach((item) => {
    const { x, y, z, mt, ms } = item;
    let url = mapProviders[ms][mt];
    if (url) {
      url = String(url).replace("{x}", x).replace("{y}", y).replace("{z}", z);
      if (url.startsWith("https")) {
        getTileWidthHttps1(url, (e) => {
          temp.push({
            url,
            x,
            y,
            z,
          });
          // 保存
          if (e) db.addByTransaction(x, y, z, e, ms, mt);
          over += 1;
          if (over === tasks.length) {
            cb();
          }
        });
      } else {
        getTileWidthHttp1(url, (e) => {
          if (e) db.addByTransaction(x, y, z, e, ms, mt);
          over += 1;
          if (over === tasks.length) {
            cb();
          }
          temp.push({
            url,
            x,
            y,
            z,
          });
        });
      }
    }
  });
}

/**
 *
 * @param {{coordinates:Array,mapSource:String,layers:Arrary<String>,maxLevel:Number,clear:boolean}} options
 * @param {database} db
 * @param {Function} onProgress
 */
function startDownload(options, db, onProgress) {
  // 1. 计算顶点
  let gpsLeftTopX = 180,
    gpsLeftTopY = -90,
    gpsRightBottomX = -180,
    gpsRightBottomY = 90;
  for (let i = 0; i < options.coordinates.length; i += 1) {
    const item = options.coordinates[i];
    if (item[0] < gpsLeftTopX) {
      gpsLeftTopX = item[0];
    }
    if (item[0] > gpsRightBottomX) {
      gpsRightBottomX = item[0];
    }
    if (item[1] > gpsLeftTopY) {
      gpsLeftTopY = item[1];
    }
    if (item[1] < gpsRightBottomY) {
      gpsRightBottomY = item[1];
    }
  }
  // const polygon = turf.polygon(options.coordinates);
  // const rect = turf.bbox(polygon);
  // 2. 粗略计算瓦片总数
  const calc =
    options.mapSource === "amap"
      ? TileLnglatTransform.TileLnglatTransformGaode
      : TileLnglatTransform.TileLnglatTransformGoogle;
  let totalCount = 0;
  const tileIndexes = [];
  for (let z = 1; z <= options.maxLevel; z += 1) {
    const start = calc.lnglatToTile(gpsLeftTopX, gpsLeftTopY, z);
    const end = calc.lnglatToTile(gpsRightBottomX, gpsRightBottomY, z);
    const startX = start.tileX,
      endX = end.tileX,
      startY = start.tileY,
      endY = end.tileY;
    const sum =
      (endX - startX + 1) * (endY - startY + 1) * options.layers.length;
    tileIndexes[z] = {
      startX,
      endX,
      startY,
      endY,
      sum,
    };
    totalCount += sum;

    // for (let x = startX; x <= endX; x += 1) {
    //   const tileLng = tile2long(x, z);
    //   for (let y = startY; y <= endY; y += 1) {
    //     // 3. 反过来校验一下瓦片是否在polygon内
    //     const tileLat = tile2lat(y, z);
    //     const isInside = turf.inside(turf.point([tileLng, tileLat]), polygon);
    //     if (isInside) {
    //       tileIndexes.push({ x, y, z });
    //     }
    //   }
    // }
  }
  // 3. 是否清除
  if (options.clear) {
    db.createDatabase();
  }
  // 4. 真的开始下载
  let levelProgress = 0;
  let totalProgress = 0;
  let z = 1;
  let x = 0;
  let y = 0;
  let curTasks = [];
  let dt = 0;
  const ddt = () => {
    if (z > options.maxLevel) return;
    const indexes = tileIndexes[z];
    const { startX, endX, startY, endY, sum } = indexes;
    x = startX;
    y = startY;
    const enqueue8 = () => {
      for (let i = 0; i < 8; i += 1) {
        // 5. 反过来校验一下瓦片是否在polygon内
        // const tileLng = tile2long(x, z);
        // const tileLat = tile2lat(y, z);
        // const isInside = turf.inside(turf.point([tileLng, tileLat]), polygon);
        // if (isInside) {
        // 6. 加入下载
        options.layers.forEach((mt) => {
          curTasks.push({ x, y, z, ms: options.mapSource, mt });
        });
        // }
        x += 1;
        levelProgress += options.layers.length;
        totalProgress += options.layers.length;
        if (x > endX) {
          y += 1;
          x = startX;
        }
        if (y > endY) break;
      }
      dt += curTasks.length;
      console.log("donwload count:::", dt, totalCount);
      // 7. 开始下载
      downloadTask(curTasks, db, () => {
        curTasks = [];
        // if (totalProgress % 100 === 0 || totalProgress === totalCount) {
        // TODO 通知进度
        onProgress({
          level: `${z}/${options.maxLevel}`,
          lp: levelProgress / sum,
          tp: totalProgress / totalCount,
          over: totalProgress === totalCount,
        });
        // }
        if (y > endY) {
          z += 1;
          levelProgress = 0;
          ddt();
        } else {
          enqueue8();
        }
      });
    };
    enqueue8();
  };
  setTimeout(() => {
    ddt();
  }, 1000);

  // for (let z = 1; z < options.maxLevel; z += 1) {
  //   levelProgress = z;
  //   let curLevelProgress = 0;
  //   const indexes = tileIndexes[z];
  //   const { startX, endX, startY, endY, sum } = indexes;
  //   for (let x = startX; x <= endX; x += 1) {
  //     const tileLng = tile2long(x, z);
  //     for (let y = startY; y <= endY; y += 1) {
  //       for (let l = 0; i < options.layers.length; l++) {
  //         const mt = options.layers[l];
  //         curLevelProgress += 1;
  //         totalProgress += 1;
  //         // 3. 反过来校验一下瓦片是否在polygon内
  //         const tileLat = tile2lat(y, z);
  //         const isInside = turf.inside(turf.point([tileLng, tileLat]), polygon);
  //         if (isInside) {
  //           // 4. 下载
  //           let url = mapTypes[options.mapSource][mt];
  //           if (url) {
  //             url = String(url)
  //               .replace("{x}", x)
  //               .replace("{y}", y)
  //               .replace("{z}", z);
  //             if (url.startsWith("https")) {
  //               getTileWidthHttps1(url, (e) => {
  //                 // 保存
  //                 addTile(x, y, z, e, ms, mt);
  //               });
  //             } else {
  //               getTileWidthHttp1(url, (e) => {
  //                 addTile(x, y, z, e, ms, mt);
  //               });
  //             }
  //           }
  //         }
  //       }
  //     }
  //   }
  // }
}

function stopDownload() {}

module.exports = {
  startDownload,
  stopDownload,
  getTileWidthHttp1,
  getTileWidthHttps,
  getTileWidthHttps1,
  getVectorPbfWidthHttps,
};
