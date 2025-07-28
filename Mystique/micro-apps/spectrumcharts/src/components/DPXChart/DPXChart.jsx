import React, { useState, useRef, useEffect, useLayoutEffect } from "react";
import PropTypes from "prop-types";
import { RainChart } from "../../lib/index.js";
import DPXChartHelper from "../utils/dpxChartHelper.js";
import ChartTemplate from "../commons/layoutTemplate/Index.jsx";
import { SeriesTypes, EventTypes } from "../utils/enums.js";

import styles from "./style.module.less";

const DPXChart = (props) => {
  const { onLoad, lightTheme, padding, mode10 } = props;

  /**
   * @type {{current:HTMLElement}}
   */
  const chartContainerRef = useRef();

  /**
   * @type {{current:RainChart}}
   */
  const chartRef = useRef();

  const prevRangeTime = useRef(0);

  /**
   * @type {[DPXChartHelper]}
   */
  const [chartHelper, setChartHelper] = useState();

  useLayoutEffect(() => {
    if (!chartHelper) {
      setChartHelper(new DPXChartHelper({}));
    }
  }, [chartHelper]);

  useEffect(() => {
    if (chartContainerRef.current && !chartRef.current) {
      chartRef.current = new RainChart(chartContainerRef.current, null, {
        onSizeChange: (w, h) => {
          // console.log(w, h);
        },
      });
      // chartRef.current.initSeries({
      //   name: SeriesTypes.real,
      //   color: "#00FF00",
      //   type: "line",
      // });
      chartRef.current.setAxisYRange(0, 30);
    }
  }, [onLoad, lightTheme, mode10]);

  useEffect(() => {
    if (chartHelper) {
      chartHelper.on(EventTypes.DataChange, (e) => {
        if (document.hidden) return;
        const { dpx } = e;
        if (dpx) {
          chartRef.current.setFixedMatrix(dpx);
        }
      });
      onLoad(chartHelper);
    }
    return () => {
      if (chartHelper) chartHelper.dispose();
    };
  }, [chartHelper]);

  return (
    <ChartTemplate padding={padding}>
      <div
        style={{ overflow: "hidden" }}
        ref={chartContainerRef}
        className={`${styles.poltCon} ${styles.chartbg}`}
      ></div>
    </ChartTemplate>
  );
};

DPXChart.defaultProps = {
  onLoad: () => {},
  lightTheme: false,
  padding: "0",
  mode10: false,
};

DPXChart.propTypes = {
  onLoad: PropTypes.func,
  lightTheme: PropTypes.bool,
  padding: PropTypes.string,
  mode10: PropTypes.bool,
};

export default DPXChart;
