import React from 'react';
import Loading from '../index';
import styles from './index.module.less';

export default function Demo() {
  return (
    <div className={styles.root}>
      <Loading type="colorful" />
      <Loading loadingMsg="数据加载中..." vertical />
      <Loading loadingMsg="数据加载中..." />
      <Loading loadingSize={50} type="single" />
      <Loading loadingSize={200} type="colorful" />
      <Loading loadingSize={100} type="double" />
    </div>
  );
}
