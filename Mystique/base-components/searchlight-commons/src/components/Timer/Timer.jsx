/*
 * @Author: XYQ
 * @Date: 2022-06-07 10:22:22
 * @LastEditors: XYQ
 * @LastEditTime: 2022-06-07 17:38:06
 * @Description: file content
 */
import React, { useEffect, useState, useRef } from 'react';
import PropTypes from 'prop-types';
import { useInterval, useSetState, useSize } from 'ahooks';
import dayjs from 'dayjs';
import styles from './Timer.module.less';

const Timer = (props) => {
  const { time, onChange, onStart, initialize } = props;
  const [state, setState] = useState(undefined);
  const [ss, setS] = useState(undefined);
  const [key, setKey] = useState(initialize);
  const [timeData, setTime] = useSetState({
    hour: null,
    s: null,
    w: 0,
    w1: 0,
  });
  const ref = useRef();
  const size = useSize(ref);

  useEffect(() => {
    return () => {};
  }, []);

  useEffect(() => {
    const { h, m } = time;
    if (h * 1 === 0 && m * 1 === 0) return;
    setState(dayjs().hour(0).minute(0).second(0).format('YYYY-MM-DD HH:mm:ss'));
  }, [time]);
  /**
   * 启动、停止
   */
  useEffect(() => {
    if (onStart) {
      // setKey(Math.round());
      const { h, m } = time;
      if (h * 1 === 0 && m * 1 === 0) return onChange(false);
      setState(dayjs().hour(0).minute(0).second(0).format('YYYY-MM-DD HH:mm:ss'));
      setS(1000);
      onChange(true);
    } else {
      setS(undefined);
    }
  }, [onStart]);
  /**
   * 初始化
   */
  useEffect(() => {
    const { h, m } = time;
    if (h * 1 === 0 && m * 1 === 0) return;
    setState(dayjs().hour(0).minute(0).second(0).format('YYYY-MM-DD HH:mm:ss'));
    setTime({
      hour: `${0} 时  ${0} 分`,
      s: `0 秒`,
      w: 0,
      w1: 0,
    });
  }, [initialize]);

  useInterval(() => {
    if (state) {
      const s = dayjs(state).add(1, 's').format('YYYY-MM-DD HH:mm:ss');
      setState(s);
      const second = dayjs(s).format('ss');
      const h = dayjs(s).format('HH');
      const m = dayjs(s).format('mm');
      const t = h * 60 + m * 1; // 剩余分钟数
      const sun = time.h * 60 + time.m * 1;
      const width1 = t * (size?.width / sun); // 分钟宽度
      const width = second * (size?.width / 60); // 秒钟宽度
      setTime({
        s: `${second * 1} 秒`,
        w: width,
        w1: width1,
        hour: `${h * 1} 时  ${m * 1} 分`,
      });
      if (time.h * 1 === h * 1 && time.m * 1 === m * 1) {
        setS(undefined);
        onChange(false);
      }
    }
  }, ss);

  return (
    <div className={styles.TimerCommons} ref={ref}>
      <div className={styles.TimerCommons__ss} number={timeData.hour} max={`${time.h * 1 || 0}时`}>
        <div className={styles.TimerCommons__ss_1} style={{ width: `${timeData.w1}px` }} min="0时" />
      </div>
      <div className={styles.TimerCommons__ss} style={{ marginTop: '20px' }} number={timeData.s} max="60秒">
        <div className={styles.TimerCommons__ss_1} style={{ width: `${timeData.w}px` }} min="0秒" />
      </div>
    </div>
  );
};
Timer.defaultProps = {
  time: null,
  onChange: () => {},
  onStart: true,
  initialize: '',
};

Timer.propTypes = {
  time: PropTypes.any,
  onChange: PropTypes.func,
  onStart: PropTypes.bool,
  initialize: PropTypes.any,
};
export default Timer;
