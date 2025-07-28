import React, { useState, useEffect } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';

import styles from './styles.module.less';

const Radio = (props) => {
  const { className, options, value, onChange, valueKey, labelKey, name, theme, disabled } = props;

  const [val, setVal] = useState(value);

  const handleChange = (vvvv) => {
    if (onChange) {
      onChange(vvvv);
    } else {
      setVal(vvvv);
    }
  };

  const kzS = (vvvv) => {
    if (vvvv === val) {
      if (onChange) {
        onChange('');
      } else {
        setVal('');
      }
    }
  };

  useEffect(() => {
    setVal(value);
  }, [value]);

  return (
    <div className={classnames(className, styles.radioNew)}>
      {options.map((option) => {
        const vvv = option[valueKey] === undefined ? option : option[valueKey];
        const lll = option[labelKey] === undefined ? option : option[labelKey];
        return (
          <div
            className={classnames(
              theme === 'line' ? styles.radioLineItem : styles.radioItem,
              theme === 'highLight' ? styles.highLightItem : styles.radioItem,
              {
                [theme === 'line' ? styles.lineActive : theme === 'highLight' ? styles.highLightActive : styles.active]:
                  val === vvv,
                [styles.disabled]: option.disabled || disabled,
              },
            )}
            key={vvv}
          >
            <input
              type="radio"
              className={styles.input}
              name={name}
              disabled={option.disabled || disabled}
              value={vvv}
              checked={val === vvv}
              onClick={() => kzS(vvv)}
              onChange={() => handleChange(vvv)}
            />
            <span>{lll}</span>
          </div>
        );
      })}
    </div>
  );
};

Radio.defaultProps = {
  options: [],
  className: '',
  value: '',
  name: '',
  onChange: null,
  valueKey: 'value',
  labelKey: 'label',
  theme: '',
  disabled: false,
};

Radio.propTypes = {
  options: PropTypes.array,
  className: PropTypes.string,
  value: PropTypes.any,
  name: PropTypes.string,
  onChange: PropTypes.func,
  valueKey: PropTypes.string,
  labelKey: PropTypes.string,
  theme: PropTypes.string,
  disabled: PropTypes.bool,
};

export default Radio;
