import React, { useEffect, useState, useRef, useMemo } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import BlockPng from './assets/block.png';
import styles from './TimeSlidingBlock.module.less';
/**
 * 天 规律展示
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

const TimeSlidingBlock = (props) => {
  const { className, faultTolerant, timeData, recall, rangeTime, resetStyle } = props;
  const { bgColor, dataColor, coverColor } = resetStyle;
  const [dayRegularity, setDayRegularity] = useState([]);
  const [chooseIndex, setChooseIndex] = useState(0);
  const [currentRangeTime, setCurrentRangeTime] = useState(rangeTime);
  const currentRangeTimeRef = useRef(JSON.parse(JSON.stringify(currentRangeTime)));
  const [coverInfo, setCoverInfo] = useState({
    start: 0,
    width: '100%',
  });
  const relativeTimeRef = useRef(1000 * 60 * 60 * 24);
  const rangeTimeRef = useRef(rangeTime);
  const timeDataRef = useRef(timeData);
  const isMousedownRef = useRef(false);
  const mouseTargetRef = useRef('');
  const pointerXRef = useRef(0);
  const isCoincideRef = useRef(false);
  let formateTimer = null;
  useEffect(() => {
    let resizeTimer = null;
    const resizeFunc = () => {
      // 生成图表
      if (resizeTimer) {
        clearTimeout(resizeTimer);
        resizeTimer = null;
      }
      resizeTimer = setTimeout(() => {
        // 绘制信号规律
        initRegularity(timeDataRef.current, rangeTimeRef.current, 'reset');
        clearTimeout(resizeTimer);
        resizeTimer = null;
      });
    };
    window.addEventListener('resize', resizeFunc, false);
    document.body.addEventListener('mousemove', pointerMousemove, false);
    document.body.addEventListener('touchmove', pointerMousemove, false);
    document.body.addEventListener('mouseleave', pointerMouseleave, false);
    document.body.addEventListener('mouseup', pointerMouseup, false);
    document.body.addEventListener('touchend', pointerMouseup, false);
    return () => {
      window.removeEventListener('resize', resizeFunc, false);
      document.body.removeEventListener('mousemove', pointerMousemove, false);
      document.body.removeEventListener('touchmove', pointerMousemove, false);
      document.body.removeEventListener('mouseleave', pointerMouseleave, false);
      document.body.removeEventListener('mouseup', pointerMouseup, false);
      document.body.removeEventListener('touchend', pointerMouseup, false);
    };
  }, []);
  const initRegularity = (data, intervalTime, type) => {
    // 绘制信号规律
    try {
      // 重置滑块位置
      const sliderStart = document.getElementById('temporalSliderStart');
      const sliderEnd = document.getElementById('temporalSliderEnd');
      const sliderTarget = document.getElementById('temporalSliderContainer');
      if (
        type === 'reset' ||
        rangeTimeRef.current.start !== intervalTime.start ||
        rangeTimeRef.current.end !== intervalTime.end
      ) {
        sliderStart.style.left = `${-sliderStart.clientWidth / 2}px`;
        sliderEnd.style.left = `${sliderTarget.clientWidth}px`;
        setCoverInfo({
          start: 0,
          width: '100%',
        });
      }
      let isRefresh = false;
      for (let i = 0; i < data.length; i += 1) {
        const startDate = new Date(data[i].startTime);
        const endDate = new Date(data[i].endTime);
        // rangtime 容错
        if (faultTolerant) {
          if (isDate(intervalTime.start)) {
            const rangeStartDate = new Date(intervalTime.start);
            if (startDate.getTime() < rangeStartDate.getTime()) {
              intervalTime.start = createFormateTime(startDate);
              isRefresh = true;
            }
          } else {
            intervalTime.start = createFormateTime(startDate);
            isRefresh = true;
          }
          if (isDate(intervalTime.end)) {
            const rangeEndDate = new Date(intervalTime.end);
            if (endDate.getTime() > rangeEndDate.getTime()) {
              intervalTime.end = createFormateTime(endDate);
              isRefresh = true;
            }
          } else {
            intervalTime.end = createFormateTime(endDate);
            isRefresh = true;
          }
        }
      }
      if (isRefresh) {
        relativeTimeRef.current = new Date(rangeTime.end).getTime() - new Date(rangeTime.start).getTime();
        currentRangeTimeRef.current = JSON.parse(JSON.stringify(rangeTime));
        setCurrentRangeTime({ ...rangeTime });
      }
      if (!isRefresh && (data.length === 0 || !intervalTime.start || !intervalTime.end)) {
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
        relativeTimeRef.current = Math.abs(
          new Date(intervalTime.end).getTime() - new Date(intervalTime.start).getTime(),
        );
        currentRangeTimeRef.current = JSON.parse(JSON.stringify(intervalTime));
        setCurrentRangeTime({ ...intervalTime });
      }
      rangeTimeRef.current = intervalTime;
      // 初始
      recall && recall(rangeTimeRef.current);
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
        if (faultTolerant) {
          return [
            ...pre,
            {
              start: `${
                ((startDate.getTime() - new Date(intervalTime.start).getTime()) / relativeTimeRef.current) * 100
              }%`,
              width: `${(Math.abs(endDate.getTime() - startDate.getTime()) / relativeTimeRef.current) * 100}%`,
              alertText: `${hour}:${minute}:${seconds}--${endHour}:${endMinute}:${endSeconds}`,
            },
          ];
        }
        if (
          startDate.getTime() >= new Date(intervalTime.start).getTime() &&
          endDate.getTime() <= new Date(intervalTime.end).getTime()
        ) {
          return [
            ...pre,
            {
              start: `${
                ((startDate.getTime() - new Date(intervalTime.start).getTime()) / relativeTimeRef.current) * 100
              }%`,
              width: `${(Math.abs(endDate.getTime() - startDate.getTime()) / relativeTimeRef.current) * 100}%`,
              alertText: `${hour}:${minute}:${seconds}--${endHour}:${endMinute}:${endSeconds}`,
            },
          ];
        }
        return pre;
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
      const container = document.getElementById('timeSlidingBlock');
      const iLeft = container.offsetLeft;
      const cont = document.getElementById('temporalSliderContainer');
      const sliderStart = document.getElementById('temporalSliderStart');
      const sliderEnd = document.getElementById('temporalSliderEnd');
      const contClientWidth = cont.clientWidth;
      let sliderTarget = mouseTargetRef.current === 'temporalSliderStart' ? 'start' : 'end';
      // let iLeft = 0;
      // // 距最左侧的距离
      // do {
      //   iLeft += cont.offsetLeft;
      //   cont = cont.parentNode;
      // } while (cont.parentNode);
      // 更换z-index
      if (sliderTarget === 'start') {
        sliderStart.style.zIndex = 20;
        sliderEnd.style.zIndex = 10;
      } else {
        sliderStart.style.zIndex = 10;
        sliderEnd.style.zIndex = 20;
      }
      // 重合后，根据滑动方向切换滑块
      if (isCoincideRef.current) {
        sliderTarget = pageX - iLeft - sliderStart.clientWidth / 2 < pointerXRef.current ? 'start' : 'end';
        mouseTargetRef.current = sliderTarget === 'start' ? 'temporalSliderStart' : 'temporalSliderEnd';
        isCoincideRef.current = false;
      }
      const pointer = sliderTarget === 'start' ? sliderStart : sliderEnd;
      const disx = Math.max(pageX - iLeft - sliderStart.clientWidth, -sliderStart.clientWidth / 2);
      // 滑块边缘判断(可重合)
      // if (sliderTarget === 'start' && disx >= sliderEnd.offsetLeft) {
      //   disx = sliderEnd.offsetLeft;
      // }
      // if (sliderTarget === 'end' && disx <= sliderStart.offsetLeft) {
      //   disx = sliderStart.offsetLeft;
      // }
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
      coverInfo.start =
        sliderTarget === 'start'
          ? Math.min(moveX + sliderStart.clientWidth / 2, sliderEnd.offsetLeft + sliderEnd.clientWidth / 2)
          : Math.min(
              sliderStart.offsetLeft + sliderStart.clientWidth / 2,
              sliderEnd.offsetLeft + sliderEnd.clientWidth / 2,
            );
      coverInfo.width = Math.abs(sliderEnd.offsetLeft - sliderStart.offsetLeft);
      setCoverInfo({ ...coverInfo });
      // 更新当前时刻显示
      if (formateTimer) {
        clearTimeout(formateTimer);
        formateTimer = null;
      }
      formateTimer = setTimeout(() => {
        let recallTime = {};
        if (
          new Date(currentRangeTimeRef.current.start).getTime() > new Date(currentRangeTimeRef.current.end).getTime()
        ) {
          recallTime.start = currentRangeTimeRef.current.end;
          recallTime.end = currentRangeTimeRef.current.start;
        } else {
          recallTime = currentRangeTimeRef.current;
        }
        recall && recall(recallTime);
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
    // 绘制信号规律
    timeDataRef.current = timeData;
    initRegularity(timeData, rangeTime, 'update');
  }, [timeData, rangeTime]);
  return (
    <div id="timeSlidingBlock" className={classnames(styles.container, className)}>
      <div
        className={styles.dataBox_content}
        // onMouseLeave={pointerMouseleave}
        onTouchEnd={pointerMouseleave}
      >
        <div
          id="temporalSliderContainer"
          className={[styles.content_top, styles.content_top_bg].join(' ')}
          style={{
            background: bgColor || 'white',
            borderColor: dataColor || '#3CE5D3',
          }}
        >
          <div
            id="temporalSliderStart"
            datatype="temporalSliderStart"
            className={[styles.top_slider, styles.top_slider_left].join(' ')}
            style={{
              background: `url(${BlockPng}) 0% 0% / 100% 100% no-repeat`,
            }}
            onMouseDown={pointerMousedown}
            onTouchStart={pointerMousedown}
          >
            <div className={styles.top_time}>{currentRangeTime.start || ''}</div>
          </div>
          <div
            id="temporalSliderEnd"
            className={[styles.top_slider, styles.top_slider_right].join(' ')}
            style={{
              background: `url(${BlockPng}) 0% 0% / 100% 100% no-repeat`,
            }}
            onMouseDown={pointerMousedown}
            onTouchStart={pointerMousedown}
          >
            <div className={styles.top_time}>{currentRangeTime.end || ''}</div>
          </div>
          {dayRegularity.map((item, index) => (
            <div
              key={`regularity-middle-${index + 1}`}
              className={
                chooseIndex === index ? [styles.top_dayItem, styles.top_dayItem_choose].join(' ') : styles.top_dayItem
              }
              style={{
                width: item.width || 0,
                left: item.start || 0,
                backgroundColor: dataColor || '#3CE5D3',
              }}
              // onClick={() => chooseItem(item, index)}
            >
              {/* <div
                  className={styles.top_alert}
                  style={{
                    display: chooseIndex === index ? 'block' : 'none',
                  }}
                >
                  <span>{item.alertText}</span>
                </div> */}
            </div>
          ))}
          <div
            className={styles.top_cover}
            style={{
              left: coverInfo.start,
              width: coverInfo.width,
              backgroundColor: coverColor || 'transperant',
            }}
          />
        </div>
      </div>
    </div>
  );
};
TimeSlidingBlock.defaultProps = {
  className: null,
  faultTolerant: true,
  timeData: [],
  recall: null,
  rangeTime: {
    start: '00:00:00',
    end: '23:59:59',
  },
  resetStyle: {
    bgColor: 'white',
    dataColor: '#3CE5D3',
    coverColor: 'transperant',
  },
};

TimeSlidingBlock.propTypes = {
  className: PropTypes.any,
  faultTolerant: PropTypes.bool,
  timeData: PropTypes.array,
  recall: PropTypes.func,
  rangeTime: PropTypes.object,
  resetStyle: PropTypes.object,
};
export default TimeSlidingBlock;
