import React, { useState, useRef, useContext, useEffect } from "react";
import ChartContext, { actions } from "../../context/chartContext.jsx";

import styles from "./style.module.less";

/**
 * @param {{marker:Marker, onDragging:Function}} props
 */
const Marker = (props) => {
  const { marker, onDragging } = props;
  const [showLine, setShowLine] = useState(false);
  const [selected, setSelected] = useState(false);
  const ctx = useContext(ChartContext);
  const [state = "", dispatch = null] = ctx;
  // const { mouseInCenterMK } = state;

  useEffect(() => {
    console.log("marker ok;;;;");
  }, []);

  return (
    <>
      {(showLine || marker?.mouseInCenter) && (
        <div
          className={styles.lineX}
          style={{ left: `${marker?.position.x}%` }}
        />
      )}
      {showLine && (
        <div
          className={styles.lineY}
          style={{ top: `calc(${marker?.position.y}% - 13px)` }}
        />
      )}

      <div
        className={`${styles.markerRoot} && ${selected && styles.sel}`}
        style={{
          left: `calc(${marker?.position.x}% - 11px)`,
          top: `calc(${marker?.position.y}% - 22px)`,
          cursor: `${showLine ? "move" : "default"}`,
        }}
        onMouseEnter={() => setShowLine(true)}
        onMouseLeave={() => setShowLine(false)}
        onClick={() => setSelected(!selected)}
        onMouseDown={() => {
          console.log("marker mouse down ======");
          if (onDragging) {
            onDragging({
              id,
              dragging: true,
            });
          }
        }}
        onMouseUp={() => {
          if (onDragging) {
            onDragging({
              id,
              dragging: false,
            });
          }
        }}
      >
        <div className={styles.symbol} />
      </div>
    </>
  );
};

export default Marker;
