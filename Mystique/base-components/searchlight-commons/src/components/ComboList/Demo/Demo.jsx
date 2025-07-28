import React, { useState } from 'react';
import { Button } from 'dui';
import ComboList from '../ComboList.jsx';
import styles from './index.module.less';

const Demo = () => {
  const [msg, setMsg] = useState(null);
  const [msgs, setMsgs] = useState(null);
  const [values, setValues] = useState(null);
  return (
    <div className={styles.status}>
      <Button
        onClick={() => {
          const a = Math.random();
          if (a > 0.5) {
            setMsg({ type: 'error', msg: '??????' });
          } else {
            setMsg('没有type');
          }
        }}
      >
        添加value
      </Button>

      <div className={styles.combo}>
        <Button
          onClick={() => {
            const a = Math.random();
            if (a > 0.5) {
              setMsgs([
                { type: 'error', msg: '错误消息' },
                { type: 'info', msg: '通知消息' },
                { type: 'warning', msg: '警告消息' },
                { type: 'warning', msg: 1 },
              ]);
            } else {
              setMsgs(['123', '456', '?????????', 'qwer', 'wasd', '142857', 1]);
            }
          }}
        >
          添加attachValues
        </Button>
        <ComboList
          // mainIcon={<div />}
          // dropIcon={<div />}
          className={styles.list}
          values={values}
          value={msg}
          attachValues={msgs}
          maxLength={20}
        />
        <Button
          onClick={() => {
            setValues([
              `上古版本:Here you see the unit normal (0, 0, 1) converted to flat blue ` +
                `(128, 128, 255) and flat blue converted to the unit normal.You'll ` +
                `learn more about this in the [normal mapping](normal-mapping.md) technique.`,
            ]);
          }}
        >
          设置Values1
        </Button>
      </div>
      <Button
        onClick={() => {
          setValues([
            '上古版本：123',
            '上古版本：456',
            '上古版本：?????????',
            '上古版本：qwer',
            '上古版本：wasd',
            1,
            '上古版本：142857',
          ]);
        }}
      >
        设置Values2
      </Button>
      <Button
        onClick={() => {
          setValues([]);
        }}
      >
        设置Values[]
      </Button>
    </div>
  );
};

Demo.defaultProps = {};

Demo.propTypes = {};

export default Demo;
