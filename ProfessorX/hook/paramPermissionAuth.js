/* eslint-disable eqeqeq */
/* eslint-disable no-restricted-globals */
// 该文件存在需要对字符串和数字 进行等于判断，故使用==
const {
  resError,
  dataTypeEnum,
  getDataType,
  isJSON,
  getLogger,
} = require('../helper/repositoryBase');

// map():返回一个新的Array，每个元素为调用func的结果。新数组的长度和原来的是一样的，他只不过是逐一对原来数据里的每个元素进行操作。

// filter():返回一个符合func条件的元素数组。筛选条件，把数组符合条件的放在新的数组里面返回。新数组和原来的数组长度不一定一样。

// some():返回一个boolean，判断是否有元素是否符合func条件。数组里面所有的元素有一个符合条件就返回true。

// every():返回一个boolean，判断每个元素是否符合func条件。数组里面所有的元素都符合才返回true。

// forEach():没有返回值，只是针对每个元素调用func   。循环数组。和for的用法一样的。

// 范围涉及，连续范围，离散范围。(连续范围要考虑边缘问题，大于小于或者小于等于)

const logger = getLogger('paramPermissionAuth');

/**
 * 检查string 类型
 *
 * @param {*} reqParamValueType
 * @param {*} reqParamValue
 * @param {*} permissionParamValue
 * @returns
 */
function checkPermissionStringType(
  reqParamValue,
  reqParamValueType,
  permissionParamValue
) {
  let paramPermissionSuccess = false;
  if (
    reqParamValueType == dataTypeEnum.string &&
    reqParamValue == permissionParamValue
  ) {
    paramPermissionSuccess = true;
    // console.log(`${p} ${paramPermissionSuccess} string`)
  }
  return paramPermissionSuccess;
}
/**
 * 检查数组类型
 *
 * @param {*} reqParamValueType
 * @param {*} reqParamValue
 * @param {*} permissionParamValue
 */
function checkPermissionArrayType(
  reqParamValue,
  reqParamValueType,
  permissionParamValue
) {
  let paramPermissionSuccess = false;
  if (
    reqParamValueType == dataTypeEnum.string &&
    permissionParamValue.includes(reqParamValue)
  ) {
    paramPermissionSuccess = true;
  } else if (reqParamValueType == dataTypeEnum.array) {
    if (
      permissionParamValue.some((s) => {
        return reqParamValue.includes(s);
      })
    ) {
      paramPermissionSuccess = true;
    }
  }
  return paramPermissionSuccess;
}

function addSeconds(time, number) {
  const date = new Date(time);
  date.setSeconds(date.setSeconds + number);
  return date;
}
// 5>3 那我的最小值
function paramGt(value) {
  if (isNaN(value) && !isNaN(value)) {
    return addSeconds(value, 1);
  }
  return parseFloat(value) + 0.001;
}
// 5<8 那我的大致7.99999
function paramLt(value) {
  if (isNaN(value) && !isNaN(value)) {
    return addSeconds(value, -1);
  }
  return parseFloat(value) - 0.001;
}
// 检查 参数类型是否是日期
function checkParamTypeDate(value) {
  return isNaN(value) && !isNaN(Date.parse(value));
}
/**
 *参数范围转换
 *
 * @param {*} paramValue
 */
function getParamRange(paramValue) {
  const maxDate = '9999-12-31';
  const minDate = '1900-1-1';
  const maxNumber = 999999999999;
  const minNumber = -99999999999;
  let maxValue = maxNumber;
  let minValue = minNumber;
  let paramRange = [];

  const isParamTypeDate = checkParamTypeDate(paramValue.value[0]);

  if (isParamTypeDate) {
    maxValue = maxDate;
    minValue = minDate;
  }
  // 其实我还需要判断参数类型
  if (paramValue.value.length === 1) {
    const operator = paramValue.operator[0];
    const value = paramValue.value[0];
    switch (operator) {
      case 'gt':
        paramRange = [paramGt(value), maxValue];
        break;
      case 'gte':
        paramRange = [value, maxValue];
        break;
      case 'lt':
        paramRange = [paramLt(value), maxValue];
        break;
      case 'lte':
        paramRange = [minValue, value];
        break;
      default:
        break;
    }
  } else if (paramValue.value.length === 2) {
    // let value = paramValue.value[0];
    // let value1 = paramValue.value[1];
    // const operator = paramValue.operator[0];
    // const operator1 = paramValue.operator[1];
    let [value, value1] = paramValue.value;
    const [operator, operator1] = paramValue.operator;

    if (
      (isParamTypeDate && new Date(value) < new Date(value1)) ||
      (!isParamTypeDate && parseFloat(value) < parseFloat(value1))
    ) {
      // 处理临界值问题
      if (operator === 'gt') {
        value = paramGt(value);
      }
      if (operator1 === 'lt') {
        value1 = paramLt(value1);
      }
      paramRange = [value, value1];
      // 第一个值小第二个值大
    } else {
      if (operator1 === 'gt') {
        value = paramGt(value);
      }
      if (operator === 'lt') {
        value1 = paramLt(value1);
      }
      paramRange = [value1, value];
    }
  }
  return paramRange;
}

