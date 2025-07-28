import React, { useEffect, useRef, useState, useContext } from "react";
import PropTypes from "prop-types";

import dayjs from "dayjs";
import { ChartTypes } from "../../utils/enums.js";
import ChartContext from "../../context/chartContext.jsx";
import styles from "./style.module.less";
import { isMobile } from "../../utils/utils.js";

/**
 * 交互层
 * 1. 游标
 * 3. 缩放
 * @param {{style:any,onMouseMove:{x:Number,y:Number,dragMarker:String}}} props
 * @returns
 */
const ZoomAndCursor = (props) => {
  const {
    style,
    onZoomChange,
    onAddMarker,
    onMouseMove,
    showCursor,
    cursorInfo,
    allowZoom,
  } = props;

  // const ctx = useContext(ChartContext);
  // const [state = "", dispatch = null] = ctx;
  // const { cursorInfo } = state;

  const draggingMarkerRef = useRef();

  /**
   * @type {{current:HTMLElement}}
   */
  const domRef = useRef();
  const prevTouchRef = useRef();

  const [containerRect, setContainerRect] = useState({ left: 0, width: 0 });
  const [zoomStart, setZoomStart] = useState(-1);
  const [zoomEnd, setZoomEnd] = useState(-1);
  const [mouseDown, setMouseDown] = useState(false);

  useEffect(() => {
    if (domRef.current) {
      const rect = domRef.current.getBoundingClientRect();
      setContainerRect(rect);
    }
  }, [domRef.current]);

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
      dragMarker: draggingMarkerRef.current,
    };
    args.width = rect.width;
    args.height = rect.height;
    onMouseMove(args);
  };

  return (
    <div
      ref={domRef}
      className={styles.zoomCursorRoot}
      style={{
        ...style,
        zIndex: mouseDown ? 9999 : 8,
        // cursor: `${mouseInCenterMK ? "col-resize" : "default"}`,
      }}
      onMouseMove={(e) => {
        onMousePosChanged(e);
        // 更新缩放结束位置
        setZoomEnd(e.clientX);
      }}
      onMouseDown={(e) => {
        setZoomStart(e.clientX);
        setZoomEnd(e.clientX);
        setMouseDown(true);
      }}
      onTouchStart={(e) => {
        const touch = e.changedTouches[0];
        // 这里更新游标位置,好在触摸模式下也可以自由双击添加marker
        onMousePosChanged(touch);
        setZoomStart(touch.clientX);
        setZoomEnd(touch.clientX);
        setMouseDown(true);
      }}
      onTouchMove={(e) => {
        const touch = e.changedTouches[0];
        onMousePosChanged(touch);
        setZoomEnd(touch.clientX);
      }}
      onTouchEnd={(e) => {
        const touch = e.changedTouches[0];
        // ######### 判断双击 ########
        if (prevTouchRef.current) {
          const { time, x, y } = prevTouchRef.current;
          if (time - new Date().getTime() < 300) {
            // 触发触摸屏的双击
            prevTouchRef.current = undefined;
            if (
              Math.abs(x - touch.clientX) < 6 &&
              Math.abs(y - touch.clientY) < 6
            ) {
              console.log("on add markr");
              onAddMarker();
            }
          }
        }
        prevTouchRef.current = {
          time: new Date().getTime(),
          x: touch.clientX,
          y: touch.clientY,
        };
        // ######### 判断双击 ########

        setMouseDown(false);
        // 触发事件
        if (onZoomChange && allowZoom) {
          if (zoomEnd - 5 > zoomStart) {
            // 放大
            let start = zoomStart - containerRect.left;
            start = start < 0 ? 0 : start;
            let end = zoomEnd - containerRect.left;
            end = end > containerRect.width ? containerRect.width : end;
            const args = { start, end, width: containerRect.width };
            onZoomChange(args);
          } else {
            // 缩小（重置放大）
            const args = {
              start: 0,
              end: containerRect.width,
              width: containerRect.width,
            };
            onZoomChange(args);
          }
        }
      }}
      onMouseUp={(e) => {
        // if (mouseMoveModeRef.current === MouseMode.zoom) {
        setMouseDown(false);
        // 触发事件
        if (onZoomChange && allowZoom) {
          if (zoomEnd - 5 > zoomStart) {
            // 放大
            let start = zoomStart - containerRect.left;
            start = start < 0 ? 0 : start;
            let end = zoomEnd - containerRect.left;
            end = end > containerRect.width ? containerRect.width : end;
            const args = { start, end, width: containerRect.width };
            onZoomChange(args);
          }
          if (zoomEnd + 5 < zoomStart) {
            // 缩小（重置放大）
            const args = {
              start: -1,
            };
            onZoomChange(args);
          }
        }
      }}
      onMouseLeave={(e) => {
        setMouseDown(false);
      }}
      onDoubleClick={(e) => {
        onAddMarker();
      }}
    >
      {showCursor && !isMobile() && cursorInfo && (
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
              {cursorInfo.dataInfo && cursorInfo.tickValue && (
                <div>刻度：{cursorInfo.tickValue}</div>
              )}
            </div>
          </div>
        </>
      )}

      {mouseDown && zoomEnd !== zoomStart && allowZoom && (
        <div
          className={zoomEnd > zoomStart ? styles.zoomIn110 : styles.zoomOut110}
          style={{
            left: `${
              zoomEnd > zoomStart
                ? zoomStart - containerRect.left
                : zoomEnd - containerRect.left
            }px`,
            width: `${Math.abs(zoomEnd - zoomStart)}px`,
          }}
        />
      )}
    </div>
  );
};

ZoomAndCursor.defaultProps = {
  style: "",
  onZoomChange: () => {},
  onAddMarker: () => {},
  onMouseMove: () => {},
  showCursor: true,
  cursorInfo: undefined,
  allowZoom: true,
};

ZoomAndCursor.propTypes = {
  style: PropTypes.string,
  onZoomChange: PropTypes.func,
  onAddMarker: PropTypes.func,
  onMouseMove: PropTypes.func,
  showCursor: PropTypes.bool,
  cursorInfo: PropTypes.any,
  allowZoom: PropTypes.bool,
};

export default ZoomAndCursor;
