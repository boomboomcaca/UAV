/*
 * @Author: XYQ
 * @Date: 2022-10-14 10:03:30
 * @LastEditors: XYQ
 * @LastEditTime: 2022-10-21 16:34:48
 * @Description: file content
 */
import React, { useState } from 'react';
import TimeScroll from '../index';

export default function Demo() {
  const [value, setValue] = useState({ startTime: '14:25:39', endTime: '23:25:39' });
  // { startTime: '14:25:39', endTime: '23:25:39' }
  const onChange = (val) => {
    setValue(val);
  };

  return (
    <div style={{ width: '50%' }}>
      <div>
        {value.startTime}
        <span>至</span>
        {value.endTime}
      </div>
      <TimeScroll options={value} onChange={onChange} />
    </div>
  );
}
