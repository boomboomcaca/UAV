export const freq = 'frequency';
export const ifbw = ['ifBandwidth', 'span'];

export const findParam = (parameters, filter) => {
  let find = null;
  if (filter instanceof Array) {
    for (let index = 0; index < filter.length; index += 1) {
      const element = filter[index];
      find = parameters.find((p) => {
        return p.name === element;
      });
      if (find) break;
    }
  } else {
    find = parameters.find((p) => {
      return p.name === filter;
    });
  }
  return find;
};
