import React, { useState } from 'react';
import Switch from '../index';

export default function Demo() {
  const [state, setstate] = useState(false);
  const onChange = (val) => {
    setstate(val);
  };

  return (
    <>
      <Switch selected={state} checkedChildren="开" unCheckedChildren="关" onChange={onChange} />
      <Switch disable selected={state} checkedChildren="开" unCheckedChildren="关" onChange={onChange} />
    </>
  );
}
