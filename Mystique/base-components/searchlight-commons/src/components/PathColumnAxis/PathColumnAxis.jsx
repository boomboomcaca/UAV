/*
 * @Author: wangXueDong
 * @Date: 2022-02-17 16:10:53
 * @LastEditors: wangXueDong
 * @LastEditTime: 2022-10-28 16:45:55
 */
/* eslint-disable jsx-a11y/mouse-events-have-key-events */
/* eslint-disable max-len */
import React, { useEffect, useMemo, useRef, useState, useLayoutEffect } from 'react';
import PropTypes from 'prop-types';
import { PlayIcon } from 'dc-icon';
import classnames from 'classnames';
import Loading from './images/loading.png';
import styles from './PathColumnAxis.module.less';

const PathColumnAxis = (props) => {
  const { value, onValueChange, datas } = props;
  // useLayoutEffect(() => {
  //   const num = datas.findIndex((e) => e.state === 1);
  //   onValueChange(datas[num]);
  // }, []);
  return (
    <div className={styles.root}>
      {datas.map((e, index) => (
        <div key={e.id} style={{ flex: datas.length > index + 1 ? 1 : 'none' }} className={styles.itemBox}>
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
              {e.label && (
                <div
                  style={{
                    right: value === e.id ? '28px' : '24px',
                  }}
                  className={classnames(styles.runLable, value === e.id ? styles.runLable_sel : styles.runLable_nosel)}
                >
                  {e.label}
                </div>
              )}

              {/* 三角形箭头 */}
              {value === e.id ? (
                <div className={styles.arrow}>
                  <PlayIcon iconSize={22} color="#3CE5D3" />
                </div>
              ) : null}
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
                    right: value === e.id ? '28px' : '24px',
                    color: e.state === 2 ? '#fff' : 'rgba(255, 255, 255, 0.5)',
                  }}
                  className={classnames(styles.lable, value === e.id ? styles.lable_sel : styles.lable_nosel)}
                >
                  {e.label}
                </div>
              )}

              {/* 三角形箭头 */}
              {value === e.id ? (
                <div className={styles.arrow}>
                  <PlayIcon iconSize={22} color="#3CE5D3" />
                </div>
              ) : null}
            </div>
          )}

          {/* 轴条 */}
          {datas.length > index + 1 ? (
            <div className={classnames(styles.line, e.state === 2 ? styles.online : styles.offline)} />
          ) : null}
        </div>
      ))}
    </div>
  );
};

PathColumnAxis.defaultProps = {
  value: null,
  onValueChange: () => {},
  datas: [],
};

PathColumnAxis.propTypes = {
  value: PropTypes.number,
  onValueChange: PropTypes.func,
  datas: PropTypes.array,
};

export default PathColumnAxis;
