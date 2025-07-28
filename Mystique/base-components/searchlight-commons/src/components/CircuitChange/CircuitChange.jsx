/*
 * @Author: wangXueDong
 * @Date: 2022-02-17 16:10:53
 * @LastEditors: wangXueDong
 * @LastEditTime: 2022-11-01 11:37:03
 */
/* eslint-disable jsx-a11y/mouse-events-have-key-events */
/* eslint-disable max-len */
import React, { useEffect, useMemo, useRef, useState, useLayoutEffect } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import img0 from './images/0.png';
import img1 from './images/1.png';
import img2 from './images/2.png';
import img3 from './images/3.png';
import img4 from './images/4.png';
import img5 from './images/5.png';
import img6 from './images/6.png';
import styles from './CircuitChange.module.less';

const CircuitChange = (props) => {
  const { value, segment } = props;
  const imgArr = [img0, img1, img2, img3, img4, img5, img6];
  return (
    <div className={styles.root}>
      <div className={styles.titleBox}>
        <div className={styles.titler}>
          <div style={{ left: '-15px', transform: 'rotate(-90deg)' }} className={classnames(styles.line)} />
          <div style={{ right: '-15px', transform: 'rotate(90deg)' }} className={classnames(styles.line)} />
          <div style={{ left: '5px', transform: 'rotate(-90deg)' }} className={classnames(styles.border)} />
          <div style={{ right: '5px', transform: 'rotate(90deg)' }} className={classnames(styles.border)} />
          {segment}
          <span>MHz</span>
        </div>
      </div>
      <div className={styles.imgBox}>
        <img alt="" src={imgArr[value]} />
      </div>
    </div>
  );
};

CircuitChange.defaultProps = {
  value: null,
  segment: '',
};

CircuitChange.propTypes = {
  value: PropTypes.number,
  segment: PropTypes.string,
};

export default CircuitChange;
