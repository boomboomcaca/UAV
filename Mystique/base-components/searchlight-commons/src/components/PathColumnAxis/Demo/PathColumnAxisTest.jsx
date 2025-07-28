/*
 * @Author: wangXueDong
 * @Date: 2022-02-17 16:14:39
 * @LastEditors: wangXueDong
 * @LastEditTime: 2022-10-27 17:35:31
 */
import React, { useState } from 'react';
import PathColumnAxis from '../index';

const PathColumnAxisTest = () => {
  const [value, setValue] = useState(null);
  const [datas] = useState([
    { label: '强干扰', id: 1, state: 2 },
    { label: '强干扰', id: 2, state: 2 },
    { label: '强干扰', id: 3, state: 2 },
    { label: '强干扰', id: 4, state: 2 },
    { label: '弱干扰', id: 5, state: 1 },
    { label: '强干扰', id: 6, state: 0 },
    { label: '强干扰', id: 7, state: -1 },
  ]);
  const onChange = (e) => {
    setValue(e?.id);
  };
  return (
    <div
      style={{
        width: '85%',
        margin: '0 auto',
        height: '100%',
      }}
    >
      <PathColumnAxis datas={datas} value={value} onValueChange={(e) => onChange(e)} />
    </div>
  );
};

export default PathColumnAxisTest;
