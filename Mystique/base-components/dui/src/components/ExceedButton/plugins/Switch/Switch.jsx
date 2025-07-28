/* eslint-disable max-len */
import React from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import styles from './index.module.less';

const Switch = (props) => {
  const { className } = props;

  return (
    <div className={classnames(styles.root, className)}>
      <div className={styles.svg}>
        <svg
          xmlns="http://www.w3.org/2000/svg"
          viewBox="0 0 14 6"
          fill="none"
          style={{ transform: 'scale(1.25)', position: 'absolute' }}
        >
          <path
            d="M4.92727 5H3C1.89543 5 1 4.10457 1 3V3C1 1.89543 1.89543 1 3 1H4.27273H6.89091V1C6.92961 1 6.94811 1.04756 6.91958 1.07372L5.69091 2.2"
            stroke="var(--theme-font-100)"
            strokeLinecap="round"
            strokeLinejoin="round"
          />
          <path
            d="M2.72723 1H4.54541C5.64998 1 6.54541 1.89543 6.54541 3V3C6.54541 4.10457 5.64998 5 4.54541 5H3.27268H0.654488V5C0.621218 5 0.607879 4.95705 0.635295 4.9382L2.29088 3.8"
            stroke="var(--theme-font-100)"
            strokeOpacity="0.5"
            strokeLinecap="round"
            strokeLinejoin="round"
            transform="translate(6.25 0)"
          />
        </svg>
      </div>
    </div>
  );
};

Switch.defaultProps = {
  className: null,
};

Switch.propTypes = {
  className: PropTypes.any,
};

export default Switch;
