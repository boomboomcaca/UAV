import React, { useState, useEffect } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import NP from 'number-precision';
import Icon from '@ant-design/icons';

import styles from './index.module.less';

const InputNumber = (props) => {
  const {
    className,
    size,
    defaultValue,
    value,
    onChange,
    style,
    onBlur,
    step,
    min,
    max,
    digits,
    suffix,
    disabled,
    hideArrow,
    allowClear,
    ...restProps
  } = props;
  const [text, setText] = useState(defaultValue);

  const handalChange = (e) => {
    const newValue = e.target.value;
    setText(newValue);
    // const cccheckValue = checkValue(newValue);
    // if (String(cccheckValue) === newValue) {
    //   onChange && onChange(cccheckValue);
    // }
  };

  const handalBlur = (e) => {
    e.persist();
    // window.setTimeout(() => {
    const newValue = e.target.value;
    const cccheckValue = checkValue(newValue);
    setText(cccheckValue);
    onChange && onChange(cccheckValue);
    // }, 120);
  };

  const delText = () => {
    setText('');
    onChange && onChange('');
  };

  const checkValue = (newValue) => {
    let number = Number(newValue);
    if (Number.isNaN(number) || newValue === '') {
      return '';
    }
    number = Number(number.toFixed(digits));
    if (max !== null && newValue > max) {
      number = max;
    }
    if (min !== null && newValue < min) {
      number = min;
    }
    return number;
  };

  const handalUp = () => {
    const number = Number(text);
    if (Number.isNaN(number)) {
      setText('');
      onChange && onChange('');
    } else {
      let newValue = NP.plus(number, step);
      if (min !== null && newValue < min) {
        newValue = min;
      }
      if (max !== null && newValue > max) {
        newValue = max;
      }
      setText(newValue);
      onChange && onChange(newValue);
    }
  };

  const handalDown = () => {
    const number = Number(text);
    if (Number.isNaN(number)) {
      setText('');
      onChange && onChange('');
    } else {
      let newValue = NP.minus(number, step);
      if (min !== null && newValue < min) {
        newValue = min;
      }
      setText(newValue);
      onChange && onChange(newValue);
    }
  };

  useEffect(() => {
    if (value !== null) {
      setText(value);
    }
  }, [value]);

  const CloseSvg = () => (
    <svg width="12" height="12" viewBox="0 0 12 12" fill="none" xmlns="http://www.w3.org/2000/svg">
      <path
        opacity={text === '' ? 0.2 : 0.5}
        d="M1 1L11 11M11 1L1 11"
        stroke="var(--theme-font-100)"
        strokeWidth="1.5"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </svg>
  );

  return (
    <div
      className={classnames(styles.inputNumber, styles[size], {
        [styles.ban]: disabled || hideArrow,
      })}
      style={style}
    >
      <input
        autoComplete="off"
        className={classnames(className, styles.input, disabled && styles.inputDisable)}
        value={text}
        disabled={disabled}
        onChange={handalChange}
        onBlur={handalBlur}
        type="number"
        {...restProps}
      />
      <div className={styles.suffix}>{suffix}</div>
      {!hideArrow && (
        <div className={styles.step}>
          <div className={styles.up} onClick={handalUp}>
            <span className={styles.ab}>
              <Icon component={UpSvg} />
            </span>
          </div>
          <div className={styles.down} onClick={handalDown}>
            <span className={styles.ab}>
              <Icon component={DownSvg} />
            </span>
          </div>
        </div>
      )}
      {hideArrow && allowClear && <Icon component={CloseSvg} className={styles.close} onClick={delText} />}
    </div>
  );
};

InputNumber.defaultProps = {
  className: '',
  style: {},
  size: 'normal',
  defaultValue: '',
  value: null,
  onChange: null,
  onBlur: null,
  min: null,
  max: null,
  step: 1,
  digits: 2,
  suffix: null,
  disabled: false,
  hideArrow: false,
  allowClear: false,
};

InputNumber.propTypes = {
  className: PropTypes.string,
  style: PropTypes.object,
  size: PropTypes.oneOf(['normal', 'large']),
  defaultValue: PropTypes.oneOfType([PropTypes.string, PropTypes.number]),
  value: PropTypes.oneOfType([PropTypes.string, PropTypes.number]),
  onChange: PropTypes.func,
  onBlur: PropTypes.func,
  min: PropTypes.number,
  max: PropTypes.number,
  step: PropTypes.number,
  digits: PropTypes.number,
  suffix: PropTypes.any,
  disabled: PropTypes.bool,
  hideArrow: PropTypes.bool,
  allowClear: PropTypes.bool,
};

export default InputNumber;

const UpSvg = () => (
  <svg
    viewBox="64 64 896 896"
    focusable="false"
    data-icon="up"
    width="10"
    height="10"
    fill="currentColor"
    aria-hidden="true"
  >
    <path d="M890.5 755.3L537.9 269.2c-12.8-17.6-39-17.6-51.7 0L133.5 755.3A8 8 0 00140 768h75c5.1 0 9.9-2.5 12.9-6.6L512 369.8l284.1 391.6c3 4.1 7.8 6.6 12.9 6.6h75c6.5 0 10.3-7.4 6.5-12.7z" />
  </svg>
);

const DownSvg = () => (
  <svg
    viewBox="64 64 896 896"
    focusable="false"
    data-icon="down"
    width="10"
    height="10"
    fill="currentColor"
    aria-hidden="true"
  >
    <path d="M884 256h-75c-5.1 0-9.9 2.5-12.9 6.6L512 654.2 227.9 262.6c-3-4.1-7.8-6.6-12.9-6.6h-75c-6.5 0-10.3 7.4-6.5 12.7l352.6 486.1c12.8 17.6 39 17.6 51.7 0l352.6-486.1c3.9-5.3.1-12.7-6.4-12.7z" />
  </svg>
);
