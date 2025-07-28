/*
 * @Author: wangXueDong
 * @Date: 2022-02-17 16:10:53
 * @LastEditors: wangXueDong
 * @LastEditTime: 2022-10-22 17:01:48
 */
/* eslint-disable jsx-a11y/mouse-events-have-key-events */
/* eslint-disable max-len */
import React, { useEffect, useMemo, useRef, useState, useLayoutEffect } from 'react';
import PropTypes from 'prop-types';
import { AddIcon, FinishIcon, RemoveIcon } from 'dc-icon';
import classnames from 'classnames';
import tip from './images/tip.png';
import styles from './PathSettingAxis.module.less';

const PathSettingAxis = (props) => {
  const { value, onValueChange, content, onMouseEnter, datas, onMove, type } = props;
  const goodDatas = useMemo(() => {
    const arr = [...datas];
    if (arr.length < 8) {
      const xun = 8 - arr.length;
      for (let i = 0; i < xun; i += 1) {
        if (i === 0) {
          arr.push({ state: 'can' });
        } else {
          arr.push({ state: 'none' });
        }
      }
    }
    return arr;
  }, [datas]);
  return (
    <div className={styles.root}>
      {goodDatas.map((e, index) => (
        // eslint-disable-next-line react/no-array-index-key
        <div key={index} style={{ flex: index !== 0 ? 1 : 'none' }} className={styles.itemBox}>
          {/* 轴条 */}
          {index !== 0 ? (
            <div
              className={classnames(
                styles.line,
                e.state === 'done' || index === value ? styles.online : styles.offline,
              )}
            />
          ) : null}
          {/* 球 */}
          <div className={styles.ballBox}>
            <div
              onClick={() => {
                (e.state === 'done' || (e.state === 'can' && type !== 'readOnly')) && onValueChange({ ...e, index });
              }}
              className={classnames(
                styles.ball,
                e.state === 'done' || (e.state === 'can' && value === index)
                  ? value === index
                    ? styles.onball_sel
                    : styles.onball_nosel
                  : e.state === 'can' && type !== 'readOnly'
                  ? styles.canball
                  : styles.offball,
              )}
            >
              {e.state === 'can' && value === index ? null : e.state === 'done' ? (
                <FinishIcon color={value === index ? '#353D5B' : '#FFFFFF'} iconSize={20} />
              ) : (
                type !== 'readOnly' && <AddIcon color={e.state === 'can' ? '#3CE5D3' : 'rgba(255, 255, 255, 0.2)'} />
              )}
            </div>
            {e.state === 'can' && index === 0 && value !== index && (
              <div className={styles.tipBox}>
                <img alt="" src={tip} />
                请点击这里添加模式
              </div>
            )}

            {/* 标签 */}
            {(e.state === 'done' || (e.state === 'can' && value === index)) && (
              <div
                style={{
                  width: e.state === 'can' && value === index ? '106px' : 'max-content',
                  top: value === index ? (index % 2 === 0 ? '-33px' : '25px') : index % 2 === 0 ? '-24px' : '24px',
                  color: '#fff',
                }}
                className={classnames(styles.lable, value === index ? styles.lable_sel : styles.lable_nosel)}
              >
                {e.name || '--'}
                {e.state === 'done' && type !== 'readOnly' && (
                  <div
                    onClick={() => {
                      onValueChange({});
                      onMove(index);
                    }}
                    className={styles.removeBtn}
                  >
                    <RemoveIcon color="#FFFFFF" />
                  </div>
                )}
              </div>
            )}
          </div>
        </div>
      ))}
    </div>
  );
};

PathSettingAxis.defaultProps = {
  value: null,
  onValueChange: () => {},
  content: null,
  onMouseEnter: () => {},
  datas: [],
  onMove: () => {},
  type: 'setting',
};

PathSettingAxis.propTypes = {
  value: PropTypes.number,
  onValueChange: PropTypes.func,
  content: PropTypes.any,
  onMouseEnter: PropTypes.func,
  datas: PropTypes.array,
  onMove: PropTypes.func,
  type: PropTypes.string,
};

export default PathSettingAxis;
