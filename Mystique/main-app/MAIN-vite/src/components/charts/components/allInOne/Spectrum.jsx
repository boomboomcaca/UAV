import React, { useLayoutEffect, useState, useRef, useEffect } from "react";
import PropTypes from "prop-types";
import AllInOne from "./Index.jsx";
import SpectrumChartHelper from "../utils/spectrumChartHelper.js";
import { defaultBlends, lightBlends } from "../assets/colors.js";
import { ChartTypes } from "../utils/enums.js";

const Spectrum = (props) => {
  const { onLoad, useGPU, lightTheme } = props;
  /**
   * @type {{current:NormalChart}}
   */
  const specChartRef = useRef();
  /**
   * @type {{current:RainChartGPUWorker}}
   */
  const rainChartRef = useRef();

  /**
   * @type {[SpectrumChartHelper]}
   */
  const [chartHelper, setChartHelper] = useState();

  useLayoutEffect(() => {
    if (!chartHelper) {
      setChartHelper(
        new SpectrumChartHelper({
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
          chartHelper={chartHelper}
          chartLoad={(e) => {
            console.log("chart loaded:::", e);
            specChartRef.current = e.specChart;
            rainChartRef.current = e.rainChart;
            onLoad(chartHelper);
          }}
        />
      )}
    </>
  );
};

Spectrum.defaultProps = {
  frequency: 100,
  bandwidth: 1000,
  visibleCharts: [ChartTypes.spectrum, ChartTypes.rain],
  chartGap: 8,
  onLoad: () => {},
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
  allowZoom: false,
};

Spectrum.propTypes = {
  frequency: PropTypes.number,
  bandwidth: PropTypes.number,
  visibleCharts: PropTypes.array,
  onLoad: PropTypes.func,
  onThresholdChange: PropTypes.func,
  chartGap: PropTypes.number,
  axisX: PropTypes.any,
  axisY: PropTypes.any,
  showThreshold: PropTypes.bool,
  showCursor: PropTypes.bool,
  allowAddMarker: PropTypes.bool,
  mobileZoomMode: PropTypes.string,
  onParameterChange: PropTypes.func,
  allowZoom: PropTypes.bool,
};

export default Spectrum;
