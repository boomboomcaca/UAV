import React, { useState } from 'react';
import Button from '../index';

export default function Demo() {
  const [state, setstate] = useState(true);

  const onClick = () => {
    console.log('111');
  };

  const onClick1 = () => {
    setstate(!state);
  };

  return (
    <>
      <h3>按钮类型</h3>
      <Button>Button</Button>
      <Button type="primary">采集新模板</Button>
      <h3>按钮大小</h3>
      <Button size="small">small</Button>
      <Button size="middle">middle</Button>
      <Button size="large">large</Button>
      <h3>禁止</h3>
      <Button onClick={onClick} disabled={state}>
        disabled
      </Button>
      <h3>事件</h3>
      <Button onClick={onClick1}>事件</Button>
    </>
  );
}
