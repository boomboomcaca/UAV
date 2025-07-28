import axios from './axios.request';

export function login(data) {
  return axios.request({
    url: '/auth/user/login',
    method: 'post',
    data,
  });
}

export const getList = (ur, data) => {
  let param = '';
  Object.keys(data).forEach((prop) => {
    param += `${prop}=${data[prop]}&`;
  });
  param = param.substring(0, param.length - 1);
  window.console.log(`${ur}/getList?${param}`);
  return axios.request({
    url: `${ur}/getList?${param}`,
    method: 'get',
  });
};

export const getByID = (ur, id) => {
  return axios.request({
    url: `${ur}/get?id=${id}`,
    method: 'get',
  });
};
