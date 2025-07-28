import React, { useState, useRef } from "react";
import PropTypes from "prop-types";

import styles from "./style.module.less";
import { useEffect } from "react";

const Band = (props) => {
  const { segments, onChange } = props;

  const [bandPosition, setBandPosition] = useState({
    left: 0,
    width: 20,
  });
  //   const dragLeftRef = useRef();
  //   const dragRightRef = useRef();
  //   const dragMoveRef = useRef();
  const dragStartRef = useRef();
  const [dragLeft, setDragLeft] = useState();
  const [dragRight, setDragRight] = useState();
  const [dragMove, setDragMove] = useState();
  /**
   * @type {{current:HTMLElement}}
   */
  const domRef = useRef();

  useEffect(() => {
    if (domRef.current && onChange) {
      const rect = domRef.current.getBoundingClientRect();
      onChange({
        left: bandPosition.left / rect.width,
        width: bandPosition.width / rect.width,
      });
    }
  }, []);

  return (
    <div
      ref={domRef}
      className={styles.bandRoot}
      style={{
        pointerEvents: dragLeft || dragRight || dragMove ? "all" : "none",
      }}
      onMouseMove={(e) => {
        if (dragLeft) {
          const offset = e.clientX - dragLeft.clientX;
          const left = dragStartRef.current.left + offset;
          let width = dragStartRef.current.width - offset;
          if (width < 10) width = 10;

          setBandPosition({
            left,
            width,
          });
        }
        if (dragRight) {
          const offset = e.clientX - dragRight.clientX;
          let width = dragStartRef.current.width + offset;
          if (width < 10) width = 10;
          setBandPosition({
            left: dragStartRef.current.left,
            width,
          });
        }
        if (dragMove) {
          const offset = e.clientX - dragMove.clientX;
          const left = dragStartRef.current.left + offset;
          setBandPosition({
            left,
            width: dragStartRef.current.width,
          });
        }
      }}
      onMouseUp={(e) => {
        dragStartRef.current = undefined;
        setDragLeft(undefined);
        setDragRight(undefined);
        setDragMove(undefined);
        if (onChange) {
          const rect = domRef.current.getBoundingClientRect();
          onChange({
            left: bandPosition.left / rect.width,
            width: bandPosition.width / rect.width,
          });
        }
      }}
    >
      <div
        className={styles.bandCon}
        style={{
          left: `${bandPosition.left}px`,
          width: `${bandPosition.width}px`,
          backgroundColor:
            dragLeft || dragRight || dragMove
              ? "rgba(255, 255, 255, 0.2)"
              : "rgba(255, 255, 255, 0.1)",
        }}
        onMouseDown={(e) => {
          if (!dragStartRef.current) {
            setDragMove(e);
            dragStartRef.current = {
              left: bandPosition.left,
              width: bandPosition.width,
            };
          }
        }}
      >
        <div
          className={styles.splitLeft}
          onMouseDown={(e) => {
            setDragLeft(e);
            dragStartRef.current = {
              left: bandPosition.left,
              width: bandPosition.width,
            };
          }}
        />
        <div className={styles.center} />
        <div
          className={styles.splitRight}
          onMouseDown={(e) => {
            setDragRight(e);
            dragStartRef.current = {
              left: bandPosition.left,
              width: bandPosition.width,
            };
          }}
        />
      </div>
    </div>
  );
};

Band.defaultProps = {
  segments: undefined,
  onChange: () => {},
};

Band.propTypes = {
  segments: PropTypes.array,
  onChange: PropTypes.func,
};

export default Band;
