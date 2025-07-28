/* eslint-disable no-empty */
import React, { useState, useEffect, useRef, useMemo } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { PopUp, Input } from 'dui';
import NP from 'number-precision';
import { useKeyPress } from 'ahooks';
import styles from './index.module.less';

const freqIptStyle = {
  width: '100%',
  height: '72px',
  background: 'var(--theme-enumSelector-captionTextBg)',
  border: '1px solid rgba(60, 229, 211,0.6)',
  boxSizing: 'border-box',
  boxShadow: 'var(--theme-input-shadow)',
  borderRadius: '2px',
};

const FrequencyInput = (props) => {
  const {
    value,
    onValueChange,
    className,
    style,
    decimals,
    miniValue,
    minValue,
    maxValue,
    disable,
    lightUp,
    hideLight,
    hideKeys,
    mode,
  } = props;

  const [showModal, setShowModal] = useState(false);
  const [suffix, setSuffix] = useState('MHz');
  const [text, setText] = useState();
  const [errors, setErrors] = useState('');
  const inputRef = useRef();

  useKeyPress('esc', () => {
    if (showModal === true) {
      setErrors('');
      setSuffix('MHz');
      setShowModal(false);
    }
  });

  useKeyPress('enter', () => {
    if (showModal === true) {
      ojbk();
    }
  });

  const setValue = (val) => {
    const docSelection = window.getSelection().toString();
    const oldVal = docSelection === text ? '' : text;
    const newValue = oldVal.concat(val);
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
    const docSelection = window.getSelection().toString();
    const newValue = docSelection === text ? '' : text.slice(0, text.length - 1);
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
    if (minValue !== undefined ? newValue < minValue : newValue < miniValue) {
      setErrors(`超出最小频率：${minValue !== undefined ? minValue : miniValue}MHz`);
      return;
    }
    if (newValue > maxValue) {
      setErrors(`超出最大频率：${maxValue}MHz`);
      return;
    }
    setSuffix('MHz');
    setErrors('');
    setShowModal(false);
    onValueChange(newValue);
  };

  const ssshow = () => {
    if (!disable) {
      setShowModal(true);
    }
  };

  useEffect(() => {
    if (value !== null && value !== undefined && typeof value === 'number') {
      setText(value.toFixed(4));
    } else {
      setText('');
    }
  }, [value, showModal]);

  useEffect(() => {
    if (showModal) {
      inputRef.current?.focus?.();
      inputRef.current?.select?.();
    }
  }, [showModal]);

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

  const showValue = useMemo(() => {
    if (value !== null && value !== undefined && typeof value === 'number') {
      if (value >= 1000) {
        return `${NP.divide(value, 1000).toFixed(4)} GHz`;
      }
      return `${value.toFixed(4)} MHz`;
    }
    return '';
  }, [value]);

  return (
    <>
      <div
        className={classnames(styles.freqInputBIGNEW, className, {
          [styles.lightUp]: lightUp,
          [styles.disable]: disable,
          [styles.hideLight]: hideLight,
        })}
        style={style}
      >
        {!hideLight && <div className={styles.left} />}
        <div className={mode === 'simple' ? styles.simple : styles.text} onClick={ssshow}>{`${showValue}`}</div>
        {!hideLight && <div className={styles.right} />}
      </div>
      <PopUp
        visible={showModal}
        popupTransition="rtg-zoom"
        onCancel={() => {
          setShowModal(false);
          setSuffix('MHz');
          setErrors('');
        }}
        usePortal
      >
        <div className={styles.freqModalBIGNEW}>
          <Input
            value={text}
            suffix={<div style={{ fontSize: 16 }}>{suffix}</div>}
            size="large"
            ref={inputRef}
            style={freqIptStyle}
            className={styles.freqipt}
            readOnly={isWap()}
            onChange={inputChange}
          />
          <div className={styles.error}>{errors}</div>
          <div className={styles.tools}>
            <div className={styles.numarea}>
              {[1, 2, 3, 4, 5, 6, 7, 8, 9, 0].map((numitem) => (
                <div className={styles.btn} onClick={() => setValue(String(numitem))} key={numitem}>
                  {numitem}
                </div>
              ))}
            </div>
            <div className={styles.toolarea}>
              <div className={styles.llg}>
                {!hideKeys.includes('+/-') && (
                  <div className={styles.btn} onClick={setNegative}>
                    +/-
                  </div>
                )}
                {!hideKeys.includes('.') && (
                  <div className={styles.btn} onClick={setPoint}>
                    .
                  </div>
                )}
                <div className={styles.btn} onClick={() => setSuffix('kHz')}>
                  kHz
                </div>
                <div className={styles.btn} onClick={() => setSuffix('MHz')}>
                  MHz
                </div>
                <div className={styles.btn} onClick={() => setSuffix('GHz')}>
                  GHz
                </div>
                {hideKeys.includes('.') && <div />}
                {hideKeys.includes('+/-') && <div />}
              </div>
              <div className={styles.rrg}>
                <div className={classnames(styles.btn, styles.bbtn)} onClick={del} style={{ fontSize: '16px' }}>
                  删 除
                </div>
                <div
                  className={classnames(styles.btn, styles.bbtn)}
                  style={{ color: '#3ce5d3', fontSize: '16px' }}
                  onClick={ojbk}
                >
                  确 认
                </div>
              </div>
            </div>
          </div>
        </div>
      </PopUp>
    </>
  );
};

FrequencyInput.defaultProps = {
  value: 98,
  onValueChange: () => {},
  className: '',
  style: null,
  decimals: 4,
  miniValue: 20,
  minValue: undefined,
  maxValue: 8000,
  disable: false,
  lightUp: false,
  hideLight: false,
  hideKeys: [],
  mode: 'normal', // simple normal
};

FrequencyInput.propTypes = {
  value: PropTypes.number,
  onValueChange: PropTypes.func,
  className: PropTypes.string,
  style: PropTypes.any,
  decimals: PropTypes.number,
  miniValue: PropTypes.number,
  minValue: PropTypes.number,
  maxValue: PropTypes.number,
  disable: PropTypes.bool,
  lightUp: PropTypes.bool,
  hideLight: PropTypes.bool,
  hideKeys: PropTypes.array,
  mode: PropTypes.any,
};

export default FrequencyInput;
