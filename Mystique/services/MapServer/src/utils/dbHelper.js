// const sqlite3 = require("sqlite3");
// const fs = require("fs");
// const { mapLayers, mapSources } = require("./mapSource");
// // const { resolve } = require("path");
// // const { rejects } = require("assert");

// // const dbName = "mapTile.gmdb";
// /**
//  * @type {sqlite3.Database}
//  */
// // let db;

// function database() {
//   this.dbName = "mapTile.gmdb";
//   this.tiles = [];
//   this.timeoutToCommit = undefined;
//   this.initDatabase();
// }

// database.prototype.createTables = function () {
//   const keys = Object.keys(mapLayers);
//   keys.forEach((item) => {
//     const sql1 = `CREATE TABLE "raster_${item}" (
//      "id" text(32) NOT NULL PRIMARY KEY,
//      "x" integer NOT NULL,
//      "y" integer NOT NULL,
//      "z" integer NOT NULL,
//      "type" text(32) NOT NULL,
//      "tile" blob
//    );`;
//     this.db.exec(sql1);
//   });
// };

// database.prototype.initDatabase = function () {
//   this.db = new sqlite3.Database(this.dbName, sqlite3.OPEN_READWRITE, (err) => {
//     if (err && err.code == "SQLITE_CANTOPEN") {
//       this.db = new sqlite3.Database(this.dbName, (err) => {
//         if (err) {
//           console.log("Getting error create database" + err);
//         } else {
//           this.createTables();
//         }
//       });
//       return;
//     } else if (err) {
//       console.log("Getting error " + err);
//     } else {
//       console.log("db initlized");
//     }
//   });
// };

// database.prototype.createDatabase = function () {
//   if (this.db) {
//     this.db.close((err) => {
//       if (err) {
//         console.log("close db error::", err);
//       } else {
//         fs.renameSync(this.dbName, `mapTile-bak-${new Date().getTime()}.gmdb`);
//         this.initDatabase();
//       }
//     });
//   }
// };

// database.prototype.addByTransaction = function (
//   x,
//   y,
//   z,
//   tile,
//   mapSource,
//   mapType
// ) {
//   if (!this.timeoutToCommit) {
//     // 2s 存一次
//     this.timeoutToCommit = setTimeout(() => {
//       this.timeoutToCommit = null;
//       // 执行存储
//       const temp = this.tiles;
//       this.tiles = [];
//       console.log("save to db:::", this.tiles);
//       this.db.serialize(function () {
//         this.db.run("BEGIN");
//         let stmt = this.db.prepare(
//           `INSERT INTO raster_${mapLayers[mapType]} VALUES ($id, $x, $y, $z, $type, $tile) ON CONFLICT(id) DO UPDATE SET tile=$tile`
//         );
//         temp.forEach((item) => stmt.run(item));
//         stmt.finalize();
//         this.db.run("COMMIT");
//       });
//     }, 2500);
//   }
//   // const id = `${x}${y}${z}${mapSource}`;
//   // const allinOne = `INSERT INTO raster_${mapLayers[mapType]} (id, x, y, z, type, tile) values
//   //              ($id, $x, $y, $z, $type, $tile) ON CONFLICT(id) DO UPDATE SET tile=$tile`;
//   this.tiles.push({
//     $id: `${x}${y}${z}${mapSource}`,
//     $x: x,
//     $y: y,
//     $z: z,
//     $type: mapSource,
//     $tile: tile,
//   });
// };
// database.prototype.addTile = function (x, y, z, tile, mapSource, mapType) {
//   if (this.db) {
//     const allinOne = `INSERT INTO raster_${mapLayers[mapType]} VALUES ($id, $x, $y, $z, $type, $tile) ON CONFLICT(id) DO UPDATE SET tile=$tile`;
//     //创建插入语句
//     this.db.exec(allinOne, {
//       $id: `${x}${y}${z}${mapSource}`,
//       $x: x,
//       $y: y,
//       $z: z,
//       $type: mapSource,
//       $tile: tile,
//     });
//   }
// };

// /**
//  *
//  * @param {*} x
//  * @param {*} y
//  * @param {*} z
//  * @param {*} mapSource
//  * @param {*} mapType
//  * @returns {Promise}
//  */
// database.prototype.getTile = function (x, y, z, mapSource, mapType, callback) {
//   if (this.db) {
//     const id = `${x}${y}${z}${mapSource}`;
//     const sql = `select tile from raster_${mapLayers[mapType]} where id=?`;
//     this.db.all(sql, id, (er, res) => {
//       callback({
//         res: res[0].tile,
//         er,
//       });
//     });
//   } else {
//     callback({
//       er: "db is null",
//     });
//   }
// };

// module.exports = {
//   database,
// };
