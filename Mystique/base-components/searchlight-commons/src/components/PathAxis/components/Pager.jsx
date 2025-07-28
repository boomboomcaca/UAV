/*
 * @Author: wangXueDong
 * @Date: 2022-02-17 16:10:53
 * @LastEditors: wangXueDong
 * @LastEditTime: 2022-10-21 11:07:28
 */
/* eslint-disable jsx-a11y/mouse-events-have-key-events */
/* eslint-disable max-len */
import React, { useEffect, useMemo, useRef, useState, useLayoutEffect } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import styles from './pager.module.less';

const Pager = (props) => {
  const { value, onValueChange, pages } = props;
  const toArr = (num) => {
    const arr = [];
    for (let a = 0; a < num; a += 1) {
      arr.push(a);
    }
    return arr;
  };
  return (
    <div className={styles.root}>
      {toArr(pages).map((e) => (
        <div
          onClick={() => onValueChange(e)}
          className={classnames(styles.circle, value === e ? styles.on : styles.off)}
        />
      ))}
    </div>
  );
};

Pager.defaultProps = {
  value: 0,
  onValueChange: () => {},
  pages: 0,
};

Pager.propTypes = {
  value: PropTypes.number,
  onValueChange: PropTypes.func,
  pages: PropTypes.number,
};

export default Pager;
