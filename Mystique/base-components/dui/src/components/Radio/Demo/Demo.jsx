import React, { useState } from 'react';
import Radio from '../index';

export default function Demo() {
  const [province, setProvince] = useState(0);
  const [radioIndex, setRadioIndex] = useState(0);

  const options = [
    {
      label: '四川',
      value: 0,
    },
    {
      label: '云南',
      value: 1,
      disabled: true,
    },
    {
      label: '贵州',
      value: 2,
    },
  ];
  const aOptions = [
    {
      label: '11111111',
      value: 0,
    },
    {
      label: '222222222222',
      value: 1,
      disabled: true,
    },
    {
      label: '333333333333',
      value: 2,
    },
  ];
  const bOptions = [
    {
      label: 'dBμV',
      value: 0,
    },
    {
      label: 'dBμV/m',
      value: 1,
      disabled: true,
    },
    {
      label: 'dBm',
      value: 2,
    },
  ];
  return (
    <div>
      <Radio options={['常规模式', '低噪模式', '低失真模式']} />
      <br />
      <br />
      <br />
      <Radio theme="highLight" options={aOptions} value={radioIndex} onChange={(value) => setRadioIndex(value)} />
      <br />
      <br />
      <br />
      <Radio options={options} value={province} onChange={(value) => setProvince(value)} />
      <br />
      <br />
      <br />
      <Radio theme="line" options={bOptions} value={province} onChange={(value) => setProvince(value)} />
    </div>
  );
}
