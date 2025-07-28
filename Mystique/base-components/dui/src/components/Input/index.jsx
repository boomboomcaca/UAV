/* eslint-disable no-async-promise-executor */
import React, { useState, forwardRef, useRef } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import Icon from '@ant-design/icons';

import RawAsyncValidator from './asyncUtil';

import styles from './index.module.less';

const Input = forwardRef((props, ref) => {
  const {
    className,
    size,
    onPressEnter,
    defaultValue,
    value,
    onChange,
    maxLength,
    onSearch,
    style,
    allowClear,
    showSearch,
    suffix,
    rules,
    name,
    type,
    ...restProps
  } = props;
  const [text, setText] = useState(defaultValue);
  const [eye, setEye] = useState(false);
  const [errors, setErrors] = useState([]);
  const inputRef = useRef();
  React.useImperativeHandle(ref, () => inputRef.current);

  const iiipuntvalue = value === undefined ? text : value;

  const CloseSvg = () => (
    <svg width="12" height="12" viewBox="0 0 12 12" fill="none" xmlns="http://www.w3.org/2000/svg">
      <path
        opacity={iiipuntvalue === '' ? 0.2 : 0.5}
        d="M1 1L11 11M11 1L1 11"
        stroke="var(--theme-font-100)"
        strokeWidth="1.5"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </svg>
  );

  const handalChange = (e) => {
    let newValue = e.target.value;
    if (maxLength) {
      newValue = fixEmojiLength(newValue);
    }
    if (value === undefined) {
      setText(newValue);
    }
    daelValue(newValue);
  };

  const onKeyDown = (e) => {
    if (e.keyCode === 13) {
      onPressEnter(iiipuntvalue);
      onSearch(iiipuntvalue);
    }
  };

  const fixEmojiLength = (str) => {
    return [...(str || '')].slice(0, maxLength).join('');
  };

  const handalSearch = () => {
    onSearch(iiipuntvalue);
  };

  const delText = () => {
    if (value === undefined) {
      setText('');
    }
    daelValue('');
    onSearch('');
  };

  const daelValue = async (val) => {
    // let error = [];
    onChange && onChange(val);
    if (rules.length > 0) {
      const errorList = await RawAsyncValidator(name, val, rules);
      setErrors(errorList);
      // error = errorList;
    }
    // onChange && onChange(val, error);
  };

  return (
    <div className={classnames(styles.inputArea, styles[size])} style={style}>
      {showSearch && <Icon component={SearchSvg} className={styles.sousuo} onClick={handalSearch} />}
      <input
        className={classnames(className, styles.input)}
        type={type === 'password' ? (eye ? 'text' : 'password') : type}
        onKeyDown={onKeyDown}
        ref={inputRef}
        value={iiipuntvalue}
        onChange={handalChange}
        {...restProps}
      />
      {type === 'password' ? (
        eye ? (
          <Icon component={EyeCloseSvg} onClick={() => setEye(false)} />
        ) : (
          <Icon component={EyeOpenSvg} onClick={() => setEye(true)} />
        )
      ) : allowClear ? (
        <Icon component={CloseSvg} className={styles.close} onClick={delText} />
      ) : (
        <div className={styles.suffix}>{suffix}</div>
      )}
      {rules.length > 0 && <div className={styles.error}>{errors.join(',')}</div>}
    </div>
  );
});

Input.defaultProps = {
  className: '',
  size: 'normal',
  name: '',
  defaultValue: '',
  allowClear: false,
  showSearch: false,
  value: undefined,
  type: 'text',
  style: {},
  onChange: () => {},
  maxLength: null,
  onSearch: () => {},
  onPressEnter: () => {},
  suffix: null,
  rules: [],
};

Input.propTypes = {
  className: PropTypes.string,
  name: PropTypes.string,
  type: PropTypes.string,
  defaultValue: PropTypes.string,
  style: PropTypes.object,
  allowClear: PropTypes.bool,
  showSearch: PropTypes.bool,
  value: PropTypes.oneOfType([PropTypes.string, PropTypes.number]),
  maxLength: PropTypes.number,
  onPressEnter: PropTypes.func,
  onChange: PropTypes.func,
  onSearch: PropTypes.func,
  size: PropTypes.oneOf(['normal', 'large']),
  suffix: PropTypes.any,
  rules: PropTypes.array,
};

export default Input;

const SearchSvg = () => (
  <svg width="16" height="16" viewBox="0 0 16 16" fill="none" xmlns="http://www.w3.org/2000/svg">
    <path
      d="M12.3495 12.2918L15 15M14.263 7.63157C14.263 11.2941 11.294 14.2631 7.6315 14.2631C3.96902 14.2631 1 11.2941 1 7.63157C1 3.96906 3.96902 1 7.6315 1C11.294 1 14.263 3.96906 14.263 7.63157Z"
      stroke="#3CE5D3"
      strokeWidth="1.5"
      strokeLinecap="round"
      strokeLinejoin="round"
    />
  </svg>
);

const EyeOpenSvg = () => (
  <svg width="18" height="10" viewBox="0 0 18 10" fill="none" xmlns="http://www.w3.org/2000/svg">
    <path
      d="M1 5C2.44064 7.36781 5.4816 9 9 9C12.5184 9 15.5594 7.36781 17 5C15.5594 2.63219 12.5184 1 9 1C5.4816 1 2.44064 2.63219 1 5Z"
      stroke="var(--theme-font-50)"
      strokeWidth="1.5"
      strokeLinecap="round"
      strokeLinejoin="round"
    />
    <circle cx="9" cy="5" r="1.75" stroke="var(--theme-font-50)" strokeWidth="1.5" />
  </svg>
);

const EyeCloseSvg = () => (
  <svg width="18" height="10" viewBox="0 0 18 10" fill="none" xmlns="http://www.w3.org/2000/svg">
    <path
      d="M1.00043 1.5C2.44107 3.83532 5.48203 5.44511 9.00043 5.44511C12.5188 5.44511 15.5598 3.83532 17.0004 1.5"
      stroke="var(--theme-font-50)"
      strokeWidth="1.5"
      strokeLinecap="round"
      strokeLinejoin="round"
    />
    <path
      d="M12.5561 5.5L13.445 7.5"
      stroke="var(--theme-font-50)"
      strokeWidth="1.5"
      strokeLinecap="round"
      strokeLinejoin="round"
    />
    <path
      d="M15.2231 3.5L17.0009 5.5"
      stroke="var(--theme-font-50)"
      strokeWidth="1.5"
      strokeLinecap="round"
      strokeLinejoin="round"
    />
    <path
      d="M5.44476 5.5L4.55587 7.5"
      stroke="var(--theme-font-50)"
      strokeWidth="1.5"
      strokeLinecap="round"
      strokeLinejoin="round"
    />
    <path
      d="M2.77777 3.5L0.999993 5.5"
      stroke="var(--theme-font-50)"
      strokeWidth="1.5"
      strokeLinecap="round"
      strokeLinejoin="round"
    />
    <path
      d="M9.00043 5.5V8.5"
      stroke="var(--theme-font-50)"
      strokeWidth="1.5"
      strokeLinecap="round"
      strokeLinejoin="round"
    />
  </svg>
);
