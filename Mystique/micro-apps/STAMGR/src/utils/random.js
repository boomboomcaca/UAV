/* eslint-disable max-len */

/**
 * 生成四位随机数
 * @returns {string}
 */
export const createFour = () =>
  // eslint-disable-next-line no-bitwise
  (((1 + Math.random()) * 0x10000) | 0).toString(16).substring(1);

/**
 * 生成全局唯一标识符GUID
 * @returns {string}
 */
export const createGUID = () => {
  return `${createFour()}${createFour()}-${createFour()}-${createFour()}-${createFour()}-${createFour()}${createFour()}${createFour()}`;
};

const addZero = (num) => {
  if (Number(num).toString() !== 'NaN' && num >= 0 && num < 10) {
    return `0${Math.floor(num)}`;
  }
  return num.toString();
};

export const getGUIDTime = () => {
  const d = new Date();
  return (
    addZero(d.getHours()) +
    addZero(d.getMinutes()) +
    addZero(d.getSeconds()) +
    addZero(parseInt(d.getMilliseconds() / 10, 10))
  );
};

export const createTimeID = () => {
  return `${getGUIDTime()}-${createFour()}-${createFour()}-${createFour()}-${createFour()}${createFour()}${createFour()}`;
};

export const defaultGUID = '00000000-0000-0000-0000-000000000000';
