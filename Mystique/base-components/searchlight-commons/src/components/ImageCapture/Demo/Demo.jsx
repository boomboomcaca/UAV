import React, { useState } from 'react';
import { Button } from 'dui';
import ImageCapture from '..';
import nikki from './nikki';
import test from './test';
import styles from './index.module.less';

export default function Demo() {
  const [uri, setUri] = useState(null);
  const capture = () => {
    const item = { url: test.uri, timestamp: new Date().getTime() };
    setUri(item);
  };
  return (
    <div className={styles.root}>
      <Button onClick={capture}>截图</Button>
      <ImageCapture imgURL={uri} />
    </div>
  );
}
