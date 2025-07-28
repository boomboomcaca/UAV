import request from './request';

export const getList = (ur, data) => {
  let param = '';
  Object.keys(data).forEach((prop) => {
    param += `${prop}=${data[prop]}&`;
  });
  param = param.substring(0, param.length - 1);
  return request({
    url: `${ur}/getList${param !== '' ? '?' : ''}${param}`,
    method: 'get',
  });
};

export const delList = (ur, data) => {
  return request({
    url: `${ur}/delList`,
    method: 'post',
    data,
  });
};
