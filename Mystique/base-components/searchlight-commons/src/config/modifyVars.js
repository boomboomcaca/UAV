// 主要
const main = {
  '@primary-color': '#3CE5D3',
  '@border-radius-base': '3px',
};

// 字体大小行高配置
const fontSizeConfig = {
  '@font-size': 14,
};

/**
 * pt转px
 *
 * @param {*} obj
 * @returns
 */
const pt2px = (obj) => {
  const r = {};
  Object.entries(obj).forEach((i) => {
    const [k, v] = i;
    r[k] = `${v / 0.75}px`;
  });

  return r;
};

const modifyVars = {
  ...main,
  ...pt2px(fontSizeConfig),
};

export default modifyVars;
