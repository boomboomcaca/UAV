/*
 * @Description:
 * @Date: 2019-10-14 14:26:15
 * @LastEditTime: 2020-09-27 10:38:06
 */
/* eslint-disable */

import axios from '../utils/axios';

export const login = ({ account, password }) => {
  const data = {
    account,
    password,
  };
  return axios({
    url: '/auth/user/login',
    method: 'post',
    data,
  });
};

export const getToken = (params) => {
  return axios({
    url: '/auth/user/getToken',
    method: 'get',
    params,
  });
};

export const refreshLogin = (token) => {
  const data = {
    token,
  };
  return axios({
    url: '/auth/user/refreshToken',
    method: 'post',
    data,
  });
};

export const getUserPermission = (params) => {
  return axios({
    url: '/auth/user/getUserPermission',
    method: 'get',
    params,
  });
};

export const changePassword = (data) => {
  return axios({
    url: '/api/account/changePassword',
    method: 'post',
    data,
  });
};

export const logout = () => {
  return axios({
    url: '/auth/user/logout',
    method: 'post',
  });
};

export const getVersion = () => {
  return axios({
    url: '/manager/runtime/getVersion',
    method: 'get',
  });
};

// 上传授权文件
// 'http://192.168.102.191:12777/License/upload'
// 下载License文件
// 'http://192.168.102.191:12777/License/downloadLicense', // '/License/downloadLicense',
export const downloadLicense = () => {
  return axios({
    url: '/License/downloadLicense', // '/License/downloadLicense'
    method: 'GET',
  });
};

// 获取云端授权信息
// 'http://192.168.102.191:12777/License/getLicenseInfo', // ' /License​/getLicenseInfo',
export const getLicenseInfo = () => {
  return axios({
    url: '/License/getLicenseInfo', // '/License​/getLicenseInfo',
    method: 'GET',
  });
};

/**
 * 获取系统运行时长
 * @returns
 */
export const getSysRunningTime = () => {
  return axios({
    url: '/manager/runtime/get',
    method: 'GET',
  });
};

export function getWorkInfo() {
  return axios({
    url: 'sys/workloadStatistics/getInfo',
    method: 'GET',
  });
}
