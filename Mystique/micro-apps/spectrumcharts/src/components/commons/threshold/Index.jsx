import React, { useState, useEffect, useRef } from "react";

import PropTypes from "prop-types";

import { ArrowIcon } from "../../assets/svgIcons";
import styles from "./style.module.less";
import { isMobile } from "../../utils/utils";

const Threshold = (props) => {
  const { onDragThreshold, thrLineTop, dragLimit, threshold, unit } = props;
  const [dragReady, setDragReady] = useState(false);
  const [dragging, setDragging] = useState(false);
  const [lineTop, setLineTop] = useState();
  /**
   * @type {{current:HTMLElement}}
   */
  const domRef = useRef();
  const domRectRef = useRef();

  /**
   *
   * @param {{clientY:Number}} e
   * @param {{y1:Number,y2:Number}} limit
   * @returns {Number}
   */
  const drag = (e, limit) => {
    let pct = (e.clientY - domRectRef.current.top) / domRectRef.current.height;
    if (pct < limit.y1) pct = limit.y1;
    if (pct > limit.y2) pct = limit.y2;
    // 外部会更新，所以避免更新两次，内部就不处理了
    //     setLineTop(pct * 100);
    return pct;
  };

  const dragEnd = (dragging) => {
    if (dragging) {
      setDragging(false);
      onDragThreshold({ end: true });
    }
  };

  useEffect(() => {
    setLineTop(thrLineTop);
  }, [thrLineTop]);

  return (
    <div
      className={styles.thrRoot}
      ref={domRef}
      style={{ pointerEvents: dragging ? "all" : "none" }}
      onMouseMove={(e) => {
        if (domRectRef.current && dragging) {
          const top = drag(e, dragLimit);
          onDragThreshold(top);
        }
      }}
      onMouseUp={() => dragEnd(dragging)}
      onTouchEnd={() => {
        setDragReady(false);
        dragEnd(dragging);
      }}
      onTouchMove={(e) => {
        if (domRectRef.current && dragging) {
          const touch = e.touches[0];
          const top = drag(touch, dragLimit);
          onDragThreshold(top);
        }
      }}
    >
      {lineTop > -2 && lineTop <= dragLimit.y2 * 102 && (
        <div
          className={styles.container}
          style={{ top: `calc(${lineTop}% - 18px)` }}
        >
          {(dragReady || dragging) && (
            <span>{unit === "dBm" ? threshold - 105 : threshold}</span>
          )}
          <div
            className={styles.line}
            onMouseEnter={() => setDragReady(true)}
            onMouseLeave={() => setDragReady(false)}
            onMouseDown={() => {
              domRectRef.current = domRef.current.getBoundingClientRect();
              setDragging(true);
            }}
          >
            <div />
          </div>
          {isMobile() && (
            <div
              className={`${styles.slider} ${dragging && styles.drag}`}
              onTouchStart={() => {
                domRectRef.current = domRef.current.getBoundingClientRect();
                setDragReady(true);
                setDragging(true);
              }}
              onTouchEnd={() => {
                setDragReady(false);
                dragEnd(dragging);
              }}
            >
              <ArrowIcon />
              <ArrowIcon rotate={180} />
              {/* <div className={styles.thumb}>{threshold}</div> */}
            </div>
          )}
        </div>
      )}
    </div>
  );
};

Threshold.defaultProps = {
  onDragThreshold: () => {},
  thrLineTop: 40,
  dragLimit: { y1: 0, y2: 49.98 },
  threshold: 0,
  unit: "dBμV",
};

Threshold.propTypes = {
  onDragThreshold: PropTypes.func,
  thrLineTop: PropTypes.number,
  dragLimit: PropTypes.any,
  threshold: PropTypes.any,
  unit: PropTypes.string,
};

export default Threshold;
