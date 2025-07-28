import React, {
  useEffect,
  useState,
  useRef,
  useLayoutEffect,
  memo,
} from "react";
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
// import Markers from "../commons/markers1/Index.jsx";
import Threshold from "../commons/threshold/Index.jsx";
import SegmentsBackground from "../commons/segmentsBackground/Index.jsx";
import SignalBands from "../commons/signalBands/Index.jsx";
import FilterBand from "../commons/filterBand/Index.jsx";
import Band from "../commons/band/Index.jsx";
import { ChartTypes, SeriesTypes, EventTypes } from "../utils/enums.js";
import styles from "./style.module.less";
import { isMobile } from "../utils/utils.js";
import { defaultBlends, lightBlends } from "../assets/colors.js";
import SpectrumChartHelper from "../utils/spectrumChartHelper.js";
import ScanChartHelper from "../utils/scanChartHelper.js";

/**
 *
 * @param {{chartHelper:SpectrumChartHelper|ScanChartHelper}} props
 * @returns
 */
const AllInOne = (props) => {
  const {
    frequency,
    bandwidth,
    segments,
    frequencyList,
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
    initCharts,
    maxMarker,
    filterBandwidth,
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
    unitValid: false,
  });
  const [segmentsView, setSegmentsView] = useState([]);
  const [zoomInfo, setZoomInfo] = useState();
  const [cursorInfo, setCursorInfo] = useState();
  const [colorBlends, setColorBlends] = useState([]);
  const [unit, setUnit] = useState("");
  const [markers, setMarkers] = useState([]);
  // 图表间隔 像素
  const [chartPixelGap, setChartPixelGap] = useState(8);
  const [chartBounding, setChartBounding] = useState({});
  // 当前图表所有的子图表
  const allCharts = [
    ChartTypes.spectrum,
    ChartTypes.rain,
    ChartTypes.wbdf,
    ChartTypes.occupancy,
  ];
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
   * @type {{current:HTMLElement}}
   */
  const occContainerRef = useRef();

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
  /**
   * @type {{current:NormalChart}}
   */
  const occChartRef = useRef();

  const [thrLineTop, setThrLineTop] = useState(40);
  const [thrValue, setThrValue] = useState(0);
  const [signalBands, setSignalBands] = useState();
  // const [selSignal, setSelSignal] = useState();

  // const [vbw, setVBW] = useState();
  // 是否通过多指缩放来更新带宽
  // const setBandwidthByTouchZoom = useRef(false);

  /**
   * 获取频谱图线条定义
   * @param {*} mode
   * @param {any} visibleIds
   */
  const getLineSeries = (mode, visibleIds, colors) => {
    console.log("init series:::", chartHelper.chartConfig);
    const series = [
      {
        name: SeriesTypes.max,
        // caption: "最大值",
        color: colors.max,
        thickness: 1,
        visible: visibleIds[SeriesTypes.max],
        type: mode ? "stepline" : "line",
      },
      {
        name: SeriesTypes.min,
        // caption: "最小值",
        color: colors.min,
        thickness: 1,
        visible: visibleIds[SeriesTypes.min],
        type: mode ? "stepline" : "line",
      },
      {
        name: SeriesTypes.avg,
        // caption: "平均值",
        color: colors.avg,
        thickness: 1,
        visible: visibleIds[SeriesTypes.avg],
        type: mode ? "stepline" : "line",
      },
      {
        name: SeriesTypes.real,
        // caption: "实时值",
        color: colors.real,
        thickness: 1,
        // visible: true,
        type: mode ? "bar" : "line",
        pointWidth: 5,
        pointColor: colors.real,
      },
      {
        name: SeriesTypes.thr,
        // caption: "实时值",
        color: "#da70d6",
        thickness: 1,
        // visible: true,
        type: "line",
        pointWidth: 5,
        pointColor: colors.thr,
        visible: visibleIds[SeriesTypes.thr],
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

  const prevSeg = useRef();
  useEffect(() => {
    if (segments) {
      const jsonStr = JSON.stringify(segments);
      if (prevSeg.current !== jsonStr) {
        prevSeg.current = jsonStr;

        chartHelper.updateSegments(segments);
        setSegmentsView(segments);
        if (specChartRef.current) {
          specChartRef.current.clear();
          // rainChartRef.current.clear();
        }
      }
    }
  }, [segments]);

  const mScanModeRef = useRef(false);

  useEffect(() => {
    if (frequencyList) {
      // 模拟频段
      const virSegments = [
        {
          startFrequency: 1,
          stopFrequency: frequencyList.length,
          stepFrequency: 1000,
        },
      ];
      chartHelper.updateSegments(virSegments, frequencyList);
      setSegmentsView(virSegments);
      mScanModeRef.current = true;
      if (specChartRef.current) {
        specChartRef.current.clear();

        // 初始化线条
        const series = getLineSeries(true, [], chartHelper.chartConfig);
        series.forEach((s) => {
          specChartRef.current.initSeries(s);
        });
      }
    }
  }, [frequencyList, chartHelper]);

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
    visibleCharts.forEach((chart, index) => {
      // if (chart === ChartTypes.spectrum) {
      const rect = specContainerRef.current.getBoundingClientRect();
      rect.y1 = (index * (gap + chartPixelGap)) / con.height;
      rect.y2 = (gap + index * (gap + chartPixelGap)) / con.height;
      bounding[chart] = rect;
      // } else if (chart === ChartTypes.rain) {
      //   const rect = rainContainerRef.current.getBoundingClientRect();
      //   if (visibleCharts.includes(ChartTypes.spectrum)) {
      //     rect.y1 = (gap + chartPixelGap) / con.height;
      //     rect.y2 = 1;
      //   } else {
      //     rect.y1 = 0;
      //     rect.y2 = gap / con.height;
      //   }
      //   bounding[chart] = rect;
      // } else if (chart === ChartTypes.wbdf) {
      //   const rect = wbdfContainerRef.current.getBoundingClientRect();
      //   if (visibleCharts.length > 1) {
      //     rect.y1 = (gap + chartPixelGap) / con.height;
      //     rect.y2 = 1;
      //   } else {
      //     rect.y1 = 0;
      //     rect.y2 = gap / con.height;
      //   }
      //   bounding[chart] = rect;
      // } else if (chart === ChartTypes.occupancy) {
      //   const rect = occContainerRef.current.getBoundingClientRect();
      //   if (visibleCharts.length > 1) {
      //     rect.y1 = (gap + chartPixelGap) / con.height;
      //     rect.y2 = 1;
      //   } else {
      //     rect.y1 = 0;
      //     rect.y2 = gap / con.height;
      //   }
      //   bounding[chart] = rect;
      // }
    });
    setChartBounding(bounding);
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
    if (specContainerRef.current && !specChartRef.current) {
      const specChart = new NormalChart(specContainerRef.current, {
        // bgColor: lightTheme ? "#fff" : "#000",
        onSizeChange: (w, h) => {
          chartHelper.setPlotArea({ width: w });
        },
      });
      // 初始化线条
      const series = getLineSeries(
        frequencyList && true,
        [],
        chartHelper.chartConfig
      );
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
            chartHelper.setPlotArea({ width: w, height: h });
          },
        });
      } else {
        rainChartRef.current = new RainChart(
          rainContainerRef.current,
          undefined,
          {
            onSizeChange: (w, h) => {
              console.log("rain chart height change:::", h);
              chartHelper.setPlotArea({ width: w, height: h });
            },
          }
        );
      }
      if (wbdfContainerRef.current) {
        // 初始化点图
        const wbdfChart = new NormalChart(wbdfContainerRef.current);
        wbdfChart.setAxisYRange(0, 360, false);
        pointSeriesRef.current.forEach((series) => {
          wbdfChart.initSeries(series);
        });
        wbdfChartRef.current = wbdfChart;
      }

      if (occContainerRef.current) {
        // 初始化占用度图
        const occChart = new NormalChart(occContainerRef.current);
        occChart.initSeries({
          name: "occ",
          color: "#00FF00",
          type: "bar",
        });
        occChart.setAxisYRange(0, 100, false);
        occChartRef.current = occChart;
      }

      listener = window.addEventListener("resize", () => {
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
  }, [chartLoad, useGPU, lightTheme, chartHelper, distributionBar]);

  useEffect(() => {
    if (specChartRef.current && prevSpectrumRef.current) {
      // specChartRef.current.zoomPercent(zoomInfo.start, zoomInfo.end);
      // rainChartRef.current.zoomPercent(zoomInfo.start, zoomInfo.end);
    }
  }, [zoomInfo]);
  useEffect(() => {
    // if (!axisYRange.unitValid) {
    //   const { minimum, maximum, tickGap } = axisYRange;
    //   setAxisYRange({
    //     minimum: minimum,
    //     maximum: maximum,
    //     tickGap,
    //     unitValid: true,
    //   });
    //   // dBmRangeRef.current = true;
    // }
  }, [unit, axisYRange]);

  useLayoutEffect(() => {
    if (chartHelper) {
      let prevRainTickUpdate = new Date().getTime();
      setColorBlends(chartHelper.chartConfig.rainColors);
      setUnit(chartHelper.chartConfig.unit);
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
          occ,
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
          const dt = new Date().getTime();
          if (dt - prevRainTickUpdate > 499) {
            prevRainTickUpdate = dt;
            setRainTS({
              currentPct: currentPct,
              rainTimeSpan,
            });
          }
        }
        if (scanDF && visibleChartsRef.current.includes(ChartTypes.wbdf)) {
          wbdfChartRef.current.setData([{ name: "real", data: scanDF.data }]);
        }
        if (occ && visibleChartsRef.current.includes(ChartTypes.occupancy)) {
          occChartRef.current.setData([{ name: "occ", data: occ.data }]);
        }
        setStreamTimeSpan(Math.round(streamTimeSpan / 100) / 10);
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
        setAxisYRange({
          minimum: min,
          maximum: max,
          tickGap,
          unitValid: false,
        });
      });
      chartHelper.on(EventTypes.CursorChange, (e) => {
        setCursorInfo(e);
      });
      chartHelper.on(EventTypes.SetSeriesVisible, (ids) => {
        // 隐藏和显示线条
        // 初始化线条
        const series = getLineSeries(
          mScanModeRef.current,
          ids,
          chartHelper.chartConfig
        );
        series.forEach((s) => {
          specChartRef.current.initSeries(s);
        });
      });
      chartHelper.on(EventTypes.ConfigChange, (e) => {
        // 更新瀑布图的色带
        setColorBlends(chartHelper.chartConfig.rainColors);
        setUnit(chartHelper.chartConfig.unit);
        // TODO 更新线图
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
            unit={unit}
            colorBlends={colorBlends} //{lightTheme ? lightBlends : defaultBlends}
            chartPadding={padding}
            chartGap={chartGap}
            onChange={(e) => {
              let min = axisYRange.minimum;
              let max = axisYRange.maximum;
              const gap = axisYRange.tickGap;
              if (e === "add") {
                if (max < 300) {
                  max = max + gap;
                  min = min + gap;
                  chartHelper.setAxisYRange(min, max, false);
                }
              } else {
                if (min > -300) {
                  max = max - gap;
                  min = min - gap;
                  chartHelper.setAxisYRange(min, max, false);
                }
              }
              chartHelper.setThreshold(thrValue);
            }}
            onTickChange={(e) => {
              setTimeout(() => {
                chartHelper.setThreshold(thrValue);
              }, 200);
              if (e === 0) {
                // 自动 计算理想高度
                return chartHelper.autoAxisYRange();
              } else {
                chartHelper.setAxisYTickGap(e);
              }
              return null;
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
          {filterBandwidth && (
            <FilterBand
              bandwidth={bandwidth}
              filterBandwidth={filterBandwidth}
            />
          )}
          <Markers
            chartHelper={chartHelper}
            markers={markers}
            chartBounding={chartBounding}
            showMarkerPanel={maxMarker < 6}
            moveable={maxMarker < 6}
            style={{
              transform:
                dragOffset > -9999
                  ? `translateX(${dragOffset}px)`
                  : zoomOffset > -9999
                  ? `scaleX(${zoomOffset})`
                  : "none",
            }}
            onDragMarker={(e) => {
              chartHelper.dragMarker(e);
            }}
            onMoveMarker={(e) => {
              chartHelper.moveMarker(e);
            }}
            onSelectChange={(e) => chartHelper.selectMarker(e)}
            onDelMarker={(e) => chartHelper.delMarker(e)}
          />
          {isMobile() && mobileZoomMode !== "normal" && (
            <TouchZoom
              allowZoom={allowZoom}
              frequency={frequency}
              bandwidth={bandwidth}
              cursorInfo={cursorInfo}
              startFrequency={
                segments && segments.length > 0
                  ? segments[0].startFrequency
                  : -1
              }
              stopFrequency={
                segments && segments.length > 0 ? segments[0].stopFrequency : -1
              }
              onMoveCursor={(e) => {
                console.log("mover cursor on touch");
                chartHelper.updateMouseDataIndex(e, !mScanModeRef.current);
                if (chartHelper) {
                  // 更新cursor 位置
                  chartHelper.moveCursor();
                }
              }}
              onDragging={(e) => {
                // 如果需要图表也跟着transform在这里处理
                if (e > 5 || e < -5) {
                  setDragOffset(e);
                  specChartRef.current.getCanvas().style.transform = `translateX(${e}px)`;
                  rainChartRef.current.getCanvas().style.transform = `translateX(${e}px)`;
                }
                // : zoomOffset > -9999
                // ? `scaleX(${zoomOffset})`
                // : "none";
              }}
              onDragChange={(e) => {
                // 拖动处理
                if (e > 5 || e < -5) {
                  setDragOffset(-9999);
                  specChartRef.current.getCanvas().style.transform = "none";
                  rainChartRef.current.getCanvas().style.transform = "none";
                  const con = chartContainerRef.current.getBoundingClientRect();
                  const scale = e / con.width;
                  onParameterChange({ drag: scale });
                }
              }}
              onZooming={(e) => {
                // TODO 如果需要图表也跟着transform在这里处理
                setZoomOffset(e);
                specChartRef.current.getCanvas().style.transform = `scaleX(${e})`;
                rainChartRef.current.getCanvas().style.transform = `scaleX(${e})`;
              }}
              onZoomChange={(e) => {
                // 多指缩放处理
                setZoomOffset(-9999);
                specChartRef.current.getCanvas().style.transform = "none";
                rainChartRef.current.getCanvas().style.transform = "none";
                onParameterChange({ zoom: 1 / e });
                // onTouchZoomChange(e);
              }}
            />
          )}
          {!isMobile() && mobileZoomMode === "normal" && (
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
                if (allowAddMarker) chartHelper.addMarker1(null, maxMarker);
              }}
            />
          )}
          {visibleCharts.includes(ChartTypes.spectrum) && showThreshold && (
            <Threshold
              dragLimit={chartBounding[ChartTypes.spectrum]}
              threshold={thrValue}
              thrLineTop={thrLineTop}
              unit={unit}
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
            {initCharts.includes(ChartTypes.spectrum) && (
              <div
                style={{
                  flex: visibleCharts.includes(ChartTypes.spectrum) ? 1 : 0,
                }}
              >
                <div className={styles.specChart} ref={specContainerRef} />
              </div>
            )}
            {initCharts.includes(ChartTypes.rain)}
            {
              <>
                <div
                  style={{
                    width: "100%",
                    height:
                      visibleCharts.includes(ChartTypes.rain) &&
                      visibleCharts.includes(ChartTypes.spectrum)
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
                      visibility: visibleCharts.includes(ChartTypes.rain)
                        ? "visible"
                        : "collapse",
                    }}
                  />
                </div>
              </>
            }
            {initCharts.includes(ChartTypes.wbdf) && (
              <>
                <div
                  style={{
                    width: "100%",
                    height:
                      visibleCharts.includes(ChartTypes.wbdf) &&
                      visibleCharts.includes(ChartTypes.rain)
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
                      visibility: visibleCharts.includes(ChartTypes.wbdf)
                        ? "visible"
                        : "collapse",
                      transform:
                        dragOffset > -9999
                          ? `translateX(${dragOffset}px)`
                          : zoomOffset > -9999
                          ? `scaleX(${zoomOffset})`
                          : "none",
                    }}
                  />
                </div>
              </>
            )}

            {initCharts.includes(ChartTypes.occupancy) && (
              <>
                <div
                  style={{
                    width: "100%",
                    height:
                      visibleCharts.includes(ChartTypes.occupancy) &&
                      visibleCharts.length > 1
                        ? `${chartPixelGap}px`
                        : 0,
                  }}
                />
                <div
                  style={{
                    flex: visibleCharts.includes(ChartTypes.occupancy) ? 1 : 0,
                  }}
                >
                  <div
                    className={styles.occChart}
                    ref={occContainerRef}
                    style={{
                      visibility: visibleCharts.includes(ChartTypes.occupancy)
                        ? "visible"
                        : "collapse",
                      transform:
                        dragOffset > -9999
                          ? `translateX(${dragOffset}px)`
                          : zoomOffset > -9999
                          ? `scaleX(${zoomOffset})`
                          : "none",
                    }}
                  />
                </div>
              </>
            )}
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
  chartGap: 6,
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
  allowZoom: true,
  // 电频流时间 s
  streamTime: 0,
  useGPU: true,
  padding: "8px 4px 0 0",
  showBand: false,
  onBandChange: () => {},
  onSignalSelect: () => {},
  threshold: 0,
  initCharts: [ChartTypes.spectrum, ChartTypes.rain],
  maxMarker: 0,
  filterBandwidth: 0,
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
  chartHelper: PropTypes.any,
  allowZoom: PropTypes.bool,
  streamTime: PropTypes.number,
  useGPU: PropTypes.bool,
  padding: PropTypes.string,
  showBand: PropTypes.bool,
  onBandChange: PropTypes.func,
  threshold: PropTypes.number,
  onSignalSelect: PropTypes.func,
  initCharts: PropTypes.array,
  maxMarker: PropTypes.number,
  filterBandwidth: PropTypes.number,
};

export default AllInOne;
