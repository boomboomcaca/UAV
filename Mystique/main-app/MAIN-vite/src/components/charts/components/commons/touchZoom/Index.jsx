import React, { useEffect, useRef } from "react";

import PropTypes from "prop-types";

import styles from "./style.module.less";
import { useState } from "react";

const TouchZoom = (props) => {
  const { onDragging, onZooming, onZoomChange, onDragChange } = props;
  const touchStartRef = useRef();
  const operatingRef = useRef(false);
  // 是否正在拖动
  const [dragging, setDragging] = useState(false);
  // 是否正在缩放
  const [zooming, setZooming] = useState(false);
  const [dragOffset, setDragOffset] = useState(0);
  const [zoomOffset, setZoomOffset] = useState(0);

  return (
    <div
      className={`${styles.touchZoomRoot} ${dragging && styles.dragging} ${
        zooming && styles.zooming
      }`}
      style={{
        transform: dragging
          ? `translateX(${dragOffset}px)`
          : zooming
          ? `scaleX(${zoomOffset})`
          : "none",
      }}
      onTouchStart={(e) => {
        if (!operatingRef.current) {
          touchStartRef.current = e.touches;
          if (e.touches.length === 2) {
            setDragging(false);
            setZooming(true);
          }
          if (e.touches.length === 1) {
            setDragging(true);
            setZooming(false);
          }
        }
      }}
      onTouchMove={(e) => {
        operatingRef.current = true;
        if (dragging) {
          const x1 = touchStartRef.current[0].clientX;
          const x2 = e.touches[0].clientX;
          const offset = x2 - x1;
          setDragOffset(offset);
          onDragging(offset);
        }
        if (zooming) {
          const x11 = touchStartRef.current[0].clientX;
          const x12 = touchStartRef.current[1].clientX;
          const offset1 = x12 - x11;
          const x21 = e.touches[0].clientX;
          const x22 = e.touches[1].clientX;
          const offset2 = x22 - x21;
          const scale = offset2 / offset1;
          setZoomOffset(scale);
          onZooming(scale);
        }
      }}
      onTouchEnd={(e) => {
        operatingRef.current = false;
        if (dragging) {
          onDragChange(dragOffset);
        }
        if (zooming) {
          onZoomChange(zoomOffset);
        }
        setZooming(false);
        setDragging(false);
        console.log("end:::");
      }}
    ></div>
  );
};

TouchZoom.defaultProps = {
  onDragChange: () => {},
  onZoomChange: () => {},
  onDragging: () => {},
  onZooming: () => {},
};

TouchZoom.propTypes = {
  onZoomChange: PropTypes.func,
  onDragChange: PropTypes.func,
  onDragging: PropTypes.func,
  onZooming: PropTypes.func,
};

export default TouchZoom;
