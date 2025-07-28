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

export const scrollToAnchor = (id) => {
  document.getElementById(id)?.scrollIntoView({
    behavior: 'smooth',
    block: 'start',
  });
};

export const PropertyType = {
  NONE: 0,
  TEXT: 0x1000, // 普通文本类型
  RANGE_INTEGER: 0x2000, // 带范围的整数
  RANGE_DECIMAL: 0x2001, // 带范围的小数
  SELECTION_SHORT: 0x3000, // 选择项:len<=5
  SELECTION_LONG: 0x3001, // 选择项:len>5
  BOOL: 0x4000, // 开关项
  LIST: 0x5000, // 嵌套列表型
};
