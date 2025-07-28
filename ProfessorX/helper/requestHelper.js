const rpn = require('request-promise-native');

exports.requestPost = async function requestPost(url, data, token = '') {
  const header = {};
  header.authorization = `Bearer ${token}`;
  const options = {
    method: 'POST',
    uri: url,
    headers: header,
    body: data,
    json: true, // 这个看你的参数而定
  };
  // 把授权的Header 加上
  const res = await rpn(options);
  return res;
};

exports.requestGet = async function requestGet(url, token = '') {
  const header = {};
  header.authorization = `Bearer ${token}`;
  const options = {
    method: 'GET',
    uri: url,
    headers: header,
    json: true, // 这个看你的参数而定
  };
  const res = await rpn(options);
  return res;
};
