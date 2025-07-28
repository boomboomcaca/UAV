/**
 * 登录信息管理，TokenKey应该与主应用保持一致
 */
const TokenKey = 'User-Token';

export function getToken() {
  return sessionStorage.getItem(TokenKey);
}

export function setToken(token) {
  return sessionStorage.setItem(TokenKey, token);
}

export function removeToken() {
  return sessionStorage.removeItem(TokenKey);
}
