/*
 * @Author: wangXueDong
 * @Date: 2022-02-17 16:14:39
 * @LastEditors: wangXueDong
 * @LastEditTime: 2022-11-05 17:16:41
 */
import React, { useState } from 'react';
import PathColumnSetAxis from '../index';

const PathColumnSetAxisTest = () => {
  const [value, setValue] = useState(null);
  const [datas, setDatas] = useState([
    { label: '强干扰测试', id: 1, disable: false },
    { label: '强干扰测试', id: 2, select: true, disable: false },
    { label: '强干扰测试', id: 3, disable: false },
    { label: '强干扰测试', id: 4, select: true, disable: false },
    { label: '弱干扰测试', id: 5, disable: true },
    { label: '强干扰测试', id: 6, disable: true },
    { label: '强干扰测试', id: 7, disable: true },
  ]);
  const onChange = (e) => {
    setValue(e?.id);
  };
  const onValuesChange = (e, type) => {
    const arr = [...datas];
    const index = arr.findIndex((ele) => e.id === ele.id);
    arr[index].select = type === 'add';
    setDatas(arr);
  };
  return (
    <div
      style={{
        width: '85%',
        margin: '0 auto',
        height: '100%',
      }}
    >
      <PathColumnSetAxis
        datas={datas}
        value={value}
        onValuesChange={onValuesChange}
        onValueChange={(e) => onChange(e)}
      />
    </div>
  );
};

export default PathColumnSetAxisTest;
