export const Format = {
  DDD: 0x00,
  DMS: 0x01,
  DM: 0x02,
  NWSE: 0x03,
  MAX: 0x04,
};

export const getNextFormat = (form) => {
  let next = form + 1;
  if (next === Format.MAX) {
    next = Format.DDD;
  }
  return next;
};

export const getFormatStr = (form) => {
  const strs = ['DDD', 'DMS', 'DM', 'NWSE'];
  return strs[form];
};

export const getDDMMSS = (val) => {
  if (val !== null && val !== '') {
    const trueVal = Number(val < 0 ? -val : val);
    const degree = parseInt(trueVal, 10);
    const minute = parseInt(trueVal * 60 - degree * 60, 10);
    const second = (trueVal * 3600 - degree * 3600 - minute * 60).toFixed(2);
    return { degree, minute, second: Number(second) };
  }
  return {};
};

export const getDDMM = (val) => {
  if (val !== null && val !== '') {
    const trueVal = Number(val < 0 ? -val : val);
    const degree = parseInt(trueVal, 10);
    const minute = (trueVal * 60 - degree * 60).toFixed(5);
    return { degree, minute: Number(minute) };
  }
  return {};
};

export const getFloat = (dms, neg) => {
  const { degree, minute, second } = dms;
  let result = 0;
  result = degree + Number(minute) / 60 + (second !== undefined ? Number(second) / 3600 : 0);
  return Number(neg ? (-result).toFixed(6) : result.toFixed(6));
};

export const gps2Degree = (gpsStr) => {
  let str = gpsStr.toLowerCase();
  str = str.replace(/\s+/g, '');
  const tempStrArray = [];
  let flag = 1;
  const strLength = gpsStr.length;
  const degree = [];
  let dir = null;
  let tempcount = 0;
  let tempString = '';
  let tempPointFlag = 0;
  if (str[0] === 'w' || str[0] === 's') {
    flag = -1;
    [dir] = str;
  } else if (str[0] === 'n' || str[0] === 'e') {
    flag = 1;
    [dir] = str;
  } else if (str[strLength - 1] === 'n' || str[strLength - 1] === 'e') {
    flag = 1;
    dir = str[strLength - 1];
  } else if (str[strLength - 1] === 'w' || str[strLength - 1] === 's') {
    flag = -1;
    dir = str[strLength - 1];
  }
  for (let i = 0; i <= strLength; i += 1) {
    if (str[i] >= '0' && str[i] <= '9') {
      tempString += str[i];
    } else if (str[i] === '.') {
      tempStrArray[tempcount] = tempString;
      tempString = '';
      tempcount += 1;
      tempStrArray[tempcount] = '.';
      tempPointFlag = 1;
      tempcount += 1;
    } else if (tempString.length > 0) {
      tempStrArray[tempcount] = tempString;
      tempString = '';
      tempcount += 1;
    }
  }
  if (tempPointFlag === 0) {
    const num1 = parseInt(tempStrArray[0], 10);
    const num2 = parseInt(tempStrArray[1] || 0, 10);
    const num3 = parseInt(tempStrArray[2] || 0, 10);
    degree[1] = (num1 + num2 / 60 + num3 / (60 * 60)).toFixed(6);
  } else if (tempPointFlag === 1) {
    const num1 = parseInt(tempStrArray[0], 10);
    const num2 = parseFloat(`${tempStrArray[1]}.${tempStrArray[3]}`, 10);
    degree[1] = (num1 + num2 / 60).toFixed(6);
  }
  degree[1] *= flag;
  degree[0] = dir;
  return degree;
};

export const degree2DMS = (degreeStr, dirStr) => {
  let str = degreeStr.toLowerCase();
  str = str.replace(/\s+/g, '');
  const dir = dirStr.toUpperCase();
  const strLength = str.length;

  const tempStrArray = [];
  let tempString = '';
  let tempCount = 0;
  let tempPointFlag = 0;

  let gpsDMS = null;

  for (let i = 0; i <= strLength; i += 1) {
    if (str[i] >= '0' && str[i] <= '9') {
      tempString += str[i];
    } else if (str[i] === '.') {
      tempStrArray[tempCount] = tempString;
      tempString = '';
      tempCount += 1;
      tempStrArray[tempCount] = '.';
      tempPointFlag = 1;
      tempCount += 1;
    } else if (tempString.length > 0) {
      tempStrArray[tempCount] = tempString;
      tempString = '';
      tempCount += 1;
    }
  }
  if (tempPointFlag === 1) {
    const num1 = tempStrArray[0];
    let num2 = parseFloat(`0${tempStrArray[1]}${tempStrArray[2]}`, 10) * 60;
    const num3 = parseInt(parseFloat((num2 - parseInt(num2, 10)) * 60, 10), 10);
    num2 = parseInt(num2, 10);
    gpsDMS = `${dir} ${num1}°${num2}′${num3}″`;
  }
  return gpsDMS;
};

export const degree2DM = (degreeStr, dirStr) => {
  let str = degreeStr.toLowerCase();
  str = str.replace(/\s+/g, '');
  const dir = dirStr.toUpperCase();
  const strLength = str.length;

  const tempStrArray = [];
  let tempString = '';
  let tempCount = 0;
  let tempPointFlag = 0;

  let gpsDM = null;

  for (let i = 0; i <= strLength; i += 1) {
    if (str[i] >= '0' && str[i] <= '9') {
      tempString += str[i];
    } else if (str[i] === '.') {
      tempStrArray[tempCount] = tempString;
      tempString = '';
      tempCount += 1;
      tempStrArray[tempCount] = '.';
      tempPointFlag = 1;
      tempCount += 1;
    } else if (tempString.length > 0) {
      tempStrArray[tempCount] = tempString;
      tempString = '';
      tempCount += 1;
    }
  }
  if (tempPointFlag === 1) {
    const num1 = tempStrArray[0];
    const num2 = parseFloat(`0${tempStrArray[1]}${tempStrArray[2]}`, 10) * 60;
    gpsDM = `${dir} ${num1}°${num2}′`;
  }
  return gpsDM;
};
