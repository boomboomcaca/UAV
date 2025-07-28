import React, { useState } from 'react';
import Pagination from '../index';
import styles from './index.module.less';

export default function Demo() {
  const [current1, setCurrent1] = useState(2);
  const [current2, setCurrent2] = useState(2);

  return (
    <div className={styles.root}>
      <Pagination />
      <Pagination
        current={current1}
        pageSize={20}
        total={100}
        onChange={(e) => {
          window.console.log(e);
          setCurrent1(e);
        }}
      />
      <Pagination
        current={current2}
        pageSize={20}
        total={1000}
        onChange={(e) => {
          window.console.log(e);
          setCurrent2(e);
        }}
      />
    </div>
  );
}
