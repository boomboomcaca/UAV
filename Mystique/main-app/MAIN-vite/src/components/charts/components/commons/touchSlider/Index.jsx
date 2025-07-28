import React, { useState, useRef } from "react";

import PropTypes from "prop-types";
import { ChartTypes } from "../../utils/enums";
import styles from "./style.module.less";
import { useEffect } from "react";

const TouchSlider = (props) => {
  const { style, position, sliderX, sliderY, onChange, visibleCharts } = props;
  const [thumbPosY, setThumbPosY] = useState(0);
  const [slidingY, setSlidingY] = useState(false);
  const slideStartRefY = useRef();
  // const [thumbYInSpectrum, setThumbYInSpectrum] = useState(true);
  const thumbYInChart = useRef(ChartTypes.spectrum);

  /**
   * @type {{current:HTMLDivElement}}
   */
  const domRefY = useRef();

  const [thumbPosX, setThumbPosX] = useState(0);
  const [slidingX, setSlidingX] = useState(false);
  const slideStartRefX = useRef();

  /**
   * @type {{current:HTMLDivElement}}
   */
  const domRefX = useRef();

  // 设置初始位置
  useEffect(() => {
    if (position) {
      const { x, y } = position;
      setThumbPosY(y);
      // const rectY = domRefX.current.getBoundingClientRect();
      // slideStartRefY.current = (rectY.height * y) / 100;
      setThumbPosX(x);
      // const rectX = domRefX.current.getBoundingClientRect();
      // slideStartRefX.current = (rectX.width * x) / 100;
      console.log(
        "update pos.....",
        position,
        slideStartRefY.current,
        slideStartRefX.current
      );
    }
  }, [position]);

  /**
   * 检查当前滑块位置所在图表区
   */
  useEffect(() => {
    if (visibleCharts) {
      thumbYInChart.current = "";
      if (visibleCharts.length === 2) {
        if (visibleCharts.includes(ChartTypes.spectrum) && thumbPosY < 50) {
          thumbYInChart.current = ChartTypes.spectrum;
        }
        if (visibleCharts.includes(ChartTypes.rain) && thumbPosY > 50) {
          thumbYInChart.current = ChartTypes.rain;
        }
      } else {
        if (visibleCharts.includes(ChartTypes.spectrum))
          thumbYInChart.current = ChartTypes.spectrum;
        if (visibleCharts.includes(ChartTypes.rain))
          thumbYInChart.current = ChartTypes.rain;
      }
      if (thumbYInChart.current === ChartTypes.spectrum) {
        // 在频谱上则统一居中
        setThumbPosY(25);
      }
    }
  }, [visibleCharts, thumbPosY]);

  return (
    <div className={styles.thumbRoot} style={{ ...style }}>
      {sliderY && (
        <div
          className={styles.thumbY}
          ref={domRefY}
          onTouchMove={(e) => {
            if (thumbYInChart.current === ChartTypes.rain) {
              const touch = e.changedTouches[0];
              const prev = slideStartRefY.current;
              // slideStartRefY.current = touch.clientY;

              // 计算百分比
              const rect = domRefY.current.getBoundingClientRect();
              const top =
                (thumbPosY * rect.height) / 100 + touch.clientY - prev;
              const pct = top / rect.height;
              setThumbPosY(pct * 100);
              if (onChange) {
                onChange({ y: pct, py: top, dragY: true });
                slideStartRefY.current = touch.clientY;
              }
            }
          }}
          onTouchEnd={() => setSlidingY(false)}
        >
          <div
            className={`${styles.sliderY} ${slidingY && styles.sliding}`}
            style={{ top: `calc(${thumbPosY}% - 66px)` }}
          >
            <div
              className={styles.thumbYTop}
              onClick={() => {
                if (thumbYInChart.current === ChartTypes.rain) {
                  // TODO 瀑布图上移一行
                  // TODO 如果已经是第一行，则将marker移动到频谱图
                }
              }}
            >
              上
            </div>
            <div
              className={styles.thumbTouch}
              onTouchStart={(e) => {
                const touch = e.changedTouches[0];
                slideStartRefY.current = touch.clientY;
                setSlidingY(true);
              }}
            />
            <div
              className={styles.thumbYBottom}
              onClick={() => {
                if (thumbYInChart.current === ChartTypes.rain) {
                  // TODO 瀑布图下移一行
                }
                if (
                  thumbYInChart.current === ChartTypes.spectrum &&
                  visibleCharts.includes(ChartTypes.rain)
                ) {
                  // TODO 如果在频谱图上，且显示了瀑布图，则直接跳到瀑布图的第一行
                  // 计算百分比
                  const rect = domRefY.current.getBoundingClientRect();
                  const top = rect.height / 2 + 9;
                  const pct = top / rect.height;
                  setThumbPosY(pct * 100);
                  if (onChange) {
                    onChange({ y: pct, py: top, dragY: true });
                  }
                }
              }}
            >
              下
            </div>
          </div>
        </div>
      )}
      {sliderX && (
        <div
          className={styles.thumbX}
          ref={domRefX}
          onTouchMove={(e) => {
            const touch = e.changedTouches[0];
            const prev = slideStartRefX.current;
            // slideStartRefX.current = touch.clientX;

            // 计算百分比
            const rect = domRefX.current.getBoundingClientRect();
            const left = (thumbPosX * rect.width) / 100 + touch.clientX - prev;
            const pct = left / rect.width;
            setThumbPosX(pct * 100);
            if (onChange) {
              console.log(pct, left);
              onChange({ x: pct, px: left, dragX: true });
              slideStartRefX.current = touch.clientX;
            }
          }}
          onTouchEnd={() => setSlidingX(false)}
        >
          <div
            className={`${styles.sliderX} ${slidingX && styles.sliding}`}
            style={{ left: `calc(${thumbPosX}% - 66px)` }}
          >
            <div className={styles.thumbXLeft}>左</div>
            <div
              className={styles.thumbTouch1}
              onTouchStart={(e) => {
                const touch = e.changedTouches[0];
                console.log("on touch start", touch);
                slideStartRefX.current = touch.clientX;
                setSlidingX(true);
              }}
            />
            <div className={styles.thumbXRight}>右</div>
          </div>
        </div>
      )}
    </div>
  );
};

TouchSlider.defaultProps = {
  style: "",
  position: undefined,
  sliderX: true,
  sliderY: true,
  visibleCharts: [ChartTypes.spectrum, ChartTypes.rain],
  onChange: () => {},
};

TouchSlider.propTypes = {
  style: PropTypes.string,
  position: PropTypes.any,
  sliderX: PropTypes.bool,
  sliderY: PropTypes.bool,
  onChange: PropTypes.func,
  visibleCharts: PropTypes.array,
};

export default TouchSlider;
