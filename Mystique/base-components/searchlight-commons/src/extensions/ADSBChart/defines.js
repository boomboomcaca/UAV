import * as echarts from 'echarts';
import NP from 'number-precision';

const defaultOption = () => {
  return {
    stateAnimation: {
      duration: 0,
    },
    grid: {
      left: '45px',
      right: '20px',
      bottom: '30px',
      top: '18%',
    },
    title: {
      text: '',
      left: 'center',
      padding: [15, 0, 0, 0],
      textStyle: {
        color: '#FFFFFF',
        fontSize: '14px',
      },
    },
    tooltip: undefined,
    xAxis: {
      type: 'category',
      position: 'bottom',
      boundaryGap: false,
      data: [],
      axisLine: {
        show: true,
        onZero: false,
        lineStyle: {
          color: '#3CE5D3',
          opacity: 0.5,
          width: 2,
        },
      },
      axisLabel: {
        color: 'rgba(255, 255, 255, 0.5)',
      },
    },
    yAxis: {
      type: 'value',
      name: 'dBμV',
      nameTextStyle: {
        color: 'rgba(255, 255, 255, 0.5)',
        align: 'right',
      },
      nameGap: 10,
      min: -20,
      max: 80,
      axisLine: {
        show: true,
        lineStyle: {
          color: '#3CE5D3',
          opacity: 0.5,
          width: 2,
        },
      },
      axisTick: {
        show: true,
      },
      axisLabel: {
        color: 'rgba(255, 255, 255, 0.5)',
        formatter: undefined,
      },
      splitLine: {
        show: false,
      },
    },
    series: [],
  };
};

const yAxisSplitLine = {
  show: true,
  lineStyle: {
    type: 'dashed',
    dashOffset: 5,
    color: '#FFFFFF',
    opacity: 0.2,
  },
};

const gridStyle = {
  show: true,
  borderColor: 'transparent',
  backgroundColor: '#1a263e',
};

const markArea = {
  silent: true, // 不响应鼠标
  itemStyle: {
    color: 'rgba(60, 229, 211, 0.1)',
    borderType: [5, 10],
    borderWidth: [0, 2],
  },

  label: {
    position: 'bottom',
    color: '#3CE5D3',
    padding: 3,
    backgroundColor: '#181C36',
    fontSize: 16,
    fontWeight: '800',
  },
  data: [],
};

const markLine = {
  silent: true, // 不响应鼠标
  symbol: 'none',
  lineStyle: {
    width: 5,
    type: 'solid',
    color: '#3CE5D3',
  },
  data: [],
};

const markPointData = (markData, axisXValues, seriesData) => {
  const data = [];
  for (let i = 0; i < axisXValues.length; i += 1) {
    const axisXLabel = axisXValues[i];
    if (markData.includes(axisXLabel)) {
      data.push({
        coord: [axisXLabel, seriesData[i]],
      });
    }
  }
  return data;
};

const lineMarkPoint = (markData, axisXValues, seriesData) => {
  const data = markPointData(markData, axisXValues, seriesData);
  return {
    symbol: 'circle',
    symbolSize: 10,
    itemStyle: {
      color: '#FFD118',
      borderColor: '#FFFFFF',
      borderWidth: 2,
    },
    data,
  };
};

const barMarkPoint = (markData, axisXValues, seriesData) => {
  const data = markPointData(markData, axisXValues, seriesData);
  return {
    symbol:
      'path://M3 5.50035H3.45416L3.83205 5.24843L7.5 2.80313V13.1976L3.83205 10.7523L3.45416 10.5004H3L1.5 10.5004V5.50035L3 5.50035ZM9 14.1318C9 14.9305 8.10985 15.4069 7.4453 14.9639L3 12.0004L1 12.0004C0.45 12.0004 0 11.5504 0 11.0004V5.00035C0 4.45035 0.45 4.00035 1 4.00035L3 4.00035L7.4453 1.03682C8.10986 0.593782 9 1.07017 9 1.86887V14.1318ZM12.3132 2.24765C12.5907 1.94021 13.065 1.91599 13.3724 2.19357C14.9811 3.64591 15.9953 5.74334 15.9953 8.06211C15.9953 10.3132 15.0392 12.3413 13.5274 13.7908C13.2284 14.0775 12.7536 14.0675 12.467 13.7685C12.1803 13.4696 12.1903 12.9948 12.4893 12.7081C13.7258 11.5225 14.4953 9.87786 14.4953 8.06211C14.4953 6.18949 13.6774 4.48977 12.3673 3.30693C12.0598 3.02936 12.0356 2.5551 12.3132 2.24765ZM10.2492 4.11186C10.5576 3.83528 11.0317 3.86104 11.3083 4.16939C12.2366 5.20435 12.7996 6.5741 12.7996 8.06271C12.7996 9.47007 12.2965 10.7553 11.4708 11.7723C11.2097 12.0939 10.7373 12.143 10.4158 11.8819C10.0942 11.6208 10.0451 11.1485 10.3062 10.8269C10.9292 10.0595 11.2996 9.10378 11.2996 8.06271C11.2996 6.95764 10.8826 5.9412 10.1917 5.17095C9.91512 4.8626 9.94088 4.38843 10.2492 4.11186Z',
    symbolSize: 20,
    symbolOffset: [0, -15],
    itemStyle: {
      color: '#3CE5D3',
    },
    data,
  };
};

