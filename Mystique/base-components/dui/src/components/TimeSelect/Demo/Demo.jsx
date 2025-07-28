import React, { useState } from 'react';
import TimeSelect from '../index';

export default function Demo() {
  const [value, setValue] = useState({
    h: 0,
    m: 0,
    h1: 0,
    m1: 0,
  });

  const onChange = (val) => {
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
      <TimeSelect valueList={value} onChange={onChange} />
    </div>
  );
}
