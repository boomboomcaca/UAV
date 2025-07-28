export const getQuery = () => {
  const { href } = window.location;
  const query = href.split('?');
  if (!query[1]) return {};

  const queryArr = decodeURI(query[1]).split('&');
  const queryObj = queryArr.reduce((prev, next) => {
    const item = next.split('=');
    return { ...prev, [item[0]]: item[1] };
  }, {});
  return queryObj;
};

export const groupBy = (array, f) => {
  const groups = {};
  array.forEach((o) => {
    const group = JSON.stringify(f(o));
    groups[group] = groups[group] || [];
    groups[group].push(o);
  });
  return Object.keys(groups).map((group) => {
    return { name: JSON.parse(group), data: groups[group] };
  });
};
