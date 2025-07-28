import request from '@/utils/request';

export function login(data) {
  return request({
    url: '/auth/user/login',
    method: 'post',
    data,
  });
}

export const loginOut = () => {
  return request({
    url: '/auth/user/loginOut',
    method: 'post',
  });
};
