import React, { useState, useRef, useEffect } from "react";
import PropTypes from "prop-types";
import { isMobile, timestamp2String } from "../../../utils/utils.js";
import { ArrowIcon, DragIcon } from "../../../assets/svgIcons.jsx";
import styles from "./style.module.less";

const Marker = (props) => {
  const {
    moveable,
    marker,
    releaseDrag,
    onReadyDrag,
    onStopDrag,
    onSelect,
    onMove,
  } = props;
  const [showLine, setShowLine] = useState(false);
  const [focus, setFocus] = useState(false);

  useEffect(() => {
    // console.log(marker);
    if (marker) {
      setShowLine(marker.selected);
    }
  }, [marker]);

  useEffect(() => {
    if (releaseDrag) {
      onStopDrag({ id: marker.id, dragX: true });
      setFocus(false);
    }
  }, [releaseDrag, marker]);

  return (
    <>
      {marker && (
        <>
          <div
            className={styles.lineX}
            key={marker?.id || "mkddd"}
            style={{
              left: `calc(${marker?.position.x}% - 3px)`,
              pointerEvents: moveable ? "all" : "none",
            }}
            onMouseDown={() => onReadyDrag({ id: marker.id, dragX: true })}
            onMouseEnter={() => setShowLine(true)}
            onMouseLeave={() => {
              if (!marker.selected) setShowLine(false);
            }}
          >
            <div style={{ visibility: showLine ? "visible" : "collapse" }} />
          </div>
          <div
            className={styles.lineY}
            style={{
              top: `calc(${marker?.position.y}% - 2px)`,
              pointerEvents: moveable ? "all" : "none",
            }}
            onMouseDown={() => onReadyDrag({ id: marker.id, dragY: true })}
            onMouseEnter={() => setShowLine(true)}
            onMouseLeave={() => setShowLine(false)}
          >
            <div style={{ visibility: showLine ? "visible" : "collapse" }} />
          </div>
        </>
      )}
      {isMobile() && marker.selected && (
        <>
          <div className={styles.sliderYCon}>
            <div
              className={`${styles.sliderY} ${focus && styles.focus}`}
              style={{ top: `calc(${marker?.sliderPosition?.y}% - 62px)` }}
            >
              <div
                className={styles.sliderYUp}
                onClick={() => onMove({ action: "up", id: marker.id })}
              >
                <ArrowIcon />
              </div>
              <div
                className={styles.sliderYTouch}
                onTouchStart={() => {
                  onReadyDrag({ id: marker.id, dragY: true });
                  setFocus(true);
                }}
              >
                <DragIcon />
              </div>
              <div
                className={styles.sliderYDown}
                onClick={() => onMove({ action: "down", id: marker.id })}
              >
                <ArrowIcon rotate={180} />
              </div>
            </div>
          </div>
          <div
            className={styles.sliderXCon}
            style={{ pointerEvents: focus ? "all" : "none" }}
          >
            <div
              className={`${styles.sliderX} ${focus && styles.focus}`}
              style={{ left: `calc(${marker?.sliderPosition?.x}% - 62px)` }}
            >
              <div
                className={styles.sliderXLeft}
                onClick={() => onMove({ action: "left", id: marker.id })}
              >
                <ArrowIcon rotate={270} />
              </div>
              <div
                className={styles.sliderXTouch}
                style={{ flex: 3 }}
                onTouchStart={() => {
                  setFocus(true);
                  onReadyDrag({ id: marker.id, dragX: true });
                }}
                onClick={() => {
                  // console.log("on drag marker onClick::");
                }}
              >
                <DragIcon />
              </div>
              <div
                className={styles.sliderXRight}
                onClick={() => onMove({ action: "right", id: marker.id })}
              >
                <ArrowIcon rotate={90} />
              </div>
            </div>
          </div>
        </>
      )}
      <div
        className={`${styles.markerRoot}`}
        style={{
          left: `calc(${marker?.position.x}% - 11px)`,
          top: `calc(${marker?.position.y}% - 22px)`,
        }}
      >
        {/* <div className={styles.markerTip}>M{marker?.markerIndex + 1}</div> */}
        <div
          className={`${styles.symbolContainer} ${
            marker?.selected && styles.sel
          }`}
          style={{ pointerEvents: moveable ? "all" : "none" }}
          onMouseEnter={() => setShowLine(true)}
          onMouseLeave={() => setShowLine(false)}
          onClick={() => onSelect({ id: marker.id, select: !marker.selected })}
          onMouseDown={() => {
            onReadyDrag({ id: marker.id, dragX: true, dragY: true });
          }}
        >
          <div className={styles.symbol}>
            {marker?.text || marker?.markerIndex + 1}
          </div>
        </div>
      </div>
      {marker?.selected && isMobile() && (
        <div className={styles.markerCaption}>
          <span>MK{marker?.text || marker?.markerIndex + 1}</span>
          <div className={styles.markerContent}>
            <div className={styles.freq0}>
              {marker?.freqInfo?.no} <span>{marker?.freqInfo?.unit}</span>
            </div>
            <div>{timestamp2String(marker?.dataInfo?.timestamp)}</div>
            <div>
              {marker?.dataInfo?.levelCaption}
              <span>{marker?.dataInfo?.unit}</span>
            </div>
          </div>
        </div>
      )}
    </>
  );
};

Marker.defaultProps = {
  marker: {},
  onReadyDrag: () => {},
  onDragging: () => {},
  onStopDrag: () => {},
  setReleaseDrag: undefined,
  onSelect: () => {},
  onMove: () => {},
  moveable: true,
};

Marker.propTypes = {
  marker: PropTypes.any,
  onReadyDrag: PropTypes.func,
  onDragging: PropTypes.func,
  onStopDrag: PropTypes.func,
  setReleaseDrag: PropTypes.any,
  onSelect: PropTypes.any,
  onMove: PropTypes.func,
  moveable: PropTypes.bool,
};

export default Marker;
