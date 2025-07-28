import React, { memo, useEffect, useRef, useState, useContext } from "react";
import PropTypes from "prop-types";
import { useSize } from "ahooks";

import dayjs from "dayjs";
import { MouseMode, ChartTypes } from "../../utils/enums.js";
import Marker from "./marker/Marker.jsx";
import ChartContext, { actions } from "../../context/chartContext.jsx";
import { isMobile } from "../../utils/utils.js";
import styles from "./style.module.less";

/**
 * 交互层
 * 1. 游标
 * 2. marker操作
 * 3. 缩放
 * @param {{style:any,onMouseMove:{x:Number,y:Number,dragMarker:String}}} props
 * @returns
 */
const Markers = memo((props) => {
  const {
    style,
    onDragMarker,
    onMoveMarker,
    onSelectChange,
    chartBounding,
    markers,
    // 这个主要是拿来解决一些疑难杂症，比如拖动图表式不显示
    visible,
  } = props;

  /**
   * @type {{current:HTMLElement}}
   */
  const domRef = useRef();

  const [draggingMarker, setDraggingMarker] = useState();
  const [releaseDrag, setReleaseDrag] = useState();

  const dragging = (draggingMarker, e) => {
    if (draggingMarker) {
      const rect = domRef.current.getBoundingClientRect();
      const px = e.clientX - rect.x;
      const py = e.clientY - rect.y;
      const letPct = px / rect.width;
      const topPct = py / rect.height;
      const args = {
        x: letPct,
        y: topPct,
        px: px,
        py: py,
      };
      args.width = rect.width;
      args.height = rect.height;
      onDragMarker({ ...args, ...draggingMarker });
    }
  };

  return (
    <div
      ref={domRef}
      className={styles.markersRoot}
      style={{
        ...style,
        pointerEvents: draggingMarker ? "all" : "none",
      }}
      onMouseMove={(e) => {
        dragging(draggingMarker, e);
      }}
      onMouseUp={(e) => {
        setReleaseDrag(e);
      }}
      onTouchMove={(e) => {
        const touch = e.targetTouches[0];
        dragging(draggingMarker, touch);
      }}
      onTouchEnd={(e) => {
        setReleaseDrag(e);
      }}
    >
      {visible &&
        markers.map((item) => {
          if (item && item.visible) {
            // console.log("render marker::::", item);
            const colone = { ...item };
            return (
              <Marker
                marker={colone}
                chartBounding={chartBounding}
                releaseDrag={releaseDrag}
                onSelect={(e) => onSelectChange(e)}
                onReadyDrag={(e) => {
                  setDraggingMarker(e);
                }}
                onStopDrag={(e) => {
                  setDraggingMarker(undefined);
                  setReleaseDrag(undefined);
                }}
                onMove={(e) => onMoveMarker(e)}
              />
            );
          }
          return null;
        })}

      {!isMobile() && (
        <div className={styles.markerContainer}>
          {markers.map((item, index) => {
            if (item) {
              return (
                <div className={styles.markerItem}>
                  <span>MK{index + 1}</span>
                  <div className={styles.markerContent}>
                    <div className={styles.freq}>{item.freqInfo?.no}</div>
                    <span>{item.freqInfo?.unit}</span>
                    <div className={styles.level}>
                      {item.dataInfo?.levelCaption}
                    </div>
                    <span>{item.dataInfo?.unit}</span>
                  </div>
                </div>
              );
            }
            return null;
          })}
        </div>
      )}
    </div>
  );
});

Markers.defaultProps = {
  style: "",
  visibleCharts: [],
  onDragMarker: () => {},
  onSelectChange: () => {},
  onMoveMarker: () => {},
  markers: [],
  visible: true,
};

Markers.propTypes = {
  style: PropTypes.string,
  visibleCharts: PropTypes.array,
  onDragMarker: PropTypes.func,
  onSelectChange: PropTypes.func,
  onMoveMarker: PropTypes.func,
  markers: PropTypes.array,
  visible: PropTypes.bool,
};

export default Markers;
