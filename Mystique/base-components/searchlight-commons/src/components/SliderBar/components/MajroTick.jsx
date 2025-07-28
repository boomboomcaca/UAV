/*
 * @Author: dengys
 * @Date: 2021-08-24 15:57:52
 * @LastEditors: dengys
 * @LastEditTime: 2022-01-27 15:31:40
 */
import React from 'react';
import PropTypes from 'prop-types';
import styles from './style.module.less';

const MajorTick = (props) => {
  const { label, unit } = props;
  return (
    <div className={styles.newmajorTick}>
      <div
        style={{
          color: 'var(--theme-font-50)',
          flex: '1',
          textAlign: 'right',
          lineHeight: '18px',
        }}
      >
        {label}
      </div>

      <div className={styles.majorTickLine} />

      <div
        style={{
          color: 'var(--theme-font-50)',
          flex: '1',
          textAlign: 'left',
        }}
      >
        {unit}
      </div>
    </div>
  );
};

MajorTick.propTypes = {
  label: PropTypes.string.isRequired,
  unit: PropTypes.string.isRequired,
};

export default MajorTick;
