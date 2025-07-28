/*
 * @Author: wangXueDong
 * @Date: 2022-02-17 16:10:53
 * @LastEditors: wangXueDong
 * @LastEditTime: 2022-07-08 14:34:11
 */
/* eslint-disable jsx-a11y/mouse-events-have-key-events */
/* eslint-disable max-len */
import React, { useEffect, useMemo, useRef, useState } from 'react';
import PropTypes from 'prop-types';
import { AddIcon, RemoveIcon } from 'dc-icon';
import MinorTick from './components/MinorTick.jsx';
import sliderImg from '../../assets/scaleSlider.png';
import styles from './columnScaleSliderBar.module.less';

const ColumnScaleSliderBar = (props) => {
  const {
    disable,
    title,
    value,
    unitName,
    width,
    onSliderValueChange,
    scaleData,
    labelData,
    minimum,
    maximum,
    step,
    isSet,
  } = props;

  // 指针对应的值
  const [sliderVal, setSliderVal] = useState(0);
  const sliderValRef = useRef(0);
  // 获取填充柱子dom
  const moveArea = useRef();
  // 是否开始拖拽
  const starting = useRef(false);
  //  正在拖拽的百分比
  const [currentPercent, setCurrentPercent] = useState(0);
  const currentPercentRef = useRef(0);
  //  判断是max，min值(true)，还是数组值(false)
  const [isMaxMin, setIsMaxMin] = useState(false);
  const [isLabel, setIsLabel] = useState(false);
  const IsMaxMinRef = useRef(false);

  useEffect(() => {
    const isMaxMinType = !scaleData || scaleData.length === 0;
    const min = isMaxMinType ? minimum : scaleData[0];
    const max = isMaxMinType ? maximum : scaleData[scaleData.length - 1];

    let percent = 0;
    if (isMaxMinType) {
      const filValue = min > value ? min : max < value ? max : value;
      const count = maximum - minimum;
      percent = (filValue - minimum) / (count + 1);
      setSliderVal(filValue);
    } else {
      let valueIndex = scaleData.findIndex((v) => value === v);
      valueIndex = valueIndex === -1 ? 0 : valueIndex;
      percent = valueIndex / (scaleData.length - 1);
      setSliderVal(scaleData[valueIndex]);
    }

    // setSliderVal(snap);
    //  判断是否传入了label数组
    setIsLabel(!!labelData);
    setIsMaxMin(isMaxMinType);
    IsMaxMinRef.current = isMaxMinType;
    setCurrentPercent(percent * 100);
  }, [value, scaleData, minimum, maximum, labelData]);

  useEffect(() => {
    document.body.addEventListener('mousemove', pointerMousemove, false);
    document.body.addEventListener('touchmove', pointerMousemove, false);
    document.body.addEventListener('mouseup', pointerMouseup, false);
    document.body.addEventListener('mouseleave', pointerMouseup, false);
    document.body.addEventListener('touchend', pointerMouseup, false);
    return () => {
      document.body.removeEventListener('mousemove', pointerMousemove, false);
      document.body.removeEventListener('touchmove', pointerMousemove, false);
      document.body.removeEventListener('mouseup', pointerMouseup, false);
      document.body.addEventListener('mouseleave', pointerMouseup, false);
      document.body.removeEventListener('touchend', pointerMouseup, false);
    };
  }, []);

  const onTrackClick = (clientY) => {
    if (disable) {
      return;
    }
    const fillRect = moveArea.current.getBoundingClientRect();
    const percent = ((fillRect.height - (clientY - fillRect.y)) / fillRect.height) * 100;
    setCurrentPercent(percent);
    if (isMaxMin) {
      // 计算值
      let snapval = Math.round((maximum - minimum + 1) * (percent / 100) + minimum);
      snapval = snapval > maximum ? maximum : snapval < minimum ? minimum : snapval;
      setSliderVal(snapval);
      sliderValRef.current = snapval;
      onSliderValueChange?.({
        value: snapval,
        end: true,
      });
    } else {
      // 一共scaleData.length - 1格，每格占比gap
      const gap = 100 / (scaleData.length - 1);
      const integer = percent / gap;
      let toIndex = parseInt(integer, 10);
      const leave = percent % gap;
      toIndex = leave >= gap / 2 ? toIndex + 1 : toIndex;
      setCurrentPercent(toIndex * gap);
      setSliderVal(scaleData[toIndex]);
      onSliderValueChange?.({
        value: scaleData[toIndex],
        end: true,
      });
    }
  };
  // 当鼠标按下
  const pointerMousedown = () => {
    if (disable) {
      return;
    }
    starting.current = true;
  };
  // 当鼠标移动中
  const pointerMousemove = (e) => {
    if (!starting.current) {
      return;
    }
    e.preventDefault();
    e.stopPropagation();
    const clientY = e.y ? e.y : e.touches && e.touches[0] && e.touches[0].clientY ? e.touches[0].clientY : 0;
    const fillRect = moveArea.current.getBoundingClientRect();
    let percent = (fillRect.height - (clientY - fillRect.y)) / fillRect.height;
    percent = percent > 1 ? 1 : percent < 0 ? 0 : percent;
    setCurrentPercent(percent * 100);
    currentPercentRef.current = percent * 100;
    if (IsMaxMinRef.current) {
      // 计算值
      const snapval = Math.round((maximum - minimum) * percent + minimum);
      if (snapval <= maximum && snapval >= minimum) {
        setSliderVal(snapval);
        sliderValRef.current = snapval;
        onSliderValueChange?.({
          value: snapval,
          end: false,
        });
      }
    }
  };
  // 当鼠标抬起的时候
  const pointerMouseup = () => {
    if (starting.current && !IsMaxMinRef.current) {
      const percent = currentPercentRef.current;
      let toIndex = 0;
      if (percent === 100) {
        toIndex = scaleData.length - 1;
      } else {
        // 一共scaleData.length - 1格，每格占比gap
        const gap = 100 / (scaleData.length - 1);
        const integer = percent / gap;
        toIndex = parseInt(integer, 10);
        const leave = percent % gap;
        toIndex = leave >= gap / 2 ? toIndex + 1 : toIndex;
        setCurrentPercent(toIndex * gap);
      }
      setSliderVal(scaleData[toIndex]);
      onSliderValueChange?.({
        value: scaleData[toIndex],
        end: true,
      });
    } else if (starting.current && IsMaxMinRef.current) {
      onSliderValueChange?.({
        value: sliderValRef.current,
        end: true,
      });
    }
    starting.current = false;
  };

  // value条宽度、滑块图标left
  const trackHeight = useMemo(() => {
    let percent = currentPercent;
    if (percent < 0) percent = 0;
    if (percent > 100) percent = 100;
    return `${percent.toFixed(2)}%`;
  }, [currentPercent]);

  //  如果存在labelData数组那么选出来的值显示对应labelData数组
  const goodSliderVal = useMemo(() => {
    return labelData && scaleData ? labelData[scaleData.indexOf(sliderVal)] : 0;
  }, [labelData, scaleData, sliderVal]);
  const goodLabelData = useMemo(() => {
    let goodArr = [];
    if (labelData) {
      if (labelData.length >= 7) {
        // 超过8项，自定义到5项
        const gap = (labelData.length - 1) / 4;
        const sl = [];
        let prevIndex = -1;
        for (let i = 0; i < labelData.length; i += gap) {
          const indx = Math.round(i);
          if (indx > prevIndex && indx < labelData.length) {
            prevIndex = indx;
            sl.push(indx);
          }
        }
        const labels = [];

        for (let i = labelData.length - 1; i > -1; i -= 1) {
          if (sl.includes(i)) {
            labels.push(labelData[i]);
          } else {
            labels.push('');
          }
        }
        goodArr = labels.reverse();
      } else {
        goodArr = [...labelData];
      }
    }

    return goodArr;
  }, [labelData]);
  const reverseArr = (arr) => {
    const res = [];
    const goodArr = arr ? [...arr] : [];
    for (let i = goodArr.length - 1; i >= 0; i -= 1) {
      res.push(goodArr.pop());
    }
    return res;
  };

  const toSetValue = (type) => {
    if (disable) {
      return;
    }
    let num = sliderVal;
    if (type === 'add') {
      num += step;
    } else {
      num -= step;
    }
    num = Math.round(num * 100) / 100;
    num = minimum > num ? minimum : maximum < num ? maximum : num;
    const count = maximum - minimum;
    const percent = (num - minimum) / (count + 1);
    setSliderVal(num);
    setCurrentPercent(percent * 100);
    onSliderValueChange?.({
      value: num,
      end: true,
    });
  };
  return (
    <div className={styles.rootCon}>
      {title && <div className={styles.title}>{title}</div>}
      {isSet ? (
        <div className={styles.setBox}>
          <div className={styles.setButton}>
            <RemoveIcon onClick={() => toSetValue('cut')} iconSize={18} color="var(--theme-primary-50)" />
          </div>
          <div className={styles.setValue}>
            {sliderVal}
            <span>{unitName}</span>
          </div>
          <div className={styles.setButton}>
            <AddIcon onClick={() => toSetValue('add')} iconSize={18} color="var(--theme-primary-50)" />
          </div>
        </div>
      ) : (
        <div className={styles.valueBox}>
          {/* 如果存在labelData数组那么选出来的值显示对应labelData数组 */}
          {isLabel && !isMaxMin ? (
            <span className={styles.valText}>{goodSliderVal}</span>
          ) : (
            <span className={styles.valText}>{sliderVal}</span>
          )}
          {!isLabel && unitName ? <span className={styles.unitText}>{unitName}</span> : undefined}
        </div>
      )}
      <div className={styles.sbroot}>
        <div className={styles.sbrootCon}>
          {/* 滑块部分 */}
          <div ref={moveArea} className={styles.sliderBox}>
            <div
              // onMouseMove={(e) => pointerMousemove(e.clientX)} // 鼠标移动触发
              // onTouchMove={(e) => pointerMousemove(e.targetTouches[0].clientX)} //  手指移动触发
              // onMouseUp={pointerMouseup} // 鼠标抬起触发
              // onMouseLeave={pointerMouseup} //  鼠标指针移出 div 元素时触发
              // onTouchEnd={pointerMouseup} // 手指抬起触发
              className={styles.dragSlider}
              style={{ bottom: `calc(${trackHeight} - 12px)` }}
            >
              <div className={styles.slider} onTouchStart={pointerMousedown} onMouseDown={pointerMousedown}>
                <img src={sliderImg} alt="" />
              </div>
            </div>
          </div>
          {/* 轨道 */}
          <div style={{ width }} className={styles.track}>
            {/* 刻度 */}
            <div style={{ right: width }} className={styles.scaleBox}>
              {isMaxMin ? (
                <>
                  <MinorTick value={String(maximum)} />
                  <MinorTick />
                  <MinorTick />
                  <MinorTick />
                  <MinorTick value={String(minimum)} />
                </>
              ) : (
                reverseArr(scaleData).map((e, index) => (
                  <MinorTick
                    isLabel={isLabel}
                    label={isLabel && goodLabelData ? reverseArr(goodLabelData)[index] : ''}
                    key={e}
                    value={String(e)}
                  />
                ))
              )}
            </div>
            <div onClick={(e) => onTrackClick(e.clientY)} className={styles.clickFillBar}>
              <div className={styles.fillBar}>
                <div
                  className={styles.valueBar}
                  style={{
                    height: trackHeight,
                  }}
                />
              </div>
            </div>
          </div>
        </div>
      </div>
      <div className={styles.valueBox}>
        {/* 如果存在labelData数组那么选出来的值显示对应labelData数组 */}
        {isLabel && !isMaxMin ? (
          <span className={styles.valText}>{goodSliderVal}</span>
        ) : (
          <span className={styles.valText}>{sliderVal}</span>
        )}
        {!isLabel && unitName ? <span className={styles.unitText}>{unitName}</span> : undefined}
      </div>
    </div>
  );
};

ColumnScaleSliderBar.defaultProps = {
  labelData: undefined,
  scaleData: undefined,
  minimum: undefined,
  maximum: undefined,
  disable: false,
  value: -999,
  unitName: 'dB',
  width: '12px',
  onSliderValueChange: () => {},
  isSet: false,
  step: 1,
  title: '',
};

ColumnScaleSliderBar.propTypes = {
  labelData: PropTypes.array,
  scaleData: PropTypes.array,
  minimum: PropTypes.number,
  maximum: PropTypes.number,
  width: PropTypes.any,
  disable: PropTypes.bool,
  value: PropTypes.number,
  unitName: PropTypes.string,
  onSliderValueChange: PropTypes.func,
  isSet: PropTypes.bool,
  step: PropTypes.number,
  title: PropTypes.string,
};

export default ColumnScaleSliderBar;
