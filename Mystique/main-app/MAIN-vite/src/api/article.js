/*
 * @Description:
 * @Date: 2019-10-14 14:26:15
 * @LastEditTime: 2020-07-23 15:10:22
 */
import axios from '../utils/axios';

// export function fetchList(query) {
//   return axios({
//     url: '/article/list',
//     method: 'get',
//     params: query,
//   });
// }

// export function fetchArticle(id) {
//   return axios({
//     url: '/article/detail',
//     method: 'get',
//     params: { id },
//   });
// }

// export function fetchPv(pv) {
//   return axios({
//     url: '/article/pv',
//     method: 'get',
//     params: { pv },
//   });
// }

// export function createArticle(data) {
//   return axios({
//     url: '/article/create',
//     method: 'post',
//     data,
//   });
// }

export function updateArticle(data) {
  return axios({
    url: '/article/update',
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
  window.console.log(param);
  return axios({
    url: `${ur}/getList?${param}`,
    method: 'get',
  });
};
