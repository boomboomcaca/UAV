import { getTimeStr } from '@/utils/time';
import error from './icons/error.png';
import loading from './icons/loading.png';
import success from './icons/success.png';

export const data = [
  { type: 0, msg: '开始自检', id: 1, device: '监测接收机', model: 'SR878111', time: '2020.12.30 23:40:20' },
  { type: 1, msg: '自检完成，结果正常', id: 1, device: '监测接收机', model: 'SR878111', time: '2020.12.30 23:40:21' },
  { type: 2, msg: '自检完成，结果异常', id: 1, device: '监测接收机', model: 'SR878111', time: '2020.12.30 23:40:22' },
];

let index = 0;
let itype = 0;
export const getData = (dataSource, round) => {
  let ret = null;
  if (index < dataSource.length) {
    const device = dataSource[index];
    const time = getTimeStr(new Date());
    if (itype === 0) {
      ret = { ...data[0], model: device.model, id: `${device.id}_${round}`, device: device.displayName, time };
      itype = device.moduleState === 'idle' || device.moduleState === 'busy' ? 1 : 2;
    } else {
      ret = { ...data[itype], model: device.model, id: `${device.id}_${round}`, device: device.displayName, time };
      itype = 0;
      index += 1;
    }
  } else index = 0;
  return { ret, index };
};

export const getIconByType = (type) => {
  switch (type) {
    case 0:
      return loading;
    case 1:
      return success;
    case 2:
      return error;
    default:
      break;
  }
  return null;
};
