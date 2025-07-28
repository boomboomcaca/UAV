/* eslint-disable no-empty */
import React, { useState, useEffect, useRef } from 'react';
import { createPortal } from 'react-dom';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { Input } from 'dui';
import { useKeyPress } from 'ahooks';
import useWindowSize from '../../hooks/useWindowSize';
import styles from './index.module.less';

const NumberInput = (props) => {
  const {
    value,
    suffix,
    placeholder,
    onValueChange,
    className,
    decimals,
    minValue,
    maxValue,
    disable,
    unavailableKeys,
    style,
  } = props;

  const [showModal, setShowModal] = useState(false);
  const [text, setText] = useState();
  const [errors, setErrors] = useState('');
  const [popstyle, setPopstyle] = useState(null);
  const inputRef = useRef();
  const FIRef = useRef();

  const size = useWindowSize((/* size */) => {});

  useKeyPress('esc', () => {
    if (showModal === true) {
      setErrors('');
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
    if (unavailableKeys.includes('+/-')) return;
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
    if (unavailableKeys.includes('.')) return;
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
    if (text !== '') {
      const newValue = Number(text);
      if (newValue < minValue) {
        setErrors(`超出最小值：${minValue}`);
        return;
      }
      if (newValue > maxValue) {
        setErrors(`超出最大值：${maxValue}`);
        return;
      }
      onValueChange(newValue);
    } else {
      onValueChange(null);
    }
    setErrors('');
    setShowModal(false);
  };

  const ssshow = () => {
    if (!disable) {
      setShowModal(true);
    }
  };

  useEffect(() => {
    setText(String(value || ''));
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

  const onMaskClick = () => {
    setShowModal(false);
  };
  const onPanClick = (tag) => {
    setValue(tag);
  };

  useEffect(() => {
    let position = null;
    if (FIRef.current && showModal) {
      position = {};
      const a = FIRef.current.getBoundingClientRect();
      const b = {
        width: document.documentElement.clientWidth,
        height: document.documentElement.clientHeight,
      };
      // TODO 待优化，不要写死高宽，参看less文件.popup
      // TODO 此处考虑的边缘情况不足
      const c = { width: 320, height: 320 };
      if (a.left + c.width > b.width) {
        position.left = b.width - c.width;
      } else {
        position.left = a.left;
      }
      if (a.top + a.height + c.height > b.height) {
        position.top = a.top - c.height - 2;
      } else {
        position.top = a.top + a.height + 2;
      }
      setPopstyle({ ...position, opacity: 1 });
    }
  }, [size, showModal]);

  return (
    <div className={classnames(styles.freqInput, className, disable ? styles.disable : '')} style={style} ref={FIRef}>
      <div className={styles.text} onClick={ssshow}>
        {value === null || value === '' ? <span className={styles.placeholder}>{placeholder}</span> : `${value}`}
      </div>
      <div className={styles.suffix} onClick={ssshow}>
        {suffix}
      </div>
      <input type="text" className={styles.input} />
      {showModal
        ? createPortal(
            <div className={styles.root} onClick={onMaskClick}>
              <div
                className={classnames(styles.popup)}
                style={popstyle}
                onClick={(e) => {
                  e.stopPropagation();
                }}
              >
                <div className={styles.content}>
                  <div className={styles.top}>
                    <Input
                      value={text}
                      suffix={suffix}
                      placeholder={placeholder}
                      size="large"
                      ref={inputRef}
                      style={{ width: '216px', height: '36px' }}
                      readOnly={isWap()}
                      onChange={inputChange}
                    />
                    <div className={styles.del} onClick={del} style={{ fontSize: '16px' }}>
                      ←
                    </div>
                    <div className={styles.error}>{errors}</div>
                  </div>
                  <div className={styles.body}>
                    <div className={styles.left}>
                      <div className={styles.nums}>
                        <div className={styles.btn} onClick={() => onPanClick('1')}>
                          1
                        </div>
                        <div className={styles.btn} onClick={() => onPanClick('2')}>
                          2
                        </div>
                        <div className={styles.btn} onClick={() => onPanClick('3')}>
                          3
                        </div>
                        <div className={styles.btn} onClick={() => onPanClick('4')}>
                          4
                        </div>
                        <div className={styles.btn} onClick={() => onPanClick('5')}>
                          5
                        </div>
                        <div className={styles.btn} onClick={() => onPanClick('6')}>
                          6
                        </div>
                        <div className={styles.btn} onClick={() => onPanClick('7')}>
                          7
                        </div>
                        <div className={styles.btn} onClick={() => onPanClick('8')}>
                          8
                        </div>
                        <div className={styles.btn} onClick={() => onPanClick('9')}>
                          9
                        </div>
                      </div>
                      <div className={styles.zero}>
                        <div className={styles.btn} onClick={() => onPanClick('0')}>
                          0
                        </div>
                        <div
                          className={classnames(styles.btn, { [styles.ban]: unavailableKeys.includes('.') })}
                          onClick={setPoint}
                        >
                          .
                        </div>
                      </div>
                    </div>
                    <div className={styles.right}>
                      <div
                        className={classnames(styles.btn, { [styles.ban]: unavailableKeys.includes('+/-') })}
                        onClick={setNegative}
                      >
                        +/-
                      </div>

                      <div className={styles.btn} style={{ color: '#3ce5d3', fontSize: '16px' }} onClick={ojbk}>
                        确 认
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>,
            document.body,
          )
        : null}
    </div>
  );
};

NumberInput.defaultProps = {
  value: '',
  suffix: '',
  onValueChange: () => {},
  className: '',
  decimals: 3,
  minValue: 20,
  maxValue: 8000,
  disable: false,
  unavailableKeys: [],
  placeholder: '',
  style: null,
};

NumberInput.propTypes = {
  value: PropTypes.number,
  suffix: PropTypes.any,
  onValueChange: PropTypes.func,
  className: PropTypes.string,
  decimals: PropTypes.number,
  minValue: PropTypes.number,
  maxValue: PropTypes.number,
  disable: PropTypes.bool,
  unavailableKeys: PropTypes.array,
  placeholder: PropTypes.string,
  style: PropTypes.any,
};

export default NumberInput;
