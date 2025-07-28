import React, { useState, useEffect } from 'react';
import { Button } from 'dui';
import StartPage from '../StartPage.jsx';

export default () => {
  const [visible, setVisible] = useState(true);
  useEffect(() => {});
  const onStart = () => {
    setVisible(!visible);
  };
  return (
    <div>
      <Button onClick={() => setVisible(!visible)}>显示/隐藏</Button>
      <div style={{ width: '360px', height: '360px' }}>
        <StartPage visible={visible} onStart={onStart} />
      </div>
    </div>
  );
};
