import React, { useState, useRef, useEffect } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { useClickAway } from 'ahooks';
import styles from './styles.module.less';
import dropIcon from './dropIcon.jsx';

const Select = (props) => {
  const { className, value, name, onChange, style } = props;
  let { children } = props;
  const [val, setVal] = useState(value);
  const [open, setOpen] = useState(false);
  const SelectRef = useRef(null);

  useClickAway(() => {
    setOpen(false);
  }, SelectRef);

  const handleChange = (str) => {
    if (onChange) {
      onChange(str);
    } else {
      setVal(str);
    }
    setOpen(false);
  };

  useEffect(() => {
    setVal(value);
  }, [value]);

  // 单一子元素是对象  转数组方便处理
  if (children && typeof children === 'object') {
    children = [children];
  }

  const currentLabel = children
    ?.flat(Infinity)
    .filter((option) => option && (option.props.value === undefined ? '' : option.props.value) === val)?.[0]
    ?.props.children;

  const currentTitle = children
    ?.flat(Infinity)
    .filter((option) => option && (option.props.value === undefined ? '' : option.props.value) === val)?.[0]
    ?.props.title;

  return (
    <div className={classnames(className, styles.select)} ref={SelectRef} style={style}>
      <input type="search" className={styles.input} name={name} value={val} title={currentTitle} onChange={() => {}} />
      <div className={styles.text} onClick={() => setOpen(!open)}>
        {currentLabel}
      </div>
      <div className={styles.ab_arrow} onClick={() => setOpen(!open)}>
        {dropIcon(open ? 180 : 0)}
      </div>
      <div className={classnames(styles.option, { [styles.open]: open })}>
        {children?.flat(Infinity).map((child) => {
          if (child) {
            return React.cloneElement(child, {
              className: val === (child.props.value === undefined ? '' : child.props.value) ? styles.checked : '',
              key: child.key === undefined ? child.key : child.props.value,
              onClick: (v) => handleChange(v),
            });
          }
          return null;
        })}
      </div>
    </div>
  );
};

Select.defaultProps = {
  className: '',
  style: {},
  value: '',
  onChange: null,
  name: '',
  children: null,
};

Select.propTypes = {
  className: PropTypes.string,
  style: PropTypes.object,
  name: PropTypes.string,
  value: PropTypes.any,
  onChange: PropTypes.func,
  children: PropTypes.any,
};

export default Select;
