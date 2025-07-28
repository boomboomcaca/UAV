import React, { useRef, useEffect, useState } from 'react';

import PropTypes from 'prop-types';
import styles from './render.module.less';

const RenderLayer = (props) => {
  const { onReady } = props;

  const [dataPoints, setDataPoints] = useState([]);

  /**
   *
   * @param {{data:Array, centerGPS:{x:Number,y:Number}, radiusX:Number,radiusY:Number}} datas
   */
  const dataSetter = (datas) => {
    // 获取数据
    const { data, centerGPS, radiusX, radiusY } = datas;
    // 计算百分比
    const drawData = data.map((item) => {
      const { lng, lat } = item;

      return {
        x: (lng * 100) / (radiusX * 2),
        y: (lat * 100) / (radiusX * 2),
      };
    });
    setDataPoints(drawData);
  };

  useEffect(() => {
    if (onReady) {
      onReady(dataSetter);
    }
  }, [onReady]);

  return (
    <div className={styles.renderRoot}>
      <div className={styles.dataPoint}>
        {dataPoints.map((item) => (
          <div className={styles.point} style={{ left: `${item.x}%`, top: `${item.y}%` }} />
        ))}
      </div>
      <div className={styles.pointer} />
      <div className={styles.center} />
    </div>
  );
};

RenderLayer.defaultProps = {
  onReady: () => {},
};

RenderLayer.propTypes = {
  onReady: PropTypes.func,
};

export default RenderLayer;
