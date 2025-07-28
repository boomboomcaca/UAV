import React, { useState, useEffect } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';

import styles from './styles.module.less';

const Checkbox = (props) => {
  const { checked, children, onChange, name, className } = props;

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
      className={classnames(styles.checkboxItem, className, {
        [styles.active]: val,
      })}
    >
      <input
        type="checkbox"
        className={styles.input}
        name={name}
        value={children}
        checked={val}
        onChange={(e) => handleChange(e.target.checked)}
      />
      {children}
    </div>
  );
};

Checkbox.defaultProps = {
  children: null,
  name: '',
  checked: false,
  onChange: null,
  className: '',
  custom: false,
};

Checkbox.propTypes = {
  children: PropTypes.any,
  checked: PropTypes.bool,
  name: PropTypes.string,
  onChange: PropTypes.func,
  className: PropTypes.string,
  custom: PropTypes.bool,
};

export default Checkbox;
