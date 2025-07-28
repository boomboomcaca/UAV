import React, { useState } from 'react';
import MultipleSwitch from '../index';
import styles from './index.module.less';

const Demo = () => {
  const [value, setValue] = useState(3);
  return (
    <div className={styles.root}>
      <MultipleSwitch
        value={value}
        onChange={(iiem) => {
          // item选择项
        }}
        options={[
          { label: 'aaaaaa', value: 1 },
          { label: 'bbbbbb', value: 2 },
          { label: 'cccccc', value: 3 },
        ]}
      />
    </div>
  );
};

export default Demo;
