/* eslint-disable jsx-a11y/mouse-events-have-key-events */
/* eslint-disable max-len */
import React, { useEffect, useMemo, useRef, useState } from 'react';
import PropTypes from 'prop-types';
import MajorTick from './components/MajroTick.jsx';
import MinorTick from './components/MinorTick.jsx';
import sliderImg from '../../assets/slider.png';
import styles from './sliderbar.module.less';

const SliderBar = (props) => {
  const {
    disable,
    minimum = 0,
    maximum = 0,
    value,
    sliderValue,
    unitName,
    stitle,
    onSliderValueChange,
    colorOptions,
    decimal,
  } = props;

  // 指针对应的值
  const [sliderVal, setSliderVal] = useState(0);
  // 获取填充柱子dom
  const moveArea = useRef();
  // 是否开始拖拽
  const starting = useRef(false);

  useEffect(() => {
    const snap = minimum > sliderValue ? minimum : maximum < sliderValue ? maximum : sliderValue;
    setSliderVal(snap);
  }, [sliderValue]);

  // 开始拖拽
  const startDragging = () => {
    if (disable) {
      return;
    }
    starting.current = true;
  };

  // 拖拽中
  const onDragging = (clientY) => {
    if (starting.current) {
      const fillRect = moveArea.current.getBoundingClientRect();
      const percent = (clientY - fillRect.y) / fillRect.height;
      // 计算值
      // const snapval = Math.round((maximum - minimum + 1) * (1 - percent) + minimum);
      let snapval = ((maximum - minimum + 1) * (1 - percent) + minimum).toFixed(decimal);
      snapval = Number(snapval);
      if (snapval <= maximum && snapval >= minimum) {
        setSliderVal(snapval);
        onSliderValueChange?.({
          value: snapval,
          end: false,
        });
      }
    }
  };

  // 停止拖拽
  const stopDragging = () => {
    if (starting.current) {
      starting.current = false;
      onSliderValueChange?.({
        value: sliderVal,
        end: true,
      });
    }
  };

  // 条条背景
  const bgcolor = useMemo(() => {
    if (colorOptions.gradientColors) {
      const colors = colorOptions.gradientColors;
      const len = colors.length;
      if (len === 1) {
        return { backgroundColor: colors[0] };
      }
      return {
        backgroundImage: `linear-gradient(to bottom, ${colors[0]} , ${colors[len - 1]})`,
      };
    }
    return {};
  }, [colorOptions]);

  // 条条top
  const trackTop = useMemo(() => {
    if (value >= maximum) {
      return '0%';
    }
    if (value <= minimum) {
      return '100%';
    }
    const gap = value - minimum;
    const percent = gap / (maximum - minimum);
    return `${((1 - percent) * 100).toFixed(2)}%`;
  }, [value, minimum, maximum]);

  // 滑块图标top
  const sliderTop = useMemo(() => {
    const gap = maximum - sliderVal;
    let percent = (gap * 100) / (maximum - minimum);
    if (percent < 0) percent = 0;
    if (percent > 100) percent = 100;
    return `${percent.toFixed(2)}%`;
  }, [sliderVal, minimum, maximum]);

  return (
    <div className={styles.rootConnew}>
      {stitle && <div className={styles.stitle}>{stitle}</div>}
      <div className={styles.sbrootnew}>
        <div className={styles.layer1}>
          <MajorTick label={String(maximum)} unit={unitName} options={colorOptions} />
          <MinorTick />
          <MinorTick />
          <MinorTick />
          <MajorTick label={String(minimum)} unit="" options={colorOptions} />
        </div>
        <div className={styles.layer2}>
          <div className={styles.track}>
            <div className={styles.fillBar} style={bgcolor}>
              <div
                className={styles.valueBar}
                style={{
                  height: trackTop,
                }}
              />
            </div>
          </div>
        </div>

        <div
          className={styles.newlayer3}
          ref={moveArea}
          onMouseMove={(e) => onDragging(e.clientY)}
          onTouchMove={(e) => onDragging(e.targetTouches[0].clientY)}
          onMouseUp={stopDragging}
          onMouseLeave={stopDragging}
          onTouchEnd={stopDragging}
        >
          <div className={styles.dragSlider} style={{ top: sliderTop }}>
            <div className={styles.sliderLabel} style={{ paddingLeft: `${25 - String(sliderVal).length * 5}px` }}>
              {sliderVal}
            </div>
            <div className={styles.slider} onTouchStart={startDragging} onMouseDown={startDragging}>
              <img src={sliderImg} alt="" />
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

SliderBar.defaultProps = {
  disable: false,
  minimum: -20,
  maximum: 120,
  value: -999,
  sliderValue: 30,
  unitName: 'dBμV',
  stitle: null,
  onSliderValueChange: () => {},
  colorOptions: {
    gradientColors: ['#FF2F2A', '#1456FF'],
  },
  // 小数位数
  decimal: 0,
};

SliderBar.propTypes = {
  disable: PropTypes.bool,
  minimum: PropTypes.number,
  maximum: PropTypes.number,
  value: PropTypes.number,
  sliderValue: PropTypes.number,
  unitName: PropTypes.string,
  stitle: PropTypes.string,
  onSliderValueChange: PropTypes.func,
  colorOptions: PropTypes.object,
  decimal: PropTypes.number,
};

export default SliderBar;
