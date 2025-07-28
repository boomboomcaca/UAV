import React from 'react';
import IconSwitch from '../index';
import { A, B, C, D } from '../SVGComponent/index.jsx';

export default function Demo() {
  const onChange = (state) => {
    console.log(state);
  };

  return (
    <>
      <IconSwitch onChange={onChange} selected icons={[<A />, <B />, <C />, <D />]} />
      <IconSwitch onChange={onChange} selected disabled icons={[<A />, <B />, <C />, <D />]} />
    </>
  );
}
