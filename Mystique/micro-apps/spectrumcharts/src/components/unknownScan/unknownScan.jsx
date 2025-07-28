import React, { useEffect, useState, useRef, useReducer } from "react";
import PropTypes from "prop-types";
import ToolBar, { toolItems } from "../commons/toolbar/Toolbar.jsx";
import { NormalChart } from "../../lib/index.js";
import FeatureLayer from "../commons/featureLayer/FeatureLayer.jsx";
import ChartContext, {
  reducer,
  initState,
  actions,
} from "../context/chartContext.jsx";
import SpectrumHelper from "./helper";
import AxisY from "../commons/axisY/AxisY.jsx";
import { ChartTypes } from "../utils/enums";
import YTicks from "../commons/axisY/ticks/Index.jsx";
import styles from "./style.module.less";

const UnknownScan = (props) => {
  const { segments, units, viewOptions, onLoad, threshold, onThresholdChange } =
    props;
  const prevSpectrumRef = useRef();
  const [state, dispatch] = useReducer(reducer, initState);
  const { minimumY, maximumY, zoomInfo, prevData } = state;
  const draggingMarkerRef = useRef("");
  const [segmentsView, setSegmentsView] = useState([]);
  const totalPointsRef = useRef(0);
  // name:String, color:String, thickness:Number, pointWidth:Number,
  // pointColor:String, showPoint:boolean, visible:boolean, type
  const lineSeriesRef = useRef([
    {
      name: "max",
      caption: "最大值",
      color: "#FF0000",
      thickness: 1,
      visible: true,
      type: "stepline",
    },
    {
      name: "min",
      caption: "最小值",
      color: "#0000FF",
      thickness: 1,
      visible: true,
      type: "line",
    },
    {
      name: "avg",
      caption: "平均值",
      color: "#FFFF00",
      thickness: 1,
      visible: true,
      type: "line",
    },
    {
      name: "real",
      caption: "实时值",
      color: "#00FF00",
      thickness: 1,
      visible: true,
      type: "line",
      pointWidth: 5,
      // showPoint: true,
      pointColor: "#FF0000",
    },
  ]);

  const specContainerRef = useRef();

  /**
   * @type {{current:ScanDataHelper}}
   */
  // const dataHelperRef = useRef();

  /**
   * @type {{current:SpectrumHelper}}
   */
  const chartHelper = useRef(
    new SpectrumHelper({
      onData: (e) => {
        const { spectrum } = e;
        if (spectrum) {
          specChartRef.current.setData([
            { name: "real", data: spectrum },
            //   {
            //     name: "max",
            //     data: spectrum.map((ii) => ii + Math.random() * 10 + 5),
            //   },
          ]);
        }

        const cursorInfo = chartHelper.current.moveCursor();
        if (cursorInfo) {
          dispatch({
            type: actions.updatecursor,
            value: { cursorInfo },
          });
        }
      },
      onMarkerChange: (markers) => {
        if (markers.length > 0) {
          dispatch({
            type: actions.updateMarker1,
            value: { markers },
          });
        }
      },
      onZoomChange: (e) => {
        specChartRef.current.zoom(e.startIndex, e.endIndex);
      },
      onResize: () => {
        if (specChartRef.current) {
          specChartRef.current.resize();
        }
      },
    })
  );
  /**
   * @type {{current:NormalChart}}
   */
  const specChartRef = useRef();

  useEffect(() => {
    chartHelper.current.updateChartArgs(props);
  }, [props]);

  useEffect(() => {
    totalPointsRef.current = chartHelper.current.updateSegments(segments);
    setSegmentsView(segments);
  }, [segments]);

  useEffect(() => {
    let listener;
    if (!specChartRef.current) {
      // 初始化线条
      const specChart = new NormalChart(specContainerRef.current);
      lineSeriesRef.current.forEach((series) => {
        specChart.initSeries(series);
      });
      specChartRef.current = specChart;
      // 自适应size
      listener = window.addEventListener("resize", () => {
        specChart.resize();
      });
      if (onLoad) {
        onLoad(chartHelper.current);
      }
    }
    return () => {
      if (listener) window.removeEventListener(listener);
    };
  }, []);

  useEffect(() => {
    if (specChartRef.current) {
      specChartRef.current.setAxisYRange(minimumY, maximumY);
    }
  }, [minimumY, maximumY]);

  useEffect(() => {
    if (specChartRef.current && prevSpectrumRef.current) {
      specChartRef.current.zoomPercent(zoomInfo.start, zoomInfo.end);
    }
  }, [zoomInfo]);

  useEffect(() => {
    const { toolbar } = viewOptions;
    if (!toolbar) {
      dispatch({
        type: "showCursor",
        value: { showCursor: false },
      });
    }
  }, [viewOptions]);

  //   useEffect(() => {
  //     if (specChartRef.current) {
  //       const gap = 1.0;
  //       const axisYRange = maximumY - minimumY;
  //       const thrPos = (maximumY - threshold) / axisYRange;

  //       dispatch({
  //         type: actions.updateThr,
  //         value: { thrPosition: thrPos * gap },
  //       });
  //     }
  //   }, [threshold, minimumY, maximumY]);

  return (
    <div className={styles.specChartRoot}>
      <ChartContext.Provider value={[state, dispatch]}>
        <div className={styles.plotArea}>
          {viewOptions.axisY && (
            <AxisY
              style={{
                width: viewOptions.axisY.inside ? "36px" : null,
                marginRight: viewOptions.axisY.inside ? "4px" : 0,
              }}
              tickVisible={
                viewOptions.axisY.inside ? false : viewOptions.axisY.tickVisible
              }
              labelVisible={
                viewOptions.axisY.inside
                  ? false
                  : viewOptions.axisY.labelVisible
              }
              units={units}
              visibleCharts={[ChartTypes.spectrum]}
              onAutoRange={() => {
                // 计算理想高度
                if (prevSpectrumRef.current) {
                  const max = Math.max(...prevSpectrumRef.current);
                  const min = Math.min(...prevSpectrumRef.current);
                  const gap = max - min + 5;
                  const tickGap = Math.ceil(gap / 50.0) * 5;
                  const minimumY = Math.floor(min / 5.0) * 5;
                  const maximumY = minimumY + tickGap * 10;
                  dispatch({
                    type: actions.setAxisYRange,
                    value: {
                      minimumY,
                      maximumY,
                    },
                  });
                }
              }}
            />
          )}
          <div className={styles.plotArea1}>
            {viewOptions.toolbar && (
              <ToolBar
                onChange={(e) => {
                  switch (e.action) {
                    case toolItems.clear:
                      if (specChartRef.current) {
                        specChartRef.current.clear();
                      }
                      break;
                    default:
                      break;
                  }
                }}
              />
            )}
            {viewOptions.axisY && viewOptions.axisY.inside && (
              <div className={styles.tickCon} style={{ height: `100%` }}>
                <YTicks labelVisible style={{ height: "100%" }} />
              </div>
            )}
            <div className={styles.chartCon}>
              <FeatureLayer
                //  showThreshold={viewOptions.showThreshold}
                visibleCharts={[ChartTypes.spectrum]}
                onMouseMove={(e) => {
                  chartHelper.current.updateMouseDataIndex(e);
                  const { dragMarker } = e;

                  draggingMarkerRef.current = dragMarker;
                  if (state.showCursor && chartHelper.current) {
                    // 更新cursor 位置
                    const cursorInfo = chartHelper.current.moveCursor();
                    if (cursorInfo) {
                      dispatch({
                        type: actions.updatecursor,
                        value: { cursorInfo },
                      });
                    }
                  }
                  if (dragMarker) {
                    chartHelper.current.dragMarker(e);
                  }
                }}
                onZoomChange={(e) => {
                  chartHelper.current.updateZoom(e);
                  dispatch({
                    type: actions.chartZoom,
                    value: chartHelper.current.zoomInfo,
                  });
                }}
                onAddMarker={(e) => {
                  chartHelper.current.addMarker();
                }}
                //  onThrChange={(e) => {
                //    dispatch({
                //      type: actions.updateThr,
                //      value: { thrPosition: e.value },
                //    });
                //    const axisYRange = maximumY - minimumY;
                //    const gap = 1.0 ;
                //    const offset = axisYRange * (e.value / gap);
                //    if (e.end && onThresholdChange)
                //      onThresholdChange(maximumY - offset);
                //  }}
              />
              <div className={styles.segbgLayer}>
                {segments.map((s, index) => {
                  const zoomInfo = chartHelper.current.zoomInfo;
                  let startPct = -1;
                  let widthPct = -1;
                  const sStart = s.startIndex;
                  const sStop = s.startIndex + s.pointCount;
                  const zStart = zoomInfo.startIndex;
                  const zStop = zoomInfo.endIndex;
                  if (sStart < zStart && sStop > zStart) {
                    startPct = 0;
                    widthPct = ((sStop - zStart) * 100) / zoomInfo.zoomLen;
                  } else if (zStart >= zStart && sStop <= zStop) {
                    startPct = ((sStart - zStart) * 100) / zoomInfo.zoomLen;
                    widthPct = (s.pointCount * 100) / zoomInfo.zoomLen;
                  } else if (sStart < zStop && sStop > zStop) {
                    startPct = ((sStart - zStart) * 100) / zoomInfo.zoomLen;
                    widthPct = ((zStop - sStart) * 100) / zoomInfo.zoomLen;
                  }
                  if (startPct < 0) return null;
                  return (
                    <div
                      className={`${styles.segbg} ${
                        index % 2 > 0 && styles.segbgeven
                      }`}
                      style={{
                        left: `${startPct}%`,
                        width: `${widthPct}%`,
                      }}
                    />
                  );
                })}
              </div>
              <div className={styles.chartLayer}>
                <div
                  style={{
                    position: "relative",
                    flex: 1,
                  }}
                >
                  <div className={styles.specChart} ref={specContainerRef} />
                </div>
              </div>
            </div>
          </div>
        </div>
        {/* {viewOptions.axisX && } */}
        {/* <AxisX frequency={frequency} bandwidth={bandwidth} /> */}
      </ChartContext.Provider>
    </div>
  );
};

UnknownScan.defaultProps = {
  segments: [
    {
      startFrequency: 87,
      stopFrequency: 108,
      stepFrequency: 25,
    },
    {
      startFrequency: 137,
      stopFrequency: 167,
      stepFrequency: 25,
    },
  ],
  viewOptions: {
    axisX: true,
    axisY: { tickVisible: true, labelVisible: true, inside: false },
    toolbar: true,
  },
  onLoad: () => {},
  threshold: 15,
  onThresholdChange: () => {},
};

UnknownScan.propTypes = {
  segments: PropTypes.array,
  viewOptions: PropTypes.object,
  onLoad: PropTypes.func,
  threshold: PropTypes.number,
  onThresholdChange: PropTypes.func,
};

export default UnknownScan;
