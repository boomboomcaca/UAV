/*
 * @Author: dengys
 * @Date: 2021-12-04 13:49:05
 * @LastEditors: dengys
 * @LastEditTime: 2022-03-18 15:36:23
 */
import React, { useState } from 'react';
import FrequencyInput from '../FrequencyInput.jsx';
import styles from './index.module.less';

export default function Demo() {
  const [value, setValue] = useState(95.5);
  return (
    <div className={styles.root}>
      <FrequencyInput value={value} lightUp onValueChange={(v) => setValue(v)} hideKeys={['+/-']} />
      <FrequencyInput value={value} hideLight onValueChange={(v) => setValue(v)} hideKeys={['+/-']} />
      <FrequencyInput value={value} minValue={10} onValueChange={(v) => setValue(v)} hideKeys={['+/-']} />
      <FrequencyInput value={value} minValue={0} hideLight onValueChange={(v) => setValue(v)} />
      {/* <FrequencyInput.Nine value={value} onValueChange={(v) => setValue(v)} /> */}
    </div>
  );
}
