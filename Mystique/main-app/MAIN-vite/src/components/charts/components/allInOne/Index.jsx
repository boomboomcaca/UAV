import React, { useEffect, useState, useRef, useLayoutEffect } from "react";
import PropTypes from "prop-types";
import {
  NormalChart,
  RainChartGPUWorker,
  RainChart,
  RainChartGPU,
} from "../../lib/index.js";
import AxisY from "../commons/axisY/AxisY.jsx";
import AxisX from "../commons/axisX/AxisX.jsx";
import ChartTemplate from "../commons/layoutTemplate/Index.jsx";
import ZoomAndCursor from "../commons/zoomAndCursor/Index.jsx";
import TouchZoom from "../commons/touchZoom/Index.jsx";
import Markers from "../commons/markers/Index.jsx";
import Threshold from "../commons/threshold/Index.jsx";
import SegmentsBackground from "../commons/segmentsBackground/Index.jsx";
import SignalBands from "../commons/signalBands/Index.jsx";
import Band from "../commons/band/Index.jsx";
import { ChartTypes, SeriesTypes } from "../utils/enums.js";
import { EventTypes } from "../utils/spectrumChartHelper.js";
import styles from "./style.module.less";
import { isMobile } from "../utils/utils.js";
import { defaultBlends, lightBlends } from "../assets/colors.js";

/**
 *
 * @param {{}} props
 * @returns
 */
