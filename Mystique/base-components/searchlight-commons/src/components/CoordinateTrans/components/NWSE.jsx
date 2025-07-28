import React, { useEffect, useState } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { Input } from 'dui';
import { degree2DMS, gps2Degree } from '../weapon';
import styles from './nwse.module.less';

const NWSE = (props) => {
  const { className, maximum, minimum, value, units, onChange } = props;

  const [text, setText] = useState('');

  useEffect(() => {
    if (value !== null && value !== '') {
      const dir = value < 0 ? units[0] : units[1];
      setText(degree2DMS(value.toFixed(6), dir));
    } else {
      setText('');
    }
  }, [value]);

  const onTextChange = (txt) => {
    setText(txt);
    const [dir, val] = gps2Degree(txt);
    const idx = units.indexOf(dir);
    let num = (idx === 0 ? -1 : 1) * val;
    if (num > maximum) num = maximum;
    if (num < minimum) num = minimum;
    onChange(num);
  };

  return (
    <div className={classnames(styles.root, className)}>
      <Input style={{ width: 264, marginRight: 5 }} placeholder="N DD°MM′SS″" value={text} onChange={onTextChange} />
    </div>
  );
};

NWSE.defaultProps = {
  className: null,
  maximum: 90,
  minimum: -90,
  value: null,
  units: ['-', '+'],
  onChange: () => {},
};

NWSE.propTypes = {
  className: PropTypes.any,
  maximum: PropTypes.any,
  minimum: PropTypes.any,
  value: PropTypes.any,
  units: PropTypes.any,
  onChange: PropTypes.func,
};

export default NWSE;
