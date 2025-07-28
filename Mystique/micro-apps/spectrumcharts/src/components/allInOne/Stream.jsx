import React, { useEffect, useState, useRef, useLayoutEffect } from "react";
import PropTypes from "prop-types";
import AllInOne from "./Index.jsx";
import StreamChartHelper from "../utils/streamChartHelper.js";
import { ChartTypes, EventTypes } from "../utils/enums.js";
import { NormalChart } from "../../lib/index.js";

const Stream = (props) => {
  const { onLoad, distributionBar, lightTheme, axisY, streamTime, showRSSI } =
    props;

  const [visibles] = useState([ChartTypes.spectrum]);
  /**
   * @type {[StreamChartHelper]}
   */
  const [chartHelper, setChartHelper] = useState();
  const [realLevel, setRealLevel] = useState(-999);
  /**
   * @type {{current:NormalChart}}
   */
  const rssiChartRef = useRef();
  /**
   * @type {{current:HTMLElement}}
   */
  const rssiContainerRef = useRef();

  useLayoutEffect(() => {
    if (!chartHelper) {
      setChartHelper(new StreamChartHelper({ distributionBar }));
    }
  }, [chartHelper, distributionBar]);

  useEffect(() => {
    if (chartHelper) {
      chartHelper.on(EventTypes.DataChange, (e) => {
        if (document.hidden) return;
        const { spectrum } = e;
        if (spectrum) {
          // 最新的电平值
          setRealLevel(spectrum.data[spectrum.data.length - 1]);
        }
      });
      chartHelper.onRssiData((rssi) => {
        if (rssiChartRef.current) {
          rssiChartRef.current.setData([{ name: "rssi", data: rssi }]);
        }
      });
    }
    return () => {
      if (chartHelper) chartHelper.dispose();
    };
  }, [chartHelper]);

  useEffect(() => {}, [rssiContainerRef]);

  return (
    <>
      {chartHelper && (
        <div style={{ position: "relative", width: "100%", height: "100%" }}>
          <AllInOne
            {...props}
            initCharts={[ChartTypes.spectrum]}
            showCursor={false}
            allowAddMarker={false}
            allowZoom={false}
            visibleCharts={visibles}
            streamTime={chartHelper.chartConfig.streamTime || streamTime}
            chartHelper={chartHelper}
            // 创建横向bar分布统计图
            distributionBar={distributionBar}
            chartLoad={(e) => {
              onLoad(chartHelper);

              if (rssiContainerRef.current) {
                rssiChartRef.current = new NormalChart(
                  rssiContainerRef.current,
                  {
                    // bgColor: lightTheme ? "#fff" : "#000",
                    onSizeChange: (w, h) => {},
                  }
                );
                rssiChartRef.current.initSeries({
                  name: "rssi",
                  // caption: "最大值",
                  color: "#f44336",
                  thickness: 2,
                  visible: true,
                  type: "line",
                });
                rssiChartRef.current.setAxisYRange(-14, 86);
              }
            }}
          />
          {showRSSI && (
            <div
              style={{
                position: "absolute",
                top: "65%",
                left: axisY.inside ? "64px" : "78px",
                right: 0,
                bottom: 0,
                opacity: 0.5,
                // backgroundColor: "darkred",
              }}
              ref={rssiContainerRef}
            ></div>
          )}
          <div
            style={{
              position: "absolute",
              left: axisY.inside ? "32px" : "88px",
              top: "8px",
              color: lightTheme ? "#222" : "white",
            }}
          >
            {`${axisY.type ? axisY.title : "电平"}：${
              realLevel > -999
                ? chartHelper.chartConfig.unit === "dBm"
                  ? realLevel.toFixed(1) - 105
                  : realLevel.toFixed(1)
                : "--"
            }`}
          </div>
        </div>
      )}
    </>
  );
};

Stream.defaultProps = {
  onLoad: () => {},
  onThresholdChange: () => {},
  axisX: { inside: false, component: null },
  axisY: {
    inside: false,
    autoRange: false,
    tickVisible: true,
    title: "时频图",
  },
  streamTime: 20,
  showThreshold: false,
  showRSSI: true,
};

Stream.propTypes = {
  onLoad: PropTypes.func,
  onThresholdChange: PropTypes.func,
  axisX: PropTypes.any,
  axisY: PropTypes.any,
  showThreshold: PropTypes.bool,
  streamTime: PropTypes.number,
  distributionBar: PropTypes.bool,
  showRSSI: PropTypes.bool,
};

export default Stream;