const AllInOne = (props) => {
  const {
    frequency,
    bandwidth,
    segments,
    units,
    visibleCharts,
    axisX,
    axisY,
    showThreshold,
    showCursor,
    allowAddMarker,
    chartLoad,
    chartGap,
    mobileZoomMode,
    onThresholdChange,
    onParameterChange,
    onTouchZoomChange,
    onSignalSelect,
    seriesLineType,
    chartHelper,
    allowZoom,
    streamTime,
    useGPU,
    padding,
    showBand,
    onBandChange,
    threshold,
    lightTheme,
    distributionBar,
  } = props;
  const prevSpectrumRef = useRef();
  // const [state, dispatch] = useReducer(reducer, initState);
  // const { minimumY, maximumY, zoomInfo, prevData } = state;
  const [rainTS, setRainTS] = useState({ currentPct: -1, rainTimeSpan: 0 });
  const [resizeFlag, setResizeFlag] = useState(0);
  const [axisYRange, setAxisYRange] = useState({
    minimum: -20,
    maximum: 80,
    tickGap: 10,
  });
  const [segmentsView, setSegmentsView] = useState([]);
  const [zoomInfo, setZoomInfo] = useState();
  const [cursorInfo, setCursorInfo] = useState();
  const [markers, setMarkers] = useState([]);
  // 图表间隔 像素
  const [chartPixelGap, setChartPixelGap] = useState(8);
  const [chartBounding, setChartBounding] = useState({});
  // 当前图表所有的子图表
  const allCharts = [ChartTypes.spectrum, ChartTypes.rain, ChartTypes.wbdf];
  const [dragOffset, setDragOffset] = useState(-9999);
  const [zoomOffset, setZoomOffset] = useState(-9999);
  const [streamTimeSpan, setStreamTimeSpan] = useState();

  /**
   * @type {{current:HTMLElement}}
   */
  const specContainerRef = useRef();
  /**
   * @type {{current:HTMLElement}}
   */
  const rainContainerRef = useRef();
  /**
   * @type {{current:HTMLElement}}
   */
  const chartContainerRef = useRef();
  /**
   * @type {{current:HTMLElement}}
   */
  const wbdfContainerRef = useRef();

  /**
   * @type {{current:NormalChart}}
   */
  const specChartRef = useRef();
  /**
   * @type {{current:RainChart||RainChartGPU}}
   */
  const rainChartRef = useRef();
  /**
   * @type {{current:NormalChart}}
   */
  const wbdfChartRef = useRef();
  const [thrLineTop, setThrLineTop] = useState(40);
  const [thrValue, setThrValue] = useState(0);
  const [signalBands, setSignalBands] = useState();
  // const [selSignal, setSelSignal] = useState();

  // const [vbw, setVBW] = useState();
  // 是否通过多指缩放来更新带宽
  // const setBandwidthByTouchZoom = useRef(false);
  useEffect(() => {
    // console.log("axisX change:::", axisX);
  }, [axisX]);
  /**
   * 获取频谱图线条定义
   * @param {*} mode
   * @param {Array} visibleIds
   */
  const getLineSeries = (mode, visibleIds) => {
    const series = [
      {
        name: SeriesTypes.max,
        // caption: "最大值",
        color: "#FF0000",
        thickness: 1,
        visible: visibleIds.includes(SeriesTypes.max),
        type: mode ? "stepLine" : "line",
      },
      {
        name: SeriesTypes.min,
        // caption: "最小值",
        color: "#0000FF",
        thickness: 1,
        visible: visibleIds.includes(SeriesTypes.min),
        type: mode ? "stepLine" : "line",
      },
      {
        name: SeriesTypes.avg,
        // caption: "平均值",
        color: "#FFFF00",
        thickness: 1,
        visible: visibleIds.includes(SeriesTypes.avg),
        type: mode ? "stepLine" : "line",
      },
      {
        name: SeriesTypes.real,
        // caption: "实时值",
        color: "#00FF00",
        thickness: 1,
        // visible: true,
        type: mode ? "bar" : "line",
        pointWidth: 5,
        pointColor: "#FF0000",
      },
      {
        name: SeriesTypes.thr,
        // caption: "实时值",
        color: "#da70d6",
        thickness: 1,
        // visible: true,
        type: "line",
        pointWidth: 5,
        pointColor: "#FF0000",
      },
    ];
    return series;
  };

  const pointSeriesRef = useRef([
    {
      name: "real", // Series名称
      symbol: "rect", // 点图 样子 'rect'|'circle'|'diamond'
      pointWidth: 3, // 点的宽度
      pointColor: "#00FF00", // 点颜色
      visible: true, // 是否显示
      type: "point", // Series 类型 'point'|'line'|'stepline'|'bar'
    },
  ]);

  useEffect(() => {
    if (segments) {
      chartHelper.updateSegments(segments);
      setSegmentsView(segments);
      if (specChartRef.current) {
        specChartRef.current.clear();
        // rainChartRef.current.clear();
      }
    }
  }, [segments]);

  useEffect(() => {
    if (specChartRef.current) {
      specChartRef.current.clear();
    }
  }, [frequency, bandwidth]);

  useEffect(() => {
    if (chartHelper) {
      chartHelper.updateChartArgs({
        frequency,
        bandwidth,
      });
      chartHelper.clear();
    }
  }, [chartHelper, frequency, bandwidth]);

  // 更新图表间隔
  useEffect(() => {
    if (chartGap != chartPixelGap) {
      setChartPixelGap(chartGap);
    }
  }, [chartGap, chartPixelGap]);

  useEffect(() => {
    if (chartHelper) {
      chartHelper.updateChartArgs({
        units,
        // 当前显示的图表，根据all charts 排序
        // visibleCharts: allCharts.filter((ct) => visibleCharts.includes(ct)),
        showThreshold,
        showCursor,
        streamTime,
      });
    }
  }, [chartHelper, units, showThreshold, showCursor, streamTime]);

  const initChartBounding = (chartHelper, visibleCharts, chartPixelGap) => {
    const bounding = {};
    const con = chartContainerRef.current.getBoundingClientRect();
    const totalHeight = con.height - chartPixelGap * (visibleCharts.length - 1);
    const gap = totalHeight / visibleCharts.length;
    visibleCharts.forEach((chart) => {
      if (chart === ChartTypes.spectrum) {
        const rect = specContainerRef.current.getBoundingClientRect();
        rect.y1 = 0;
        rect.y2 = gap / con.height;
        bounding[chart] = rect;
      } else if (chart === ChartTypes.rain) {
        const rect = rainContainerRef.current.getBoundingClientRect();
        if (visibleCharts.includes(ChartTypes.spectrum)) {
          rect.y1 = (gap + chartPixelGap) / con.height;
          rect.y2 = 1;
        } else {
          rect.y1 = 0;
          rect.y2 = gap / con.height;
        }
        bounding[chart] = rect;
      } else if (chart === ChartTypes.wbdf) {
        const rect = wbdfContainerRef.current.getBoundingClientRect();
        if (
          visibleCharts.includes(ChartTypes.spectrum) ||
          visibleCharts.includes(ChartTypes.rain)
        ) {
          rect.y1 = (gap + chartPixelGap) / con.height;
          rect.y2 = 1;
        } else {
          rect.y1 = 0;
          rect.y2 = gap / con.height;
        }
        bounding[chart] = rect;
      }
    });
    setChartBounding(bounding);
    console.log("set chart bounding:::", bounding);
    chartHelper.updateChartArgs({
      chartBounding: bounding,
      // 当前显示的图表，根据all charts 排序
      visibleCharts: allCharts.filter((ct) => visibleCharts.includes(ct)),
    });
  };

  const visibleChartsRef = useRef();

  // 根据当前显示的chart计算每个图表的高度占比
  // 约定显示原则：1. 至少显示其中一个，2. 最多显示其中两个， 3. 显示顺序必须按照allCharts来
  useEffect(() => {
    visibleChartsRef.current = visibleCharts;
    setTimeout(() => {
      if (specChartRef.current) {
        initChartBounding(chartHelper, visibleCharts, chartPixelGap);
      }
    }, 100);
  }, [visibleCharts, chartPixelGap, resizeFlag, chartHelper]);

  useEffect(() => {
    let listener;
    if (!specChartRef.current) {
      const specChart = new NormalChart(specContainerRef.current, {
        // bgColor: lightTheme ? "#fff" : "#000",
        onSizeChange: (w, h) => {
          chartHelper.setPlotArea({ width: w });
        },
      });
      // 初始化线条
      const series = getLineSeries(seriesLineType, []);
      series.forEach((s) => {
        specChart.initSeries(s);
      });
      if (distributionBar) {
        specChart.initSeries({
          name: "distBar",
          type: "bar",
          barOrientation: "vertical",
          barAlign: "right",
          color: lightTheme ? "#00FFE055" : "#EAFF0055",
        });
      }
      specChartRef.current = specChart;
      if (useGPU) {
        rainChartRef.current = new RainChartGPU(rainContainerRef.current, {
          onSizeChange: (w, h) => {
            console.log("rect change:::", w, h);
            // chartHelper.resize(w, h);
            chartHelper.setPlotArea({ width: w, height: h });
          },
        });
      } else {
        rainChartRef.current = new RainChart(
          rainContainerRef.current,
          undefined,
          {
            onSizeChange: (w, h) => {
              console.log(w, h);
              // chartHelper.resize(w, h);
              chartHelper.setPlotArea({ width: w, height: h });
            },
          }
        );
      }
      // 初始化点图
      const wbdfChart = new NormalChart(wbdfContainerRef.current);
      wbdfChart.setAxisYRange(0, 360);
      pointSeriesRef.current.forEach((series) => {
        wbdfChart.initSeries(series);
      });
      wbdfChartRef.current = wbdfChart;
      listener = window.addEventListener("resize", () => {
        //    specChart.resize();
        //    rainChartRef.current.resize();
        setResizeFlag(new Date().getTime());
      });
      chartLoad({
        specChart: specChartRef.current,
        rainChart: rainChartRef.current,
      });
    }
    return () => {
      if (listener) {
        window.removeEventListener(listener);
        rainChartRef.current.dispose();
      }
    };
  }, [
    chartLoad,
    seriesLineType,
    useGPU,
    lightTheme,
    chartHelper,
    distributionBar,
  ]);

  useEffect(() => {
    if (specChartRef.current && prevSpectrumRef.current) {
      // specChartRef.current.zoomPercent(zoomInfo.start, zoomInfo.end);
      // rainChartRef.current.zoomPercent(zoomInfo.start, zoomInfo.end);
    }
  }, [zoomInfo]);

  useLayoutEffect(() => {
    if (chartHelper) {
      chartHelper.on(EventTypes.DataChange, (e) => {
        if (document.hidden) return;
        const {
          spectrum,
          rain,
          streamTimeSpan,
          scanDF,
          currentPct,
          rainTimeSpan,
          signals,
        } = e;
        if (signals) {
          setSignalBands(signals);
        }
        if (
          spectrum &&
          visibleChartsRef.current.includes(ChartTypes.spectrum)
        ) {
          const specRenderData = [{ name: "real", data: spectrum.data }];
          if (spectrum.thr) {
            specRenderData.push({ name: "thr", data: spectrum.thr });
          }
          if (spectrum.max) {
            specRenderData.push({ name: "max", data: spectrum.max });
          }
          if (spectrum.avg) {
            specRenderData.push({ name: "avg", data: spectrum.avg });
          }
          if (spectrum.min) {
            specRenderData.push({ name: "min", data: spectrum.min });
          }
          if (spectrum.distBar) {
            specRenderData.push({ name: "distBar", data: spectrum.distBar });
          }
          specChartRef.current.setData(specRenderData);
        }
        if (rain && visibleChartsRef.current.includes(ChartTypes.rain)) {
          rainChartRef.current.setImageData(rain);
          setRainTS({
            currentPct,
            rainTimeSpan,
          });
        }
        if (scanDF && visibleChartsRef.current.includes(ChartTypes.wbdf)) {
          wbdfChartRef.current.setData([{ name: "real", data: scanDF.data }]);
        }
        setStreamTimeSpan(streamTimeSpan);
      });
      chartHelper.on(EventTypes.MarkerChange, (markers) => {
        setMarkers(markers);
      });
      chartHelper.on(EventTypes.MarkerSelectChange, (sel) => {
        console.log("on marker select change");
        // TODO
        // setSelMarker(sel.position);
      });
      console.log("regsiter zoom change :::");
      chartHelper.on(EventTypes.ZoomChange, (e) => {
        // 通过数据来驱动，这里可以不写了
        // specChartRef.current.zoom(e.startIndex, e.endIndex);
        // rainChartRef.current.zoom(e.startIndex, e.endIndex);
        setZoomInfo(e);
      });
      chartHelper.on(EventTypes.ThresholdChange, (thr) => {
        // console.log("thr change:::", thr);
        setThrValue(thr.threshold);
        setThrLineTop(thr.top);
      });
      chartHelper.on(EventTypes.AxisYRangeChange, (min, max, tickGap) => {
        if (specChartRef.current) {
          specChartRef.current.setAxisYRange(min, max);
          // rainChartRef.current.setAxisYRange(min, max);
        }
        // console.log("dddddddddddddddd", min, max, tickGap);
        setAxisYRange({ minimum: min, maximum: max, tickGap });
      });
      chartHelper.on(EventTypes.CursorChange, (e) => {
        setCursorInfo(e);
      });
      chartHelper.on(EventTypes.SetSeriesVisible, (ids) => {
        // 隐藏和显示线条
        // 初始化线条
        const series = getLineSeries(seriesLineType, ids);
        series.forEach((s) => {
          specChartRef.current.initSeries(s);
        });
      });
    }
  }, [chartHelper]);

  useEffect(() => {
    // setThrValue(threshold);
    if (chartHelper && chartBounding[ChartTypes.spectrum]) {
      // setTimeout(() => {
      chartHelper.setThreshold(threshold);
      // }, 250);
    }
  }, [chartHelper, threshold, chartBounding]);

  return (
    <ChartTemplate
      lightTheme={lightTheme}
      axisYInside={axisY?.inside}
      axisXInside={axisX?.inside}
      padding={padding}
      axisY={
        axisY ? (
          <AxisY
            {...axisY}
            units={units}
            range={axisYRange}
            visibleCharts={visibleCharts}
            rainTickInfo={rainTS}
            colorBlends={lightTheme ? lightBlends : defaultBlends}
            chartPadding={padding}
            chartGap={chartGap}
            onChange={(e) => {
              let min = axisYRange.minimum;
              let max = axisYRange.maximum;
              const gap = axisYRange.tickGap;
              if (e === "add") {
                if (max < 250) {
                  max = max + gap;
                  min = min + gap;
                  chartHelper.setAxisYRange(min, max, false);
                }
              } else {
                if (min > -250) {
                  max = max - gap;
                  min = min - gap;
                  chartHelper.setAxisYRange(min, max, false);
                }
              }
            }}
            onTickChange={(e) => {
              if (e === 0) {
                // 自动 计算理想高度
                chartHelper.autoAxisYRange();
              } else {
                chartHelper.setAxisYTickGap(e);
              }
            }}
          />
        ) : null
      }
      axisX={
        axisX ? (
          <AxisX
            {...axisX}
            segments={segments ? segmentsView : undefined}
            frequency={frequency}
            bandwidth={bandwidth}
            zoomInfo={zoomInfo}
            streamTime={streamTimeSpan || streamTime}
          />
        ) : null
      }
    >
      <div className={styles.plotArea1}>
        <div className={styles.chartCon} ref={chartContainerRef}>
          {/* {segments && ( */}
          <SegmentsBackground segments={segmentsView} zoomInfo={zoomInfo} />
          {/* )} */}
          {signalBands && visibleCharts.includes(ChartTypes.spectrum) && (
            // 信号列表，选中框选
            <SignalBands
              signalList={signalBands}
              onChange={(e) => {
                console.log("signal select change :::", e);
                onSignalSelect(e);
              }}
              onDrag={(e) => {
                if (!zoomInfo) return;
                // 计算
                const { left, width, id } = e;
                const startIndex = Math.floor(
                  zoomInfo.startIndex + zoomInfo.zoomLen * left
                );
                const band = Math.round(zoomInfo.zoomLen * width);
                let segIndex = -1;
                let segStart = -1;
                let segEnd = -1;
                // 查找所在段&数据索引
                for (let i = 0; i < segmentsView.length; i += 1) {
                  const seg = segmentsView[i];
                  if (
                    startIndex >= seg.startIndex &&
                    seg.startIndex + seg.pointCount > startIndex
                  ) {
                    segIndex = i;
                    segStart = startIndex - seg.startIndex;
                    segEnd = segStart + band;
                    if (startIndex + band > seg.startIndex + seg.pointCount) {
                      segEnd = seg.pointCount - 1;
                    }
                    break;
                  }
                }
                onBandChange({
                  id,
                  segmentIndex: segIndex,
                  segmentOffset: segStart,
                  segmentOffset1: segEnd,
                });
              }}
            />
          )}
          {showBand && (
            // 信号框选
            <Band
              onChange={(e) => {
                if (!zoomInfo) return;
                // 计算
                const { left, width } = e;
                const startIndex = Math.floor(
                  zoomInfo.startIndex + zoomInfo.zoomLen * left
                );
                const band = Math.round(zoomInfo.zoomLen * width);
                let segIndex = -1;
                let segStart = -1;
                let segEnd = -1;
                // 查找所在段&数据索引
                for (let i = 0; i < segmentsView.length; i += 1) {
                  const seg = segmentsView[i];
                  if (
                    startIndex >= seg.startIndex &&
                    seg.startIndex + seg.pointCount > startIndex
                  ) {
                    segIndex = i;
                    segStart = startIndex - seg.startIndex;
                    segEnd = segStart + band;
                    if (startIndex + band > seg.startIndex + seg.pointCount) {
                      segEnd = seg.pointCount - 1;
                    }
                    break;
                  }
                }
                onBandChange({
                  segmentIndex: segIndex,
                  segmentOffset: segStart,
                  segmentOffset1: segEnd,
                });
              }}
            />
          )}
          <Markers
            markers={markers}
            chartBounding={chartBounding}
            style={{
              transform:
                dragOffset > -9999
                  ? `translateX(${dragOffset}px)`
                  : zoomOffset > -9999
                  ? `scaleX(${zoomOffset})`
                  : "none",
            }}
            onDragMarker={(e) => {
              console.log("drag marker::", e);
              chartHelper.dragMarker(e);
            }}
            onMoveMarker={(e) => {
              chartHelper.moveMarker(e);
            }}
            onSelectChange={(e) => chartHelper.selectMarker(e)}
          />
          {isMobile() && mobileZoomMode !== "normal" && allowZoom ? (
            <TouchZoom
              onDragging={(e) => {
                // TODO 如果需要图表也跟着transform在这里处理
                setDragOffset(e);
                console.log("dragging:::", e);
              }}
              onDragChange={(e) => {
                setDragOffset(-9999);
                const con = chartContainerRef.current.getBoundingClientRect();
                const scale = e / con.width;
                onTouchZoomChange(scale);
                //  const freqOffset = bandwidth * scale;
                //  const freq = frequency - freqOffset / 1000;
                //  onParameterChange({
                //    frequency: freq, // Math.round(freq * 10000) / 10000,
                //  });
              }}
              onZooming={(e) => {
                // TODO 如果需要图表也跟着transform在这里处理
                setZoomOffset(e);
              }}
              onZoomChange={(e) => {
                setZoomOffset(-9999);
                const bw = Math.round(bandwidth * e);
                // chartHelper.current.setVBW(bw);
                onParameterChange({ bandwidth: bw });
                // if (onParameterChange({ bandwidth: bw })) {
                //   setBandwidthByTouchZoom.current = true;
                // }
                //  没有真正改变参数，自行缩放
                // 但是如果真的改变了参数也不可能就是=参数带宽，也就是改了参数也需要缩放
                // setVBW(bw);
              }}
            />
          ) : (
            <ZoomAndCursor
              allowZoom={allowZoom}
              cursorInfo={cursorInfo}
              visibleCharts={visibleCharts}
              showCursor={showCursor}
              onMouseMove={(e) => {
                chartHelper.updateMouseDataIndex(e);
                if (showCursor && chartHelper) {
                  // 更新cursor 位置
                  chartHelper.moveCursor();
                }
              }}
              onZoomChange={(e) => {
                chartHelper.updateZoom(e);
              }}
              onAddMarker={() => {
                if (allowAddMarker) chartHelper.addMarker();
              }}
            />
          )}

          {visibleCharts.includes(ChartTypes.spectrum) && showThreshold && (
            <Threshold
              dragLimit={chartBounding[ChartTypes.spectrum]}
              threshold={thrValue}
              thrLineTop={thrLineTop}
              onDragThreshold={(e) => {
                // chartHelper.current.updateThreshold(e);
                if (e.end) {
                  console.log("fir thr::", thrValue);
                  onThresholdChange(thrValue);
                  // 清理数据
                  chartHelper;
                } else {
                  const specBounding = chartBounding[ChartTypes.spectrum];
                  if (specBounding) {
                    const rangeGap = axisYRange.maximum - axisYRange.minimum;
                    const offset = e / (specBounding.y2 - specBounding.y1);

                    const thr = Math.round(
                      axisYRange.maximum - rangeGap * offset
                    );

                    chartHelper.setThreshold(thr);
                  }
                  // onThresholdChange(thr);
                  // setThrValue(thr);
                }
              }}
            />
          )}
          <div className={styles.chartLayer}>
            <div
              className={styles.specContainer}
              style={{
                flex: visibleCharts.includes(ChartTypes.spectrum) ? 1 : 0,
              }}
            >
              <div
                className={styles.specChart}
                ref={specContainerRef}
                style={{
                  transform:
                    dragOffset > -9999
                      ? `translateX(${dragOffset}px)`
                      : zoomOffset > -9999
                      ? `scaleX(${zoomOffset})`
                      : "none",
                }}
              />
            </div>
            <div
              style={{
                width: "100%",
                height: visibleCharts.includes(ChartTypes.rain)
                  ? `${chartPixelGap}px`
                  : 0,
              }}
            />
            <div
              style={{
                flex: visibleCharts.includes(ChartTypes.rain) ? 1 : 0,
              }}
            >
              <div
                className={styles.rainChart}
                ref={rainContainerRef}
                style={{
                  transform:
                    dragOffset > -9999
                      ? `translateX(${dragOffset}px)`
                      : zoomOffset > -9999
                      ? `scaleX(${zoomOffset})`
                      : "none",
                }}
              />
            </div>
            <div
              style={{
                width: "100%",
                height: visibleCharts.includes(ChartTypes.wbdf)
                  ? `${chartPixelGap}px`
                  : 0,
              }}
            />
            <div
              style={{
                flex: visibleCharts.includes(ChartTypes.wbdf) ? 1 : 0,
              }}
            >
              <div
                className={styles.wbdf}
                ref={wbdfContainerRef}
                style={{
                  transform:
                    dragOffset > -9999
                      ? `translateX(${dragOffset}px)`
                      : zoomOffset > -9999
                      ? `scaleX(${zoomOffset})`
                      : "none",
                }}
              />
            </div>
          </div>
        </div>
      </div>
    </ChartTemplate>
  );
};

