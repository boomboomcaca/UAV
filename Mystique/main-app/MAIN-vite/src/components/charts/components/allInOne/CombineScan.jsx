import React, { useLayoutEffect, useState, useEffect } from "react";
import PropTypes from "prop-types";

import AllInOne from "./Index.jsx";
import { ChartTypes } from "../utils/enums.js";
import ScanChartHelper from "../utils/scanChartHelper";
import { defaultBlends, lightBlends } from "../assets/colors.js";

const CombineScan = (props) => {
  const { onLoad, useGPU, mScanMode, lightTheme } = props;

  /**
   * @type {[ScanChartHelper]}
   */
  const [chartHelper, setChartHelper] = useState();

  useLayoutEffect(() => {
    if (!chartHelper) {
      setChartHelper(
        new ScanChartHelper({
          colorBlends: lightTheme ? lightBlends : defaultBlends,
          gpuRain: useGPU,
          defaultColor: lightTheme ? 255 : 0,
        })
      );
    }
  }, [chartHelper, lightTheme]);

  useEffect(() => {
    return () => {
      if (chartHelper) chartHelper.dispose();
    };
  }, [chartHelper]);

  return (
    <>
      {chartHelper && (
        <AllInOne
          {...props}
          seriesLineType={mScanMode}
          chartHelper={chartHelper}
          chartLoad={(e) => {
            console.log("chart loaded:::", e);
            // specChartRef.current = e.specChart;
            // rainChartRef.current = e.rainChart;
            onLoad(chartHelper);
          }}
        />
      )}
    </>
  );
};

CombineScan.defaultProps = {
  segments: [{ startFrequency: 87, stopFrequency: 108, stepFrequency: 25 }],
  visibleCharts: [ChartTypes.spectrum, ChartTypes.rain],
  chartGap: 8,
  onLoad: () => {},
  onThresholdChange: () => {},
  threshold: 0,
  axisX: { inside: false, component: null },
  axisY: {
    inside: false,
    autoRange: true,
    tickVisible: true,
  },
  showThreshold: false,
  showCursor: true,
  allowAddMarker: true,
  mScanMode: false,
  // 仅移动端&只有一个频段时生效
  // normal : 内部处理缩放
  // complex 复杂模式，扫描直接修改参数，
  // 测量拖动修改中心频率，缩放根据当前span决定是否修改带宽
  mobileZoomMode: "normal",
  onParameterChange: () => {},
  padding: "",
  showBand: false,
  onBandChange: () => {},
  onSignalSelect: () => {},
};

CombineScan.propTypes = {
  segments: PropTypes.array,
  visibleCharts: PropTypes.array,
  onLoad: PropTypes.func,
  onThresholdChange: PropTypes.func,
  threshold: PropTypes.number,
  chartGap: PropTypes.number,
  axisX: PropTypes.any,
  axisY: PropTypes.any,
  showThreshold: PropTypes.bool,
  showCursor: PropTypes.bool,
  allowAddMarker: PropTypes.bool,
  mScanMode: PropTypes.bool,
  mobileZoomMode: PropTypes.string,
  onParameterChange: PropTypes.func,
  padding: PropTypes.string,
  showBand: PropTypes.bool,
  onBandChange: PropTypes.func,
  onSignalSelect: PropTypes.func,
  // lightTheme: PropTypes.bool.isRequired,
};

export default CombineScan;
