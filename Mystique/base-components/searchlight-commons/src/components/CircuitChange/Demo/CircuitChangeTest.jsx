/*
 * @Author: wangXueDong
 * @Date: 2022-02-17 16:14:39
 * @LastEditors: wangXueDong
 * @LastEditTime: 2022-11-01 11:36:16
 */
import { IconButton } from 'dui';
import React, { useState } from 'react';
import CircuitChange from '../index';

const CircuitChangeTest = () => {
  const [value, setValue] = useState(0);
  const [segment, setSegment] = useState('614~1000');
  return (
    <div
      style={{
        width: '100%',
        height: '374px',
      }}
    >
      <CircuitChange segment={segment} value={value} />
      <IconButton
        onClick={() => {
          setValue(value >= 6 ? 0 : value + 1);
        }}
      >
        切换图片
      </IconButton>
    </div>
  );
};

export default CircuitChangeTest;
