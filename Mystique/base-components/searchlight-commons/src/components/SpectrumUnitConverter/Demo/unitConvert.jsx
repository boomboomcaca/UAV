/* eslint-disable */
import React, { useState } from 'react';
import { Button } from 'dui';
import converters from '../';

const { spectrum2dBm } = converters;

const UnitCovnert = () => {
  const [testTip, setTestTip] = useState('');

  const test = () => {
    // 构造测试数据
    const dataCount = 1000000;
    const data = [];
    for (let i = 0; i < dataCount; i += 1) {
      data[i] = Math.random() * 65;
    }
    const startTime = new Date().getTime();
    const datas = spectrum2dBm(data);
    const endTime = new Date().getTime();
    const gap = endTime - startTime;
    setTestTip(`本次测试数据量：${dataCount}，运算耗时：${gap}ms`);
  };

  return (
    <div>
      <Button onClick={() => test()}>来一发</Button>
      <div style={{ color: 'gray', fontSize: '15px' }}>{testTip}</div>
    </div>
  );
};

export default UnitCovnert;
