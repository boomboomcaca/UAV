import React, { useState, useRef, useEffect, useMemo, useCallback, useImperativeHandle } from 'react';
import PropTypes from 'prop-types';
import { Button, message } from 'dui';
import { TranslateColoredIcon, ArrowLeftIcon, ArrowRightIcon } from 'dc-icon';
// import Theme from '@dc/theme/dist/tools';
import { getPixelRatio } from './utils';
import PointerPng from './assets/pointer-icon.png';
import styles from './FreBandChart.module.less';
/**
 * 频段占用表
 */

const btnStyle = {
  padding: 0,
  display: 'flex',
  flexDirection: 'column',
  justifyContent: 'center',
  backgroundColor: 'transparent',
  boxShadow: 'none',
};
const FreBandChart = React.forwardRef((props, ref) => {
  const { initData, recall, children, fixedLen } = props;
  const [signalType, setSignalType] = useState([
    {
      value: '新信号',
      color: '#A0D911',
      key: '1',
      num: 0,
      choose: false,
    },
    {
      value: '未知信号',
      color: '#FFD118',
      key: '2',
      num: 0,
      choose: false,
    },
    {
      value: '合法信号',
      color: '#40A9FF',
      key: '3',
      num: 0,
      choose: false,
    },
    {
      value: '非法信号',
      color: '#FF4C2B',
      key: '4',
      num: 0,
      choose: false,
    },
  ]);
  const signalTypeRef = useRef(signalType);
  const relativeWidthRef = useRef(0);
  const [loading, setLoading] = useState(true);
  const [pageStatus, setPageStatus] = useState({
    prePage: false,
    nextPage: false,
  });
  const pageStatusRef = useRef(pageStatus);
  const [occupancyData, setOccupancyData] = useState(initData);
  const occupancyDataRef = useRef(occupancyData);
  const [freBandScopes, setFreBandScopes] = useState([]);
  const freBandScopesRef = useRef(freBandScopes);
  const [freBandScopeIdx, setFreBandScopeIdx] = useState(-1);
  const freBandScopeIdxRef = useRef(freBandScopeIdx);
  const [freBandSignal, setFreBandSignal] = useState({});
  const currentSignalRef = useRef(freBandSignal);
  const freBandSignalsRef = useRef([]);
  const [freBandSignalIdx, setFreBandSignalIdx] = useState(-1);
  const freBandSignalIdxRef = useRef(freBandSignalIdx);
  const filterTypeRef = useRef(null);
  const canvasRef = useRef({});
  const pointerXRef = useRef(0);
  const isMousedownRef = useRef(false);
  const scopeHeight = 10;
  const strokeLineWidth = 1;
  let switchSignalTimer = null;
  useEffect(() => {
    // 获取占用度统计信息
    initChartStatistics(initData, signalType);
    let resizeTimer = null;
    const resizeFunc = () => {
      // 生成图表
      if (resizeTimer) {
        clearTimeout(resizeTimer);
        resizeTimer = null;
      }
      resizeTimer = setTimeout(() => {
        const eWrap = document.getElementById('canvasWrap');
        const eCanvas = eWrap.getElementsByTagName('canvas')[0];
        if (eCanvas) {
          // 设置canvas画布的高度
          eCanvas.height = 0;
          eCanvas.style.height = 0;
          canvasRef.current.clearRect(0, 0, eWrap.offsetWidth, eWrap.offsetHeight);
        }
        createCharts(occupancyDataRef.current);
        clearTimeout(resizeTimer);
        resizeTimer = null;
      }, 500);
    };
    // 获取canvasWrap元素
    window.addEventListener('resize', resizeFunc, false);

    document.body.addEventListener('mousemove', pointerMousemove, false);
    document.body.addEventListener('touchmove', pointerMousemove, false);
    document.body.addEventListener('mouseup', pointerMouseup, false);
    document.body.addEventListener('touchend', pointerMouseup, false);
    return () => {
      window.removeEventListener('resize', resizeFunc, false);

      document.body.removeEventListener('mousemove', pointerMousemove, false);
      document.body.removeEventListener('touchmove', pointerMousemove, false);
      document.body.removeEventListener('mouseup', pointerMouseup, false);
      document.body.removeEventListener('touchend', pointerMouseup, false);
    };
  }, []);
  const initChartStatistics = (chartData) => {
    // 获取占用度统计信息
    const data = [];
    signalTypeRef.current.map((item) => {
      item.num = 0;
      return item;
    });
    // 筛选占用度统计数据
    for (let i = 0; i < chartData.length; i += 1) {
      const validSignals = [];
      if (chartData[i].startFreq && chartData[i].stopFreq && chartData[i].signals && chartData[i].signals.length > 0) {
        const scopeSignals = chartData[i].signals;
        for (let j = 0; j < scopeSignals.length; j += 1) {
          if (
            scopeSignals[j].frequency &&
            scopeSignals[j].frequency >= chartData[i].startFreq &&
            scopeSignals[j].frequency <= chartData[i].stopFreq
          ) {
            validSignals.push(scopeSignals[j]);
            signalTypeRef.current.map((item) => {
              if (scopeSignals[j].type && Number(item.key) === Number(scopeSignals[j].type)) {
                item.num += 1;
              }
              return item;
            });
          }
        }
      }
      chartData[i].signals = validSignals;
      data.push(chartData[i]);
    }
    relativeWidthRef.current = data.reduce((pre, item) => {
      if (item.startFreq && item.stopFreq) {
        pre += Math.abs(item.stopFreq - item.startFreq);
        return pre;
      }
      return pre;
    }, 0);
    occupancyDataRef.current = data;
    setOccupancyData(data);
    setSignalType([...signalTypeRef.current]);
    if (filterTypeRef.current) {
      // 过滤对应type数据
      createFilterData(filterTypeRef.current);
    } else {
      // 生成图表
      createCharts(data);
    }
    setLoading(false);
  };
  const switchType = (item) => {
    // 展示相应type信号
    if (!item.num) {
      message.warning('暂无对应信号');
      return;
    }
    if (item.key !== filterTypeRef.current) {
      filterTypeRef.current = item.key;
      signalTypeRef.current.map((tp) => {
        tp.choose = tp.key === filterTypeRef.current;
        return tp;
      });
      setSignalType([...signalTypeRef.current]);
      // 重置默认信号
      freBandSignalIdxRef.current = -1;
      setFreBandSignalIdx(-1);
      // 过滤对应type数据
      createFilterData(item.key);
    } else {
      filterTypeRef.current = null;
      signalTypeRef.current.map((tp) => {
        tp.choose = false;
        return tp;
      });
      setSignalType([...signalTypeRef.current]);
      // 重置默认信号
      freBandSignalIdxRef.current = -1;
      setFreBandSignalIdx(-1);
      // 生成图表
      createCharts(occupancyDataRef.current);
    }
  };
  const createFilterData = (type) => {
    // 过滤对应type数据
    const data = [];
    for (let i = 0; i < occupancyDataRef.current.length; i += 1) {
      const { signals } = occupancyDataRef.current[i];
      if (signals) {
        const filterSignal = signals.filter((sig) => sig.type && Number(type) === Number(sig.type));
        data.push({ ...occupancyDataRef.current[i], signals: filterSignal });
      }
    }
    // 生成图表
    createCharts(data);
  };
  const createCharts = (data) => {
    // 创建图表函数 data：图表数据
    freBandSignalsRef.current = [];
    freBandScopesRef.current = [];
    // 图表游标轴dom
    const chartXLine = document.getElementById('x_line');
    const areaX = document.getElementById('area_x');
    // 获取canvasWrap元素
    const eWrap = document.getElementById('canvasWrap');
    // 获取canvasWrap元素宽度和高度，用于设置canvas画布大小
    const nWrapW = areaX.offsetWidth;
    const nWrapH = areaX.offsetHeight;
    // 创建canvas画布
    const eCanvas = eWrap.getElementsByTagName('canvas')[0];
    // 创建绘图上下文环境
    const oCtx = eCanvas.getContext('2d');
    const ratio = getPixelRatio(oCtx);
    // 设置canvas画布的宽度和高度
    eCanvas.width = nWrapW * ratio;
    eCanvas.height = nWrapH * ratio;
    eCanvas.style.width = `${nWrapW}px`;
    eCanvas.style.height = `${nWrapH}px`;
    oCtx.beginPath();
    // 放大倍数
    oCtx.scale(ratio, ratio);
    canvasRef.current = oCtx;
    // const dcTheme = new Theme();
    // 绘制图表背景色
    // oCtx.fillStyle = dcTheme.getTheme() === 'dark' ? '#04051B' : '#f7f8f9';
    oCtx.fillStyle = '#04051B';
    // oCtx.fillStyle = 'var(--theme-frebandchart-background)';
    oCtx.fillRect(0, 0, nWrapW, nWrapH);
    // x-绘制频段
    const scopeWidths = [];
    // 匹配信号类型
    const matchType = (type) => {
      let typeColor = '#FFD118';
      for (let i = 0; i < signalType.length; i += 1) {
        if (signalType[i] && signalType[i].key && Number(signalType[i].key) === Number(type) && signalType[i].color) {
          typeColor = signalType[i].color;
        }
      }
      return typeColor;
    };
    let scopeWidPlus = 0;
    for (let i = 0; i < data.length; i += 1) {
      if (data[i]) {
        const intervalFreq = data[i].startFreq && data[i].stopFreq ? Math.abs(data[i].stopFreq - data[i].startFreq) : 0;
        // 频段相对x轴宽度
        const infoWidth = intervalFreq
          ? (intervalFreq / relativeWidthRef.current) * nWrapW
          : (100 / data.length) * nWrapW;
        scopeWidths.push(infoWidth);
        // 频段
        freBandScopesRef.current.push({
          startFreq: data[i].startFreq || 0,
          stopFreq: data[i].stopFreq || 0,
          width: infoWidth,
          start: scopeWidPlus,
          end: scopeWidPlus + infoWidth,
        });
        scopeWidPlus += infoWidth;
        // 绘制当前频段
        const signalFillX = scopeWidths.slice(0, i).reduce((pre, item) => {
          pre += item;
          return pre;
        }, 0);
        // x-绘制频段对应信号
        if (data[i].signals && data[i].signals.length > 0) {
          for (let j = 0; j < data[i].signals.length; j += 1) {
            const signalFillLeft =
              intervalFreq &&
              data[i].signals[j].frequency &&
              (data[i].signals[j].bandwidth || data[i].signals[j].bandwidth === 0)
                ? (Math.abs(
                    data[i].signals[j].frequency - data[i].signals[j].bandwidth / 1000 / 2 - data[i].startFreq,
                  ) /
                    intervalFreq) *
                    infoWidth +
                  signalFillX
                : signalFillX;
            const signalFillW = data[i].signals[j].bandwidth
              ? (data[i].signals[j].bandwidth / 1000 / intervalFreq) * infoWidth
              : 1;
            const signalFillH = data[i].signals[j].occupancy ? (data[i].signals[j].occupancy / 100) * nWrapH : 0;
            oCtx.fillStyle = data[i].signals[j].type ? matchType(data[i].signals[j].type) : '#FFD118';
            oCtx.fillRect(
              signalFillLeft,
              nWrapH - signalFillH,
              scopeWidPlus - signalFillLeft > signalFillW ? signalFillW : scopeWidPlus - signalFillLeft,
              signalFillH,
            );
            // 记录信号绘制位置
            freBandSignalsRef.current.push({
              scopeIdx: i,
              start: signalFillLeft,
              end: signalFillLeft + signalFillW > scopeWidPlus ? scopeWidPlus : signalFillLeft + signalFillW,
              ...data[i].signals[j],
            });
          }
        }
      }
    }
    freBandSignalsRef.current
      .sort((a, b) => {
        return b.bandwidth - a.bandwidth;
      })
      .sort((a, b) => {
        return a.frequency - b.frequency;
      });
    setFreBandScopes([...freBandScopesRef.current]);

    /* 需要改造 */
    if (freBandSignalsRef.current.length > 0) {
      if (freBandSignalIdxRef.current === -1) {
        // 初始默认信号
        freBandSignalIdxRef.current = 0;
        currentSignalRef.current = freBandSignalsRef.current[freBandSignalIdxRef.current];
        freBandScopeIdxRef.current = freBandSignalsRef.current[0].scopeIdx;
        pageStatusRef.current.prePage = false;
        pageStatusRef.current.nextPage = freBandSignalsRef.current.length > 1;
        setPageStatus({ ...pageStatusRef.current });
        setFreBandSignalIdx(0);
        setFreBandSignal({ ...freBandSignalsRef.current[0] });
        // 当前信号
        recall({
          type: 'FreBandChart-signal',
          data: freBandSignalsRef.current[0],
        });
      } else if (currentSignalRef.current.start && currentSignalRef.current.start >= freBandSignalsRef.current[0].end) {
        // 处理默认第一条数据前产生新数据，更新默认索引
        const currentIdx = freBandSignalsRef.current.findIndex(
          (item) =>
            Number(item.frequency).toFixed(fixedLen) === Number(currentSignalRef.current.frequency).toFixed(fixedLen) &&
            Number(item.bandwidth).toFixed(fixedLen) === Number(currentSignalRef.current.bandwidth).toFixed(fixedLen) &&
            Number(item.occupancy).toFixed(fixedLen) === Number(currentSignalRef.current.occupancy).toFixed(fixedLen) &&
            Number(item.bandwidth).toFixed(fixedLen) === Number(currentSignalRef.current.bandwidth).toFixed(fixedLen) &&
            Number(item.type).toFixed(fixedLen) === Number(currentSignalRef.current.type).toFixed(fixedLen),
        );
        if (currentIdx > -1) {
          pageStatusRef.current.prePage = currentIdx > 0;
          pageStatusRef.current.nextPage = currentIdx < freBandSignalsRef.current.length - 1;
          setPageStatus({ ...pageStatusRef.current });
          freBandSignalIdxRef.current = currentIdx;
          setFreBandSignalIdx(currentIdx);
        }
      }
      pointerXRef.current = freBandSignalsRef.current[freBandSignalIdxRef.current].start;
      chartXLine.style.left = `${freBandSignalsRef.current[freBandSignalIdxRef.current].start}px`;
    }
  };
  const pointerMousedown = () => {
    if (occupancyDataRef.current.length > 0) {
      isMousedownRef.current = true;
    }
  };
  const pointerMousemove = (e) => {
    if (!isMousedownRef.current) {
      return;
    }
    e.preventDefault();
    e.stopPropagation();
    const pageX = e.x ? e.x : e.touches && e.touches[0] && e.touches[0].clientX ? e.touches[0].clientX : 0;
    let areaX = document.getElementById('area_x');
    const chartX = document.getElementById('chart_x');
    const chartXLine = document.getElementById('x_line');
    let offsetLeft = 0;
    do {
      offsetLeft += areaX.offsetLeft;
      areaX = areaX.parentNode;
    } while (areaX.parentNode);
    const disx = Math.max(pageX - offsetLeft / 2, 0);
    const moveX = Math.min(disx, chartX.clientWidth);
    chartXLine.style.left = `${moveX}px`;
    pointerXRef.current = moveX;
    // 信号翻页按钮状态
    const pageBtnStatus = {
      prePage: false,
      nextPage: false,
    };
    // 对应信号业务 当前信号索引
    let scopeIdx = -1;
    let signalIdx = -1;
    for (let i = 0; i < freBandScopesRef.current.length; i += 1) {
      if (moveX >= freBandScopesRef.current[i].start && moveX <= freBandScopesRef.current[i].end) {
        // 游标位于频段
        scopeIdx = i;
        break;
      }
    }
    if (moveX === chartX.clientWidth) {
      scopeIdx = freBandScopesRef.current.length - 1;
    }
    for (let i = 0; i < freBandSignalsRef.current.length; i += 1) {
      if (moveX >= freBandSignalsRef.current[i].start && moveX <= freBandSignalsRef.current[i].end) {
        // 游标位于信号
        signalIdx = i;
        break;
      }
    }
    if (freBandSignalsRef.current.length > 0) {
      if (
        (moveX > freBandSignalsRef.current[0].end &&
          moveX < freBandSignalsRef.current[freBandSignalsRef.current.length - 1].start) ||
        moveX > freBandSignalsRef.current[freBandSignalsRef.current.length - 1].end
      ) {
        // 游标前有信号
        pageBtnStatus.prePage = true;
      }
      if (
        moveX < freBandSignalsRef.current[0].start ||
        (moveX > freBandSignalsRef.current[0].end &&
          moveX < freBandSignalsRef.current[freBandSignalsRef.current.length - 1].start)
      ) {
        // 游标后有信号
        pageBtnStatus.nextPage = true;
      }
    }
    if (scopeIdx > -1 && scopeIdx !== freBandScopeIdxRef.current) {
      freBandScopeIdxRef.current = scopeIdx;
      setFreBandScopeIdx(scopeIdx);
    }
    if (signalIdx > -1 && signalIdx !== freBandSignalIdxRef.current) {
      freBandSignalIdxRef.current = signalIdx;
      currentSignalRef.current = freBandSignalsRef.current[signalIdx];
      setFreBandSignalIdx(signalIdx);
      setFreBandSignal({ ...freBandSignalsRef.current[signalIdx] });
    } else if (signalIdx === -1) {
      freBandSignalIdxRef.current = signalIdx;
      currentSignalRef.current = {};
      setFreBandSignal({});
    }
    if (switchSignalTimer) {
      clearTimeout(switchSignalTimer);
      switchSignalTimer = null;
    }
    switchSignalTimer = setTimeout(() => {
      pageStatusRef.current = pageBtnStatus;
      setPageStatus({ ...pageBtnStatus });
      // 当前信号
      signalIdx > -1 &&
        recall({
          type: 'FreBandChart-signal',
          data: freBandSignalsRef.current[signalIdx],
        });
    }, 500);
  };
  const pointerMouseup = () => {
    isMousedownRef.current = false;
  };
  const pointerMouseleave = () => {
    isMousedownRef.current = false;
  };
  const turnPage = (type) => {
    // 信号上下选择
    const chartXLine = document.getElementById('x_line');
    let scopeIdx = -1;
    if (type === 'up') {
      // 上一信号
      if (freBandSignalIdxRef.current > 0) {
        freBandSignalIdxRef.current -= 1;
        pointerXRef.current = freBandSignalsRef.current[freBandSignalIdxRef.current].start;
        scopeIdx = freBandSignalsRef.current[freBandSignalIdxRef.current].scopeIdx;
      } else {
        let signalIdx = -1;
        if (pointerXRef.current > freBandSignalsRef.current[freBandSignalsRef.current.length - 1].end) {
          // 尾信号 选择
          signalIdx = freBandSignalsRef.current.length - 1;
        } else {
          // 首尾信号之前 选择
          for (let i = 0; i < freBandSignalsRef.current.length - 1; i += 1) {
            if (
              pointerXRef.current > freBandSignalsRef.current[i].end &&
              pointerXRef.current < freBandSignalsRef.current[i + 1].start
            ) {
              // 游标临近的上一信号序号
              signalIdx = i;
              break;
            }
          }
        }
        if (signalIdx > -1) {
          freBandSignalIdxRef.current = signalIdx;
          pointerXRef.current = freBandSignalsRef.current[signalIdx].start;
          scopeIdx = freBandSignalsRef.current[signalIdx].scopeIdx;
        }
      }
      if (pageStatusRef.current.prePage) {
        pageStatusRef.current.prePage = freBandSignalIdxRef.current > 0;
        pageStatusRef.current.nextPage = freBandSignalIdxRef.current < freBandSignalsRef.current.length - 1;
        chartXLine.style.left = `${pointerXRef.current}px`;
        if (scopeIdx > -1 && scopeIdx !== freBandScopeIdxRef.current) {
          freBandScopeIdxRef.current = scopeIdx;
          setFreBandScopeIdx(scopeIdx);
        }
        currentSignalRef.current = freBandSignalsRef.current[freBandSignalIdxRef.current];
        setFreBandSignal({ ...freBandSignalsRef.current[freBandSignalIdxRef.current] });
        setPageStatus({ ...pageStatusRef.current });
        // 当前信号
        recall({
          type: 'FreBandChart-signal',
          data: freBandSignalsRef.current[freBandSignalIdxRef.current],
        });
      }
    } else if (type === 'down') {
      // 下一信号
      if (freBandSignalIdxRef.current >= 0 && freBandSignalIdxRef.current < freBandSignalsRef.current.length - 1) {
        freBandSignalIdxRef.current += 1;
        pointerXRef.current = freBandSignalsRef.current[freBandSignalIdxRef.current].start;
        scopeIdx = freBandSignalsRef.current[freBandSignalIdxRef.current].scopeIdx;
      } else {
        let signalIdx = -1;
        if (pointerXRef.current < freBandSignalsRef.current[0].start) {
          // 首之前 选择
          signalIdx = 0;
        } else {
          // 首尾信号之前 选择
          for (let i = 1; i < freBandSignalsRef.current.length; i += 1) {
            if (
              pointerXRef.current > freBandSignalsRef.current[i - 1].end &&
              pointerXRef.current < freBandSignalsRef.current[i].start
            ) {
              // 游标临近的下一信号序号
              signalIdx = i;
              break;
            }
          }
        }
        if (signalIdx > -1) {
          freBandSignalIdxRef.current = signalIdx;
          pointerXRef.current = freBandSignalsRef.current[signalIdx].start;
          scopeIdx = freBandSignalsRef.current[signalIdx].scopeIdx;
        }
      }
      if (pageStatusRef.current.nextPage) {
        chartXLine.style.left = `${pointerXRef.current}px`;
        pageStatusRef.current.prePage = freBandSignalIdxRef.current > 0;
        pageStatusRef.current.nextPage = freBandSignalIdxRef.current < freBandSignalsRef.current.length - 1;
        if (scopeIdx > -1 && scopeIdx !== freBandScopeIdxRef.current) {
          freBandScopeIdxRef.current = scopeIdx;
          setFreBandScopeIdx(scopeIdx);
        }
        currentSignalRef.current = freBandSignalsRef.current[freBandSignalIdxRef.current];
        setFreBandSignal({ ...freBandSignalsRef.current[freBandSignalIdxRef.current] });
        setPageStatus({ ...pageStatusRef.current });
        // 当前信号
        recall({
          type: 'FreBandChart-signal',
          data: freBandSignalsRef.current[freBandSignalIdxRef.current],
        });
      }
    }
  };
  const matchSignal = (val, key) => {
    // 匹配信号类型
    let typeColor = '#FFD118';
    let typeText = '';
    for (let i = 0; i < signalType.length; i += 1) {
      if (signalType[i] && signalType[i].key && Number(signalType[i].key) === Number(val) && signalType[i].color) {
        typeColor = signalType[i].color;
        typeText = signalType[i].value.replace('信号', '');
      }
    }
    return key === 'color' ? typeColor : typeText;
  };
  // 自定义暴露给父组件的方法或者变量
  useImperativeHandle(
    ref,
    () => ({
      updateChart: (signal) => {
        // 更新图标数据
        const { frequency, bandwidth } = signal;
        if (frequency && bandwidth) {
          for (let i = 0; i < occupancyData.length; i += 1) {
            const { startFreq, stopFreq, signals } = occupancyData[i];
            const preSignals = signals || [];
            const existIdx = preSignals.findIndex((item) => item.frequency && item.frequency === frequency);
            if (startFreq && stopFreq && existIdx === -1 && frequency >= startFreq && frequency <= stopFreq) {
              preSignals.push(signal);
              // 数据溢出处理
              if (preSignals.length > 1000) {
                preSignals.shift();
              }
              occupancyData[i].signals = preSignals;
              // 获取占用度统计信息
              initChartStatistics(occupancyData);
              break;
            }
          }
        }
      },
      updateArrChart: (data) => {
        // 更新图标数据 数组
        const eWrap = document.getElementById('canvasWrap');
        const eCanvas = eWrap.getElementsByTagName('canvas')[0];
        if (eCanvas) {
          // 设置canvas画布的高度
          eCanvas.height = 0;
          eCanvas.style.height = 0;
          canvasRef.current.clearRect(0, 0, eWrap.offsetWidth, eWrap.offsetHeight);
        }
        occupancyData.map((item) => {
          item.signals = [];
          return item;
        });
        if (data && data.length > 0) {
          for (let i = 0; i < data.length; i += 1) {
            if (
              Object.prototype.hasOwnProperty.call(data[i], 'segIndex') &&
              occupancyData[data[i].segIndex].startFreq &&
              occupancyData[data[i].segIndex].stopFreq &&
              data[i].frequency &&
              data[i].frequency >= occupancyData[data[i].segIndex].startFreq &&
              data[i].frequency <= occupancyData[data[i].segIndex].stopFreq
            ) {
              occupancyData[data[i].segIndex].signals.push(data[i]);
            } else if (data[i].frequency) {
              for (let j = 0; j < occupancyData.length; j += 1) {
                if (
                  occupancyData[j].startFreq &&
                  occupancyData[j].stopFreq &&
                  data[i].frequency >= occupancyData[j].startFreq &&
                  data[i].frequency <= occupancyData[j].stopFreq
                ) {
                  occupancyData[j].signals.push(data[i]);
                }
              }
            }
          }
        }
        // 获取占用度统计信息
        initChartStatistics(occupancyData);
      },
      reset: () => {
        const eWrap = document.getElementById('canvasWrap');
        const eCanvas = eWrap.getElementsByTagName('canvas')[0];
        if (eCanvas) {
          // 设置canvas画布的高度
          eCanvas.height = 0;
          eCanvas.style.height = 0;
          canvasRef.current.clearRect(0, 0, eWrap.offsetWidth, eWrap.offsetHeight);
        }
        // 重置默认信号
        freBandSignalIdxRef.current = -1;
        occupancyData.map((item) => {
          item.signals = [];
          return item;
        });
        occupancyDataRef.current = occupancyData;
        signalTypeRef.current.map((item) => {
          item.num = 0;
          item.choose = false;
          return item;
        });
        setSignalType([...signalTypeRef.current]);
        setFreBandSignalIdx(-1);
        // 获取占用度统计信息
        initChartStatistics(occupancyData);
      },
    }),
    [occupancyData],
  );
  const typeList = [
    {
      label: '新信号',
      color: '#A0D911',
    },
    {
      label: '非法信号',
      color: '#FF4C2B',
    },
    {
      label: '未知信号',
      color: '#FFD118',
    },
    {
      label: '合法信号',
      color: '#40A9FF',
    },
  ];
  return (
    <div className={styles.container}>
      <div className={styles.chart_title}>
        <div className={styles.title_type}>
          {signalType.map((item, index) => (
            <div
              key={`type-${index + 1}`}
              className={item.choose ? [styles.type_info, styles.type_choose].join(' ') : styles.type_info}
              onClick={() => switchType(item)}
            >
              <span style={{ backgroundColor: item.color }} />
              <span>{item.value}</span>
              <span title={item.num}>{item.num}</span>
            </div>
          ))}
        </div>
        <div className={styles.chart_switch}>
          <div className={styles.switch_lBtn}>
            <Button
              style={{ ...btnStyle, opacity: !pageStatus.prePage ? 0.2 : 1 }}
              disabled={!pageStatus.prePage}
              onClick={() => turnPage('up')}
            >
              <ArrowLeftIcon color="var(--theme-font-100)" />
            </Button>
          </div>
          <div className={styles.switchButton}>
            <div
              className={styles.switchButton_text}
              style={{
                backgroundColor: freBandSignal.type ? matchSignal(freBandSignal.type, 'color') : 'transparent',
              }}
            >
              {freBandSignal.type ? matchSignal(freBandSignal.type, 'text') : ''}
            </div>
            <div className={styles.switchButton_option}>
              <div>
                <b>{freBandSignal.frequency ? Number(freBandSignal.frequency).toFixed(fixedLen) : '--'}</b>
                <span className={styles.col_title}>MHz</span>
              </div>
              <p>/</p>
              <div>
                <b>
                  {freBandSignal.bandwidth || freBandSignal.bandwidth === 0
                    ? Number(freBandSignal.bandwidth).toFixed(fixedLen)
                    : '--'}
                </b>
                <span className={styles.col_title}>kHz</span>
              </div>
            </div>
            <div className={styles.switchButton_option}>
              <b>
                {freBandSignal.elecLevel || freBandSignal.elecLevel === 0
                  ? Number(freBandSignal.elecLevel).toFixed(fixedLen)
                  : '--'}
              </b>
              <span>dBμV</span>
            </div>
          </div>
          <div className={styles.switch_rBtn}>
            <Button
              style={{ ...btnStyle, opacity: !pageStatus.nextPage ? 0.2 : 1 }}
              disabled={!pageStatus.nextPage}
              onClick={() => turnPage('down')}
            >
              <ArrowRightIcon color="var(--theme-font-100)" />
            </Button>
          </div>
        </div>
        <div className={styles.title_info}>{children && children}</div>
      </div>
      <div className={styles.chart_area} onMouseLeave={pointerMouseleave} onTouchEnd={pointerMouseleave}>
        <div className={styles.area_x} id="area_x">
          <div className={styles.x_canvas} id="canvasWrap">
            <canvas />
            {/* <div className={styles.x_slider}>
                {freBandScopes.map((item, index) => (
                  <div
                    key={`x_slider-${index + 1}`}
                    className={
                      freBandScopeIdx === index
                        ? [styles.slider_item_choose, styles.slider_item, 'slider_item'].join(' ')
                        : [styles.slider_item, 'slider_item'].join(' ')
                    }
                    style={{
                      width: item.width,
                    }}
                  >
                    <span
                      style={{
                        display: freBandScopeIdx === index ? 'block' : 'none',
                        transform: index !== 0 ? 'translate(-100%, 100%)' : 'translateY(100%)',
                      }}
                    >
                      <span>{item.startFreq || '--'}</span>
                      MHz
                    </span>
                    <span
                      style={{
                        display: freBandScopeIdx === index ? 'block' : 'none',
                        transform: index !== freBandScopes.length - 1 ? 'translate(100%, 100%)' : 'translateY(100%)',
                      }}
                    >
                      <span>{item.stopFreq || '--'}</span>
                      MHz
                    </span>
                  </div>
                ))}
              </div> */}
          </div>
          <div className={styles.x_pointer} id="chart_x">
            <div className={styles.pointer_line} id="x_line">
              <div className={styles.line_text}>
                {/* <span>{freBandSignal.frequency ? Number(freBandSignal.frequency).toFixed(4) : '--'}</span> */}
              </div>
              <div className={styles.pointer_box} onMouseDown={pointerMousedown} onTouchStart={pointerMousedown}>
                <Button style={btnStyle} disabled={occupancyData.length === 0} draggable={false}>
                  <img src={PointerPng} alt="Pointer" />
                </Button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
});
FreBandChart.defaultProps = {
  initData: [],
  recall: null,
  children: null,
  fixedLen: 5,
};

FreBandChart.propTypes = {
  initData: PropTypes.array,
  recall: PropTypes.func,
  children: PropTypes.any,
  fixedLen: PropTypes.number,
};
export default FreBandChart;
