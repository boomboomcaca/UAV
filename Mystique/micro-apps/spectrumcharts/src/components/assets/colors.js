export const defaultBlends = ["#FF0000", "#00FF00", "#0000FF"];
export const lightBlends = ["#FF3636", "#FFEB36", "#2DF16A", "#3B4FFF"];
const configs = {
  rainColors: null,
  max: "#FF0000",
  avg: "#e0965b",
  min: "#0000FF",
  real: "#00FF00",
  thr: "#da70d6",
  unit: "dBμV",
  streamTime: 20,
  rssiTime: 0,
};
const chartConfStoreName = "chart-conf-light";
const chartConfStoreName1 = "chart-conf-dark";
export const getChartConfig = (light) => {
  const confStr = localStorage.getItem(
    light ? chartConfStoreName : chartConfStoreName1
  );
  if (confStr) {
    const conf = JSON.parse(confStr);
    return conf;
  } else {
    const conf = { ...configs };
    conf.rainColors = light ? lightBlends : defaultBlends;
    return conf;
  }
};

export const saveChartConfig = (conf, light) => {
  if (light) {
    localStorage.setItem(chartConfStoreName, JSON.stringify(conf));
  } else {
    localStorage.setItem(chartConfStoreName1, JSON.stringify(conf));
  }
};
