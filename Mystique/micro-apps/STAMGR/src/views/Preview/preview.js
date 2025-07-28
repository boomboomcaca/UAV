import { statistics } from '@/api/cloud';

/* eslint-disable no-param-reassign */
export const getModules = (edges) => {
  let modules = [];
  edges.forEach((e) => {
    if (e && e.modules) {
      modules = [
        ...modules,
        ...e.modules
          .filter((m) => {
            return m.moduleType === 'device';
          })
          .map((m) => {
            return { state: m.moduleState, id: m.id, edgeId: e.edgeId };
          }),
      ];
    }
  });
  return modules;
};

export const updateModules = (modules, update) => {
  const ret = modules?.map((item) => {
    if (item.edgeId === update.edgeId) {
      update.dataCollection?.forEach((info) => {
        // 模块状态
        if (info.type === 'moduleStateChange') {
          if (item.id === info.id) {
            item.state = info.state;
          }
        }
      });
    }
    return item;
  });
  return ret;
};

// TODO 根据站点下的设备状态获取统计信息
export const getStatus = (modules) => {
  const f = (s) => {
    return s.state;
  };
  const groups = {};
  modules?.forEach((o) => {
    const group = JSON.stringify(f(o));
    groups[group] = groups[group] || [];
    groups[group].push(o);
  });
  const ret = {};
  Object.keys(groups)
    .map((group) => {
      return { name: JSON.parse(group), data: groups[group] };
    })
    .forEach((r) => {
      ret[r.name] = r.data.length;
    });
  return ret;
};

export const getStatistics = async () => {
  const res = await statistics();
  if (res.result) {
    const { stationStatus } = res.result;
    return stationStatus;
  }
  return {};
};
