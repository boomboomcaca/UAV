import React, { useEffect, useState, useRef } from 'react';
import { SwitchButton } from 'dui';

import PropTypes from 'prop-types';
import style from './chart.module.less';
import layer1 from './assets/layer1.png';
import layer1white from './assets/layer1white.png';
import layer3 from './assets/layer3.png';
import layer3white from './assets/layer3white.png';
import layer4 from './assets/layer4.png';
import layer4r from './assets/layer4_r.png';
import compassIcon from './assets/compass.png';
import seatIcon from './assets/seat.png';
// layer 5 质量电平门限，需要单独做
// import ThresholdView from './components/ThresholdView';

import layer6 from './assets/layer6.png';
import layer8Light from './assets/layer8_l.png';

import layer8 from './assets/layer8.png';
import layer8s from './assets/layer8s.png';
import layer9 from './assets/layer9.png';
import layer9white from './assets/layer9white.png';

import pointer1 from './assets/layer801.png';
import pointer2 from './assets/layer802.png';
import pointer3 from './assets/layer803.png';
import pointer4 from './assets/layer804.png';

const positionDict = {
  left: 'flex-start',
  right: 'flex-end',
  center: 'center',
};

/**
 *
 * @param {Object} props
 * @param {boolean} props.northView 正北模式
 * @param {number} props.bearing 指针角度
 * @param {number} props.realtimeBearing 实时值
 * @param {number} props.compass 罗盘值
 * @param {boolean} props.show 隐藏显示
 * @param {boolean} props.showSwitch 是否显示模式切换按钮
 * @param {Function} props.onSwitch 模式切换触发事件
 * @param {boolean} props.realTimeBearingFlag 显示实时指针
 * @returns
 */
