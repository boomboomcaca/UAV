export const UPDATE = 'UPDATE';
export const VALIDATE = 'VALIDATE';
export const LABELSTYLE = 'LABELSTYLE';

export const defaultState = {
  data: null,
  validate: 0,
  labelStyle: null,
};

export const reducer = (state, action) => {
  const { type, payload } = action;
  const { data, validate } = state;
  switch (type) {
    case UPDATE:
      return { ...state, data: { ...data, ...payload } };
    case VALIDATE:
      return { ...state, validate: validate + 1 };
    case LABELSTYLE:
      return { ...state, labelStyle: payload };
    default:
      return state;
  }
};
