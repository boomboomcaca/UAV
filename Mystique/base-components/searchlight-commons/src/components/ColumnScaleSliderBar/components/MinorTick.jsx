/*
 * @Author: dengys
 * @Date: 2021-08-24 15:57:52
 * @LastEditors: wangXueDong
 * @LastEditTime: 2022-03-11 16:01:45
 */
import React from 'react';
import PropTypes from 'prop-types';
import styles from './style.module.less';

const MinorTick = (props) => {
  const { value, label, isLabel } = props;
  return (
    <div className={styles.minorTick}>
      {value && <div className={styles.minorTickVal}>{isLabel ? label : value}</div>}

      <div className={styles.minorTickLine} />
    </div>
  );
};
MinorTick.defaultProps = {
  value: undefined,
  label: undefined,
  isLabel: false,
};

MinorTick.propTypes = {
  value: PropTypes.any,
  label: PropTypes.any,
  isLabel: PropTypes.bool,
};
export default MinorTick;
