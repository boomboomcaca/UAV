export const sortPro = (data, keys = [], limit) => {
  // keys可以传一个数组
  const c = [];
  const d = {};
  for (const element of data) {
    let element_keyStr = '';
    const element_key = [];
    const element_keyObj = {};
    for (const key of keys) {
      element_key.push(element[key]);
      element_keyObj[key] = element[key];
    }
    element_keyStr = element_key.join('_');
    if (!d[element_keyStr]) {
      const a = limitHandle(limit, [element]);
      c.push({
        ...element_keyObj,
        children: a,
      });
      d[element_keyStr] = element;
    } else {
      for (const ele of c) {
        const isTrue = keys.some((key) => {
          return ele[key] != element[key];
        });
        if (!isTrue) {
          const a = limitHandle(limit, [element]);
          ele.children.push(...a);
        }
      }
    }
  }
  return c;
};

export const limitHandle = (l, arr) => {
  const a = arr.filter((i) => {
    return (
      i.startFrequency >= l.min &&
      i.startFrequency <= l.max &&
      i.stopFrequency >= l.min &&
      i.stopFrequency <= l.max &&
      l.stepItems.includes(i.stepFrequency)
    );
  });
  return a;
};

export const getStepList = (a) => {
  const arr = a.map((i) => {
    return { value: i, display: `${i} kHz` };
  });
  return arr;
};

const createFour = () =>
  // eslint-disable-next-line no-bitwise
  (((1 + Math.random()) * 0x10000) | 0).toString(16).substring(1);

/**
 * 生成全局唯一标识符GUID
 * @returns {string}
 */
export const createGUID = () => {
  // eslint-disable-next-line
  return `${createFour()}${createFour()}-${createFour()}-${createFour()}-${createFour()}-${createFour()}${createFour()}${createFour()}`;
};
