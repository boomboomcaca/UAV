import request from '@/utils/request';

export function getEdgesList() {
  return request({
    url: '/rmbt/edge/getEdgeList?isParam=false',
    method: 'get',
  });
}

export const getDictionary = (data) => {
  return request({
    url: `dic/dictionary/getDic?dicNo=${data.join(',')}`,
    method: 'get',
  });
};

export const getList = (ur, data) => {
  let param = '';
  Object.keys(data).forEach((prop) => {
    param += `${prop}=${data[prop]}&`;
  });
  param = param.substring(0, param.length - 1);
  window.console.log(param);
  return request({
    url: `${ur}/getList?${param}`,
    method: 'get',
  });
};

export const add = (ur, data) => {
  return request({
    url: `${ur}/add`,
    method: 'post',
    data,
  });
};

export const update = (ur, data) => {
  return request({
    url: `${ur}/update`,
    method: 'post',
    data,
  });
};

export const del = (ur, data) => {
  return request({
    url: `${ur}/del`,
    method: 'post',
    data,
  });
};

export const delList = (ur, data) => {
  return request({
    url: `${ur}/delList`,
    method: 'post',
    data,
  });
};

export function restartEdge(params) {
  return request({
    url: '/rmbt/edge/restart',
    method: 'post',
    data: params,
  });
}

export function statistics() {
  return request({
    url: '/rmbt/edge/getStatistics',
    method: 'get',
  });
}

export const updateGroupName = (data) => {
  return request({
    url: `/rmbt/edge/updateGroup`,
    method: 'post',
    data,
  });
};
