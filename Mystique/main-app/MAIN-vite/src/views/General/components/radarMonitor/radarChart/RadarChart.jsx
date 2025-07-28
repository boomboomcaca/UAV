import React, { useEffect, useRef, useState } from "react";
import PropTypes from "prop-types";
import Dial from "./Dial.jsx";
import TickLabel from "./TickLabel.jsx";
import RenderLayer from "./RenderLayer.jsx";
import styles from "./styles.module.less";

export default function RadarChart(props) {
  const { showTick, showTickLabel, tickInside, onLoaded, resize } = props;
  const [scope, setScope] = useState(0);
  /**
   * @type {{current:HTMLElement}}
   */
  const wbdf = useRef();
  const prevSizeRefg = useRef(0);

  useEffect(() => {
    const tmr = setInterval(() => {
      const { width, height } = wbdf.current.parentElement.getClientRects()[0];
      let offset = width >= height ? height : width;
      if (prevSizeRefg.current !== offset) {
        prevSizeRefg.current = offset;
        setScope(offset);
      }
    }, 1000);
    return () => {
      clearInterval(tmr);
    };
  }, []);

  return (
    <div
      className={styles.WBDFPolar}
      style={{ height: scope, width: scope }}
      ref={wbdf}
    >
      <div
        className={
          (!showTick || tickInside) && !showTickLabel
            ? styles.dialNoLabelAndTick
            : showTick && !tickInside
            ? styles.dial1
            : styles.dial
        }
      >
        <Dial {...props} />
      </div>
      {showTickLabel && <TickLabel {...props} />}
      <RenderLayer onReady={onLoaded} />
    </div>
  );
}

RadarChart.defaultProps = {
  showTick: true,
  showTickLabel: true,
  tickInside: false,
  onLoaded: undefined,
  resize: 0,
};

RadarChart.propTypes = {
  showTick: PropTypes.bool,
  showTickLabel: PropTypes.bool,
  tickInside: PropTypes.bool,
  onLoaded: PropTypes.func,
  resize: PropTypes.any,
};
