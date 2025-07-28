import React, { useEffect, useState } from 'react';
import PropTypes from 'prop-types';
import styles from './index.module.less';

const svg = (
  <svg width="12" height="12" viewBox="0 0 12 12" fill="none" xmlns="http://www.w3.org/2000/svg">
    <path d="M2.5 4.49951L9.5 4.49951L7.5 2.99951" stroke="#7580A6" strokeLinecap="round" strokeLinejoin="round" />
    <path d="M9.5 7.49951L2.5 7.49951L4.5 8.99951" stroke="#7580A6" strokeLinecap="round" strokeLinejoin="round" />
  </svg>
);

const TabButton = (props) => {
  const { onChange, option, disabled, state, disabledOptions } = props;
  const [left, setLeft] = useState({
    left: 0,
    width: 0,
  });
  useEffect(() => {
    const doc = window.document.getElementById(`id_${state.key}`);
    setLeft({ left: doc.offsetLeft + 1, width: doc.clientWidth - 2 });
  }, [state]);

  return (
    <div className={styles.tabButton}>
      <span className={styles.container} style={{ opacity: disabled ? '.5' : '' }}>
        {option.map((it) => {
          return (
            <div
              id={`id_${it.key}`}
              key={it.key}
              onClick={() => {
                if (!disabled && state.key !== it.key && !disabledOptions.includes(it.key)) {
                  onChange(it);
                }
              }}
              className={styles.bottomBox}
              style={{
                opacity: disabled || disabledOptions.includes(it.key) ? '.5' : '',
                cursor: disabled || disabledOptions.includes(it.key) ? 'not-allowed' : 'pointer',
              }}
            >
              {it.value}
            </div>
          );
        })}
        <div style={{ left: left.left, width: left.width }} className={styles.tabBox}>
          <div className={styles.icon}>{svg}</div>
          {state.value}
        </div>
      </span>
    </div>
  );
};

TabButton.defaultProps = {
  onChange: () => {},
  state: {},
  disabled: false,
  option: [],
  disabledOptions: [],
};

TabButton.propTypes = {
  onChange: PropTypes.func,
  state: PropTypes.object,
  disabled: PropTypes.bool,
  option: PropTypes.any,
  disabledOptions: PropTypes.array,
};

export default TabButton;
