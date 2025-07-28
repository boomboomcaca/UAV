/*
 * @Author: wangXueDong
 * @Date: 2022-02-17 16:14:39
 * @LastEditors: wangXueDong
 * @LastEditTime: 2022-10-22 17:03:55
 */
import React, { useState } from 'react';
import PathSettingAxis from '../index';

const PathSettingAxisTest = () => {
  const [value, setValue] = useState(null);
  const [value1, setValue1] = useState(null);
  const [datas, setDatas] = useState([
    { state: 'done', name: '强干扰测试' },
    { state: 'done', name: '强干扰测试' },
    { state: 'done', name: '弱干扰测试' },
  ]);
  const onIndexChange = (e) => {
    setValue(e.index);
    console.log(e);
  };
  const onIndexChange1 = (e) => {
    setValue1(e.index);
    console.log(e);
  };
  const onMove = (index) => {
    const arr = [...datas];
    arr.splice(index, 1);
    setDatas(arr);
  };
  return (
    <div
      style={{
        width: '85%',
        margin: '0 auto',
        paddingTop: '20%',
      }}
    >
      {/* <PathSettingAxis datas={[]} onMove={onMove} value={value} onValueChange={(e) => onIndexChange(e)} />
      <div style={{ height: '80px' }} /> */}
      <PathSettingAxis datas={datas} onMove={onMove} value={value} onValueChange={(e) => onIndexChange(e)} />
      <PathSettingAxis type="readOnly" datas={datas} value={value1} onValueChange={(e) => onIndexChange1(e)} />
    </div>
  );
};

export default PathSettingAxisTest;
