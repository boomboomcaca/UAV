import React, { useEffect, useRef, useState } from "react";
import PropTypes from "prop-types";

import { ChartTypes, UnitItems } from "../../utils/enums";

import Spectrum from "./spectrum/Index.jsx";
import Rain from "./rain/Index.jsx";
import Bearing from "./bearing/Index.jsx";
import Occupancy from "./occupancy/Index.jsx";
import styles from "./style.module.less";

const AxisY = (props) => {
  const { visibleCharts, chartPadding, onTickChange, unit } = props;

  const rootIdRef = useRef("axixy-" + Math.random().toFixed(6));
  const [showCaption, setShowCaption] = useState(false);
  const [tickGap, setTickGap] = useState(10);
  const [showGapPanel, setShowGapPanel] = useState(false);

  useEffect(() => {
    setTimeout(() => {
      const htmlEle = document.getElementById(rootIdRef.current);
      if (htmlEle) {
        const rect = htmlEle.getBoundingClientRect();
        setShowCaption(rect.height >= 240);
      }
    });
    const listener = window.addEventListener("resize", () => {
      setTimeout(() => {
        const htmlEle = document.getElementById(rootIdRef.current);
        if (htmlEle) {
          const rect = htmlEle.getBoundingClientRect();
          setShowCaption(rect.height >= 240);
        }
      }, 1000);
    });

    return () => {
      if (listener) window.removeEventListener(listener);
    };
  }, []);

  const [paddingInfo, setPaddingInfo] = useState({ top: 8, bottom: 0 });

  useEffect(() => {
    if (chartPadding) {
      const arr = String(chartPadding).split(" ");
      const top = arr[0].replace("px", "");
      let bottom = top;
      if (arr.length === 4) {
        bottom = arr[3].replace("px", "");
      }
      setPaddingInfo({
        top,
        bottom,
      });
    }
  }, [chartPadding]);

  const tmrRef = useRef();
  // 3s 后自动隐藏
  useEffect(() => {
    if (tmrRef.current) clearTimeout(tmrRef.current);
    tmrRef.current = setTimeout(() => {
      // setShowGapPanel(false);
    }, 3000);
  }, [showGapPanel]);

  return (
    <div className={styles.axisYRoot} id={rootIdRef.current}>
      <div className={styles.container}>
        {visibleCharts.includes(ChartTypes.spectrum) && (
          <Spectrum
            {...props}
            showCaption={showCaption}
            paddingInfo={paddingInfo}
            onSetGap={(e) => {
              if (e) {
                // 自动调整
                const newRange = onTickChange(0);
                if (newRange) setTickGap(newRange);
              } else {
                setShowGapPanel(true);
              }
            }}
            tickGap={tickGap}
          />
        )}
        {visibleCharts.includes(ChartTypes.rain) && (
          <Rain {...props} paddingInfo={paddingInfo} />
        )}
        {visibleCharts.includes(ChartTypes.wbdf) && (
          <Bearing showCaption={showCaption} />
        )}
        {visibleCharts.includes(ChartTypes.occupancy) && (
          <Occupancy showCaption={showCaption} />
        )}
      </div>
      {showGapPanel && (
        <div className={styles.tickGap}>
          {[1, 5, 10, 15, 20].map((item) => {
            return (
              <div
                className={`${styles.divItem} ${
                  item === tickGap && styles.sel
                }`}
                onClick={() => {
                  setTickGap(item);
                  setShowGapPanel(false);
                  // 更新刻度范围
                  onTickChange(item);
                }}
              >
                {`${item}/div`}
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
};

AxisY.defaultProps = {
  visibleCharts: [ChartTypes.spectrum, ChartTypes.rain],
  units: ["dBμV", "dBm"],
  tickVisible: true,
  // onAutoRange: () => {},
  onTickChange: () => {},
  rainTickInfo: {},
  autoRange: false,
  range: { minimum: -20, maximum: 80 },
  onChange: () => {},
  chartPadding: null,
  chartGap: 8,
};

AxisY.propTypes = {
  visibleCharts: PropTypes.array,
  units: PropTypes.array,
  tickVisible: PropTypes.bool,
  onTickChange: PropTypes.func,
  rainTickInfo: PropTypes.any,
  autoRange: PropTypes.bool,
  range: PropTypes.any,
  onChange: PropTypes.func,
  chartPadding: PropTypes.string,
  chartGap: PropTypes.number,
};

export default AxisY;
