import React, { useEffect, useState } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import styles from './index.module.less';

const svg = (
  <svg width="12" height="12" viewBox="0 0 12 12" fill="none" xmlns="http://www.w3.org/2000/svg">
    <path d="M2.5 4.49951L9.5 4.49951L7.5 2.99951" stroke="#7580A6" strokeLinecap="round" strokeLinejoin="round" />
    <path d="M9.5 7.49951L2.5 7.49951L4.5 8.99951" stroke="#7580A6" strokeLinecap="round" strokeLinejoin="round" />
  </svg>
);

const MultipleSwitch = (props) => {
  const { onChange, options, disabled, value } = props;
  const [selectIdx, SetSelectIdx] = useState(0);
  useEffect(() => {
    if (value !== undefined && options.length > 0) {
      let initIdx = -1;
      for (let i = 0; i < options.length; i += 1) {
        if (options[i].value !== undefined && options[i].value === value) {
          initIdx = i;
          break;
        }
      }
      if (initIdx > -1) {
        SetSelectIdx(initIdx);
      }
    }
    return () => {};
  }, []);
  return (
    <div className={styles.containerButtonSwitch} style={{ opacity: disabled ? '.5' : '' }}>
      <div className={styles.switchBox}>
        {options.length > 0 &&
          options.map((item, idx) => (
            <div
              key={`switch-option-${idx + 1}`}
              className={styles.bottomBox}
              onClick={() => {
                if (selectIdx !== idx) {
                  onChange(item);
                  SetSelectIdx(idx);
                }
              }}
            >
              {options[idx].label || ''}
            </div>
          ))}
        <div
          style={{
            width: `${100 / options.length}%`,
            left: !selectIdx ? '2px' : `${(100 / options.length) * selectIdx}%`,
          }}
          className={styles.tabBox}
        >
          <div className={styles.icon}>{svg}</div>
          {options.length > 0 ? options[selectIdx].label : ''}
        </div>
      </div>
    </div>
  );
};

MultipleSwitch.defaultProps = {
  onChange: () => {},
  value: null,
  disabled: false,
  options: [],
};

MultipleSwitch.propTypes = {
  onChange: PropTypes.func,
  value: PropTypes.any,
  disabled: PropTypes.bool,
  options: PropTypes.any,
};

export default MultipleSwitch;
