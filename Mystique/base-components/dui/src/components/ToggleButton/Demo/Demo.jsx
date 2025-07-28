import React, { useState } from 'react';
import ToggleButton from '../index';
import styles from './index.module.less';

const Demo = () => {
  const [checked, setChecked] = useState(false);
  const [visible, setVisible] = useState(true);
  const [disabled, setDisabled] = useState(false);

  const [info, setInfo] = useState(false);

  const onToggleBtnClick = (chk, tag) => {
    switch (tag) {
      case 'button':
        setInfo('obsolite');
        break;
      case 'toggle':
        setChecked(!chk);
        break;
      case 'visible':
        setVisible(!chk);
        break;
      case 'disabled':
        setDisabled(!chk);
        break;
      default:
        break;
    }
  };

  const onDoubleClick = (tag) => {
    setInfo(tag);
  };

  const onPress = (tag) => {
    setInfo(tag);
  };

  const [value, setValue] = useState([0]);

  const [test, setTest] = useState(['a', 'b', 'c']);

  return (
    <div className={styles.root}>
      <div className={styles.info}>{info}</div>
      <ToggleButton
        indicator={<></>}
        tag="toggle"
        multiSelect
        tooltip="---------------"
        checked={test.includes('a')}
        onClick={() => {
          if (test.includes('a')) {
            setTest(['b', 'c']);
          } else {
            setTest(['a', 'b', 'c']);
          }
        }}
        onDoubleClick={() => {}}
        onPress={() => {}}
        value={value}
        options={[
          { key: 0, value: '门限1' },
          { key: 1, value: '自定义门限' },
          { key: 2, value: '门限3' },
        ]}
        onSelectChange={(a, b, c) => {
          window.console.log(a, b, c);
          setValue(b);
        }}
      >
        测量门限
      </ToggleButton>
      <ToggleButton
        tag="button"
        className={styles.comp}
        visible={visible}
        disabled={disabled}
        onClick={onToggleBtnClick}
      >
        btn:obsolite
      </ToggleButton>
      <ToggleButton
        tag="toggle"
        className={styles.comp}
        visible={visible}
        disabled={disabled}
        // checked={checked}
        indicator={<div className={styles.ind} />}
        onClick={onToggleBtnClick}
      >
        ToggleTest
      </ToggleButton>
      <ToggleButton tag="visible" className={styles.comp} checked={visible} onClick={onToggleBtnClick}>
        visible
      </ToggleButton>
      <ToggleButton tag="disabled" className={styles.comp} checked={disabled} onClick={onToggleBtnClick}>
        disabled
      </ToggleButton>
      <ToggleButton
        tag="doubleClick"
        className={styles.comp}
        visible={visible}
        disabled={disabled}
        checked={checked}
        onDoubleClick={onDoubleClick}
      >
        doubleClick
      </ToggleButton>
      <ToggleButton
        tag="press1"
        className={styles.comp}
        visible={visible}
        disabled={disabled}
        checked={checked}
        onPress={onPress}
      >
        press
      </ToggleButton>
      <ToggleButton
        tag="press2"
        className={styles.comp}
        visible={visible}
        disabled={disabled}
        checked={checked}
        onPress={onPress}
        style={{ height: 90 }}
        options={[
          { key: 0, value: 'aaaa' },
          { key: 1, value: 'bbbb' },
        ]}
      >
        press
      </ToggleButton>
      <ToggleButton tag="twink" className={styles.comp} visible={visible} twinkling disabled={disabled} checked>
        twink
      </ToggleButton>
    </div>
  );
};

export default Demo;
