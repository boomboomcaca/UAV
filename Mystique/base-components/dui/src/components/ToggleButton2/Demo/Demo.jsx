import React, { useState } from 'react';
import ToggleButton2 from '../index';
import styles from './index.module.less';

const Demo = () => {
  const [visible, setVisible] = useState(true);
  const [disabled, setDisabled] = useState(false);

  const [info, setInfo] = useState(false);

  const onToggleBtnClick = (chk, tag) => {
    switch (tag) {
      case 'button':
        setInfo('obsolite');
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

  const [value, setValue] = useState('WORD');

  return (
    <div className={styles.root}>
      <div className={styles.info}>{info}</div>
      <ToggleButton2
        tag="toggle"
        tooltip="---------------"
        // checked={test.includes('a')}
        onClick={(e) => {
          window.console.log(e);
        }}
        waiting
        onDoubleClick={() => {}}
        onPress={() => {}}
        value={value}
        options={[
          { label: 'Word', value: 'WORD' },
          { label: 'Pdf', value: 'PDF' },
          { label: 'Excel', value: 'EXCEL' },
          { label: 'Txt', value: 'TXT' },
        ]}
        onSelectChange={(e) => {
          window.console.log(e);
          setValue(e);
        }}
      >
        生成报表
      </ToggleButton2>
      <ToggleButton2
        tag="toggle"
        tooltip="---------------"
        // checked={test.includes('a')}
        onClick={(e) => {
          window.console.log(e);
        }}
        waiting={false}
        onDoubleClick={() => {}}
        onPress={() => {}}
        value={value}
        options={[
          { label: 'Word', value: 'WORD' },
          { label: 'Pdf', value: 'PDF' },
          { label: 'Excel', value: 'EXCEL' },
          { label: 'Txt', value: 'TXT' },
        ]}
        onSelectChange={(e) => {
          window.console.log(e);
          setValue(e);
        }}
      >
        生成报表
      </ToggleButton2>
      <ToggleButton2
        tag="toggle"
        tooltip="---------------"
        // checked={test.includes('a')}
        onClick={(e) => {
          window.console.log(e);
        }}
        waiting={5000}
        onDoubleClick={() => {}}
        onPress={() => {}}
        value={value}
        options={[
          { label: 'Word', value: 'WORD' },
          { label: 'Pdf', value: 'PDF' },
          { label: 'Excel', value: 'EXCEL' },
          { label: 'Txt', value: 'TXT' },
        ]}
        onSelectChange={(e) => {
          window.console.log(e);
          setValue(e);
        }}
      >
        生成报表
      </ToggleButton2>
    </div>
  );
};

export default Demo;
