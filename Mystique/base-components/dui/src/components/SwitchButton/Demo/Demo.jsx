import React, { useState } from 'react';
import SwitchButton from '../index';
import styles from './index.module.less';

const Demo = () => {
  const [info, setInfo] = useState(false);

  const [visible, setVisible] = useState(true);
  const [disabled, setDisabled] = useState(false);

  const [value, setValue] = useState('spc');
  const [values, setValues] = useState(['spc', 'lvl', 'xx']);
  const [displayValues, setDisplayValues] = useState(['频谱测量', '时域测量', 'xxxx']);

  const [value2, setValue2] = useState(1);
  const [values2, setValues2] = useState([1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18]);
  const [displayValues2, setDisplayValues2] = useState([1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18]);

  const [state, setState] = useState('normal');

  const onBtnSwitch = (tag, idx, val) => {
    window.console.log(tag, idx, val);
    switch (tag) {
      case 'sb1':
        setValue(val);
        setInfo(`tag:${tag} idx:${idx} val:${val}`);
        break;
      case 'sb2':
        setState(val);
        if (val === 'normal') {
          setVisible(true);
          setDisabled(false);
        }
        if (val === 'visible') {
          setVisible(false);
          setDisabled(false);
        }
        if (val === 'disabled') {
          setVisible(true);
          setDisabled(true);
        }
        break;

      default:
        break;
    }
  };

  return (
    <div className={styles.root}>
      <div className={styles.info}>{info}</div>

      <SwitchButton
        tag="sb1"
        title="title"
        tooltip="tooltip"
        visible={visible}
        disabled={disabled}
        value={value}
        values={values}
        onClick={onBtnSwitch}
      >
        {displayValues}
      </SwitchButton>

      <SwitchButton tag="sb" visible={visible} disabled={disabled} value={value} values={values} onClick={onBtnSwitch}>
        {displayValues}
      </SwitchButton>

      <SwitchButton
        tag="sb1"
        className={styles.comp1}
        visible={visible}
        disabled={disabled}
        value={value}
        values={values}
        onClick={onBtnSwitch}
      >
        {displayValues}
      </SwitchButton>

      <SwitchButton
        tag="sb2"
        className={styles.comp2}
        indicator={
          <div
            style={{
              backgroundColor: 'orangered',
              width: '100%',
              height: '100%',
            }}
          />
        }
        value={state}
        values={['normal', 'visible', 'disabled']}
        onClick={onBtnSwitch}
      >
        <div>normal</div>
        <div>visible:false</div>
        <div>disabled:true</div>
      </SwitchButton>

      <SwitchButton
        tag="sb3"
        visible={visible}
        disabled={disabled}
        value={value2}
        values={values2}
        onClick={(t, i, v) => {
          window.console.log({ t, i, v });
          setValue2(v);
        }}
      >
        {displayValues2}
      </SwitchButton>
    </div>
  );
};

export default Demo;
