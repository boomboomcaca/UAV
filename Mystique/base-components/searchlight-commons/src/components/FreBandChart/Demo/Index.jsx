import React, { useEffect, useState, useRef } from 'react';
import { Button } from 'dui';
import FreBandChart from '../FreBandChart.jsx';
import styles from './index.module.less';

export default () => {
  const [initData, setInitData] = useState([
    {
      startFreq: 27.5,
      stopFreq: 39.5,
      signals: [],
    },
    {
      startFreq: 156,
      stopFreq: 164,
      signals: [],
    },
  ]);
  const chartRef = useRef();
  let chartTimer = null;
  useEffect(() => {
    return () => {
      if (chartTimer) {
        clearInterval(chartTimer);
        chartTimer = null;
      }
    };
  }, []);
  const addChartData = () => {
    // 添加数据
    if (chartTimer) {
      clearInterval(chartTimer);
      chartTimer = null;
    }
    chartTimer = setInterval(() => {
      const randomIdx = Math.round(Math.random() * 1);
      const randomFre =
        initData[randomIdx].startFreq + Math.random() * (initData[randomIdx].stopFreq - initData[randomIdx].startFreq);
      const randomOcc = Math.random() * 100;
      const randomType = 1 + Math.floor(Math.random() * 3);
      const randomElec = 20 + Math.round(Math.random() * 50);
      chartRef.current.updateChart &&
        chartRef.current.updateChart({
          frequency: randomFre,
          bandwidth: 50,
          occupancy: randomOcc,
          type: randomType,
          elecLevel: randomElec,
        });
    }, 100);
  };
  const addArrChartData = () => {
    // 添加数据
    if (chartTimer) {
      clearInterval(chartTimer);
      chartTimer = null;
    }
    chartTimer = setInterval(() => {
      const arrData = [];
      const len = Math.round(Math.random() * 30) + 20;
      for (let i = 0; i < len; i += 1) {
        const randomIdx = Math.round(Math.random() * 1);
        const randomFre =
          initData[randomIdx].startFreq +
          Math.random() * (initData[randomIdx].stopFreq - initData[randomIdx].startFreq);
        const randomOcc = Math.random() * 100;
        const randomType = 1 + Math.floor(Math.random() * 3);
        const randomElec = 20 + Math.round(Math.random() * 50);
        arrData.push({
          frequency: randomFre,
          bandwidth: 50,
          occupancy: randomOcc,
          type: randomType,
          elecLevel: randomElec,
          segIndex: randomFre > initData[0].stopFreq ? 1 : 0,
        });
      }
      chartRef.current.updateChart && chartRef.current.updateArrChart(arrData);
    }, 200);
  };
  const stopChartData = () => {
    if (chartTimer) {
      clearInterval(chartTimer);
      chartTimer = null;
    }
  };
  const resetChartData = () => {
    chartRef.current.reset && chartRef.current.reset();
  };
  const recall = (e) => {
    console.log('recall--->e', e);
  };
  return (
    <div className={styles.container}>
      <div>
        <Button onClick={addChartData}>添加数据</Button>
        <Button onClick={addArrChartData}>添加数组数据</Button>
        <Button onClick={stopChartData}>停止</Button>
        <Button onClick={resetChartData}>清除</Button>
      </div>
      <FreBandChart ref={chartRef} initData={initData} recall={recall} fixedLen={1}>
        <span>btn1</span>
        <span>btn2</span>
      </FreBandChart>
    </div>
  );
};
