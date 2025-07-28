import React, { useState } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import styles from './index.module.less';

const Selection = (props) => {
  const { value, onValueChange } = props;
  const [dropdown, setDropdown] = useState(false);
  return (
    <div
      className={styles.root}
      onClick={() => {
        setDropdown((prev) => {
          return !prev;
        });
      }}
    >
      <div className={styles.content}>{value === 1 ? '1x' : '2x'}</div>
      <div className={styles.icon} style={dropdown ? { transform: 'rotate(180deg)' } : null}>
        <svg width="10" height="8" viewBox="0 0 10 8" fill="none" xmlns="http://www.w3.org/2000/svg">
          <path
            d="M4.17801 1.18645C4.57569 0.61244 5.42431 0.612441 5.82199 1.18645L9.10877 5.93051C9.56825 6.59371 9.0936 7.5 8.28678 7.5L1.71322 7.5C0.906401 7.5 0.431746 6.59371 0.891226 5.93051L4.17801 1.18645Z"
            fill="white"
          />
        </svg>
      </div>
      <div className={classnames(styles.dropdown, dropdown ? styles.dropshow : null)}>
        <div
          className={value === 1 ? styles.dropselect : styles.dropitem}
          onClick={(e) => {
            onValueChange(1);
            // e.stopPropagation();
          }}
        >
          <div>1x</div>
        </div>
        <div
          className={value === 2 ? styles.dropselect : styles.dropitem}
          onClick={(e) => {
            onValueChange(2);
            // e.stopPropagation();
          }}
        >
          <div>2x</div>
        </div>
      </div>
    </div>
  );
};

Selection.defaultProps = {
  value: 1,
  visible: true,
  onValueChange: () => {},
};

Selection.propTypes = {
  value: PropTypes.any,
  visible: PropTypes.bool,
  onValueChange: () => {},
};

export default Selection;
