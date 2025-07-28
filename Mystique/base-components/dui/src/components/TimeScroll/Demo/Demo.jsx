import React, { useState } from 'react';
import TimeScroll from '../index';

export default function Demo() {
  const [value, setValue] = useState({
    h: 0,
    m: 0,
    h1: 0,
    m1: 0,
  });

  const onChange = (val) => {
    if (val.h * 60 + val.m > val.h1 * 60 + val.m1) {
    }
    setValue(val);
  };

  return (
    <div>
      <div>
        <span>{value.h}</span>
        <span>:</span>
        <span>{value.m}</span>
        <span>至</span>
        <span>{value.h1}</span>
        <span>:</span>
        <span>{value.m1}</span>
      </div>
      <TimeScroll valueList={value} onChange={onChange} />
      <TimeScroll valueList={value} onChange={onChange}  rangeSelection={false}/>
    </div>
  );
}
