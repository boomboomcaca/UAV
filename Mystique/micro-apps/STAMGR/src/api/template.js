import request from '@/utils/request';
import { getList } from './cloud';

export const getTemplates = (type) => {
  const data = {
    page: 1,
    rows: 1000,
    sort: 'desc',
    order: 'createTime',
    moduleType: type,
  };
  return getList('rmbt/template', data);
};

export const getTemplate = (id) => {
  return request({
    url: `/rmbt/template/getOne?id=${id}`,
    method: 'get',
  });
};

export const getDeviceParam = (id, url = 'rmbt/device') => {
  return request({
    url: `/${url}/getOne?id=${id}`,
    method: 'get',
  });
};
