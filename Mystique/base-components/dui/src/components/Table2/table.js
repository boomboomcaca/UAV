export const state = ['none', 'asc', 'desc'];

export const getNextState = (sta) => {
  const idx = state.indexOf(sta);
  let index = idx + 1;
  if (index > state.length - 1) {
    index = 0;
  }
  if (state[index] === 'none') return state[1];
  return state[index];
};
