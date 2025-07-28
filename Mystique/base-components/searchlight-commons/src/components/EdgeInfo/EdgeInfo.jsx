import React, { useRef, useState, useCallback } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { BaseStationIcon } from 'dc-icon';
import langT from 'dc-intl';
import { pointOutArea, getArea } from './graph';
import movable from './movable.jsx';
import halfCircle from './halfCircle.jsx';
import warning from './warning.jsx';
import styles from './index.module.less';

const EdgeInfo = (props) => {
  const { className, edgeInfo, avaliable, disable, children, onClick } = props;

  const rootRef = useRef(null);
  const popupRef = useRef(null);

  const [showPopup, setShowPopup] = useState(false);

  const getLatLng = useCallback((lat, lng) => {
    if (lat !== undefined && lng !== undefined)
      return `${lng.toFixed(1)}°${lng > 0 ? 'E' : 'W'} ${lat.toFixed(1)}°${lat > 0 ? 'N' : 'S'}`;
    return '--° --°';
  }, []);

  const onMouseMove = (e) => {
    const div1 = rootRef.current;
    const div2 = popupRef.current;
    const point = { x: e.clientX, y: e.clientY };
    const area1 = getArea(div1);
    const area2 = getArea(div2);
    const area = {
      point1: { x: Math.min(area1.point1.x, area2.point1.x), y: Math.min(area1.point1.y, area2.point1.y) },
      point2: { x: Math.max(area1.point2.x, area2.point2.x), y: Math.max(area1.point2.y, area2.point2.y) },
    };
    if (pointOutArea(point, area)) {
      setShowPopup(false);
      window.removeEventListener('mousemove', onMouseMove);
    }
  };

  return (
    <div
      className={classnames(styles.root, !edgeInfo || avaliable === false ? styles.none : null, className)}
      onClick={() => {
        if (!disable) onClick();
      }}
    >
      {edgeInfo && avaliable ? (
        <div className={styles.edgeInfo}>
          <div
            ref={rootRef}
            className={styles.icon}
            onMouseEnter={() => {
              if (!showPopup && children) {
                setShowPopup(true);
                window.addEventListener('mousemove', onMouseMove);
              }
            }}
          >
            <div className={styles.type} style={disable ? { opacity: 0.5 } : null}>
              {halfCircle}
            </div>
            <div className={styles.type2} style={disable ? { opacity: 0.5 } : null}>
              {edgeInfo?.category || 1}
            </div>
            {edgeInfo?.edgeType || edgeInfo?.type === 'stationaryCategory' ? (
              <BaseStationIcon color={disable ? '#3CE5D380' : '#3CE5D3'} />
            ) : (
              movable
            )}
            {children ? (
              <div
                className={classnames(styles.popup, showPopup ? styles.show : null)}
                onClick={(e) => {
                  e.stopPropagation();
                }}
              >
                <div ref={popupRef} className={styles.list}>
                  {children}
                </div>
                <div className={styles.popArrow}>
                  <div className={styles.triangle} />
                </div>
              </div>
            ) : null}
          </div>
          <div className={styles.info} style={disable ? { opacity: 0.5 } : null}>
            <div className={styles.name} title={edgeInfo?.edgeName || edgeInfo?.name}>
              <div>{(edgeInfo?.edgeName || edgeInfo?.name)?.substring(0, 10) || '--'}</div>
            </div>
            <div className={styles.latlng}>{getLatLng(edgeInfo?.latitude, edgeInfo?.longitude)}</div>
          </div>
        </div>
      ) : (
        <div className={styles.warning}>
          {warning}
          {/* TODO warning */}
          {langT('commons', 'noStations')}
        </div>
      )}
    </div>
  );
};

EdgeInfo.defaultProps = {
  className: null,
  edgeInfo: null,
  avaliable: true,
  disable: false,
  children: null,
  onClick: () => {},
};

EdgeInfo.propTypes = {
  className: PropTypes.any,
  edgeInfo: PropTypes.any,
  avaliable: PropTypes.bool,
  disable: PropTypes.bool,
  children: PropTypes.any,
  onClick: PropTypes.func,
};

export default EdgeInfo;
