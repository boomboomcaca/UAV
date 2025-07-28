/* eslint-disable no-param-reassign */
export const getGroup = (groups, r, key) => {
  const f = (s) => {
    return s[key];
  };
  const group = JSON.stringify(f(r));
  groups[group] = groups[group] || 0;
  groups[group] += 1;
};

export const getGroupCount = (groups, r, key, value = null) => {
  const val = value === null ? key : value;
  const find = groups.find((g) => {
    return g.key === r[key] && g.value === r[val];
  });
  if (find) {
    find.count += 1;
  } else {
    const item = {
      id: `${r[key]}_${r[val]}`,
      key: r[key],
      value: r[val],
      count: 1,
    };
    groups.push(item);
  }
};