const Chart = (props) => {
  // northView 当前是否为正北视图，默认为 true，false显示相对
  // bearing 测向角度（示向度），默认值 -1 （无效值）
  // compass 罗盘角度，默认值 -1 （无效值）
  // specChart 频谱图插槽，空位，默认值undefined

  const {
    northView,
    bearing,
    realtimeBearing,
    compass,
    specChart,
    show,
    showSwitch,
    onSwitch,
    position,
    realTimeBearingFlag,
    theme,
    displayBearing,
    showIcon,
  } = props;

  const containerRef = useRef();
  const [polarRect, setPolarRect] = useState({
    x: 0,
    y: 0,
    width: 500,
    height: 500,
  });

  const pointers = [pointer1, pointer2, pointer3, pointer4];

  // 当前显示角度，保留一位小数
  const [angle, setAngle] = useState(-1);
  const [angle2, setAngle2] = useState(-1);
  const [angleLabel, setAngleLabel] = useState('--');

  useEffect(() => {
    // 计算指针位置和显示角度
    if (bearing >= 0) {
      if (northView) {
        // 正北显示
        const c = compass >= 0 && compass <= 360 ? compass : 0;
        const val = (bearing + c) % 360;
        setAngle(val);
        setAngleLabel(`${val.toFixed(1)}°`);
      } else {
        // 相对显示
        setAngle(bearing);
        setAngleLabel(`${bearing.toFixed(1)}°`);
      }
    } else {
      setAngleLabel('--');
    }
  }, [bearing]);

  useEffect(() => {
    if (bearing instanceof Array && typeof displayBearing === 'number') {
      if (displayBearing >= 0 && displayBearing <= 360) setAngleLabel(`${displayBearing.toFixed(1)}°`);
      else setAngleLabel(`--`);
    }
  }, [displayBearing, bearing]);

  useEffect(() => {
    // 计算指针位置和显示角度
    if (realtimeBearing >= 0) {
      if (northView) {
        // 正北显示
        const c = compass >= 0 && compass <= 360 ? compass : 0;
        const val = (realtimeBearing + c) % 360;
        setAngle2(val);
      } else {
        // 相对显示
        setAngle2(realtimeBearing);
      }
    }
  }, [realtimeBearing]);

  /**
   * 监听尺寸变化
   * 动态填充
   */
  useEffect(() => {
    let resizeObserver;
    if (containerRef.current) {
      resizeObserver = new ResizeObserver((entries) => {
        if (entries.length > 0) {
          const rect = entries[0].contentRect;

          const SIZE = Math.min(rect.height, rect.width);

          const trueFontSize = Math.floor((48 / 700) * SIZE);
          let x = 0;
          let y = 0;

          // if (northView) {
          //   // 缩小对齐
          //   x = width * 0.027;
          //   y = width * 0.054;
          //   width -= width * 0.054;
          // }
          setPolarRect({
            x,
            y,
            width: SIZE,
            height: SIZE,
            fontSize: trueFontSize,
          });
          // console.log('Size changed', rect);
        }
      });
      resizeObserver.observe(containerRef.current);
    }
    return () => {
      if (resizeObserver) {
        resizeObserver.unobserve(containerRef.current);
        resizeObserver.disconnect();
      }
    };
  }, [containerRef]);

  return (
    <div className={style.chartRoot} ref={containerRef} style={{ display: show ? 'block' : 'none' }}>
      <div className={style.polarContainer} style={{ justifyContent: positionDict[position] }}>
        <div
          style={{
            position: 'relative',
            // left: `${polarRect.x}px`,
            // top: `${polarRect.y}px`,
            width: `${polarRect.width}px`,
            height: `${polarRect.height}px`,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
          }}
        >
          {/* 背景图 */}
          <div
            className={style.layerBase}
            style={{
              zIndex: 0,
              backgroundImage: `url(${theme === 'light' ? layer1white : layer1}) `,
              backgroundSize: '100% 100%',
            }}
          />
          <div
            style={{
              position: 'absolute',
              // left: northView ? '0' : '2.7%',
              // width: northView ? '100%' : '94.6%',
              // top: northView ? '0' : '5.4%',
              // height: northView ? '100%' : '94.6%',
              left: '0',
              width: '100%',
              top: '0',
              height: '100%',
            }}
          >
            {/* 固定图层1 */}
            <div
              className={style.layerBase}
              style={{
                zIndex: 1,
                backgroundImage: `url(${theme === 'light' ? layer3white : layer3}) `,
                backgroundSize: '100% 100%',
              }}
            />
            <div
              className={style.layerBase}
              style={{
                zIndex: 2,
                backgroundImage: `url(${northView ? layer4 : layer4r}) `,
                backgroundSize: '100% 100%',
              }}
            />
            <div
              className={style.layerBase}
              style={{
                zIndex: 3,
                backgroundImage: `url(${layer6}) `,
                backgroundSize: '100% 100%',
              }}
            />
            {/* 图层8 指针 */}

            {typeof bearing === 'number' && bearing >= 0 && (
              <>
                <div
                  className={`${style.layerBase} ${style.ggPointer}`}
                  style={{
                    zIndex: 4,
                    backgroundImage: `url(${layer8}) `,
                    backgroundSize: '100% 100%',

                    transform: `rotate(${angle}deg)`,
                  }}
                />
                <div
                  className={`${style.layerBase} ${style.ggPointer}`}
                  style={{
                    zIndex: 0,
                    backgroundImage: `url(${layer8Light}) `,
                    backgroundSize: '100% 100%',

                    transform: `rotate(${angle - 2}deg)`,
                  }}
                />
              </>
            )}
            {bearing instanceof Array &&
              bearing.map((p, index) => (
                <div
                  className={`${style.layerBase} ${style.ggPointer}`}
                  style={{
                    zIndex: 4,
                    backgroundImage: `url(${pointers[index]}) `,
                    backgroundSize: '100% 100%',

                    transform: `rotate(${p}deg)`,
                  }}
                />
              ))}
            {realTimeBearingFlag && realtimeBearing >= 0 && (
              <div
                className={`${style.layerBase} ${style.ggPointer}`}
                style={{
                  zIndex: 5,
                  backgroundImage: `url(${layer8s}) `,
                  backgroundSize: '100% 100%',

                  transform: `rotate(${angle2}deg)`,
                }}
              />
            )}
            {/* 图层8.1 频谱图插槽 */}
            {specChart && (
              <div
                style={{
                  position: 'absolute',
                  zIndex: 6,
                  left: '35.8%',
                  top: '67.5%',
                  width: '28%',
                  height: '19%',
                }}
              >
                {specChart}
              </div>
            )}
            {/* 图层9 中心圆 */}
            <div
              className={style.layerBase}
              style={{
                zIndex: 7,
                backgroundImage: `url(${theme === 'light' ? layer9white : layer9}) `,
                backgroundSize: '100% 100%',
              }}
            />

            {/* 示向度 */}
            <div
              style={{
                position: 'absolute',
                bottom: 0,
                zIndex: 8,
                color: 'var(--theme-font-100)',
                width: '100%',
                height: '100%',
                display: 'flex',
                flexDirection: 'row',
                justifyContent: 'center',
                alignItems: 'center',
                fontSize: `${polarRect.fontSize}px`,
              }}
            >
              {angleLabel}
            </div>
            {showSwitch && (
              <div
                className={style.switchImg}
                style={{
                  width: `${polarRect.width * 0.115}px`,
                  height: `${polarRect.height * 0.115}px`,
                }}
                onClick={() => {
                  typeof onSwitch === 'function' && onSwitch(northView);
                }}
              >
                <img src={northView ? compassIcon : seatIcon} alt="switch" />
              </div>
            )}
            {showIcon && (
              <div className={style.switchImg2}>
                <img src={northView ? compassIcon : seatIcon} alt="switch" />
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

Chart.defaultProps = {
  northView: true,
  bearing: -1,
  compass: -1,
  realtimeBearing: -1,
  specChart: undefined,

  show: true,
  showSwitch: false,
  showIcon: false,
  onSwitch: () => {},
  position: 'center',
  realTimeBearingFlag: false,
  theme: 'dark',
};

Chart.propTypes = {
  northView: PropTypes.bool,
  bearing: PropTypes.number,
  compass: PropTypes.number,
  realtimeBearing: PropTypes.number,
  specChart: PropTypes.any,
  showIcon: PropTypes.bool,
  show: PropTypes.bool,
  showSwitch: PropTypes.bool,
  onSwitch: PropTypes.func,
  position: PropTypes.string,
  realTimeBearingFlag: PropTypes.bool,
  theme: PropTypes.string,
};

export default Chart;
