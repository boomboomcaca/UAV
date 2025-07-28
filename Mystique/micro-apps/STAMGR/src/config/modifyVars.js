/**
 * less全局共享变量配置
 */
const main = {
  '@primary-color': '#3CE5D3',

  '@background-color-light': '#04041B',
  '@background-color-dark': '#04041B',

  '@success-color': '#35E065',
  '@warning-color': '#FFD118',
  '@error-color': '#FF4C2B',

  '@btn-primary-bg': '#353D5B',
  '@btn-focus-bg': '#242437',
  '@btn-hover-bg': '#242437',

  '@font-color-bg': '#242437',
  '@font-family': "'Source Han Sans CN','Roboto'",
  '@font-weight-regular': 400,
  '@font-weight-bold': 600,

  '@font-color-white100': '#FFFFFF',
  '@font-color-white80': 'rgba(255, 255, 255, 0.8)',
  '@font-color-white60': 'rgba(255, 255, 255, 0.6)',
  '@font-color-white50': 'rgba(255, 255, 255, 0.5)',
  '@font-color-white30': 'rgba(255, 255, 255, 0.3)',
  '@font-color-white20': 'rgba(255, 255, 255, 0.2)',
  '@font-color-white': '#FFFFFF',

  '@border-radius-base': '2px',
  '@border-radius-sm': '2px',
  '@border-radius-md': '4px',
  '@border-radius-bg': '8px',
};

// 字体大小行高配置
const fontSizeConfig = {
  '@font-h1-size': 18,
  '@font-h1-line-height': 26,

  '@font-h2-size': 16,
  '@font-h2-line-height': 24,

  '@font-h3-size': 14,
  '@font-h3-line-height': 20,

  '@font-h4-size': 12,
  '@font-h4-line-height': 18,

  '@font-h5-size': 12,
  '@font-h5-line-height': 16,
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
