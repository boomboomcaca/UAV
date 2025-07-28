import React, { useEffect, useState, useRef } from "react";
import PropTypes from "prop-types";

import styles from "./style.module.less";

/**
 *
 * @param {{signalList:Array<{id:String,left:Number,width:Number}>}} props
 * @returns
 */
const SignalBands = (props) => {
  const { signalList, height, onChange, onDrag } = props;

  const [bandPosition, setBandPosition] = useState({});

  // const dragLeftRef = useRef();
  // const dragRightRef = useRef();
  // const dragMoveRef = useRef();
  const dragStartRef = useRef();
  const [dragLeft, setDragLeft] = useState();
  const [dragRight, setDragRight] = useState();
  const [dragMove, setDragMove] = useState();
  const [select, setSelect] = useState();

  /**
   * @type {{current:HTMLElement}}
   */
  const domRef = useRef();

  useEffect(() => {
    if (domRef.current && select) {
      const rect = domRef.current.getBoundingClientRect();
      setBandPosition({
        left: (rect.width * select.left) / 100,
        width: (rect.width * select.width) / 100,
      });
    }
  }, [select]);

  return (
    <div
      ref={domRef}
      className={styles.signalBandRoot}
      style={{
        height: height,
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
        if (!select) return;
        const rect = domRef.current.getBoundingClientRect();
        onDrag({
          id: select.id,
          left: bandPosition.left / rect.width,
          width: bandPosition.width / rect.width,
        });
      }}
    >
      {select ? (
        <div
          className={`${styles.signalItem} ${styles.sel}`}
          style={{
            left: `${bandPosition.left}px`,
            width: `${bandPosition.width}px`,
          }}
        >
          <div
            className={styles.head}
            onClick={() => {
              console.log(select.id);
              setSelect(null);
              onChange(null);
            }}
          />

          <div
            className={styles.bandCon}
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
      ) : (
        signalList.map((item) => {
          if (item.left >= 0) {
            return (
              <div
                className={`${styles.signalItem} ${
                  select === item.id && styles.sel
                }`}
                style={{ left: `${item.left}%`, width: `${item.width}%` }}
              >
                <div
                  className={styles.head}
                  onClick={() => {
                    setSelect(item);
                    onChange(item.id);
                  }}
                />
              </div>
            );
          }
          return null;
        })
      )}
    </div>
  );
};

SignalBands.defaultProps = {
  signalList: [],
  // select: "",
  height: "",
  onChange: () => {},
  onDrag: () => {},
};

SignalBands.propTypes = {
  signalList: PropTypes.array,
  // select: PropTypes.any,
  height: PropTypes.string,
  onChange: PropTypes.func,
  onDrag: PropTypes.func,
};

export default SignalBands;
