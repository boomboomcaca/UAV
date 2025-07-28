import React, { useEffect, useState, useRef, useMemo } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import styles from './TemporalDistribution.module.less';
/**
 * 天 规律展示
 */
const createRegularity = (space) =>
  [...new Array(space).keys()].map((item, index) => {
    return {
      month: index + 1,
      hasData: false,
      hasSignals: false,
    };
  });
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

const TemporalDistribution = (props) => {
  const { className, timeData, interval, recall, purpose, rangeTime } = props;
  const [regularity, setRegularity] = useState(createRegularity(interval));
  const [dayRegularity, setDayRegularity] = useState([]);
  const [chooseIndex, setChooseIndex] = useState(0);
  const [currentRangeTime, setCurrentRangeTime] = useState(rangeTime);
  const currentRangeTimeRef = useRef(JSON.parse(JSON.stringify(currentRangeTime)));
  const relativeTimeRef = useRef(1000 * 60 * 60 * 24);
  const rangeTimeRef = useRef(rangeTime);
  const isMousedownRef = useRef(false);
  const mouseTargetRef = useRef('');
  const pointerXRef = useRef(0);
  const isCoincideRef = useRef(false);
  let formateTimer = null;
  useEffect(() => {
    let resizeTimer = null;
    setRegularity(createRegularity(interval));
    const resizeFunc = () => {
      // 生成图表
      if (resizeTimer) {
        clearTimeout(resizeTimer);
        resizeTimer = null;
      }
      resizeTimer = setTimeout(() => {
        initRegularity(timeData);
        clearTimeout(resizeTimer);
        resizeTimer = null;
      });
    };
    window.addEventListener('resize', resizeFunc, false);
    document.body.addEventListener('mousemove', pointerMousemove, false);
    document.body.addEventListener('touchmove', pointerMousemove, false);
    document.body.addEventListener('mouseup', pointerMouseup, false);
    document.body.addEventListener('touchend', pointerMouseup, false);
    return () => {
      window.removeEventListener('resize', resizeFunc, false);
      document.body.removeEventListener('mousemove', pointerMousemove, false);
      document.body.removeEventListener('touchmove', pointerMousemove, false);
      document.body.removeEventListener('mouseup', pointerMouseup, false);
      document.body.removeEventListener('touchend', pointerMouseup, false);
    };
  }, []);
  const initRegularity = (data) => {
    // 获取信号日规律
    try {
      // 重置滑块位置
      if (rangeTimeRef.current.start !== rangeTime.start || rangeTimeRef.current.end !== rangeTime.end) {
        const sliderStart = document.getElementById('temporalSliderStart');
        const sliderEnd = document.getElementById('temporalSliderEnd');
        sliderStart.style.left = `${-sliderStart.clientWidth / 2}px`;
        sliderEnd.style.right = `${-sliderEnd.clientWidth / 2}px`;
      }
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
            start:
              purpose === 'normal'
                ? `${((startDate.getHours() + startDate.getMinutes() / 60) / 24) * 100}%`
                : `${((startDate.getTime() - new Date(rangeTime.start).getTime()) / relativeTimeRef.current) * 100}%`,
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
  const pointerMousedown = (e) => {
    isMousedownRef.current = true;
    mouseTargetRef.current = e.target.id;
  };
  const pointerMousemove = (e) => {
    if (!isMousedownRef.current) {
      return;
    }
    e.preventDefault();
    e.stopPropagation();
    const pageX = e.x ? e.x : e.touches && e.touches[0] && e.touches[0].clientX ? e.touches[0].clientX : 0;
    if (mouseTargetRef.current === 'temporalSliderStart' || mouseTargetRef.current === 'temporalSliderEnd') {
      let cont = document.getElementById('temporalSliderContainer');
      const sliderStart = document.getElementById('temporalSliderStart');
      const sliderEnd = document.getElementById('temporalSliderEnd');
      const contClientWidth = cont.clientWidth;
      let sliderTarget = mouseTargetRef.current === 'temporalSliderStart' ? 'start' : 'end';
      let iLeft = 0;
      // 距最左侧的距离
      do {
        iLeft += cont.offsetLeft;
        cont = cont.parentNode;
      } while (cont.parentNode);
      // 重合后，根据滑动方向切换滑块
      if (isCoincideRef.current) {
        sliderTarget = pageX - iLeft / 2 - sliderStart.clientWidth / 2 < pointerXRef.current ? 'start' : 'end';
        mouseTargetRef.current = sliderTarget === 'start' ? 'temporalSliderStart' : 'temporalSliderEnd';
        isCoincideRef.current = false;
      }
      const pointer = sliderTarget === 'start' ? sliderStart : sliderEnd;
      let disx = Math.max(pageX - iLeft / 2 - sliderStart.clientWidth / 2, -sliderStart.clientWidth / 2);
      // 滑块边缘判断(可重合)
      if (sliderTarget === 'start' && disx >= sliderEnd.offsetLeft) {
        disx = sliderEnd.offsetLeft;
      }
      if (sliderTarget === 'end' && disx <= sliderStart.offsetLeft) {
        disx = sliderStart.offsetLeft;
      }
      const moveX = Math.min(disx, contClientWidth - sliderStart.clientWidth / 2);
      // 计算滑块相对时间（滑动边界计算时间存在误差，rangeTime为起始边界值）
      let currentTime =
        new Date(rangeTimeRef.current.start).getTime() + (moveX / contClientWidth) * relativeTimeRef.current;
      // start(左边界)
      if (moveX === -sliderStart.clientWidth / 2) {
        currentTime = new Date(rangeTimeRef.current.start).getTime();
      }
      // end(右边界)
      if (moveX === contClientWidth - sliderStart.clientWidth / 2) {
        currentTime = new Date(rangeTimeRef.current.end).getTime();
      }
      currentRangeTimeRef.current[sliderTarget] = createFormateTime(currentTime);
      pointer.style.left = `${moveX}px`;
      // 更新当前时刻显示
      if (formateTimer) {
        clearTimeout(formateTimer);
        formateTimer = null;
      }
      formateTimer = setTimeout(() => {
        recall && recall(currentRangeTimeRef.current);
        setCurrentRangeTime({ ...currentRangeTimeRef.current });
      }, 200);
    }
  };
  const pointerMouseup = () => {
    const sliderStart = document.getElementById('temporalSliderStart');
    const sliderEnd = document.getElementById('temporalSliderEnd');
    isMousedownRef.current = false;
    mouseTargetRef.current = null;
    isCoincideRef.current = sliderStart.offsetLeft === sliderEnd.offsetLeft;
    pointerXRef.current =
      mouseTargetRef.current === 'temporalSliderStart' ? sliderStart.offsetLeft : sliderEnd.offsetLeft;
  };
  const pointerMouseleave = () => {
    isMousedownRef.current = false;
    mouseTargetRef.current = null;
    isCoincideRef.current = false;
  };

  useMemo(() => {
    // 获取信号规律
    initRegularity(timeData);
  }, [timeData, rangeTime]);
  return (
    <div className={classnames(styles.container, className)}>
      <div className={styles.container_dataBox}>
        <div
          className={styles.dataBox_content}
          // onMouseLeave={pointerMouseleave}
          onTouchEnd={pointerMouseleave}
        >
          <div
            id="temporalSliderContainer"
            className={
              purpose === 'normal' ? styles.content_top : [styles.content_top, styles.content_top_bg].join(' ')
            }
          >
            <div
              id="temporalSliderStart"
              datatype="temporalSliderStart"
              className={[styles.top_slider, styles.top_slider_left].join(' ')}
              onMouseDown={pointerMousedown}
              onTouchStart={pointerMousedown}
              style={{ display: purpose === 'normal' ? 'none' : 'block' }}
            />
            <div
              id="temporalSliderEnd"
              className={[styles.top_slider, styles.top_slider_right].join(' ')}
              onMouseDown={pointerMousedown}
              onTouchStart={pointerMousedown}
              style={{ display: purpose === 'normal' ? 'none' : 'block' }}
            />
            {purpose === 'normal' &&
              regularity.map((item, index) => (
                <div
                  key={`regularity-top-${index + 1}`}
                  className={
                    item.hasSignals
                      ? chooseIndex === index
                        ? [styles.top_item, styles.top_item_choose].join(' ')
                        : [styles.top_item, styles.top_item_has].join(' ')
                      : styles.top_item
                  }
                  style={{
                    cursor: item.hasData ? 'pointer' : 'default',
                  }}
                  onClick={() => chooseItem(item, index)}
                >
                  {index === 0 ? <span>00:00:00</span> : index === regularity.length - 1 ? <span>23:59:59</span> : ''}
                </div>
              ))}
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
                <div
                  className={styles.top_alert}
                  style={{
                    display: purpose === 'normal' && chooseIndex === index ? 'block' : 'none',
                  }}
                >
                  <span>{item.alertText}</span>
                </div>
              </div>
            ))}
            <div className={styles.top_time} style={{ display: purpose === 'normal' ? 'none' : 'block', left: 0 }}>
              {currentRangeTime.start || ''}
            </div>
            <div className={styles.top_time} style={{ display: purpose === 'normal' ? 'none' : 'block', right: 0 }}>
              {currentRangeTime.end || ''}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
TemporalDistribution.defaultProps = {
  className: null,
  timeData: [],
  interval: 48,
  recall: null,
  purpose: 'normal',
  rangeTime: {
    start: '00:00:00',
    end: '23:59:59',
  },
};

TemporalDistribution.propTypes = {
  className: PropTypes.any,
  timeData: PropTypes.array,
  interval: PropTypes.number,
  recall: PropTypes.func,
  purpose: PropTypes.string,
  rangeTime: PropTypes.object,
};
export default TemporalDistribution;
