export const stationType = 'stationType';
export const stationaryCategory = 'stationaryCategory';
export const mobileCategory = 'mobileCategory';
export const movableCategory = 'movableCategory';
export const sensorCategory = 'sensorCategory';
export const portableCategory = 'portableCategory';
export const airCategory = 'airCategory';
export const mcsType = 'mcsType';
export const fmAddrType = 'fmAddrType';

// TODO 不要写字典数据
export const states = [
  { key: 'unknown', tag: '未知', color: '#ffffff99' },
  { key: 'disabled', tag: '禁用', color: '#ffffff99' },
  { key: 'idle', tag: '空闲', color: '#35E065' },
  { key: 'online', tag: '在线', color: '#35E065' },
  { key: 'busy', tag: '忙碌', color: '#FFD118' },
  { key: 'fault', tag: '故障', color: '#ff4c2a' },
  { key: 'offline', tag: '离线', color: '#ffffff99' },
];

export const getState = (state) => {
  if (state) {
    return states.find((s) => {
      return s.key === state;
    });
  }
  return states[0];
};
