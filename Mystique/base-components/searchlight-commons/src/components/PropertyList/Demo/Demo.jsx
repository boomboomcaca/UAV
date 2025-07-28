import React, { useEffect, useState } from 'react';
import PropertyList from '..';
import paramData from './paramtersDemo';
import setData from './settingsDemo';
import styles from './index.module.less';

export default function Demo() {
  const [params, setParams] = useState([]);
  const [disableKeys, setDisableKeys] = useState([]);

  useEffect(() => {
    setParams(
      setData.filter((p) => {
        return p.displayName !== '';
      }),
    );
    setInterval(() => {
      if (Math.random() > 0.5) {
        setDisableKeys(['unitSelection', 'capture', 'frequency', 'rawSwitch']);
      } else {
        setDisableKeys(['unitSelection']);
      }
    }, 2000);
  }, []);
  return (
    <div className={styles.root}>
      <PropertyList
        enable
        refresh
        filter={null}
        params={params}
        disableKeys={disableKeys}
        // disableKeys={[]}
        // disableKeys={undefined}
        // disableKeys={null}
        hideKeys={['polarization', 'test2']}
        OnParamsChanged={(ps, name, old, val) => {
          // 参数值变更
          window.console.log({ ps, name, old, val });
          setParams(ps);
        }}
      />
      <PropertyList
        enable={false}
        params={params}
        hideKeys={['polarization', 'test2']}
        OnParamsChanged={(ps, name, old, val) => {
          // 参数值变更
          window.console.log({ ps, name, old, val });
        }}
      />
    </div>
  );
}
