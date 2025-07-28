import React, { useState, useRef, useEffect, useImperativeHandle, forwardRef } from 'react';
import * as echarts from 'echarts';
import PropTypes from 'prop-types';
import { useDeepCompareEffect } from 'ahooks';
import defaultOption, {
  gridStyle,
  markArea as markAreaStyle,
  markLine,
  yAxisSplitLine,
  lineSeries,
  barSeries,
  lineMarkPoint,
  cursorConfig,
} from './defines';

const ADSBChart = forwardRef((props, ref) => {
  const {
    minY,
    maxY,
    title,
    axisXValues,
    seriesDatas,
    markData,
    selectArea,
    type,
    onSelectChange,
    onPlayAudio,
    legend,
  } = props;
  const chartCon = useRef();
  const chartRef = useRef();
  const optionRef = useRef(defaultOption(-20, 80, []));
  // 当前游标位置
  const cursorPosRef = useRef();

  // 时间范围选择变更
  const [selectAreaValue, setSelectAreaValue] = useState();
  // 柱状图选择变更
  const [selectBar, setSelectBar] = useState();

  // 触发播放音频
  const [clickAudio, setClickAudio] = useState();

  const handleSelectArea = (xValue) => {
    const timeInfo = xValue.split(':');
    const hour = parseInt(timeInfo[0]);
    // MD , 加了一堆容错处理，处理边界选择....
    const valIndex = axisXValues.indexOf(xValue);
    let selectStart = `${timeInfo[0]}:00`;
    let startIndex = axisXValues.indexOf(selectStart);
    if (startIndex < 0) {
      startIndex = valIndex > 0 ? valIndex - 1 : valIndex;
      selectStart = axisXValues[startIndex];
    }

    let slectEnd = hour === 23 ? '23:45' : hour < 9 ? `0${hour + 1}:00` : `${hour + 1}:00`;
    let endIndex = axisXValues.indexOf(slectEnd);
    if (endIndex < 0) {
      endIndex = valIndex < axisXValues.length - 1 ? valIndex + 1 : valIndex;
      slectEnd = axisXValues[endIndex];
    }
    // MD , end
    setSelectAreaValue({
      start: selectStart,
      end: slectEnd,
      startIndex,
      endIndex,
    });
  };

  const initEchartsEvents = () => {
    // MD 点击 markpoint 会先触发选中变更？？？
    let prevSelectBar;
    chartRef.current.off('click');
    chartRef.current.on('click', (e) => {
      if (e.componentType === 'markPoint') {
        if (optionRef.current.series[0].type !== 'bar') {
          // 处理时间范围选择
          const xValue = e.data.coord[0];
          // warning 这里的选择处理需要依赖数据格式，一旦格式变了，需要重新处理
          handleSelectArea(xValue);
        } else {
          // 处理音频播放
          setClickAudio(new Date());
        }
      }
    });
    chartRef.current.off('mouseover');
    chartRef.current.on('mouseover', (e) => {
      // marker里禁用 选择，否则开启
      if (optionRef.current.series[0].type === 'bar') {
        if (e.componentType === 'markPoint') {
          optionRef.current.series[0].selectedMode = false;
        } else {
          optionRef.current.series[0].selectedMode = 'single';
        }
        chartRef.current.setOption(optionRef.current);
      }
    });
    chartRef.current.off('selectchanged');
    chartRef.current.on('selectchanged', (e) => {
      if (optionRef.current.series[0].type === 'bar' && e.selected.length > 0) {
        if (prevSelectBar !== JSON.stringify(e.selected[0])) {
          prevSelectBar = JSON.stringify(e.selected[0]);
          setSelectBar(e.selected[0]);
        }
      }
    });
    // chart.on('mouseout', (e) => {
    //   console.log('mouse out===', e);
    // });
    chartRef.current.off('globalout');
    chartRef.current.on('globalout', (e) => {
      cursorPosRef.current = undefined;
    });
  };

  useEffect(() => {
    if (!chartRef.current) {
      const chart = echarts.init(chartCon.current);
      chartRef.current = chart;
    }
    let windowSizeChange = () => {
      if (chartRef.current) {
        chartRef.current.resize();
      }
    };
    window.addEventListener('resize', windowSizeChange);
    return () => {
      if (chartRef.current) {
        chartRef.current.dispose();
        chartRef.current = undefined;
      }
      window.removeEventListener(windowSizeChange);
      windowSizeChange = undefined;
    };
  }, []);

  /**
   * 时间范围选择处理
   */
  useEffect(() => {
    if (optionRef.current.series.length > 0 && optionRef.current.series[0].markArea) {
      if (selectAreaValue) {
        optionRef.current.series[0].markArea.data[0] = [
          {
            name: `${selectAreaValue.start} ${selectAreaValue.end}`,
            xAxis: selectAreaValue.start,
          },
          {
            xAxis: selectAreaValue.end,
          },
        ];
        optionRef.current.series[0].markLine.data[0] = [
          {
            yAxis: minY,
            xAxis: selectAreaValue.start,
          },
          {
            yAxis: minY,
            xAxis: selectAreaValue.end,
          },
        ];
        // 更新
        chartRef.current.setOption(optionRef.current);
        // 通知
        if (onSelectChange) {
          onSelectChange(selectAreaValue);
        }
      } else {
        optionRef.current.series[0].markArea.data = [];
        optionRef.current.series[0].markLine.data = [];
        // 更新
        chartRef.current.setOption(optionRef.current);
      }
    }
  }, [selectAreaValue, onSelectChange, minY]);

  /**
   * 柱状图选择处理
   */
  useEffect(() => {
    if (selectBar) {
      const { dataIndex } = selectBar;
      optionRef.current.series[0].markPoint.data[0] = {
        coord: [axisXValues[dataIndex], optionRef.current.series[0].data[dataIndex].value],
      };

      chartRef.current.setOption(optionRef.current);
      // 通知选择
      if (onSelectChange) {
        onSelectChange({
          audioTime: axisXValues[dataIndex],
          dataIndex,
        });
      }
    }
  }, [selectBar, axisXValues, onSelectChange]);

  /**
   * 音频播放处理
   */
  useEffect(() => {
    if (clickAudio && onPlayAudio) {
      const { dataIndex } = selectBar;
      setClickAudio(undefined);
      onPlayAudio({
        audioTime: axisXValues[dataIndex],
        dataIndex,
      });
    }
  }, [clickAudio, onPlayAudio, selectBar, axisXValues]);

  /**
   * 设置标题
   */
  useEffect(() => {
    if (title) {
      optionRef.current.title.text = title;
      chartRef.current.setOption(optionRef.current);
    }
  }, [title]);

  /**
   * 设置Y轴范围
   */
  useEffect(() => {
    if (chartRef.current) {
      optionRef.current.yAxis.min = minY;
      optionRef.current.yAxis.max = maxY;
      if (type === 'bar' && minY < 0) {
        const gap = Math.abs(minY);
        optionRef.current.yAxis.min = 0;
        optionRef.current.yAxis.max = maxY + gap;
        optionRef.current.yAxis.axisLabel.formatter = (value, index) => {
          if (value === null || value === undefined) return value;
          // 自定义Y轴，如果有负数的情况
          if (minY < 0) return value - gap;
          return value;
        };
      } else {
        optionRef.current.yAxis.axisLabel.formatter = undefined;
      }

      // option.yAxis.min = minY;
      // option.yAxis.max = maxY;
      chartRef.current.setOption(optionRef.current);
    }
  }, [type, minY, maxY]);

  /**
   * 设置X轴值/标签
   */
  useEffect(() => {
    if (axisXValues && chartRef.current) {
      optionRef.current.xAxis.data = axisXValues;
      chartRef.current.setOption(optionRef.current);
    }
  }, [axisXValues]);

  useEffect(() => {
    if (chartRef.current && legend) {
      optionRef.current.legend = {
        align: 'left',
        right: '20px',
        itemGap: 16,
        itemHeight: 8,
        padding: [15, 0, 0, 0],
        textStyle: {
          color: '#FFFFFF',
          fontSize: 14,
        },
      };
    }
  }, [legend]);

  /**
   * 设置数据 设置数据标记  设置显示类型， line area bar
   */
  // useEffect(() => {

  // }, [seriesDatas, axisXValues, markData, selectArea, type, minY]);

  useDeepCompareEffect(() => {
    if (chartRef.current) {
      setSelectAreaValue(undefined);
      optionRef.current.series = [];
      if (seriesDatas && seriesDatas.length > 0 && axisXValues && axisXValues.length > 0) {
        initEchartsEvents();
        if (type !== 'area' && !selectArea) {
          // y轴网格线
          optionRef.current.yAxis.splitLine = yAxisSplitLine;
        }
        if (type !== 'bar' && selectArea) {
          // 绘图区背景
          optionRef.current.grid = { ...optionRef.current.grid, ...gridStyle };
        }
        if (type === 'line' || type === 'area') {
          const series = seriesDatas.map((s, index) => {
            let yData = s.data;
            // 2022-9-25 liujian 非柱状图不需要管Y轴有负数的情况
            // if (minY < 0) {
            //   yData = s.data.map((d) => {
            //     if (d === null || d === undefined) return -999;
            //     return d - minY;
            //   });
            // }
            // const yData = minY < 0 ? s.data.map((d) => d - minY) : data;
            return lineSeries(s.name, yData, s.color, type === 'area');
          });

          if (selectArea) {
            series[0].markArea = markAreaStyle;
            series[0].markLine = markLine;
            // 默认选中处理
            if (markData && markData.length > 0) {
              handleSelectArea(markData[0]);
            }
            if (type === 'line') {
              handleSelectArea('00:30');
            }
          }
          const markPointConf = markData ? lineMarkPoint(markData, axisXValues, series[0].data) : undefined;
          series[0].markPoint = markPointConf;
          optionRef.current.series = series;
        } else {
          // 柱状图
          const series = barSeries(seriesDatas[0].name, seriesDatas[0].data, minY, markData, axisXValues);
          optionRef.current.xAxis.boundaryGap = true;
          optionRef.current.series = [series];
          setSelectBar(undefined);
          // 默认选中处理
          if (markData && markData.length > 0) {
            setTimeout(() => {
              chartRef.current.dispatchAction({
                type: 'select',
                // 用 index 或 id 或 name 来指定系列。
                // 可以使用数组指定多个系列。
                seriesIndex: 0,
                // 数据项的 index，如果不指定也可以通过 name 属性根据名称指定数据项
                dataIndex: axisXValues.indexOf(markData[0]),
              });
            }, 1500);
          }
        }
        if (!optionRef.current.tooltip) {
          optionRef.current.tooltip = cursorConfig(
            (e) => {
              // 缓存游标信息，方便选中
              if (!cursorPosRef.current || (cursorPosRef.current.dataIndex !== e[0].dataIndex && type !== 'bar')) {
                cursorPosRef.current = e[0];
              }
            },
            type === 'bar',
            minY,
          );
        }
      }
      chartRef.current.setOption(optionRef.current, true);
    }
  }, [seriesDatas, axisXValues, markData, selectArea, type, minY]);

  /**
   * 容器点击事件
   */
  const conClickForAreaSelect = () => {
    if (cursorPosRef.current && selectArea && type === 'line') {
      const { dataIndex } = cursorPosRef.current;
      handleSelectArea(axisXValues[dataIndex]);
    }
  };

  /**
   * 对外暴漏resize
   */
  useImperativeHandle(
    ref,
    () => {
      return {
        resize: () => {
          chartRef.current?.resize();
        },
      };
    },
    // [chartRef.current],
  );

  return <div ref={chartCon} style={{ height: '100%', width: '100%' }} onClick={conClickForAreaSelect} />;
});

ADSBChart.defaultProps = {
  title: '',
  minY: -20,
  maxY: 100,
  markData: undefined,
  type: 'line',
  selectArea: false,
  onSelectChange: () => {},
  // axisXValues:undefined,
  onPlayAudio: () => {},
  // ref: undefined,
  legend: false,
};

ADSBChart.prototype = {
  title: PropTypes.string,
  minY: PropTypes.number,
  maxY: PropTypes.number,
  markData: PropTypes.array,
  type: PropTypes.string,
  selectArea: PropTypes.bool,
  axisXValues: PropTypes.array.isRequired,
  seriesDatas: PropTypes.arrayOf(
    PropTypes.shape({
      name: PropTypes.string,
      data: PropTypes.array,
    }),
  ).isRequired,
  onSelectChange: PropTypes.func,
  onPlayAudio: PropTypes.func,
  // ref: PropTypes.any,
  legend: PropTypes.bool,
};

export default ADSBChart;