AllInOne.defaultProps = {
  frequency: 100,
  bandwidth: 1000,
  visibleCharts: [ChartTypes.spectrum, ChartTypes.rain],
  chartGap: 8,
  //   onLoad: () => {},
  onThresholdChange: () => {},
  axisX: { inside: false, component: null },
  axisY: {
    inside: false,
    autoRange: false,
    tickVisible: true,
  },
  showThreshold: false,
  showCursor: true,
  allowAddMarker: true,
  // 仅移动端生效 normal : 内部处理缩放
  // complex 复杂模式，扫描直接修改参数，
  // 测量拖动修改中心频率，缩放根据当前span决定是否修改带宽
  mobileZoomMode: "normal",
  onParameterChange: () => {},
  onTouchZoomChange: () => {},
  seriesLineType: "",
  allowZoom: true,
  // 电频流时间 s
  streamTime: 0,
  useGPU: true,
  padding: "8px 4px 0 0",
  showBand: false,
  onBandChange: () => {},
  onSignalSelect: () => {},
  threshold: 0,
  // signalList: [
  //   {
  //     id: "01",
  //     segmentIdx: 0,
  //     startFreqIdx: 10,
  //     stopFreqIdx: 15,
  //   },
  //   {
  //     id: "02",
  //     segmentIdx: 0,
  //     startFreqIdx: 80,
  //     stopFreqIdx: 105,
  //   },
  //   {
  //     id: "03",
  //     segmentIdx: 0,
  //     startFreqIdx: 200,
  //     stopFreqIdx: 208,
  //   },
  // ],
};

AllInOne.propTypes = {
  frequency: PropTypes.number,
  bandwidth: PropTypes.number,
  segments: PropTypes.array,
  visibleCharts: PropTypes.array,
  chartLoad: PropTypes.func.isRequired,
  onThresholdChange: PropTypes.func,
  chartGap: PropTypes.number,
  axisX: PropTypes.any,
  axisY: PropTypes.any,
  showThreshold: PropTypes.bool,
  showCursor: PropTypes.bool,
  allowAddMarker: PropTypes.bool,
  mobileZoomMode: PropTypes.string,
  onParameterChange: PropTypes.func,
  onTouchZoomChange: PropTypes.func,
  seriesLineType: PropTypes.string,
  chartHelper: PropTypes.any,
  allowZoom: PropTypes.bool,
  streamTime: PropTypes.number,
  useGPU: PropTypes.bool,
  padding: PropTypes.string,
  showBand: PropTypes.bool,
  onBandChange: PropTypes.func,
  threshold: PropTypes.number,
  onSignalSelect: PropTypes.func,
};

export default AllInOne;
