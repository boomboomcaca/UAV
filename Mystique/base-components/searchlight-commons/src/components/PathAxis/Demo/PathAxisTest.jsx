/*
 * @Author: wangXueDong
 * @Date: 2022-02-17 16:14:39
 * @LastEditors: wangXueDong
 * @LastEditTime: 2022-11-18 15:46:24
 */
import React, { useState } from 'react';
import PathAxis from '../index';

const PathAxisTest = () => {
  const [value, setValue] = useState(null);
  const [hoverData, setHoverData] = useState({});
  const [datas] = useState([
    { label: '150~153', id: 1, state: 2 },
    { label: '153~322', id: 2, state: 2 },
    { label: '150~153', id: 3, state: 2 },
    { label: '153~322', id: 4, state: 2 },
    { label: '150~153', id: 5, state: 1 },
    { label: '153~322', id: 6, state: -1 },
    { label: '150~153', id: 7, state: 0 },
    { label: '153~322', id: 8, state: 0 },
  ]);
  const onChange = (e) => {
    setValue(e.id);
  };
  const onMouseEnter = (e) => {
    setHoverData(e);
  };
  const content = <div style={{ height: '326px', width: '480px' }}>{hoverData.label}</div>;
  return (
    <div
      style={{
        width: '85%',
        margin: '0 auto',
        paddingTop: '40%',
      }}
    >
      <PathAxis
        datas={datas}
        onMouseEnter={onMouseEnter}
        value={value}
        onValueChange={(e) => onChange(e)}
        content={content}
        mode="bottom"
      />
    </div>
  );
};

export default PathAxisTest;
