const fs = require("fs");

const tianditu_tk = "f12dce6502a9ba75333b2fe50d7611cb";
const tianditu_tk1 = "ece19938313e7b5ba019d1f8e7f08a46";
const mapbox_tk =
  "pk.eyJ1IjoiZ2lzb3IiLCJhIjoiY2xmdnkzdnJjMGJ0YzNkcXBsNmNsMTYyZiJ9.3mkw2ilDFTtQ2fx4uA3tjQ";

let provideNames;
let layerTypes;
let mapProviders;

function initDefault() {
  provideNames = {
    amap: "amap",
    google: "google",
    tianditu: "tianditu",
    complex: "complex",
  };

  layerTypes = {
    statelite: "statelite",
    roades: "roades",
    normal2d: "normal2d",
    terrain: "terrain",
  };
  const ampTileIndex = "x={x}&y={y}&z={z}";
  const tdtTileIndex = `SERVICE=WMTS&REQUEST=GetTile&VERSION=1.0.0&STYLE=default&TILEMATRIXSET=w&FORMAT=tiles&TILECOL={x}&TILEROW={y}&TILEMATRIX={z}`;
  mapProviders = {
    amap: {
      statelite: `http://webst03.is.autonavi.com/appmaptile?style=6&${ampTileIndex}`,
      roades: `http://webst01.is.autonavi.com/appmaptile?${ampTileIndex}&lang=zh_cn&size=2&scale=1&style=8`,
      normal2d: `http://webst02.is.autonavi.com/appmaptile?${ampTileIndex}&lang=zh_cn&size=2&scale=1&style=7`,
    },
    google: {
      statelite: `http://gac-geo.googlecnapps.cn/maps/vt?lyrs=s&${ampTileIndex}`,
      // "https://khms0.google.com/kh/v=944?x={x}&y={y}&z={z}",
    },
    tianditu: {
      statelite: `https://t0.tianditu.gov.cn/img_w/wmts?LAYER=img&${tdtTileIndex}&tk=${tianditu_tk}`,
      roades: `https://t6.tianditu.gov.cn/cia_w/wmts?LAYER=cia&${tdtTileIndex}&tk=${tianditu_tk1}`,
      normal2d: `https://t0.tianditu.gov.cn/vec_w/wmts?LAYER=vec&transparent=true&${tdtTileIndex}&tk=${tianditu_tk}`,
    },
    complex: {
      statelite: `http://gac-geo.googlecnapps.cn/maps/vt?lyrs=s&${ampTileIndex}`,
      // http://gac-geo.googlecnapps.cn/maps/vt?lyrs=s
      // "https://khms0.google.com/kh/v=944?x={x}&y={y}&z={z}",
      roades: `https://t6.tianditu.gov.cn/cia_w/wmts?LAYER=cia&${tdtTileIndex}&tk=${tianditu_tk1}`,
    },
    mapbox: {
      dem: `https://api.mapbox.com/v4/mapbox.terrain-rgb/{z}/{x}/{y}.pngraw?access_token=${mapbox_tk}`,
      buildings: `https://api.mapbox.com/v4/mapbox.mapbox-streets-v8,mapbox.mapbox-terrain-v2,mapbox.mapbox-bathymetry-v2/{z}/{x}/{y}.vector.pbf?sku=101Ok9jcm6ljW&access_token=${mapbox_tk}`,
    },
  };
}

// https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/16/27127/51974

function initProviders() {
  if (!provideNames) {
    if (fs.existsSync("./mapProviders.json")) {
      console.log("load config from json");
      const contentStr = fs.readFileSync("./mapProviders.json");
      mapProviders = JSON.parse(contentStr);
      provideNames = {};
      layerTypes = {};
      const names = Object.keys(mapProviders);
      names.forEach((item) => {
        provideNames[item] = item;
        const layers = Object.keys(mapProviders[item]);
        layers.forEach((l) => (layerTypes[l] = l));
      });
    } else {
      initDefault();
    }
  }
  return {
    provideNames,
    layerTypes,
    mapProviders,
  };
}

module.exports = { initProviders };
