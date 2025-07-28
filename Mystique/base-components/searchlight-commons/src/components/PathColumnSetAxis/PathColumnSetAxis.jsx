/*
 * @Author: wangXueDong
 * @Date: 2022-02-17 16:10:53
 * @LastEditors: wangXueDong
 * @LastEditTime: 2022-11-05 17:18:15
 */
/* eslint-disable jsx-a11y/mouse-events-have-key-events */
/* eslint-disable max-len */
import React, { useEffect, useMemo, useRef, useState, useLayoutEffect } from 'react';
import PropTypes from 'prop-types';
import { PlayIcon, AddIcon, FinishIcon, RemoveIcon } from 'dc-icon';
import classnames from 'classnames';
import styles from './PathColumnSetAxis.module.less';

const PathColumnSetAxis = (props) => {
  const { value, onValueChange, datas, onValuesChange } = props;
  // useLayoutEffect(() => {
  //   const num = datas.findIndex((e) => e.state === 1);
  //   onValueChange(datas[num]);
  // }, []);
  const onMove = (e) => {
    onValuesChange(e, 'cut');
  };
  const onClickBall = (e) => {
    if (e.disable) {
      return;
    }
    if (!e.select) {
      onValuesChange(e, 'add');
    }
    onValueChange(e);
  };
  return (
    <div className={styles.root}>
      {datas.map((e, index) => (
        <div key={e.id} style={{ flex: index !== 0 ? 1 : 'none' }} className={styles.itemBox}>
          {/* 轴条 */}
          {index !== 0 ? (
            <div className={classnames(styles.line, !e.disable ? styles.online : styles.offline)} />
          ) : null}
          {/* 球 */}
          <div className={styles.ballBox}>
            <div
              onClick={() => {
                onClickBall(e);
              }}
              className={classnames(
                styles.ball,
                e.disable ? styles.offball_nosel : value === e.id ? styles.onball_sel : styles.onball_nosel,
                e.disable ? styles.disable : null,
              )}
            >
              {e.select && !e.disable ? (
                <FinishIcon color={value === e.id ? '#181C36' : '#FFFFFF'} iconSize={20} />
              ) : (
                !e.select && !e.disable && <AddIcon color={value === e.id ? '#181C36' : '#FFFFFF'} />
              )}
            </div>
            {/* 标签 */}
            {!e.disable && (
              <div
                style={{
                  right: value === e.id ? '28px' : '24px',
                  color: value === e.id || e.select ? '#fff' : 'rgba(255, 255, 255, 0.5)',
                  fontWeight: value === e.id ? 'bold' : '500',
                  border: value === e.id ? '2px solid #3ce5d3' : e.select ? '1px solid #35E065' : 'unset',
                  borderRadius: value === e.id || e.select ? '8px' : '4px',
                }}
                className={classnames(styles.lable, value === e.id || e.select ? styles.lable_sel : styles.lable_nosel)}
              >
                {e.label}
                {e.select && (
                  <div
                    onClick={() => {
                      onMove(e);
                    }}
                    className={styles.removeBtn}
                  >
                    <RemoveIcon color="#FFFFFF" />
                  </div>
                )}
              </div>
            )}
            {/* 三角形箭头 */}
            {value === e.id ? (
              <div className={styles.arrow}>
                <PlayIcon iconSize={22} color="#3CE5D3" />
              </div>
            ) : null}
          </div>
        </div>
      ))}
    </div>
  );
};

PathColumnSetAxis.defaultProps = {
  value: null,
  onValueChange: () => {},
  onValuesChange: () => {},
  datas: [],
};

PathColumnSetAxis.propTypes = {
  value: PropTypes.number,
  onValueChange: PropTypes.func,
  onValuesChange: PropTypes.func,
  datas: PropTypes.array,
};

export default PathColumnSetAxis;
