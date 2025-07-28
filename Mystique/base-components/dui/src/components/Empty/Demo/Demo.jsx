import React from 'react';
import Empty from '../index';
import styles from './index.module.less';

const test = (
  <svg width="80" height="80" viewBox="0 0 80 80" fill="none" xmlns="http://www.w3.org/2000/svg">
    <rect x="10" y="35" width="66" height="39" fill="#282D49" />
    <path d="M4 57.64L11 37V76.56L4 80V57.64Z" fill="#272C47" />
    <rect x="16" width="56" height="70" rx="1" fill="#646989" />
    <rect x="13" y="3" width="56" height="70" rx="1" fill="#868BA4" />
    <rect x="10" y="7" width="56" height="70" rx="1" fill="#ADAFC4" />
    <rect x="17" y="14" width="42" height="24" rx="1" fill="#9095AE" />
    <rect x="17" y="45" width="42" height="7" rx="3.5" fill="#9095AE" />
    <path d="M70 57.5L76 35V74L70 80V57.5Z" fill="#303858" />
    <rect x="4" y="57" width="66" height="23" fill="#474D6B" />
  </svg>
);

export default function Demo() {
  return (
    <div className={styles.root}>
      <Empty emptype={Empty.Normal} />
      <Empty emptype={Empty.Feature} message="无可用功能" />
      <Empty emptype={Empty.Device} message="无可用设备" />
      <Empty emptype={Empty.Box} />
      <Empty emptype={Empty.RunningTask} message="无运行任务" />
      <Empty emptype={Empty.UAV} />
      <Empty emptype={Empty.Station} message="无可用站点" />
      <Empty emptype={Empty.Station} svg={test} />
      <Empty emptype={Empty.Template} />
    </div>
  );
}
