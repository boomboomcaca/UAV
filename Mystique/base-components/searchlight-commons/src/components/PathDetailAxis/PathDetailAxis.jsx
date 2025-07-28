/*
 * @Author: wangXueDong
 * @Date: 2022-02-17 16:10:53
 * @LastEditors: wangXueDong
 * @LastEditTime: 2022-11-23 11:54:21
 */
/* eslint-disable jsx-a11y/mouse-events-have-key-events */
/* eslint-disable max-len */
import React, { useEffect, useMemo, useRef, useState, useLayoutEffect, useCallback } from 'react';
import { useUpdateEffect } from 'ahooks';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { ArrowLeftIcon, ArrowRightIcon } from 'dc-icon';
import Pager from './components/Pager.jsx';
import Loading from './images/loading.png';
import styles from './PathDetailAxis.module.less';

const PathDetailAxis = (props) => {
  const { value, onValueChange, datas } = props;
  const axisRef = useRef(null);
  const rootRef = useRef(null);
  const rootConRef = useRef(null);
  const itemRef = useRef(null);
  const [judgeLength, setJudgelength] = useState(1);
  const [currentIndex, setCurrentIndex] = useState(0);
  // useLayoutEffect(() => {
  //   const num = datas.findIndex((e) => e.state === 1);
  //   onValueChange(datas[num]);
  // }, []);
  function useDebounceCallback(fn, ms, deps) {
    const timerRef = useRef(null);
    const funcRef = useRef(null);
    useEffect(() => {
      clearTimeout(timerRef?.current);
      funcRef.current = null;
    }, []);
    return useMemo(() => {
      funcRef.current = fn;
      return (...args) => {
        clearTimeout(timerRef?.current);
        timerRef.current = setTimeout(() => funcRef?.current?.(...args), ms);
      };
    });
  }
  const handleResize = useDebounceCallback(
    () => {
      if (!rootConRef.current || !itemRef.current) {
        return;
      }
      //  存在clientWidth精确度问题，误差在0-1px之间
      const length =
        Math.abs(
          itemRef.current.clientWidth * (datas.length - 1) + 24 + 90 - datas.length - rootConRef.current.clientWidth,
        ) <= 3
          ? 1
          : Math.ceil(
              (itemRef.current.clientWidth * (datas.length - 1) + 24 + 90 - datas.length) /
                rootConRef.current.clientWidth,
            );
      setJudgelength(length);
    },
    200,
    [rootConRef, itemRef],
  );
  useEffect(() => {
    handleResize();
    window.addEventListener('resize', handleResize);
    return () => {
      window.removeEventListener('resize', handleResize);
    };
  }, []);
  useUpdateEffect(() => {
    handleResize();
  }, [datas]);
  const toMove = (type) => {
    let index = type === 'left' ? currentIndex - 1 : currentIndex + 1;
    index = index < 0 ? 0 : index > judgeLength - 1 ? judgeLength - 1 : index;
    setCurrentIndex(index);
  };
  const moveLength = useMemo(() => {
    let distance = 0;
    if (rootConRef.current && axisRef.current) {
      const rootConWidth = rootConRef.current.clientWidth;
      const axisWidth = itemRef.current.clientWidth * (datas.length - 1) + 24 + 90 - datas.length;

      if (currentIndex < judgeLength - 1) {
        distance = currentIndex * rootConWidth;
      } else {
        // 计算有误
        distance = axisWidth - rootConWidth;
      }
    }
    return distance;
  }, [currentIndex, judgeLength]);
  const onPangeChange = (e) => {
    setCurrentIndex(e);
  };
  return (
    <div ref={rootRef} className={styles.root}>
      {judgeLength > 1 ? (
        <div
          onClick={() => toMove('left')}
          style={{ cursor: currentIndex === 0 ? 'not-allowed' : 'pointer' }}
          className={styles.leftArrow}
        >
          <ArrowLeftIcon color={currentIndex === 0 ? '#3CE5D380' : '#3CE5D3'} />
        </div>
      ) : null}

      <div
        style={{ width: judgeLength > 1 ? 'calc(100% - 48px)' : '100%' }}
        ref={rootConRef}
        className={styles.rootCon}
      >
        <div
          style={{
            transform: `translateX(-${moveLength}px)`,
            // width: isLess ? '100%' : 'max-content',
            // boxSizing: isLess ? 'border-box' : 'unset',
          }}
          ref={axisRef}
          className={styles.axis}
        >
          {datas.map((e, index) => (
            <div
              ref={index === 0 ? itemRef : null}
              key={e.id}
              style={{ flex: index !== 0 ? 1 : 'none' }}
              className={styles.itemBox}
            >
              {/* 轴条 */}
              {index !== 0 ? (
                <div
                  className={classnames(styles.line, e.state === 2 || e.state === 1 ? styles.online : styles.offline)}
                />
              ) : null}
              {/* 球 */}
              {e.state === 1 ? (
                <div className={styles.ballBox}>
                  <div
                    onClick={() => {
                      onValueChange(e);
                    }}
                    className={classnames(styles.runBall, value === e.id ? styles.runBall_sel : styles.runBall_nosel)}
                  >
                    <img className={styles.loadingImg} alt="" src={Loading} />
                  </div>
                  {/* 标签 */}
                  <div
                    style={{
                      top: value === e.id ? (index % 2 === 0 ? '-34px' : '22px') : index % 2 === 0 ? '-24px' : '24px',
                    }}
                    className={classnames(
                      styles.runLable,
                      value === e.id ? styles.runLable_sel : styles.runLable_nosel,
                    )}
                  >
                    {e.label}
                    <span>MHz</span>
                  </div>
                </div>
              ) : (
                <div className={styles.ballBox}>
                  <div
                    onClick={() => {
                      e.state !== -1 && onValueChange(e);
                    }}
                    className={classnames(
                      styles.ball,
                      e.state === 2
                        ? value === e.id
                          ? styles.onball_sel
                          : styles.onball_nosel
                        : value === e.id
                        ? styles.offball_sel
                        : styles.offball_nosel,
                      e.state === -1 ? styles.disable : null,
                    )}
                  />
                  {/* 标签 */}
                  {e.label && (
                    <div
                      style={{
                        top: value === e.id ? (index % 2 === 0 ? '-30px' : '22px') : index % 2 === 0 ? '-20px' : '24px',
                        color: e.state === 2 ? '#fff' : 'rgba(255, 255, 255, 0.5)',
                      }}
                      className={classnames(styles.lable, value === e.id ? styles.lable_sel : styles.lable_nosel)}
                    >
                      {e.label}
                      <span>MHz</span>
                    </div>
                  )}
                </div>
              )}
            </div>
          ))}
        </div>
        {judgeLength > 1 && <Pager onValueChange={(e) => onPangeChange(e)} pages={judgeLength} value={currentIndex} />}
      </div>
      {judgeLength > 1 ? (
        <div
          style={{ cursor: currentIndex + 1 === judgeLength ? 'not-allowed' : 'pointer' }}
          onClick={() => toMove('right')}
          className={styles.rightArrow}
        >
          <ArrowRightIcon color={currentIndex + 1 === judgeLength ? '#3CE5D380' : '#3CE5D3'} />
        </div>
      ) : null}
    </div>
  );
};

PathDetailAxis.defaultProps = {
  value: null,
  onValueChange: () => {},
  datas: [],
};

PathDetailAxis.propTypes = {
  value: PropTypes.number,
  onValueChange: PropTypes.func,
  datas: PropTypes.array,
};

export default PathDetailAxis;
