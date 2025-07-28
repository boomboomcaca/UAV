import React from 'react';
import { FreeTime } from '../icon.jsx';
import styles from './style.module.less';

const FreetimeTask = () => {
  return (
    <div className={styles.main}>
      <FreeTime />
      <p>设备空闲自动运行</p>
    </div>
  );
};

export default FreetimeTask;
