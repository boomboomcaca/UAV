import React, { useCallback, useRef, useState } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { getArea, pointOutArea } from '../utils/area';
import triangle from '../Image/triangle.png';
import styles from './index.module.less';

const MScanPoints = (props) => {
  const { className, points } = props;

  const SelectRef = useRef(null);
  const refDropDiv = useRef(null);
  const [droped, setDroped] = useState(false);

  const getSimple = useCallback((ptns, idx) => {
    if (ptns && ptns?.length > 0 && idx < ptns?.length) {
      const { frequency, filterBandwidth, ifBandwidth, measureThreshold } = ptns[idx];
      return `${frequency}MHz@${ifBandwidth || filterBandwidth}kHz :${measureThreshold}`;
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
    <div ref={SelectRef} className={classnames(styles.root, className)}>
      {getSimple(points, 0)}
      {points && points.length > 0 ? (
        <div
          className={styles.thumb}
          onClick={() => {
            setDroped(true);
            window.addEventListener('mouseup', onMouseUp);
          }}
        >
          <div className={styles.content} />
          <div className={classnames(styles.drop, droped ? null : styles.hide)} ref={refDropDiv}>
            <img alt="" className={styles.img} src={triangle} />
            <div className={styles.info}>
              <div className={styles.scroll}>
                {points?.map((s, idx) => {
                  return (
                    <div className={styles.item} key={s.id}>
                      <div className={styles.type}>{s.demMode || '--'}</div>
                      {getSimple(points, idx)}
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

MScanPoints.defaultProps = {
  className: null,
  points: null,
};

MScanPoints.propTypes = {
  className: PropTypes.any,
  points: PropTypes.any,
};

export default MScanPoints;
