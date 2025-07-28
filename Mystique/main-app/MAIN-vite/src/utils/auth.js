/*
 * 因为js-cookie在electron端无法使用，暂时修改为sessionStorage存储用户信息
 */

const TokenKey = 'User-Token';

// export function getSessionToken() {
//   return sessionStorage.getItem(TokenKey) || localStorage.getItem(TokenKey);
// }

export function setToken(token, remember) {
  sessionStorage.setItem(TokenKey, token);
  if (remember) {
    localStorage.setItem(TokenKey, token);
  }
}

export function removeToken() {
  sessionStorage.removeItem(TokenKey);
  localStorage.removeItem(TokenKey);
}
