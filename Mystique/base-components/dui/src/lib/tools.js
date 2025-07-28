const key = 'DUI_DEMO_KEY';

export function getDefaultDemoKey() {
  return sessionStorage.getItem(key);
}

export function saveDefaultDemoKey(token) {
  return sessionStorage.setItem(key, token);
}

export function removeToken() {
  return sessionStorage.removeItem(key);
}
export const createFour = () =>
  // eslint-disable-next-line no-bitwise
  (((1 + Math.random()) * 0x10000) | 0).toString(16).substring(1);

export const elementKey = () => {
  return `${createFour()}${createFour()}`;
};
