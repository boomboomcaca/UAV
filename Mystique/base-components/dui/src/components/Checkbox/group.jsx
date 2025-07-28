import React, { useState, useEffect } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';

import styles from './styles.module.less';

const Group = (props) => {
  const { onChange, options, value, valueKey, labelKey, name, optionCls, sort, className, itemClass } = props;

  const [val, setVal] = useState(value);

  const handleChange = (option) => {
    const optionIndex = val.indexOf(option[valueKey]);
    const newValue = [...val];
    if (optionIndex === -1) {
      newValue.push(option[valueKey]);
    } else {
      newValue.splice(optionIndex, 1);
    }
    const newSortValue = sort
      ? newValue.sort((a, b) => {
          const indexA = options.findIndex((opt) => opt[valueKey] === a);
          const indexB = options.findIndex((opt) => opt[valueKey] === b);
          return indexA - indexB;
        })
      : newValue;
    if (onChange) {
      onChange(newSortValue);
    } else {
      setVal(newSortValue);
    }
  };

  useEffect(() => {
    setVal(value);
  }, [value]);

  return (
    <div className={classnames(styles.checkboxGroup, className)}>
      {options.map((option) => (
        <div
          className={classnames(styles.checkboxItem, itemClass, optionCls, {
            [styles.active]: val.includes(option[valueKey]),
          })}
          key={option[valueKey]}
        >
          {option.icon && <span>{option.icon}</span>}
          <input
            type="checkbox"
            className={styles.input}
            name={name}
            value={option[labelKey]}
            checked={val.includes(option[valueKey])}
            onChange={() => handleChange(option)}
          />
          {option[labelKey]}
        </div>
      ))}
    </div>
  );
};

Group.defaultProps = {
  options: [],
  onChange: null,
  value: [],
  name: '',
  valueKey: 'value',
  labelKey: 'label',
  optionCls: '',
  className: '',
  itemClass: '',
  sort: true,
};

Group.propTypes = {
  options: PropTypes.array,
  onChange: PropTypes.func,
  value: PropTypes.arrayOf(PropTypes.string),
  name: PropTypes.string,
  valueKey: PropTypes.string,
  labelKey: PropTypes.string,
  optionCls: PropTypes.string,
  className: PropTypes.string,
  itemClass: PropTypes.string,
  sort: PropTypes.bool,
};

export default Group;
