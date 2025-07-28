export const getStepList = (a) => {
  const arr = a.map((i) => {
    return { value: i, display: `${i} kHz` };
  });
  return arr;
};
