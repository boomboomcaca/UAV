/* eslint-disable no-param-reassign */
import getConfig from '@/config';

const { webMapUrl, webMapFontUrl, mapType } = getConfig();

export const mapBoxId = 'f0943d29953d47c4850dc9ddf2c63105_mapBox';

export const getMapOptions = (id = null, position = null) => {
  return {
    container: id || mapBoxId,
    position: position || [104.072515, 30.551073],
    zoom: 6,
    coordinate: 'WGS84',
    mapType: mapType || 'amap',
    type: 'grid',
    customUrl: webMapUrl,
    fontUrl: webMapFontUrl,
    showMeasureTool: true,
    showScaleControl: false,
    baseToolPosition: 'top-right',
    measureToolPosition: 'top-right',
  };
};

export const getMapEdges = (edges, select) => {
  return edges.map((item) => {
    const ret = {
      longitude: item.longitude,
      latitude: item.latitude,
      type: item.type === 'mobileCategory' || item.type === 'movableCategory' ? 'mobile' : 'stationary',
      id: item.id,
      status: item.state === 'online' ? 'normal' : item.state || 'unknow',
      edgeID: item.id,
      name: item.name,
    };
    if (select && item.id === select.id) {
      ret.selected = 'Y';
    }
    return ret;
  });
};

export const updateEdges = (edges, update) => {
  const ret = edges?.map((item) => {
    if (item.id === update.edgeId) {
      update.dataCollection?.forEach((info) => {
        // 站点坐标
        if (info.type === 'gps') {
          item.longitude = info.longitude;
          item.latitude = info.latitude;
        }
        // 模块状态
        if (info.type === 'moduleStateChange') {
          const changedModule = item.modules?.find((m) => m.id === info.id);
          if (changedModule) {
            changedModule.moduleState = info.state;
          }
        }
        // 站点上下线状态
        if (info.type === 'edgeStateChange') {
          item.isActive = info.state === 'online';
        }
      });
    }
    return item;
  });
  return ret;
};

export const getPosition = (edges) => {
  let minLat = 90;
  let maxLat = -90;
  let minLng = 180;
  let maxLng = -180;

  edges.forEach((e) => {
    if (e.latitude && e.longitude) {
      if (e.latitude < minLat) minLat = e.latitude;
      if (e.latitude > maxLat) maxLat = e.latitude;
      if (e.longitude < minLng) minLng = e.longitude;
      if (e.longitude > maxLng) maxLng = e.longitude;
    }
  });

  let lat = 0;
  let lng = 0;

  if (maxLat > 0 && minLat < 0) {
    lat = (maxLat - Math.abs(minLat)) / 2;
  } else {
    lat = (minLat + maxLat) / 2;
  }

  if (maxLng > 0 && minLng < 0) {
    lng = (maxLng - Math.abs(minLng)) / 2;
  } else {
    lng = (minLng + maxLng) / 2;
  }
  return [lng, lat];
};
