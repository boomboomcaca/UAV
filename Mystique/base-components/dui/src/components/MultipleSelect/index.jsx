import React, { useState, useRef, useEffect } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { CaretDownOutlined, CloseOutlined } from '@ant-design/icons';
import { useClickAway } from 'ahooks';
import styles from './styles.module.less';

const Select = (props) => {
  const { className, value, name, onChange, children, style } = props;
  const [val, setVal] = useState(value);
  const [open, setOpen] = useState(false);
  const SelectRef = useRef(null);

  useClickAway(() => {
    setOpen(false);
  }, SelectRef);

  const handleChange = (str) => {
    const optionIndex = val.indexOf(str);
    const newValue = [...val];
    if (optionIndex === -1) {
      newValue.push(str);
    } else {
      newValue.splice(optionIndex, 1);
    }
    const newSortValue = newValue.sort((a, b) => {
      const indexA = children.findIndex((opt) => opt.props.value === a);
      const indexB = children.findIndex((opt) => opt.props.value === b);
      return indexA - indexB;
    });
    if (onChange) {
      onChange(newSortValue);
    } else {
      setVal(newSortValue);
    }
  };

  const delLabel = (e, str) => {
    e.stopPropagation();
    const optionIndex = val.indexOf(str);
    const newValue = [...val];
    newValue.splice(optionIndex, 1);
    if (onChange) {
      onChange(newValue);
    } else {
      setVal(newValue);
    }
  };

  useEffect(() => {
    setVal(value);
  }, [value]);

  return (
    <div className={classnames(className, styles.select)} ref={SelectRef} style={style}>
      <input type="search" className={styles.input} name={name} value={val} onChange={() => {}} />
      <div className={styles.text} onClick={() => setOpen(!open)}>
        {children
          .filter((item) => val.includes(item.props.value))
          .map((child) => {
            return (
              <div key={child.key || child.props.value} className={styles.label}>
                <span>{child.props.children}</span>
                {open && <CloseOutlined className={styles.close} onClick={(e) => delLabel(e, child.props.value)} />}
              </div>
            );
          })}
      </div>
      <CaretDownOutlined
        onClick={() => setOpen(!open)}
        className={classnames(styles.ab_arrow, { [styles.trun]: open })}
      />
      <div className={classnames(styles.option, { [styles.open]: open })}>
        {children.map((child) => {
          return React.cloneElement(child, {
            className: val === child.props.value ? styles.checked : '',
            key: child.key || child.props.value,
            onClick: (v) => handleChange(v),
          });
        })}
      </div>
    </div>
  );
};

Select.defaultProps = {
  className: '',
  value: [],
  onChange: null,
  name: '',
  style: {},
  children: null,
};

Select.propTypes = {
  className: PropTypes.string,
  style: PropTypes.object,
  children: PropTypes.any,
  name: PropTypes.string,
  value: PropTypes.arrayOf(PropTypes.string),
  onChange: PropTypes.func,
};

export default Select;
