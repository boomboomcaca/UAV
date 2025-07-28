import React from 'react';
import PCMPlayerTest from './PCMPlayerTest.jsx';
import AduioPlayerTest from './AudioPlayerTest.jsx';
import styles from './index.module.less';

export default function Demo() {
  return (
    <div className={styles.root}>
      <PCMPlayerTest />
      <AduioPlayerTest />
    </div>
  );
}
