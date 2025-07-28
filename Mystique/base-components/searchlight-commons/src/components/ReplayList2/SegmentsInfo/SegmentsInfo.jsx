import React, { useCallback, useState, useRef } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { getArea, pointOutArea } from '../utils/area';
import triangle from '../Image/triangle.png';
import styles from './index.module.less';

const SegmentsInfo = (props) => {
  const { className, segments } = props;

  const SelectRef = useRef(null);
  const refDropDiv = useRef(null);
  const [dropStyle, setDropStyle] = useState(null);

  const [droped, setDroped] = useState(false);

  const getSimple = useCallback((segs, idx) => {
    if (segs && segs?.length > 0 && idx < segs?.length) {
      const { startFrequency, stopFrequency, stepFrequency } = segs[idx];
      return `${startFrequency}MHz-${stopFrequency}MHz@${stepFrequency}kHz`;
    }
    return '--';
  }, []);

  const onMouseUp = (e) => {
    const div1 = SelectRef.current;
    const div2 = refDropDiv.current;
    const point = { x: e.clientX, y: e.clientY };
    const area1 = getArea(div1);
    const area2 = getArea(div2);
    if (pointOutArea(point, area1) && pointOutArea(point, area2)) {
      setDroped(false);
      window.removeEventListener('mouseup', onMouseUp);
    }
  };

  return (
    <div className={classnames(styles.root, className)} ref={SelectRef}>
      {segments ? getSimple(segments, 0) : '--'}
      {segments && segments.length > 0 ? (
        <div
          className={styles.thumb}
          onClick={() => {
            setDroped(true);
            window.addEventListener('mouseup', onMouseUp);
            // if (SelectRef) {
            //   const area1 = getArea(SelectRef.current);
            //   const { point1, point2 } = area1;
            //   const left = point1.x;
            //   let top = point2.y;
            //   const maxh = 400;
            //   let h = maxh;
            //   const length = segments?.flat(Infinity).length;
            //   if (40 * length < maxh) {
            //     h = 40 * length;
            //   }
            //   const bodyRect = document.body.getBoundingClientRect();
            //   if (top + h >= bodyRect.bottom - bodyRect.top) {
            //     top = point1.y - h;
            //   }
            //   setDropStyle({ left, top });
            // }
          }}
        >
          <div className={styles.content} />
          <div className={classnames(styles.drop, droped ? null : styles.hide)} style={dropStyle} ref={refDropDiv}>
            <img alt="" className={styles.img} src={triangle} />
            <div className={styles.info}>
              <div className={styles.scroll}>
                {segments?.map((s, idx) => {
                  return (
                    <div className={styles.item} key={s.id}>
                      <div className={styles.type}>{s.type || '--'}</div>
                      {getSimple(segments, idx)}
                    </div>
                  );
                })}
              </div>
            </div>
          </div>
        </div>
      ) : null}
    </div>
  );
};

SegmentsInfo.defaultProps = {
  className: null,
  segments: null,
};

SegmentsInfo.propTypes = {
  className: PropTypes.any,
  segments: PropTypes.any,
};

export default SegmentsInfo;
