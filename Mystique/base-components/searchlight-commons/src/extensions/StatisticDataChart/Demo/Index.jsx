import React, { useState, useEffect, useRef } from 'react';
import { Button } from 'dui';
import StatisticDataChart from '../StatisticDataChart.jsx';
import data from '../data';
import styles from './index.module.less';

export default () => {
  const [point, setPoint] = useState({});
  const chartRef = useRef(null);
  useEffect(() => {
    let idx = 0;
    const d = data.data;
    setInterval(() => {
      const date = new Date();
      let times = date.getTime();
      if (idx < d.length - 1) {
        chartRef.current.onDraw && chartRef.current.onDraw({ time: (times += 5000), level: d[idx] });
        setPoint({ time: (times += 5000), level: d[idx] });
        idx += 1;
      }
    }, 50);
  }, []);
  const reset = () => {
    chartRef.current.reset && chartRef.current.reset();
  };
  return (
    <div className={styles.content}>
      <Button onClick={reset}>重置</Button>
      <StatisticDataChart point={point} ref={chartRef} />
    </div>
  );
};
