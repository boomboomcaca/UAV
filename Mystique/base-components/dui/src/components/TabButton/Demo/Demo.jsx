import React, { useState } from 'react';
import TabButton from '../index';
import styles from './index.module.less';

const Demo = () => {
  const [state, setState] = useState({
    value: '车载模式aaa',
    key: 'b',
  });
  const option = [
    {
      value: '云端模式aaaa',
      key: 'a',
    },
    {
      value: '车载模式aaa',
      key: 'b',
    },
    {
      value: '中心云端',
      key: 'c',
    },
    {
      value: '云端模式aaaa',
      key: 'd',
    },
    {
      value: '车载模式aaa',
      key: 'e',
    },
    {
      value: '中心云端',
      key: 'f',
    },
  ];
  return (
    <div className={styles.root}>
      <TabButton state={state} onChange={(d) => setState(d)} option={option} />
      <TabButton state={state} onChange={(d) => setState(d)} option={option} disabledOptions={['a', 'd', 'e']} />
      <TabButton state={state} onChange={(d) => setState(d)} option={option} disabled />
    </div>
  );
};

export default Demo;
