import React, { useState } from 'react';
import ButtonSwitch from '../index';
import { A, B } from '../SVGComponent/index.jsx';
import styles from './index.module.less';

const Demo = () => {
  const [state, setState] = useState(false);
  return (
    <div className={styles.root}>
      <ButtonSwitch state={state} onChange={() => setState(!state)} selected icons={[<A />, <B />]} />
    </div>
  );
};

export default Demo;