const lineSeries = (name, data, color, showArea) => {
  return {
    name,
    type: 'line',
    smooth: true,
    data,
    markArea: undefined,
    markLine: undefined,
    symbol: 'circle',
    symbolSize: 5,
    lineStyle: {
      color,
    },
    itemStyle: {
      color,
    },
    // selectedMode: false,
    // select: {
    //   disabled: true,
    // },
    areaStyle: showArea
      ? {
          origin: 'start',
          silent: true,
          color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
            {
              offset: 0,
              color: 'rgba(60, 230, 213, 0.4)',
            },
            {
              offset: 1,
              color: 'rgba(60, 230, 213, 0)',
            },
          ]),
        }
      : undefined,
  };
};

/**
 *
 * @param {String} name
 * @param {Array} data
 * @returns
 */
const barSeries = (name, data, minY, markData, axisXValues, fillColor) => {
  return {
    name,
    type: 'bar',
    smooth: true,
    // boundaryGap: true,
    data: data.map((d, index) => {
      if (markData && markData.includes(axisXValues[index])) {
        return {
          value: minY < 0 ? d - minY : d,
          itemStyle: {
            color: fillColor || '#FFD118',
          },
        };
      }
      return minY < 0 ? d - minY : d;
    }),
    selectedMode: 'single',
    itemStyle: {
      color: '#3CE5D3',
    },
    select: {
      itemStyle: {
        borderWidth: 5,
        borderColor: 'white',
        borderRadius: 3,
      },
    },
    markPoint: {
      symbol:
        'path://M3 5.50035H3.45416L3.83205 5.24843L7.5 2.80313V13.1976L3.83205 10.7523L3.45416 10.5004H3L1.5 10.5004V5.50035L3 5.50035ZM9 14.1318C9 14.9305 8.10985 15.4069 7.4453 14.9639L3 12.0004L1 12.0004C0.45 12.0004 0 11.5504 0 11.0004V5.00035C0 4.45035 0.45 4.00035 1 4.00035L3 4.00035L7.4453 1.03682C8.10986 0.593782 9 1.07017 9 1.86887V14.1318ZM12.3132 2.24765C12.5907 1.94021 13.065 1.91599 13.3724 2.19357C14.9811 3.64591 15.9953 5.74334 15.9953 8.06211C15.9953 10.3132 15.0392 12.3413 13.5274 13.7908C13.2284 14.0775 12.7536 14.0675 12.467 13.7685C12.1803 13.4696 12.1903 12.9948 12.4893 12.7081C13.7258 11.5225 14.4953 9.87786 14.4953 8.06211C14.4953 6.18949 13.6774 4.48977 12.3673 3.30693C12.0598 3.02936 12.0356 2.5551 12.3132 2.24765ZM10.2492 4.11186C10.5576 3.83528 11.0317 3.86104 11.3083 4.16939C12.2366 5.20435 12.7996 6.5741 12.7996 8.06271C12.7996 9.47007 12.2965 10.7553 11.4708 11.7723C11.2097 12.0939 10.7373 12.143 10.4158 11.8819C10.0942 11.6208 10.0451 11.1485 10.3062 10.8269C10.9292 10.0595 11.2996 9.10378 11.2996 8.06271C11.2996 6.95764 10.8826 5.9412 10.1917 5.17095C9.91512 4.8626 9.94088 4.38843 10.2492 4.11186Z',
      symbolSize: 20,
      symbolOffset: [0, -15],
      itemStyle: {
        color: '#3CE5D3',
      },
      data: [],
    },
  };
};

/**
 * 游标配置
 * @param {*} callback
 * @returns
 */
const cursorConfig = (callback, isBar, minY) => {
  return {
    trigger: 'axis',
    // formatter: '时间:{b} <br />{a0}: {c0}',
    valueFormatter: (value) => {
      if (value === null || value === undefined || value <= -999) return 'NoData';
      if (isBar) {
        return NP.plus(value, minY).toFixed(1);
      }
      return value.toFixed(1);
    },
    position: function (point, params, dom, rect, size) {
      if (callback) {
        callback(params);
      }
      // 固定在顶部
      if (point[0] < size.viewSize[0] / 2) return [point[0] + 10, '18%'];
      return [point[0] - size.contentSize[0] - 10, '18%'];
    },
  };
};

export default defaultOption;
export {
  gridStyle,
  markArea,
  markLine,
  yAxisSplitLine,
  lineSeries,
  barSeries,
  lineMarkPoint,
  barMarkPoint,
  cursorConfig,
};
