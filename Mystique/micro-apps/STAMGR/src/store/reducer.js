export const UPDATE_QIANKUN_ACITON = 'UPDATE_QIANKUN_ACITON';

export const HEADER_RETURN = 'HEADER_RETURN';

export const LOCATION = 'LOCATION';

export const STEPABLE = 'STEPABLE';

export const defaultState = {
  actions: null,
  headereturn: 0,
  location: '',
  stepable: true,
};

export const reducer = (state, action) => {
  const { type, payload } = action;
  switch (type) {
    case UPDATE_QIANKUN_ACITON:
      return { ...state, actions: payload };
    case HEADER_RETURN: {
      const { headereturn } = state;
      return { ...state, headereturn: headereturn + 1 };
    }
    case LOCATION:
      return { ...state, location: payload };
    case STEPABLE:
      return { ...state, stepable: payload };
    default:
      return state;
  }
};
