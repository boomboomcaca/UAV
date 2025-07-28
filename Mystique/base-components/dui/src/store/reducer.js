export const UPDATE_THEME = 'UPDATE';

export const defaultState = {
  theme: {},
};

export const reducer = (state, action) => {
  const { type, payload } = action;
  switch (type) {
    case UPDATE_THEME:
      return { ...state, theme: payload };
    default:
      return state;
  }
};

// 占用文件夹而已 无他用
