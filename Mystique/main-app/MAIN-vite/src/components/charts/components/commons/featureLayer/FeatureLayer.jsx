import React, { useEffect, useRef, useState, useContext } from "react";
import PropTypes from "prop-types";
import { useSize } from "ahooks";

import dayjs from "dayjs";
import { MouseMode, ChartTypes } from "../../utils/enums.js";
import Marker from "../markers/marker/Marker.jsx";
import ChartContext, { actions } from "../../context/chartContext.jsx";
import styles from "./style.module.less";

/**
 * 交互层
 * 1. 游标
 * 2. marker操作
 * 3. 缩放
 * @param {{style:any,onMouseMove:{x:Number,y:Number,dragMarker:String}}} props
 * @returns
 */
const FeatureLayer = (props) => {
  const {
    style,
    onZoomChange,
    onAddMarker,
    onMouseMove,
    onThrChange,
    showThreshold,
    visibleCharts,
  } = props;

  const ctx = useContext(ChartContext);
  const [state = "", dispatch = null] = ctx;
  const { markers, prevData, showCursor, cursorInfo } = state;

  const draggingMarkerRef = useRef();

  /**
   * @type {{current:HTMLElement}}
   */
  const domRef = useRef();
  const domSize = useSize(domRef);

  const [containerRect, setContainerRect] = useState({ left: 0, width: 0 });
  const [zoomStart, setZoomStart] = useState(-1);
  const [zoomEnd, setZoomEnd] = useState(-1);
  const [mouseDown, setMouseDown] = useState(false);
  const [mouseInCenterMK, setMouseInCenterMK] = useState("");
  const mouseMoveModeRef = useRef(MouseMode.none);
  const mouseOnThrRef = useRef(false);
  const [mousePctPosition, setMousePosition] = useState({ x: 0, y: 0 });
  const [thrPos, setThrPos] = useState(0);
  const [mouseOnThr, setMouseOnThr] = useState(false);

  useEffect(() => {
    // console.log('cursor effect:::', domSize);
  }, [domSize]);

  useEffect(() => {
    // console.log('cursor effect:::0000');
    // 检查鼠标是否在某个marker的中心线
    // if(markers)
  }, [markers]);

  useEffect(() => {
    if (domRef.current) {
      const rect = domRef.current.getBoundingClientRect();
      setContainerRect(rect);
    }
  }, [domRef.current]);

  useEffect(() => {
    // console.log('dddddddddd');
  });

  return (
    <div
      ref={domRef}
      className={styles.cursorRoot}
      style={{
        ...style,
        cursor: `${mouseInCenterMK ? "col-resize" : "default"}`,
      }}
      onMouseMove={(e) => {
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
          dragMarker: draggingMarkerRef.current,
        };
        args.width = rect.width;
        args.height = rect.height;
        setMousePosition({
          x: letPct * 100,
          y: topPct * 100,
        });
        onMouseMove(args);
        // console.log("mouse move:::", mouseMoveModeRef.current);
        if (mouseMoveModeRef.current === MouseMode.none) {
          // 查找鼠标在中心线上的marker
          let mouseInCenter = "";
          for (let i = 0; i < markers.length; i += 1) {
            const { position, id, inRain } = markers[i];
            if (!inRain) {
              const centerPx = (position.x * rect.width) / 100;
              if (px >= centerPx - 3 && px <= centerPx + 1) {
                mouseInCenter = id;
                break;
              }
            }
          }
          setMouseInCenterMK(mouseInCenter);
        } else if (mouseMoveModeRef.current === MouseMode.dragMarker) {
          // 通过helper更新位置
        } else if (mouseMoveModeRef.current === MouseMode.dragThreshold) {
          // TODO
          let top = topPct;
          const gap = 1.0 / visibleCharts.length;
          if (top > gap) top = gap;
          onThrChange({ value: top, end: false });
        } else if (mouseMoveModeRef.current === MouseMode.zoom) {
          // 更新缩放结束位置
          setZoomEnd(e.clientX);
        }
      }}
      onMouseDown={(e) => {
        // if (mouseMoveModeRef.current === MouseMode.dragThreshold) {
        // } else if (mouseMoveModeRef.current === MouseMode.dragMarker) {
        // } else if (mouseMoveModeRef.current === MouseMode.zoom) {
        // } else {
        //   // none
        // }
        if (mouseMoveModeRef.current === MouseMode.dragThreshold) {
        } else {
          const draggingMK = draggingMarkerRef.current || mouseInCenterMK;
          if (draggingMK) {
            mouseMoveModeRef.current = MouseMode.dragMarker;
            draggingMarkerRef.current = mouseInCenterMK;
          } else {
            setZoomStart(e.clientX);
            setZoomEnd(e.clientX);
            setMouseDown(true);
            mouseMoveModeRef.current = MouseMode.zoom;
          }
        }
      }}
      onMouseUp={(e) => {
        if (mouseMoveModeRef.current === MouseMode.zoom) {
          setMouseDown(false);
          // 触发事件
          if (onZoomChange) {
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
        } else if (mouseMoveModeRef.current === MouseMode.dragThreshold) {
          onThrChange({ value: state.thrPosition, end: true });
        }
        setMouseOnThr(false);
        mouseMoveModeRef.current = MouseMode.none;
        draggingMarkerRef.current = undefined;
      }}
      onMouseLeave={(e) => {
        setMouseDown(false);
      }}
      onDoubleClick={(e) => {
        if (state.allowAddMarker) {
          onAddMarker();
        }
      }}
    >
      {state.showCursor && (
        <>
          <div
            className={styles.cursorx}
            style={{ top: `${cursorInfo.cursorPosY}%` }}
            hidden={cursorInfo.cursorPosY < 0}
          />
          <div
            className={styles.cursory}
            style={{ left: `${cursorInfo.cursorPosX}%` }}
            hidden={cursorInfo.cursorPosX < 0}
          />

          <div
            className={styles.caption}
            style={{
              left:
                mousePctPosition.x <= 50 ? `${mousePctPosition.x}%` : undefined,
              right:
                mousePctPosition.x > 50
                  ? `${100 - mousePctPosition.x}%`
                  : undefined,
              top:
                mousePctPosition.y <= 50 ? `${mousePctPosition.y}%` : undefined,
              bottom:
                mousePctPosition.y > 50
                  ? `${100 - mousePctPosition.y}%`
                  : undefined,
            }}
          >
            <div>频率：{cursorInfo.frequency?.toFixed(6)}MHz</div>
            {cursorInfo.onChart === ChartTypes.spectrum ? (
              <div>
                电平：
                {cursorInfo.level > -9999
                  ? `${cursorInfo.level.toFixed(1)}dBμV`
                  : "无数据"}
              </div>
            ) : (
              <div>
                示向度：
                {cursorInfo.level > -1
                  ? `${cursorInfo.level.toFixed(1)}°`
                  : "无数据"}
              </div>
            )}
            {cursorInfo.timestamp > 0 && (
              <div>
                时间：
                {dayjs(new Date(cursorInfo.timestamp)).format("HH:mm:ss.SSS")}
              </div>
            )}
          </div>
        </>
      )}
      {showThreshold && visibleCharts.includes(ChartTypes.spectrum) && (
        <div
          className={styles.threshold}
          style={{
            cursor: mouseOnThr ? "row-resize" : "default",
            top: `${state.thrPosition * 100}%`,
          }}
          onMouseOver={() => {
            if (mouseMoveModeRef.current == MouseMode.none) {
              setMouseOnThr(true);
            }
          }}
          onMouseOut={() => {
            setMouseOnThr(false);
          }}
          onMouseDown={() => {
            if (mouseMoveModeRef.current == MouseMode.none)
              mouseMoveModeRef.current = MouseMode.dragThreshold;
          }}
          onMouseUp={() => {
            // mouseMoveModeRef.current = MouseMode.none;
          }}
        >
          <div />
        </div>
      )}
      {markers.map((item) => {
        return (
          <Marker
            marker={item}
            onDragging={(e) => {
              draggingMarkerRef.current = e.dragging ? e.id : "";
              mouseMoveModeRef.current = MouseMode.dragMarker;
            }}
          />
        );
      })}

      {mouseDown && zoomEnd !== zoomStart && (
        <div
          className={zoomEnd > zoomStart ? styles.zoomIn : styles.zoomOut}
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

FeatureLayer.defaultProps = {
  style: "",
  onZoomChange: () => {},
  onAddMarker: () => {},
  onMouseMove: () => {},
  onThrChange: () => {},
  showThreshold: false,
  visibleCharts: [],
};

FeatureLayer.propTypes = {
  style: PropTypes.string,
  onZoomChange: PropTypes.func,
  onAddMarker: PropTypes.func,
  onMouseMove: PropTypes.func,
  onThrChange: PropTypes.func,
  showThreshold: PropTypes.bool,
  visibleCharts: PropTypes.array,
};

export default FeatureLayer;
