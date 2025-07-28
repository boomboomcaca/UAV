import React, { useState, useRef, useEffect } from "react";
import PropTypes from "prop-types";
import { NormalChart } from "../../lib/index.js";
import ChartTemplate from "../commons/layoutTemplate/Index.jsx";
import { SeriesTypes } from "../utils/enums.js";

import styles from "./style.module.less";

const DDCChart = (props) => {
  const { onLoad, lightTheme, padding, mode10 } = props;

  /**
   * @type {{current:HTMLElement}}
   */
  const chartContainerRef = useRef();

  /**
   * @type {{current:NormalChart}}
   */
  const chartRef = useRef();

  const prevRangeTime = useRef(0);

  useEffect(() => {
    if (chartContainerRef.current && !chartRef.current) {
      chartRef.current = new NormalChart(chartContainerRef.current, {
        onSizeChange: (w, h) => {
          // console.log(w, h);
        },
      });
      chartRef.current.initSeries({
        name: SeriesTypes.real,
        color: "#00FF00",
        type: "line",
      });

      /**
       *
       * @param {Array} data
       * @returns
       */
      const autoRange = (data) => {
        let min = 99999;
        let max = -99999;
        for (let i = 0; i < data.length; i += 1) {
          const d = data[i];
          if (d < min) {
            min = d;
          }
          if (d > max) {
            max = d;
          }
        }
        return { min, max };
      };
      const prevRange = { min: 0, max: 80 };

      onLoad({
        setData: (data) => {
          let spec = data;
          const dt = new Date().getTime();
          if (dt - prevRangeTime.current > 1000) {
            if (mode10) {
              spec = data.slice(0);
              
              for (let i = 0; i < spec.length; i += 1) {
                spec[i] = spec[i] / 10;
              }
            }
            const minMax = autoRange(spec);

            const range = minMax.max - minMax.min;
            const min = Math.floor(minMax.min - range * 0.1);
            const max = Math.ceil(minMax.max + range * 0.1);
            if (min < prevRange.min) {
              prevRange.min = min;
            }
            if (max > prevRange.max) {
              prevRange.max = max;
            }

            chartRef.current.setAxisYRange(prevRange.min, prevRange.max, false);
            prevRangeTime.current = dt;
          }
          chartRef.current.setData([{ name: SeriesTypes.real, data: spec }]);
        },
      });
    }
  }, [onLoad, lightTheme, mode10]);

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

DDCChart.defaultProps = {
  onLoad: () => {},
  lightTheme: false,
  padding: "0",
  mode10: false,
};

DDCChart.propTypes = {
  onLoad: PropTypes.func,
  lightTheme: PropTypes.bool,
  padding: PropTypes.string,
  mode10: PropTypes.bool,
};

export default DDCChart;
