/* eslint-disable no-restricted-syntax */
/* eslint-disable no-param-reassign */

import { message } from 'dui';

const showImage = (domID, callback) => {
  const elem = document.getElementById(domID);
  if (window.App && window.App.screenshot && elem) {
    window.App.screenshot(elem)
      .then((res) => {
        // window.console.log('screenshot.filePath', res);
        callback(res.uri);
      })
      .catch((e) => {
        window.console.log(e);
        message.error('截图失败！');
        callback(null);
      });
  } else {
    message.error('当前设备不支持截图！');
    callback(null);
  }
};

export const downloadImage = (url, name) => {
  if (window.App && window.App.saveFile) {
    window.App.saveFile({ fileName: name, dataString: url, utf8: false, type: 'screenshot' }).catch((err) => {
      message.error('保存截图文件异常！');
    });
  } else {
    message.error('保存截图文件异常！');
  }
};

/**
 * 时间戳转换成时间
 * @param {*} n
 * @returns
 */
export const getDateTime = (t = +new Date(), sep1 = '-', sep2 = '.') => {
  return new Date(t + 8 * 3600 * 1000).toJSON().substr(0, 19).replace('T', 'T').replace(/-/g, sep1).replace(/:/g, sep2);
};

export const getFormatedDate = (s = new Date(), format = 'yyyyMMddHHmmss') => {
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
  if (/(y+)/.test(format)) {
    format = format.replace(RegExp.$1, `${dtime.getFullYear()}`.substr(4 - RegExp.$1.length));
  }
  for (const k in o) {
    if (new RegExp(`(${k})`, 'g').test(format))
      format = format.replace(RegExp.$1, RegExp.$1.length === 1 ? o[k] : `00${o[k]}`.substr(`${o[k]}`.length));
  }
  return format + o.S;
};

export default showImage;
