export const pointOutArea = (point, area) => {
  if (!point || !area || !area.point1 || !area.point2) return false;
  if (point.x < area.point1.x || point.x > area.point2.x || point.y < area.point1.y || point.y > area.point2.y) {
    return true;
  }
  return false;
};

export const getArea = (div) => {
  if (!div) return {};
  const rect = div.getBoundingClientRect();
  return {
    point1: {
      x: rect.left,
      y: rect.top,
    },
    point2: {
      x: rect.right,
      y: rect.bottom,
    },
  };
};
