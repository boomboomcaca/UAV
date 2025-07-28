import React, { useState } from 'react';
import { Modal } from 'dui';
import NumberInput from '..';
import styles from './index.module.less';

export default function Demo() {
  const [value, setValue] = useState(null);
  const [show, setShow] = useState(false);
  return (
    <>
      <div
        className={styles.root}
        onClick={() => {
          setShow(true);
        }}
      >
        ????
      </div>
      <Modal
        visible={show}
        title="????"
        footer={null}
        style={{ width: 1440, top: '50%', transform: 'translateY(-50%)' }}
        bodyStyle={{ padding: 0 }}
        onCancel={() => {
          setShow(false);
        }}
      >
        <div style={{ height: 400, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
          <NumberInput
            value={value}
            placeholder="请输入..."
            suffix="xHz/μV"
            onValueChange={(v) => setValue(v)}
            unavailableKeys={['+/-']}
            className={styles.input}
          />
        </div>
      </Modal>
    </>
  );
}
