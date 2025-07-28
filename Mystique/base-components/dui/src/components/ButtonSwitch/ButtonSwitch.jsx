import React from 'react';
import PropTypes from 'prop-types';
import styles from './index.module.less';

const svg = (
  <svg width="12" height="12" viewBox="0 0 12 12" fill="none" xmlns="http://www.w3.org/2000/svg">
    <path d="M2.5 4.49951L9.5 4.49951L7.5 2.99951" stroke="#7580A6" strokeLinecap="round" strokeLinejoin="round" />
    <path d="M9.5 7.49951L2.5 7.49951L4.5 8.99951" stroke="#7580A6" strokeLinecap="round" strokeLinejoin="round" />
  </svg>
);

const ButtonSwitch = (props) => {
  const { onChange, icons, disabled, state } = props;

  return (
    <div
      onClick={() => {
        if (disabled) return null;
        return onChange(state);
      }}
      className={styles.containerButtonSwitch}
      style={{ opacity: disabled ? '.5' : '' }}
    >
      <div className={styles.bottomBox}>{icons[0]}</div>
      <div className={styles.bottomBox}>{icons[1]}</div>
      <div style={{ left: state ? '1px' : '65px' }} className={styles.tabBox}>
        <div className={styles.icon}>{svg}</div>
        {state ? icons[0] : icons[1]}
      </div>
    </div>
  );
};

ButtonSwitch.defaultProps = {
  onChange: () => {},
  state: true,
  disabled: false,
  icons: [],
};

ButtonSwitch.propTypes = {
  onChange: PropTypes.func,
  state: PropTypes.bool,
  disabled: PropTypes.bool,
  icons: PropTypes.any,
};

export default ButtonSwitch;
