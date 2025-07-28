import React, { useState } from 'react';
import IconButton from '../index';
import styles from './index.module.less';

const icon1 = (color) => {
  return (
    <div style={{ width: 30, height: 30 }}>
      <svg viewBox="0 0 1024 1024" xmlns="http://www.w3.org/2000/svg" preserveAspectRatio="xMinYMin meet">
        <path d="M404.9 336.3v352.6l305.4-176.3z" fill={color} />
      </svg>
    </div>
  );
};

const icon2 = (
  <svg version="1.1" viewBox="0 0 500 500" preserveAspectRatio="xMinYMin meet" width="30" height="30">
    <circle
      fill="#F7941E"
      stroke="#231F20"
      strokeWidth="10"
      strokeMiterlimit="10"
      cx="250"
      cy="250"
      r="100"
      opacity="0.6"
    />
  </svg>
);

const Demo = () => {
  const [info, setInfo] = useState(false);

  const [visible, setVisible] = useState(true);
  const [disabled, setDisabled] = useState(false);
  const [checked, setChecked] = useState(false);
  const [loading, setLoading] = useState(false);

  const onIconClick = (chk, tag) => {
    switch (tag) {
      case 'start':
        setLoading(true);
        setTimeout(() => {
          setChecked(!chk);
          setLoading(false);
        }, 2000);
        break;
      case 'visible':
        setVisible(!visible);
        setInfo(`visible:${!visible}`);
        break;
      case 'disabled':
        setDisabled(!disabled);
        setInfo(`disabled:${!disabled}`);
        break;
      default:
        break;
    }
  };

  return (
    <div className={styles.root}>
      <div className={styles.info}>{info}</div>
      <IconButton tag="visible" className={styles.comp} onClick={onIconClick}>
        <div>X</div>
      </IconButton>
      <IconButton tag="disabled" className={styles.comp} onClick={onIconClick}>
        {icon2}
      </IconButton>
      <IconButton tag="disabled" text="DISABLED" className={styles.comp} onClick={onIconClick}>
        {icon2}
      </IconButton>
      <IconButton
        tag="start"
        className={styles.comp}
        visible={visible}
        disabled={disabled}
        checked={checked}
        loading={loading}
        onClick={onIconClick}
      >
        {icon1(checked ? 'red' : 'white')}
      </IconButton>
      <IconButton
        tag="start"
        text="START"
        className={styles.compx}
        visible={visible}
        disabled={disabled}
        checked={checked}
        loading={loading}
        onClick={onIconClick}
      >
        {icon1(checked ? 'red' : 'white')}
      </IconButton>
    </div>
  );
};

export default Demo;
