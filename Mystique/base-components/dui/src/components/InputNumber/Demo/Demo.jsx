import React from 'react';
import InputNumber from '../index';

export default function Demo() {
  return (
    <div>
      <InputNumber
        min={20}
        max={1000}
        digits={4}
        step={0.0001}
        suffix="dBμV"
        onChange={(val) => window.console.log(val)}
      />
      <br />
      <br />
      <br />
      <InputNumber size="large" defaultValue="100" />
      <br />
      <br />
      <br />
      <InputNumber size="large" value="100" hideArrow allowClear disabled />
    </div>
  );
}
