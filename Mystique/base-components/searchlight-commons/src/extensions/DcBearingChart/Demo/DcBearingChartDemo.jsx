import React, { useState, useRef, useEffect } from 'react';
import { Button, InputNumber } from 'dui';

import DcBearingChart from '../Chart.jsx';
import styles from './DcBearingChartDemo.module.less';

const DcBearingChartDemo = () => {
  const v1 = useRef(0);
  const [multiFlag, setMultiFlag] = useState(false);
  const [v2, setV2] = useState(0);
  const [northView, setNorthView] = useState(false);
  const [theme, setTheme] = useState('dark');
  const bearings = useRef([]);
  useEffect(() => {
    let timer = setInterval(() => {
      v1.current++;
      bearings.current = [v1.current + 45, v1.current + 125, v1.current + 205, v1.current + 300];
      setV2(Math.random() * 10);
    }, 1000);
    return () => {
      clearInterval(timer);
      timer = null;
    };
  }, []);

  const [ww, setWw] = useState(500);
  const [hh, setHh] = useState(500);
  return (
    <div>
      <div>
        <div>多指针参数 bearing(Array&lt;Number&gt;)</div>
        <Button
          onClick={() => {
            setTheme('dark');
          }}
        >
          DARK{' '}
        </Button>
        <Button
          onClick={() => {
            setTheme('light');
          }}
        >
          LIGHT{' '}
        </Button>

        <Button
          onClick={() => {
            setMultiFlag(!multiFlag);
          }}
        >
          Multiple Pointers
        </Button>
        <InputNumber
          value={ww}
          onChange={(val) => {
            setWw(val);
          }}
          placeholder="修改容器宽度"
        />
        <InputNumber
          value={hh}
          onChange={(val) => {
            setHh(val);
          }}
          placeholder="修改容器高度"
        />
      </div>
      <div style={{ width: `${ww}px`, height: `${hh}px`, marginTop: '60px' }}>
        <DcBearingChart
          northView={northView}
          show
          bearing={multiFlag ? bearings.current : v1.current}
          realtimeBearing={v2}
          displayBearing={bearings.current[0]}
          compass={5}
          realTimeBearingFlag
          theme={theme}
          showIcon
          onSwitch={() => {
            setNorthView(!northView);
          }}
        />
      </div>
    </div>
  );
};

DcBearingChartDemo.defaultProps = {};

DcBearingChartDemo.propTypes = {};

export default DcBearingChartDemo;
