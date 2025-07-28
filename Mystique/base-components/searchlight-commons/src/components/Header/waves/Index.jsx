/*
 * @Author: XYQ
 * @Date: 2022-02-22 13:47:00
 * @LastEditors: XYQ
 * @LastEditTime: 2022-02-25 10:49:06
 * @Description: file content
 */
import React from 'react';
import PropTypes from 'prop-types';
import styles from './style.module.less';

export default function waves(props) {
  const { number, color, style } = props;

  const sty = {
    top: `${100 - number}%`,
    background: `${number > 60 ? '#FF4C2B' : color || ''}`,
  };
  return (
    <div style={style}>
      <div className={styles.hrmor_waves}>
        <div className={styles.hrmor_waves_wave1} style={sty} />
        <div className={styles.hrmor_waves_wave2} style={sty} />
        <div className={styles.hrmor_waves_wave3} style={sty} />
      </div>
    </div>
  );
}

waves.propTypes = {
  title: PropTypes.string,
  color: PropTypes.string,
  number: PropTypes.number,
  style: PropTypes.object,
};

waves.defaultProps = {
  number: 15,
  color: '#40a9ff',
  style: {},
};
