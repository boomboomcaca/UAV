const math = require('lodash/math');

const EARTH_RADIUS = 6378.137;
const PI = 3.14159265;
const pixelLength = 512; // 扩散像素长度
const regionRadius = 25; // 单点定位范围半径，单位米
const multiRegionRadius = 500; // 多点定位范围半径，单位米

function rad(d) {
  return (d * Math.PI) / 180.0;
}

function getDistance(lat1, lng1, lat2, lng2) {
  const radLat1 = rad(lat1);
  const radLat2 = rad(lat2);
  const a = radLat1 - radLat2;
  const b = rad(lng1) - rad(lng2);
  let distance =
    2 *
    Math.asin(
      Math.sqrt(
        Math.sin(a / 2) ** 2 +
          Math.cos(radLat1) * Math.cos(radLat2) * Math.sin(b / 2) ** 2
      )
    );
  distance *= EARTH_RADIUS;
  distance = Math.round(distance * 10000) / 10000;
  return distance;
}

function getAround(lat, lon, radius) {
  const degree = (24901 * 1609) / 360.0;
  const radiusMile = radius;

  const dpmLat = 1 / degree;
  const radiusLat = dpmLat * radiusMile;
  const minLat = lat - radiusLat;
  const maxLat = Number.parseFloat(lat) + radiusLat;

  const mpdLng = degree * Math.cos(lat * (PI / 180));
  const dpmLng = 1 / mpdLng;
  const radiusLng = dpmLng * radiusMile;

  const minLng = lon - radiusLng;
  const maxLng = Number.parseFloat(lon) + radiusLng;

  return [minLat, minLng, maxLat, maxLng];
}

function calHeatmapData(
  lngLatDatas,
  leftTopLongitude,
  leftTopLatitude,
  rightBottomLongitude,
  rightBottomLatitude
) {
  const pixelLngInterval =
    (rightBottomLongitude - leftTopLongitude) / pixelLength;
  const pixelLatInterval =
    (leftTopLatitude - rightBottomLatitude) / pixelLength;

  const optimalLngs = [];
  const optimalLats = [];
  const heatmapLngLat = [];
  for (let i = 0; i < pixelLength; i += 32) {
    const itemLongitude = leftTopLongitude + i * pixelLngInterval;
    for (let j = 0; j < pixelLength; j += 32) {
      const itemLatitude = leftTopLatitude - j * pixelLatInterval;
      let itemWeight = 0;
      for (let k = 0; k < lngLatDatas.length; k += 2) {
        const distance = getDistance(
          itemLatitude,
          itemLongitude,
          lngLatDatas[k + 1],
          lngLatDatas[k]
        );
        if (distance <= 0.5) {
          itemWeight += 1;
        }
      }
      if (itemWeight === lngLatDatas.length / 2) {
        optimalLngs.push(itemLongitude);
        optimalLats.push(itemLatitude);
      }
      if (itemWeight > 0) {
        heatmapLngLat.push({
          longitude: itemLongitude,
          latitude: itemLatitude,
          value: itemWeight / (lngLatDatas.length / 2),
        });
      }
    }
  }
  const optimalLng = math.mean(optimalLngs);
  const optimalLat = math.mean(optimalLats);
  const result = {
    optimalLng,
    optimalLat,
    heatmapLngLat,
  };
  return result;
}

async function getHeatmapData(lngLatDatas) {
  let result;
  if (lngLatDatas.length === 2) {
    const around = getAround(lngLatDatas[1], lngLatDatas[0], regionRadius);
    result = calHeatmapData(
      lngLatDatas,
      around[1],
      around[2],
      around[3],
      around[0]
    );
  } else {
    let minLongitude = lngLatDatas[0];
    let maxLatitude = lngLatDatas[1];
    let maxLongitude = lngLatDatas[0];
    let minLatitude = lngLatDatas[1];
    for (let i = 0; i < lngLatDatas.length; i += 2) {
      if (minLongitude > lngLatDatas[i]) {
        minLongitude = lngLatDatas[i];
      }
      if (maxLongitude < lngLatDatas[i]) {
        maxLongitude = lngLatDatas[i];
      }
      if (minLatitude > lngLatDatas[i + 1]) {
        minLatitude = lngLatDatas[i + 1];
      }
      if (maxLatitude < lngLatDatas[i + 1]) {
        maxLatitude = lngLatDatas[i + 1];
      }
    }
    const aroundL = getAround(maxLatitude, minLongitude, multiRegionRadius);
    const aroundR = getAround(minLatitude, maxLongitude, multiRegionRadius);
    result = calHeatmapData(
      lngLatDatas,
      aroundL[1],
      aroundL[2],
      aroundR[3],
      aroundR[0]
    );
  }
  return result;
}

module.exports = {
  getHeatmapData,
};
