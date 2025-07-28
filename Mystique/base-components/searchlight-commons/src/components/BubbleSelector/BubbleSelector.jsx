/*
 * @Author: wangXueDong
 * @Date: 2022-02-17 16:10:53
 * @LastEditors: wangXueDong
 * @LastEditTime: 2022-03-21 17:22:11
 */
/* eslint-disable jsx-a11y/mouse-events-have-key-events */
/* eslint-disable max-len */
import React, { useEffect, useMemo, useRef, useState } from 'react';
import PropTypes from 'prop-types';
import { useClickAway } from 'ahooks';
import dropIcon from './dropIcon.jsx';
import CenterDrop from './components/CenterDrop.jsx';
import styles from './BubbleSelector.module.less';

const BubbleSelector = (props) => {
  // 2022-8-30 liujian 增加 onShow 事件
  const { value, dataSource, position, onValueChange, disable, width, keyBoardType, onShow } = props;
  const [open, setOpen] = useState(false);
  const [selIndx, setSelIndx] = useState(0);
  const selectRef = useRef(null);
  const selectBoxRef = useRef(null);
  const dropBoxRef = useRef(null);
  const onChange = (e) => {
    const item = dataSource.find((d) => d.value === e.value);
    onValueChange(item);
  };
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
    const indx = dataSource.findIndex((e) => e.value === value) || 0;
    setSelIndx(indx);
  }, [value, dataSource]);

  const useWidth = (num) => {
    const style = {
      justifyContent: 'space-between',
      width: `${num}px`,
    };
    return style;
  };

  useEffect(() => {
    if (open && onShow) {
      onShow();
    }
  }, [open]);

  return (
    <div ref={selectRef} className={styles.selectBox}>
      <div
        onClick={() => {
          if (!disable) {
            setOpen(!open);
          }
        }}
        className={styles.selectBoxCon}
        style={width ? useWidth(width) : {}}
      >
        <div className={styles.selectText}>{dataSource[selIndx]?.display}</div>
        {dropIcon(open ? 180 : 0)}
      </div>
      <CenterDrop
        onClose={popDisappear}
        dataSource={dataSource}
        position={position}
        keyBoardType={keyBoardType}
        visiable={open}
        value={value}
        onChange={(e) => onChange(e)}
      />
    </div>
  );
};

BubbleSelector.defaultProps = {
  dataSource: [],
  value: 0,
  position: 'right',
  onValueChange: () => {},
  onShow: () => {},
  disable: false,
  width: null,
  keyBoardType: 'complex',
};

BubbleSelector.propTypes = {
  dataSource: PropTypes.array,
  value: PropTypes.number,
  position: PropTypes.string,
  onValueChange: PropTypes.func,
  disable: PropTypes.bool,
  width: PropTypes.any,
  keyBoardType: PropTypes.string,
  onShow: PropTypes.func,
};

export default BubbleSelector;
