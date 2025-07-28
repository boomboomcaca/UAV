import React, { useState, useRef, useEffect } from "react";
import PropTypes from "prop-types";
import { NormalChart } from "../../lib/index.js";
import ChartTemplate from "../commons/layoutTemplate/Index.jsx";
import { SeriesTypes } from "../utils/enums.js";

import styles from "./style.module.less";

const IQChart = (props) => {
  const { onLoad, lightTheme, padding, planisphere } = props;

  const [containerSize, setContainerSize] = useState(100);

  /**
   * @type {{current:HTMLElement}}
   */
  const containerRef = useRef();

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
      chartRef.current = new NormalChart(chartContainerRef.current);
      chartRef.current.initSeries({
        // I
        name: SeriesTypes.real,
        color: "#507d00",
        type: planisphere ? "point" : "line",
        pointWidth: 3,
        pointColor: "#507d00",
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
      if (!planisphere) {
        chartRef.current.initSeries({
          // Q
          name: SeriesTypes.max,
          color: "#FF0000",
          thickness: 1,
          type: "line",
        });

        onLoad({
          setData: (iData, qData) => {
            const dt = new Date().getTime();
            if (dt - prevRangeTime.current > 1000) {
              const minMax = autoRange(iData);

              const range = minMax.max - minMax.min;
              const min = Math.floor(minMax.min - range * 0.1);
              const max = Math.ceil(minMax.max + range * 0.1);
              if (min < prevRange.min) {
                prevRange.min = min;
              }
              if (max > prevRange.max) {
                prevRange.max = max;
              }

              chartRef.current.setAxisYRange(
                prevRange.min,
                prevRange.max,
                false
              );
              prevRangeTime.current = dt;
            }
            chartRef.current.setData([
              { name: SeriesTypes.real, data: iData },
              {
                name: SeriesTypes.max,
                data: qData,
              },
            ]);
          },
        });
      } else {
        onLoad({
          setData: (iData, qData) => {
            const minMax = autoRange(iData);
            const range = minMax.max - minMax.min;
            const min = Math.floor(minMax.min - range * 0.1);
            const max = Math.ceil(minMax.max + range * 0.1);
            chartRef.current.setAxisYRange(min, max, false);

            chartRef.current.setXYData([
              { name: SeriesTypes.real, xData: iData, yData: qData },
            ]);
          },
        });
      }
    }
  }, [onLoad, lightTheme, planisphere]);

  /**
   * @type {{current:ResizeObserver}}
   */
  const resizeObserver = useRef();

  useEffect(() => {
    let resizing = false;
    if (planisphere) {
      setTimeout(() => {
        if (containerRef.current) {
          resizeObserver.current = new ResizeObserver((entries) => {
            if (entries.length > 0 && !resizing) {
              const rect = entries[0].contentRect;
              let size = Math.round(rect.width);
              if (size > rect.height) {
                size = Math.round(rect.height);
              }
              console.log(size);
              resizing = true;
              setContainerSize(size);

              setTimeout(() => {
                resizing = false;
              }, 20);
            }
          });
          resizeObserver.current.observe(containerRef.current);
        }
      }, 100);
    }
    return () => {
      if (resizeObserver.current) {
        try {
          resizeObserver.current.unobserve(containerRef.current);
          resizeObserver.current.disconnect();
        } catch {}
      }
    };
  }, [planisphere]);

  return (
    <ChartTemplate padding={padding} lightTheme={lightTheme}>
      {planisphere ? (
        <div ref={containerRef} className={styles.chartCon}>
          <div
            ref={chartContainerRef}
            className={styles.chartbg}
            style={{
              width: `${containerSize}px`,
              height: `${containerSize}px`,
            }}
          ></div>
        </div>
      ) : (
        <div className={styles.chartCon1}>
          <div
            ref={chartContainerRef}
            className={`${styles.poltCon} ${styles.chartbg}`}
          ></div>
          <div className={`${styles.lengend} ${lightTheme && styles.light}`}>
            <div
              style={{
                backgroundColor: "#507d00",
                width: "16px",
                height: "2px",
                margin: "0 6px 0 16px",
              }}
            />
            <span>I</span>
            <div
              style={{
                backgroundColor: "#FF0000",
                width: "16px",
                height: "2px",
                margin: "0 6px 0 16px",
              }}
            />
            <span>Q</span>
          </div>
        </div>
      )}
    </ChartTemplate>
  );
};

IQChart.defaultProps = {
  onLoad: () => {},
  lightTheme: false,
  padding: "8px 4px 0 0",
  planisphere: false,
};

IQChart.propTypes = {
  onLoad: PropTypes.func,
  lightTheme: PropTypes.bool,
  padding: PropTypes.string,
  planisphere: PropTypes.bool,
};

export default IQChart;
