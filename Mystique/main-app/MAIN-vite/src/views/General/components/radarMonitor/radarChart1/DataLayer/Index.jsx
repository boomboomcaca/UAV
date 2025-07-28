import React, { useRef, useEffect, useState } from "react";
import PropTypes from "prop-types";

import { mainConf } from "../../../../../../config";
import styles from "./style.module.less";

const DataLayer = (props) => {
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

  useEffect(() => {
    let mocker;
    if (mainConf) {
      mocker = setInterval(() => {
        setDataPoints([
          {
            x: (Math.random() * 100) / 1.414,
            y: (Math.random() * 100) / 1.414,
          },
          {
            x: (Math.random() * 100) / 1.414,
            y: (Math.random() * 100) / 1.414,
          },
        ]);
      }, 1000);
    }
    return () => {
      if (mocker) clearInterval(mocker);
    };
  }, []);

  return (
    <div className={styles.renderRoot}>
      <div className={styles.dataPoint}>
        {dataPoints.map((item) => (
          <div
            key={String(item.x)}
            className={styles.point}
            style={{ left: `${item.x}%`, top: `${item.y}%` }}
          />
        ))}
      </div>
    </div>
  );
};

DataLayer.defaultProps = {
  onReady: () => {},
};

DataLayer.propTypes = {
  onReady: PropTypes.func,
};

export default DataLayer;
