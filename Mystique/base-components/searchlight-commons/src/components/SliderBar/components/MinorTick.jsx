/*
 * @Author: dengys
 * @Date: 2021-08-24 15:57:52
 * @LastEditors: dengys
 * @LastEditTime: 2022-01-27 15:28:15
 */
import React from 'react';
import styles from './style.module.less';

const MinorTick = () => {
  return (
    <div className={styles.newminorTick}>
      <div className={styles.minorTickLine} />
    </div>
  );
};

export default MinorTick;
