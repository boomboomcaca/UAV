const { config } = require('../helper/repositoryBase');
// map():返回一个新的Array，每个元素为调用func的结果。新数组的长度和原来的是一样的，他只不过是逐一对原来数据里的每个元素进行操作。

// filter():返回一个符合func条件的元素数组。筛选条件，把数组符合条件的放在新的数组里面返回。新数组和原来的数组长度不一定一样。

// some():返回一个boolean，判断是否有元素是否符合func条件。数组里面所有的元素有一个符合条件就返回true。

// every():返回一个boolean，判断每个元素是否符合func条件。数组里面所有的元素都符合才返回true。

// forEach():没有返回值，只是针对每个元素调用func   。循环数组。和for的用法一样的。

// 范围涉及，连续范围，离散范围。(连续范围要考虑边缘问题，大于小于或者小于等于)
// 参数转换
// 获取参数名称和操作符
function getParamNameAndOperator(paramName) {
  // 会有问题gt lte 是无法区分的！
  // 会有问题 gt 和 gte 无法区分
  // createDate.gte
  const paramArray = paramName.split('.');
  if (paramArray.length < 2) {
    return null;
  }
  const param = config.operator.find((s) => s.key === paramArray[1]);
  if (!param) {
    return null;
  }
  // 返回参数名称和操作符
  return {
    paramName: paramArray[0],
    operator: param.key,
    dbOperator: param.value,
  };
}
// 获取新的请求参数结构
function getNewReq(p, newReqData, operator, value, dbOperator) {
  // 如果包含参数
  const tempReqData = newReqData;
  if (Object.entries(tempReqData).find((s) => s[0] === p)) {
    tempReqData[p].value.push(value);
    tempReqData[p].operator.push(operator);
    tempReqData[p].dbOperator.push(dbOperator);
    // 把操作符也同时写到这里面去后面就不在需要判断了
  } else {
    // 同一个字段 .gte 和.lte 会被覆盖
    tempReqData[p] = {
      value: [value],
      operator: [operator],
      dbOperator: [dbOperator],
    };
  }
  return tempReqData;
}

exports.paramConvert = async (req) => {
  // 只进行参数部分转换Get请求 不管是get还是Post请求都只处理地址栏上的
  let reqData = req.query;
  // 如果是getList的请求,则会对Body 里面的内容进行参数解析！应为getList是查询请求
  // 查询请求
  if (req.url.includes('getList') && req.method === 'POST') {
    // 这样产生了新的对象
    reqData = { ...req.query, ...req.body };
    // 引用传值 修改reqData 等同于修改reqBody
    req.body = reqData;
  }
  const deleteField = [];
  let newReqData = {};
  // eslint-disable-next-line no-restricted-syntax
  for (const p in reqData) {
    // todo: 需要考虑是get请求还是Post请求 请求Body里面的东西本质上不应该处理
    if (config.notConvertField.find((s) => s === p)) {
      continue;
    }
    const res = getParamNameAndOperator(p);
    if (res != null) {
      newReqData = getNewReq(
        res.paramName,
        newReqData,
        res.operator,
        reqData[p],
        res.dbOperator
      );
      deleteField.push(p);
    } else if (typeof reqData[p] === 'string' && reqData[p].includes(',')) {
      reqData[p] = reqData[p].split(',');
    }
  }
  // eslint-disable-next-line guard-for-in
  // eslint-disable-next-line no-restricted-syntax
  for (const p in newReqData) {
    reqData[p] = newReqData[p];
  }
  deleteField.forEach((item) => {
    delete reqData[item];
  });
  // 最后还是应该再次进行赋值
};
