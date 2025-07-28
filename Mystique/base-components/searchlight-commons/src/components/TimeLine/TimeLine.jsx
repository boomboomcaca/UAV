import React, { useEffect, useState, useRef, useMemo } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import styles from './TimeLine.module.less';
/**
 * 单频测量 规律展示
 */
const isDate = (time) => {
  try {
    const targetTime = new Date(time);
    return targetTime.getFullYear() > 1970;
  } catch (err) {
    return false;
  }
};
export const createFormateTime = (time) => {
  const nowDate = new Date(time);
  const year = nowDate.getFullYear();
  let month = nowDate.getMonth() + 1;
  month = month > 9 ? month : `0${month}`;
  let days = nowDate.getDate();
  days = days > 9 ? days : `0${days}`;
  let hours = nowDate.getHours();
  hours = hours > 9 ? hours : `0${hours}`;
  let minutes = nowDate.getMinutes();
  minutes = minutes > 9 ? minutes : `0${minutes}`;
  let seconds = nowDate.getSeconds();
  seconds = seconds > 9 ? seconds : `0${seconds}`;
  return `${year}-${month}-${days} ${hours}:${minutes}:${seconds}`;
};

const TimeLine = (props) => {
  const { className, recall, option } = props;
  const [dayRegularity, setDayRegularity] = useState([]);
  const [chooseIndex, setChooseIndex] = useState(0);
  const [currentRangeTime, setCurrentRangeTime] = useState({});
  const currentRangeTimeRef = useRef(JSON.parse(JSON.stringify(currentRangeTime)));
  const relativeTimeRef = useRef(1000 * 60 * 60 * 24);
  const rangeTimeRef = useRef({});
  useEffect(() => {
    return () => {};
  }, []);
  const initRegularity = (data, rangeTime) => {
    // 获取信号规律
    try {
      let isRefresh = false;
      for (let i = 0; i < data.length; i += 1) {
        const startDate = new Date(data[i].startTime);
        const endDate = new Date(data[i].endTime);
        if (isDate(rangeTime.start)) {
          const rangeStartDate = new Date(rangeTime.start);
          if (startDate.getTime() < rangeStartDate.getTime()) {
            rangeTime.start = createFormateTime(startDate);
            isRefresh = true;
          }
        } else {
          rangeTime.start = createFormateTime(startDate);
          isRefresh = true;
        }
        if (isDate(rangeTime.end)) {
          const rangeEndDate = new Date(rangeTime.end);
          if (endDate.getTime() > rangeEndDate.getTime()) {
            rangeTime.end = createFormateTime(endDate);
            isRefresh = true;
          }
        } else {
          rangeTime.end = createFormateTime(endDate);
          isRefresh = true;
        }
      }
      if (isRefresh) {
        relativeTimeRef.current = new Date(rangeTime.end).getTime() - new Date(rangeTime.start).getTime();
        currentRangeTimeRef.current = JSON.parse(JSON.stringify(rangeTime));
        setCurrentRangeTime({ ...rangeTime });
      }
      if (!isRefresh && data.length === 0) {
        const nowDate = new Date();
        const year = nowDate.getFullYear();
        let month = nowDate.getMonth() + 1;
        month = month > 9 ? month : `0${month}`;
        let days = nowDate.getDate();
        days = days > 9 ? days : `0${days}`;
        if (!isDate(rangeTime.start)) {
          rangeTime.start = `${year}-${month}-${days} 00:00:00`;
        } else {
          rangeTime.start = createFormateTime(rangeTime.start);
        }
        if (!isDate(rangeTime.end)) {
          rangeTime.end = `${year}-${month}-${days} 23:59:59`;
        } else {
          rangeTime.end = createFormateTime(rangeTime.end);
        }
        relativeTimeRef.current = Math.abs(new Date(rangeTime.end).getTime() - new Date(rangeTime.start).getTime());
        currentRangeTimeRef.current = JSON.parse(JSON.stringify(rangeTime));
        setCurrentRangeTime({ ...rangeTime });
      } else {
        relativeTimeRef.current = Math.abs(new Date(rangeTime.end).getTime() - new Date(rangeTime.start).getTime());
        setCurrentRangeTime({ ...rangeTime });
      }
      rangeTimeRef.current = rangeTime;
      const regularityData = data.reduce((pre, item) => {
        const startDate = new Date(item.startTime);
        let hour = startDate.getHours();
        hour = hour > 9 ? hour : `0${hour}`;
        let minute = startDate.getMinutes();
        minute = minute > 9 ? minute : `0${minute}`;
        let seconds = startDate.getSeconds();
        seconds = seconds > 9 ? seconds : `0${seconds}`;
        const endDate = new Date(item.endTime);
        let endHour = endDate.getHours();
        endHour = endHour > 9 ? endHour : `0${endHour}`;
        let endMinute = endDate.getMinutes();
        endMinute = endMinute > 9 ? endMinute : `0${endMinute}`;
        let endSeconds = endDate.getSeconds();
        endSeconds = endSeconds > 9 ? endSeconds : `0${endSeconds}`;
        return [
          ...pre,
          {
            start: `${((startDate.getTime() - new Date(rangeTime.start).getTime()) / relativeTimeRef.current) * 100}%`,
            width: `${(Math.abs(endDate.getTime() - startDate.getTime()) / relativeTimeRef.current) * 100}%`,
            alertText: `${hour}:${minute}:${seconds}--${endHour}:${endMinute}:${endSeconds}`,
          },
        ];
      }, []);
      setDayRegularity([...regularityData]);
    } catch (e) {
      throw Error('参数有误');
    }
  };
  const chooseItem = (item, index) => {
    // 选中数据段
    setChooseIndex(index);
    recall && recall(index, item);
  };

  useMemo(() => {
    // 获取信号规律
    const { timeData, rangeTime } = option;
    initRegularity(timeData, rangeTime || {});
  }, [option]);
  return (
    <div className={classnames(styles.container, className)}>
      <div className={styles.content}>
        <div className={[styles.content_top, styles.content_top_bg].join(' ')}>
          {dayRegularity.map((item, index) => (
            <div
              key={`regularity-middle-${index + 1}`}
              className={
                chooseIndex === index ? [styles.top_dayItem, styles.top_dayItem_choose].join(' ') : styles.top_dayItem
              }
              style={{
                width: item.width || 0,
                left: item.start || 0,
              }}
              onClick={() => chooseItem(item, index)}
            >
              <div className={styles.top_alert}>
                <span>{item.alertText}</span>
              </div>
            </div>
          ))}
          <div className={[styles.top_time, styles.time_left].join(' ')}>{currentRangeTime.start || ''}</div>
          <div className={[styles.top_time, styles.time_right].join(' ')}>{currentRangeTime.end || ''}</div>
        </div>
      </div>
    </div>
  );
};
TimeLine.defaultProps = {
  className: null,
  recall: null,
  option: {},
};

TimeLine.propTypes = {
  className: PropTypes.any,
  recall: PropTypes.func,
  option: PropTypes.object,
};
export default TimeLine;
