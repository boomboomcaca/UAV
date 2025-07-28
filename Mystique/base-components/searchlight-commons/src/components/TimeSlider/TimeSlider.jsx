import React, { useEffect, useState, useRef, useMemo } from 'react';
import PropTypes from 'prop-types';
import BlockPng from './assets/block.png';
import styles from './TimeSlider.module.less';
/**
 * 天 规律展示
 */
const isDate = (time) => {
  const targetTime = new Date(time).getTime();
  return targetTime;
};

const timestampToTime = (timestamp) => {
  const date = new Date(timestamp);
  const Y = date.getFullYear();
  const M = date.getMonth() + 1 < 10 ? `0${date.getMonth() + 1}` : date.getMonth() + 1;
  const D = date.getDate() < 10 ? `0${date.getDate()}` : date.getDate();
  const h = date.getHours() < 10 ? `0${date.getHours()}` : date.getHours();
  const m = date.getMinutes() < 10 ? `0${date.getMinutes()}` : date.getMinutes();
  const s = date.getSeconds() < 10 ? `0${date.getSeconds()}` : date.getSeconds();
  return `${Y}-${M}-${D} ${h}:${m}:${s}`;
};

const TimeSlider = (props) => {
  const { timeData, timeRange, recall } = props;
  const [max, setMax] = useState(0);
  const [min, setMin] = useState(0);
  const [now, setNow] = useState(0);
  const [divRange, setDivRange] = useState([]);
  // 是否开始拖拽
  const starting = useRef(false); // 获取填充柱子dom
  const moveArea = useRef();
  // 滑块图标top

  useEffect(() => {
    const a = isDate(timeRange[0].createTime);
    const b = isDate(timeRange[timeRange.length - 1].createTime);
    const arr = [];
    timeRange.forEach((i, idx) => {
      if (idx > 0) {
        const c = isDate(i.createTime) - isDate(timeRange[idx - 1].createTime);
        if (c > 10000) {
          const left = `${((isDate(timeRange[idx - 1].createTime) - a) * 100) / (b - a)}%`;
          const width = `${(c * 100) / (b - a)}%`;
          arr.push([left, width]);
        }
      }
    });
    setDivRange(arr);
  }, [timeRange]);

  useEffect(() => {
    setMin(isDate(timeRange[0].createTime));
    setMax(isDate(timeRange[timeRange.length - 1].createTime));
    setNow(isDate(timeData.createTime));
  }, [timeRange, timeData]);

  const sliderLeft = useMemo(() => {
    const gap = max - now;
    let percent = (gap * 100) / (max - min);
    if (percent <= 0) percent = 0;
    if (percent >= 100) percent = 100;
    return `${(100 - percent).toFixed(2)}%`;
  }, [now, max, min]);

  // 开始拖拽
  const startDragging = () => {
    starting.current = true;
  };

  const onDragging = (clientX) => {
    if (starting.current) {
      const fillRect = moveArea.current.getBoundingClientRect();
      const percent = (clientX - fillRect.x) / fillRect.width;
      // 计算值
      const snapval = (max - min + 1) * percent + min;
      if (snapval <= max && snapval >= min) {
        setNow(Math.round(snapval));
        recall?.({
          value: { createTime: timestampToTime(Math.round(snapval)) },
          end: false,
        });
      }
    }
  };

  // 停止拖拽
  const stopDragging = () => {
    if (starting.current) {
      starting.current = false;
      recall?.({
        value: { createTime: timestampToTime(now) },
        end: true,
      });
    }
  };

  return (
    <div
      className={styles.timeSlider}
      ref={moveArea}
      onMouseMove={(e) => onDragging(e.clientX)}
      onTouchMove={(e) => onDragging(e.targetTouches[0].clientX)}
      onMouseUp={stopDragging}
      onMouseLeave={stopDragging}
      onTouchEnd={stopDragging}
    >
      <div className={styles.bar}>
        {divRange.map((i, idx) => {
          return <div key={`${String(idx)}_div`} className={styles.divRange} style={{ left: i[0], width: i[1] }} />;
        })}
        <div className={styles.dragSlider} style={{ left: `calc(${sliderLeft} - 14px)` }}>
          <div className={styles.sliderLabel} style={{ marginLeft: -20 }}>
            {timeData.createTime.split(' ')[1]}
          </div>
          <div className={styles.slider} onTouchStart={startDragging} onMouseDown={startDragging}>
            <img src={BlockPng} alt="blockPng" />
          </div>
        </div>
      </div>
      <div className={styles.time}>
        <span>{timeRange[0].createTime.split(' ')[1]}</span>
        <span>{timeRange[timeRange.length - 1].createTime.split(' ')[1]}</span>
      </div>
    </div>
  );
};
TimeSlider.defaultProps = {};

TimeSlider.propTypes = {
  timeData: PropTypes.object.isRequired,
  timeRange: PropTypes.array.isRequired,
  recall: PropTypes.func.isRequired,
};
export default TimeSlider;
