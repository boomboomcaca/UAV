import React, { useState, useEffect } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';

import styles from './styles.module.less';

const TraditionalCheckbox = (props) => {
  const { checked, children, onChange, name, className, disabled } = props;

  const [val, setVal] = useState(checked);

  const handleChange = (bl) => {
    if (onChange) {
      onChange(bl);
    } else {
      setVal(bl);
    }
  };

  useEffect(() => {
    setVal(checked);
  }, [checked]);

  return (
    <div
      className={classnames(styles.traditionalnew, className, {
        [styles.disabled]: disabled,
      })}
    >
      <input
        type="checkbox"
        className={styles.input}
        name={name}
        disabled={disabled}
        value={children}
        checked={val}
        onChange={(e) => handleChange(e.target.checked)}
      />
      {val ? (
        <svg width="16" height="16" viewBox="0 0 16 16" fill="none" xmlns="http://www.w3.org/2000/svg">
          <rect width="16" height="16" rx="2" fill="#3CE5D3" />
          <path
            d="M12 5L6.74795 11L4 7.86779"
            stroke="#353D5B"
            strokeWidth="1.5"
            strokeLinecap="round"
            strokeLinejoin="round"
          />
        </svg>
      ) : (
        <div className={styles.kuang} />
      )}
      {children}
    </div>
  );
};

TraditionalCheckbox.defaultProps = {
  children: null,
  name: '',
  checked: false,
  disabled: false,
  onChange: null,
  className: '',
};

TraditionalCheckbox.propTypes = {
  children: PropTypes.any,
  checked: PropTypes.bool,
  disabled: PropTypes.bool,
  name: PropTypes.string,
  onChange: PropTypes.func,
  className: PropTypes.string,
};

export default TraditionalCheckbox;
