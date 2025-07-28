import React, { useEffect, useRef, useState } from "react";

import PropTypes from "prop-types";
import dayjs from "dayjs";
import { frequency2String } from "../../utils/utils";
import styles from "./style.module.less";

const TouchZoom = (props) => {
  const {
    frequency,
    bandwidth,
    startFrequency,
    stopFrequency,
    onDragging,
    onZooming,
    onZoomChange,
    onDragChange,
    onMoveCursor,
    cursorInfo,
    allowZoom,
  } = props;
  /**
   * @type {{current:HTMLDivElement}}
   */
  const domRef = useRef();
  const domRectRef = useRef();
  const touchStartRef = useRef();
  const operatingRef = useRef(false);
  // 是否正在拖动
  const [dragging, setDragging] = useState(false);
  // 是否正在多指缩放
  const [zooming, setZooming] = useState(false);
  const [touchMoved, setTouchMoved] = useState(false);
  const [dragOffset, setDragOffset] = useState(0);
  const [zoomOffset, setZoomOffset] = useState(0);

  const [offsetFrequency, setOffsetFrequency] = useState(0);
  const [offsetBandwidth, setOffsetBandwidth] = useState(0);
  const [cursorTimeout, setCursorTimeout] = useState(false);
  const cursorHideTmr = useRef();

  const onMousePosChanged = (mousePos) => {
    const rect = domRef.current.getBoundingClientRect();
    const px = mousePos.clientX - rect.x;
    const py = mousePos.clientY - rect.y;
    const letPct = px / rect.width;
    const topPct = py / rect.height;
    const args = {
      x: letPct,
      y: topPct,
      px: px,
      py: py,
      dragMarker: false,
    };
    args.width = rect.width;
    args.height = rect.height;
    onMoveCursor(args);
  };

  return (
    <>
      <div
        ref={domRef}
        className={`${styles.touchZoomRoot} ${
          dragging && touchMoved && styles.dragging
        } ${zooming && touchMoved && styles.zooming}`}
        style={{
          transform: dragging
            ? `translateX(${dragOffset}px)`
            : zooming
            ? `scaleX(${zoomOffset})`
            : "none",
        }}
        onClick={(e) => {
          onMousePosChanged(e);
          // 移动端单击移动游标，5s后自动消失
          if (cursorHideTmr.current) {
            clearTimeout(cursorHideTmr.current);
            cursorHideTmr.current = null;
          }
          setCursorTimeout(false);
          cursorHideTmr.current = setTimeout(() => {
            setCursorTimeout(true);
          }, 10000);
        }}
        onTouchStart={(e) => {
          if (!operatingRef.current && allowZoom) {
            touchStartRef.current = e.touches;
            domRectRef.current = domRef.current.getBoundingClientRect();
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
            setTouchMoved(true);
            const x1 = touchStartRef.current[0].clientX;
            const x2 = e.touches[0].clientX;
            const offset = x2 - x1;
            if (offset > 5 || offset < -5) {
              setTouchMoved(true);
              setDragOffset(offset);
              const scale = offset / domRectRef.current.width;
              if (startFrequency < 0) {
                const offsetBW = bandwidth * scale;
                setOffsetFrequency(frequency - Math.round(offsetBW / 10) / 100);
              } else {
                const bw = stopFrequency - startFrequency;
                const offsetBW = bw * scale;
                setOffsetFrequency(
                  startFrequency + bw - Math.round(offsetBW * 100) / 100
                );
              }
              onDragging(offset);
            }
          }
          if (zooming) {
            setTouchMoved(true);
            const x11 = touchStartRef.current[0].clientX;
            const x12 = touchStartRef.current[1].clientX;
            const offset1 = x12 - x11;
            const x21 = e.touches[0].clientX;
            const x22 = e.touches[1].clientX;
            const offset2 = x22 - x21;
            const scale = offset2 / offset1;
            setZoomOffset(scale);
            if (startFrequency < 0) {
              const offsetBW = bandwidth * (1 / scale);
              setOffsetBandwidth(Math.round(offsetBW / 10) / 100);
            } else {
              const offsetBW = (stopFrequency - startFrequency) * (1 / scale);
              setOffsetBandwidth(Math.round(offsetBW * 100) / 100);
            }
            onZooming(scale);
          }
        }}
        onTouchEnd={(e) => {
          setTouchMoved(false);
          operatingRef.current = false;
          if (dragging) {
            onDragChange(dragOffset);
            setDragOffset(0);
          }
          if (zooming) {
            onZoomChange(zoomOffset);
            setZoomOffset(0);
          }
          setZooming(false);
          setDragging(false);
        }}
      >
        {cursorInfo && !cursorTimeout && (
          <>
            <div
              className={styles.cursorx}
              style={{ top: `calc(${cursorInfo.cursorPosY}% - 2px)` }}
              hidden={cursorInfo.cursorPosY < 0 && cursorInfo.cursorPosX < 0}
            />
            <div
              className={styles.cursory}
              style={{ left: `${cursorInfo.cursorPosX}%` }}
              hidden={cursorInfo.cursorPosX < 0}
            />

            <div className={styles.caption}>
              <div className={styles.captionContent}>
                <div>
                  频率：{cursorInfo.frequency?.toFixed(6)}
                  <span>MHz</span>
                </div>
                {cursorInfo.dataInfo && cursorInfo.dataInfo.timestamp > 0 && (
                  <div>
                    时间：
                    {dayjs(new Date(cursorInfo.dataInfo.timestamp)).format(
                      "HH:mm:ss.SSS"
                    )}
                  </div>
                )}
                {cursorInfo.dataInfo && (
                  <div>
                    数据：
                    {cursorInfo.dataInfo.levelCaption}
                    <span>{cursorInfo.dataInfo.unit}</span>
                  </div>
                )}
                {cursorInfo.dataInfo && cursorInfo.tickValue > -9999 && (
                  <div>刻度：{cursorInfo.tickValue}</div>
                )}
              </div>
            </div>
          </>
        )}
      </div>
      {dragging && touchMoved && (
        <div className={styles.touchZoomCaption}>
          中心频率
          <div>{frequency2String(offsetFrequency)}</div>
        </div>
      )}
      {zooming && touchMoved && (
        <div className={styles.touchZoomCaption}>
          带宽
          <div>{frequency2String(offsetBandwidth)}</div>
        </div>
      )}
    </>
  );
};

TouchZoom.defaultProps = {
  onDragChange: () => {},
  onZoomChange: () => {},
  onDragging: () => {},
  onZooming: () => {},
  onMoveCursor: () => {},
  frequency: 98,
  startFrequency: 87,
  stopFrequency: 108,
};

TouchZoom.propTypes = {
  onZoomChange: PropTypes.func,
  onDragChange: PropTypes.func,
  onDragging: PropTypes.func,
  onZooming: PropTypes.func,
  frequency: PropTypes.number,
  startFrequency: PropTypes.number,
  stopFrequency: PropTypes.number,
  onMoveCursor: PropTypes.func,
  cursorInfo: PropTypes.any,
};

export default TouchZoom;
