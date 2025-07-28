/*
 * @Author: wangXueDong
 * @Date: 2022-02-17 16:10:53
 * @LastEditors: wangXueDong
 * @LastEditTime: 2022-04-28 17:57:04
 */
/* eslint-disable jsx-a11y/mouse-events-have-key-events */
/* eslint-disable max-len */
import React, { useEffect, useMemo, useRef, useState } from 'react';
import PropTypes from 'prop-types';
import { useClickAway } from 'ahooks';
import CenterDrop from './components/CenterDrop.jsx';
import styles from './Popover.module.less';

const Popover = (props) => {
  const { children, position, width, content, onVisiableChange, style } = props;
  const [open, setOpen] = useState(false);
  const selectRef = useRef(null);
  const selectBoxRef = useRef(null);
  const dropBoxRef = useRef(null);
  //  点击别的地方蒙版消失
  useClickAway(() => {
    popDisappear();
  }, selectRef);
  const popDisappear = () => {
    setTimeout(() => {
      setOpen(false);
    }, 150);
  };
  useEffect(() => {
    onVisiableChange(open);
  }, [open]);
  return (
    <div style={style} ref={selectRef} className={styles.selectBox}>
      <div
        onClick={(e) => {
          // e.stopPropagation();
          setOpen(!open);
        }}
      >
        {children}
      </div>
      <CenterDrop
        popoverWidth={selectRef?.current?.clientWidth}
        popoverHeight={selectRef?.current?.clientHeight}
        content={content}
        onClose={popDisappear}
        position={position}
        visiable={open}
      />
    </div>
  );
};

Popover.defaultProps = {
  style: {},
  position: 'right',
  width: null,
  children: null,
  content: null,
  onVisiableChange: () => {},
};

Popover.propTypes = {
  onVisiableChange: PropTypes.func,
  position: PropTypes.string,
  width: PropTypes.any,
  children: PropTypes.any,
  content: PropTypes.any,
  style: PropTypes.object,
};

export default Popover;
