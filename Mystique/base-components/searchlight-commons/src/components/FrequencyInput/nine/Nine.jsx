/* eslint-disable no-empty */
import React, { useState, useEffect, useRef, useMemo } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import NP from 'number-precision';
import { Input } from 'dui';
import { useClickAway } from 'ahooks';
import styles from './nine.module.less';

const FrequencyInput = (props) => {
  const { value, onValueChange, className, decimals, miniValue, maxValue, disable, placement, zIndex } = props;

  const [open, setOpen] = useState(false);
  const [suffix, setSuffix] = useState('MHz');
  const [text, setText] = useState();
  const [errors, setErrors] = useState('');
  const inputRef = useRef();
  const FIRef = useRef();

  useClickAway(() => {
    setOpen(false);
  }, FIRef);

  const setValue = (val) => {
    const newValue = text.concat(val);
    const indx = newValue.indexOf('.');
    if (decimals > 0 && indx > 0 && newValue.length - indx - 1 > decimals) {
      setErrors(`最多支持${decimals}位小数`);
      return;
    }
    setErrors('');
    setText(newValue);
  };

  const setNegative = () => {
    if (text.startsWith('-')) {
      setText((prev) => {
        return prev.slice(1, text.length);
      });
    } else {
      setText((prev) => {
        return `-${prev}`;
      });
    }
  };

  const setPoint = () => {
    if (!text.includes('.')) {
      const newValue = text.concat('.');
      setText(newValue === '.' ? '0.' : newValue);
    }
  };

  const del = () => {
    const newValue = text.slice(0, text.length - 1);
    setText(newValue);
    setErrors('');
  };

  const inputChange = (newValue) => {
    const addChar = newValue.length > text.length;
    const lastKey = String(newValue).slice(newValue.length - 1, newValue.length);
    if (addChar && !['1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '.', '-'].includes(lastKey)) {
      return;
    }
    if (addChar && lastKey === '.' && text.includes('.')) {
      return;
    }
    if (addChar && lastKey === '-' && text !== '') {
      return;
    }
    const indx = String(newValue).indexOf('.');
    if (decimals > 0 && indx > 0 && String(newValue).length - indx - 1 > decimals) {
      setErrors(`最多支持${decimals}位小数`);
      return;
    }
    setErrors('');
    setText(newValue);
  };

  const ojbk = () => {
    let newValue = Number(text);
    if (suffix === 'kHz') {
      newValue = NP.divide(newValue, 1000);
    }
    if (suffix === 'GHz') {
      newValue = NP.times(newValue, 1000);
    }
    if (newValue < miniValue) {
      setErrors(`超出最小频率：${miniValue}MHz`);
      return;
    }
    if (newValue > maxValue) {
      setErrors(`超出最大频率：${maxValue}MHz`);
      return;
    }
    setSuffix('MHz');
    setErrors('');
    setOpen(false);
    onValueChange(newValue);
  };

  const changeHide = () => {
    if (!disable) {
      setOpen(!open);
    }
  };

  useEffect(() => {
    if (value !== null && value !== undefined && typeof value === 'number') {
      setText(value.toFixed(4));
    } else {
      setText('');
    }
  }, [value, open]);

  useEffect(() => {
    if (open) {
      inputRef?.current?.focus?.();
      // inputRef?.current?.select?.();
    }
  }, [open]);

  const isWap = () => {
    const ua = navigator.userAgent;
    const isMidp = ua.match(/midp/i) === 'midp';
    const isUc7 = ua.match(/rv:1.2.3.4/i) === 'rv:1.2.3.4';
    const isUc = ua.match(/ucweb/i) === 'ucweb';
    const isAndroid = ua.match(/android/i) === 'android';
    const isCE = ua.match(/windows ce/i) === 'windows ce';
    const isWM = ua.match(/windows mobile/i) === 'windows mobile';
    const isIphone = ua.indexOf('iPhone') !== -1;
    const isIPad = !isIphone && 'ontouchend' in document;
    if (isIPad || isIphone || isMidp || isUc7 || isUc || isAndroid || isCE || isWM) {
      return true;
    }
    return false;
  };

  const placementT = useMemo(() => {
    let tt = { left: 0, top: '100%' };
    if (placement === 'topRight') {
      tt = { right: 0, top: '100%' };
    }
    if (placement === 'bottomLeft') {
      tt = { left: 0, bottom: '100%' };
    }
    if (placement === 'bottomRight') {
      tt = { right: 0, bottom: '100%' };
    }
    if (placement === 'leftTop') {
      tt = { left: '100%', top: 0 };
    }
    if (placement === 'leftBottom') {
      tt = { left: '100%', bottom: 0 };
    }
    if (placement === 'rightTop') {
      tt = { right: '100%', top: 0 };
    }
    if (placement === 'rightBottom') {
      tt = { right: '100%', bottom: 0 };
    }
    return tt;
  }, [placement]);

  const showValue = useMemo(() => {
    if (value !== null && value !== undefined && typeof value === 'number') {
      if (value > 1000) {
        return `${NP.divide(value, 1000).toFixed(4)} GHz`;
      }
      return `${value.toFixed(4)} MHz`;
    }
    return '';
  }, [value]);

  return (
    <div className={classnames(styles.freqInputnine, className, disable ? styles.disable : '')} ref={FIRef}>
      <div className={styles.text} onClick={changeHide}>{`${showValue}`}</div>
      <input type="text" className={styles.input} />
      <div className={classnames(styles.abfreq, { [styles.hidden]: !open })} style={{ ...placementT, zIndex }}>
        <Input
          value={text}
          suffix={suffix}
          size="large"
          ref={inputRef}
          style={{ width: '100%', height: '48px' }}
          readOnly={isWap()}
          onChange={inputChange}
        />
        <div className={styles.error}>{errors}</div>
        <div className={styles.unit}>
          <div className={styles.btn} onClick={() => setSuffix('kHz')}>
            kHz
          </div>
          <div className={styles.btn} onClick={() => setSuffix('MHz')}>
            MHz
          </div>
          <div className={styles.btn} onClick={() => setSuffix('GHz')}>
            GHz
          </div>
        </div>
        <div className={styles.hang}>
          <div className={styles.item} onClick={() => setValue('1')}>
            1
          </div>
          <div className={styles.item} onClick={() => setValue('2')}>
            2
          </div>
          <div className={styles.item} onClick={() => setValue('3')}>
            3
          </div>
          <div className={styles.item} onClick={del}>
            删除
          </div>
          <div className={styles.item} onClick={() => setValue('4')}>
            4
          </div>
          <div className={styles.item} onClick={() => setValue('5')}>
            5
          </div>
          <div className={styles.item} onClick={() => setValue('6')}>
            6
          </div>
          <div className={classnames(styles.item, styles.bigitem)} onClick={ojbk}>
            确定
          </div>
          <div className={styles.item} onClick={() => setValue('7')}>
            7
          </div>
          <div className={styles.item} onClick={() => setValue('8')}>
            8
          </div>
          <div className={styles.item} onClick={() => setValue('9')}>
            9
          </div>
          <div className={styles.item} onClick={setNegative}>
            +/-
          </div>
          <div className={styles.item} onClick={() => setValue('0')}>
            0
          </div>
          <div className={styles.item} onClick={setPoint}>
            .
          </div>
        </div>
      </div>
    </div>
  );
};

FrequencyInput.defaultProps = {
  value: 98,
  onValueChange: () => {},
  className: '',
  decimals: 4,
  miniValue: 20,
  maxValue: 8000,
  disable: false,
  placement: 'topLeft',
  zIndex: 1,
};

FrequencyInput.propTypes = {
  value: PropTypes.number,
  onValueChange: PropTypes.func,
  className: PropTypes.string,
  decimals: PropTypes.number,
  miniValue: PropTypes.number,
  maxValue: PropTypes.number,
  zIndex: PropTypes.number,
  disable: PropTypes.bool,
  placement: PropTypes.oneOf([
    'topLeft',
    'topRight',
    'bottomLeft',
    'bottomRight',
    'leftTop',
    'leftBottom',
    'rightTop',
    'rightBottom',
  ]),
};

export default FrequencyInput;
