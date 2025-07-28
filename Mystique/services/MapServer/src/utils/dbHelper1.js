// const sqlite = require("better-sqlite3/build/Release/better_sqlite3.node");
// const sqlite3 = require("better-sqlite3")(sqlite);

const sqlite3 = require("better-sqlite3");
const fs = require("fs");

/**
 * @param {Object} mapProviders
 * @param {Array<String>} layerTypes
 */
function database(mapProviders, layerTypes) {
  this.dbName = "mapTile.gmdb";
  this.tiles = {};
  this.tables = Object.keys(layerTypes);
  this.mapProviders = mapProviders;
  this.timeoutToCommit = undefined;
  this.initDatabase();
}

database.prototype.createTables = function () {
  console.log(this.tables);
  this.tables.forEach((item) => {
    const sql1 = `CREATE TABLE "raster_${item}" (
     "id" text(32) NOT NULL PRIMARY KEY,
     "x" integer NOT NULL,
     "y" integer NOT NULL,
     "z" integer NOT NULL,
     "type" text(32) NOT NULL,
     "tile" blob,
     "remark" blob
   );`;
    this.db.exec(sql1);
  });
};

database.prototype.initDatabase = function () {
  if (fs.existsSync(this.dbName)) {
    this.db = new sqlite3(this.dbName);
  } else {
    fs.writeFileSync(this.dbName, "");
    this.db = new sqlite3(this.dbName, { fileMustExist: true });
    this.createTables();
  }
};

database.prototype.createDatabase = function () {
  if (this.db) {
    this.db.close();
    fs.renameSync(this.dbName, `mapTile-bak-${new Date().getTime()}.gmdb`);
    this.initDatabase();
  }
};

database.prototype.addByTransaction = function (
  x,
  y,
  z,
  tile,
  mapSource,
  mapType,
  remark
) {
  if (!this.timeoutToCommit) {
    // 2s 存一次
    this.timeoutToCommit = setTimeout(() => {
      this.timeoutToCommit = null;
      // 执行存储
      const temp = this.tiles;
      this.tiles = {};
      const keys = Object.keys(temp);

      for (var key of keys) {
        console.log("save to db:::", key);
        let stmt = this.db.prepare(
          `INSERT INTO raster_${key} VALUES ($id, $x, $y, $z, $type, $tile, $remark) ON CONFLICT(id) DO UPDATE SET tile=$tile`
        );
        const insertMany = this.db.transaction((items) => {
          for (const item of items) stmt.run(item);
        });
        insertMany(temp[key]);
      }
    }, 2500);
  }
  // const id = `${x}${y}${z}${mapSource}`;
  // const allinOne = `INSERT INTO raster_${mapLayers[mapType]} (id, x, y, z, type, tile) values
  //              ($id, $x, $y, $z, $type, $tile) ON CONFLICT(id) DO UPDATE SET tile=$tile`;
  if (!this.tiles[mapType]) {
    this.tiles[mapType] = [];
  }
  this.tiles[mapType].push({
    id: `${x}${y}${z}${mapSource}`,
    x: x,
    y: y,
    z: z,
    type: mapSource,
    tile: tile,
    remark,
  });
};
database.prototype.addTile = function (
  x,
  y,
  z,
  tile,
  mapSource,
  mapType,
  remark
) {
  if (this.db) {
    const allinOne = `INSERT INTO raster_${mapType} VALUES ($id, $x, $y, $z, $type, $tile, $remark) ON CONFLICT(id) DO UPDATE SET tile=$tile`;
    //创建插入语句
    const stmt = this.db.prepare(allinOne);
    stmt.run({
      id: `${x}${y}${z}${mapSource}`,
      x: x,
      y: y,
      z: z,
      type: mapSource,
      tile: tile,
      remark,
    });
  }
};

/**
 *
 * @param {*} x
 * @param {*} y
 * @param {*} z
 * @param {*} mapSource
 * @param {*} mapType
 * @returns {Promise}
 */
database.prototype.getTile = function (x, y, z, mapSource, mapType, callback) {
  if (this.db) {
    const id = `${x}${y}${z}${mapSource}`;
    const sql = `select tile,remark from raster_${mapType} where id=?`;
    const stmt = this.db.prepare(sql);
    const tile = stmt.get(id);
    callback({
      res: tile,
    });
  } else {
    callback({
      er: "db is null",
    });
  }
};

module.exports = {
  database,
};
