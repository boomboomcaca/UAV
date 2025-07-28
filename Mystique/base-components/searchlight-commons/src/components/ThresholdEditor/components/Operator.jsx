import React, { useState, memo, useEffect, useRef } from 'react';
import PropTypes from 'prop-types';
// import { Button } from 'dui';
// import { DrawIcon, MoveIcon, ZoomInIcon, ZoomOutIcon } from 'dc-icon';
// import langT from 'dc-intl';
// import AxisY from './AxisY.jsx';
// import Editor from './editor';
// import NewSegmentsEditor from '../NewSegmentsEditor/NewSegmentsEditor.jsx';
import styles from './index.module.less';

const Operator = (props) => {
  const { onZoomChange } = props;
  const operationRef = useRef();
  const [containerRect, setContainerRect] = useState({ left: 0 });
  const [zoomStart, setZoomStart] = useState(-1);
  const [zoomEnd, setZoomEnd] = useState(-1);
  const [mouseDown, setMouseDown] = useState(false);

  useEffect(() => {
    if (zoomStart >= 0 && operationRef.current) {
      const rect = operationRef.current.getBoundingClientRect();
      setContainerRect({ left: rect.left, width: rect.width });
    }
  }, [zoomStart, operationRef.current]);

  return (
    <>
      <div
        ref={operationRef}
        style={{ position: 'absolute', top: 0, left: 0, width: '100%', height: '100%' }}
        onMouseDown={(e) => {
          setZoomStart(e.clientX);
          setZoomEnd(e.clientX);
          setMouseDown(true);
        }}
        onMouseMove={(e) => {
          setZoomEnd(e.clientX);
        }}
        onMouseUp={(e) => {
          setMouseDown(false);
          // 触发事件
          if (onZoomChange) {
            let start = zoomStart - containerRect.left;
            start = start < 0 ? 0 : start;
            let end = zoomEnd - containerRect.left;
            end = end > containerRect.width ? containerRect.width : end;
            const args = { start, end, ...containerRect };
            onZoomChange(args);
          }
        }}
        onMouseLeave={(e) => {
          setMouseDown(false);
        }}
      >
        {mouseDown && zoomEnd !== zoomStart && (
          <div
            className={zoomEnd > zoomStart ? styles.zoomIn : styles.zoomOut}
            style={{
              left: `${zoomEnd > zoomStart ? zoomStart - containerRect.left : zoomEnd - containerRect.left}px`,
              width: `${Math.abs(zoomEnd - zoomStart)}px`,
            }}
          />
        )}
      </div>
    </>
  );
};

Operator.defaultProps = {};

Operator.propTypes = {
  onZoomChange: PropTypes.func.isRequired,
};

export default memo(Operator);
