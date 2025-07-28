/* eslint-disable no-restricted-syntax */
/* eslint-disable no-param-reassign */
function PrefixInteger(num, n) {
  return (Array(n).join(0) + num).slice(-n);
}

export const getTimSpanByTicks = (ticks) => {
  const span = {
    hours: parseInt(ticks / 3600, 10),
    minutes: parseInt((ticks % 3600) / 60, 10),
    seconds: ticks % 60,
  };
  const hStr = `${PrefixInteger(span.hours, 2)}:`; // span.hours > 0 ? `${PrefixInteger(span.hours, 2)}:` : '';
  return `${hStr}${PrefixInteger(span.minutes, 2)}:${PrefixInteger(span.seconds, 2)}`;
};

const getTimeSpanHMS = (timeStr1, timeStr2, percent) => {
  if (timeStr1 === undefined || timeStr2 === undefined) {
    return '';
  }
  const date1 = new Date(timeStr1 /* .replace(/-/g, '/') */);
  const date2 = new Date(timeStr2 /* .replace(/-/g, '/') */);
  const total = Math.ceil((((date1.getTime() - date2.getTime()) / 1000) * percent) / 100);
  const span = {
    hours: parseInt(total / 3600, 10),
    minutes: parseInt((total % 3600) / 60, 10),
    seconds: total % 60,
  };
  return `${PrefixInteger(span.hours, 2)}:${PrefixInteger(span.minutes, 2)}:${PrefixInteger(span.seconds, 2)}`;
};

const getTimeStamp = (timeStr) => {
  const date = new Date(timeStr /* .replace(/-/g, '/') */);
  return date.getTime();
};

// 格式化时间文本
export const stampDate = (s = new Date(), format = 'yyyy-MM-dd HH:mm:ss') => {
  const dtime = new Date(s);
  const o = {
    'M+': dtime.getMonth() + 1, // month
    'd+': dtime.getDate(), // day
    'H+': dtime.getHours(), // hour
    'm+': dtime.getMinutes(), // minute
    's+': dtime.getSeconds(), // second
    'q+': Math.floor((dtime.getMonth() + 3) / 3), // quarter
    'f+': dtime.getMilliseconds(), // millisecond
    S: dtime.getMilliseconds(), // millisecond
  };
  if (/(y+)/.test(format)) format = format.replace(RegExp.$1, `${dtime.getFullYear()}`.substr(4 - RegExp.$1.length));
  for (const k in o)
    if (new RegExp(`(${k})`, 'g').test(format))
      format = format.replace(RegExp.$1, RegExp.$1.length === 1 ? o[k] : `00${o[k]}`.substr(`${o[k]}`.length));
  return format;
};

export default {};
export { getTimeStamp, getTimeSpanHMS };
