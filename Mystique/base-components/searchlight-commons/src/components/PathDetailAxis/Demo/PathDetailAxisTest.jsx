/*
 * @Author: wangXueDong
 * @Date: 2022-02-17 16:14:39
 * @LastEditors: wangXueDong
 * @LastEditTime: 2022-10-29 11:40:55
 */
import React, { useState } from 'react';
import PathDetailAxis from '../index';

const PathDetailAxisTest = () => {
  const [value, setValue] = useState(null);
  const [datas] = useState([
    { label: '150~153', id: 1, state: 2 },
    { label: '153~322', id: 2, state: 2 },
    { label: '322~329', id: 3, state: 2 },
    { label: '329~406', id: 4, state: 2 },
    { label: '406~410', id: 5, state: 1 },
    { label: '410~608', id: 6, state: 0 },
    { label: '608~614', id: 8, state: 0 },
    { label: '608~614', id: 9, state: 0 },
    { label: '608~614', id: 10, state: -1 },
    // { label: '608~614', id: 11, state: -1 },
    // { label: '608~614', id: 12, state: -1 },
    // { label: '608~614', id: 13, state: -1 },
    // { label: '608~614', id: 14, state: -1 },
    // { label: '608~614', id: 15, state: -1 },
  ]);
  const onChange = (e) => {
    setValue(e.id);
  };
  return (
    <div
      style={{
        width: '80%',
        margin: '0 auto',
        paddingTop: '40%',
      }}
    >
      <PathDetailAxis datas={datas} value={value} onValueChange={(e) => onChange(e)} />
    </div>
  );
};

export default PathDetailAxisTest;