function checkParamValuePermission(permissionParamValue, reqParamValue) {
  if (
    typeof permissionParamValue.value === 'undefined' ||
    typeof permissionParamValue.operator === 'undefined'
  ) {
    // resError 不需要改
    resError({ message: '权限参数配置错误' });
  }
  let reqParamValueRange = [];
  if (
    getDataType(reqParamValue) === dataTypeEnum.string ||
    getDataType(reqParamValue) === dataTypeEnum.number
  ) {
    reqParamValueRange = [reqParamValue, reqParamValue];
  } else {
    reqParamValueRange = getParamRange(reqParamValue);
  }
  const permissionParamRange = getParamRange(permissionParamValue);

  // 表示该操作符不需要权限验证
  if (permissionParamRange.length <= 0) {
    return true;
  }
  const isParamTypeDate = checkParamTypeDate(permissionParamValue.value[0]);

  // logger.trace(`Permission ${permissionParamRange} request ${reqParamValueRange}`);
  // 日期不能直接比较大小
  if (
    isParamTypeDate &&
    new Date(reqParamValueRange[0]) >= new Date(permissionParamRange[0]) &&
    new Date(reqParamValueRange[1]) <= new Date(permissionParamRange[1])
  ) {
    return true;
  }
  if (
    !isParamTypeDate &&
    reqParamValueRange[0] >= permissionParamRange[0] &&
    reqParamValueRange[1] <= permissionParamRange[1]
  ) {
    return true;
  }
  return false;
}
/**
 *检查对象类型
 *
 * @param {*} reqParamValueType
 * @param {*} reqParamValue
 * @param {*} permissionParamValue
 */
function checkPermissionObjectType(
  reqParamValue,
  reqParamValueType,
  permissionParamValue
) {
  let paramPermissionSuccess = false;
  switch (reqParamValueType) {
    case dataTypeEnum.string:
    case dataTypeEnum.object:
      // 其实我需要解析出数据库存储的界限值
      paramPermissionSuccess = checkParamValuePermission(
        permissionParamValue,
        reqParamValue
      );
      break;
    case dataTypeEnum.array:
      // todo:一个多个 some every 判断的是所有条件的一个符合则通过
      paramPermissionSuccess = reqParamValue.some((s) => {
        return checkParamValuePermission(permissionParamValue, s);
      });
      break;
    default:
      break;
  }

  return paramPermissionSuccess;
}
// 参数权限检查
exports.paramPermissionAuth = async (req) => {
  // console.log("开始进行权限参数验证");
  // const { tableName } = req.urlPermission;
  // const { dataPermission } = req;
  const { params } = req.urlPermission;
  // 统一处理GET请求数据也可以在boyd 里面获取
  const reqData = req.body;
  if (params === '' || !isJSON(params)) {
    logger.debug('参数权限不需要检查');
    return;
  }
  const paramObj = JSON.parse(params);
  let paramPermissionSuccess = true;
  const paramKeys = Object.keys(paramObj);
  paramKeys.forEach((p) => {
    if (
      Object.entries(reqData).find((s) => s[0] == p) &&
      paramPermissionSuccess
    ) {
      const reqParamValue = reqData[p];
      const permissionParamValue = paramObj[p];
      const reqParamValueType = getDataType(reqParamValue);
      const permissionParamValueType = getDataType(permissionParamValue);
      switch (permissionParamValueType) {
        case dataTypeEnum.string:
          paramPermissionSuccess = checkPermissionStringType(
            reqParamValue,
            reqParamValueType,
            permissionParamValue
          );
          break;
        case dataTypeEnum.array:
          paramPermissionSuccess = checkPermissionArrayType(
            reqParamValue,
            reqParamValueType,
            permissionParamValue
          );
          break;
        case dataTypeEnum.object:
          paramPermissionSuccess = checkPermissionObjectType(
            reqParamValue,
            reqParamValueType,
            permissionParamValue
          );
          break;
        default:
          break;
      }
      if (paramPermissionSuccess) {
        logger.debug(`${p} 参数权限验证通过`);
        return;
      }
      logger.debug(`${p} 参数权限验证不通过`);
    }
  });

  // if
  if (!paramPermissionSuccess) {
    resError({ message: '参数权限不足' });
  }
};
