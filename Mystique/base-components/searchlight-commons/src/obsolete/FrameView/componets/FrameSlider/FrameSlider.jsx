import React, { useEffect, useRef } from 'react';
import PropTypes from 'prop-types';
import styles from './index.module.less';

const FrameSlider = (props) => {
  const { value, time, timePercent, onValueChanged, onDragging } = props;

  const valueRef = useRef(null);

  const isMouseDownRef = useRef(false);
  const mouseXRef = useRef(null);
  const eventRef = useRef(null);

  const sliderRef = useRef(null);
  const trackRef = useRef(null);
  const progressRef = useRef(null);
  const thumbRef = useRef(null);

  const getRef = () => {
    const { current: slider } = sliderRef;
    const { current: track } = trackRef;
    const { current: progress } = progressRef;
    const { current: thumb } = thumbRef;
    if (track && progress && thumb) {
      return { slider, track, progress, thumb };
    }
    return null;
  };

  useEffect(() => {
    let startEvt = 'mousedown';
    let moveEvt = 'mousemove';
    let endEvt = 'mouseup';
    if ('ontouchstart' in window) {
      startEvt = 'touchstart';
      moveEvt = 'touchmove';
      endEvt = 'touchend';
    }
    eventRef.current = { startEvt, moveEvt, endEvt };

    window.addEventListener('resize', resize);
    return () => {
      window.removeEventListener('resize', resize);
    };
  }, []);

  useEffect(() => {
    valueRef.current = value;
    updateByValue(value);
  }, [value]);

  const updateByValue = (val) => {
    const tpt = getRef();
    if (tpt) {
      const { track, progress, thumb } = tpt;
      const w = track.offsetWidth;
      const k = thumb.offsetWidth / 2;
      const width = ((w - 2 * k) * val) / 100;
      thumb.style.left = `${width}px`;
      progress.style.width = `${width + k}px`;
    }
  };

  const resize = () => {
    updateByValue(valueRef.current);
  };

  const getX = (obj) => {
    let parent = obj;
    let left = obj.offsetLeft;
    while (parent.offsetParent) {
      parent = parent.offsetParent;
      left += parent.offsetLeft;
    }
    return left;
  };

  const onTrackDown = (args) => {
    const tpt = getRef();
    if (tpt) {
      const { slider, track, thumb } = tpt;
      const w = track.offsetWidth;
      const k = thumb.offsetWidth / 2;
      let width = args.clientX - getX(slider) - 2 * k;
      if (width < k) width = 0;
      if (width > w - 2 * k) width = w - 2 * k;
      const bili = (width * 100) / (w - 2 * k);
      onValueChanged(bili);
    }

    isMouseDownRef.current = true;

    mouseXRef.current = args.touches ? args.touches[0].clientX : args.clientX;

    const { moveEvt, endEvt } = eventRef.current;
    document.addEventListener(moveEvt, onThumbMove);
    document.addEventListener(endEvt, onThumbUp);

    onDragging(true);
  };

  const onTrackClick = (args) => {
    const tpt = getRef();
    if (tpt) {
      const { slider, track, thumb } = tpt;
      const w = track.offsetWidth;
      const k = thumb.offsetWidth / 2;
      let width = args.clientX - getX(slider) - 2 * k;
      if (width < k) width = 0;
      if (width > w - 2 * k) width = w - 2 * k;
      const bili = (width * 100) / (w - 2 * k);
      onValueChanged(bili, true);
    }
  };

  const onThumbDown = (e) => {
    e.stopPropagation();

    isMouseDownRef.current = true;

    mouseXRef.current = e.touches ? e.touches[0].clientX : e.clientX;

    const { moveEvt, endEvt } = eventRef.current;
    document.addEventListener(moveEvt, onThumbMove);
    document.addEventListener(endEvt, onThumbUp);

    onDragging(true);
  };

  const onThumbMove = (e) => {
    if (isMouseDownRef.current) {
      const tpt = getRef();
      if (tpt) {
        const { track, thumb } = tpt;
        const w = track.offsetWidth;
        const k = thumb.offsetWidth / 2;
        const rest = w - 2 * k;
        const position = e.touches ? e.touches[0].clientX : e.clientX;

        const moveL = position - mouseXRef.current;
        mouseXRef.current = position;
        let newL = thumb.offsetLeft + moveL;
        if (newL < 0) {
          newL = 0;
        }
        if (newL >= rest) {
          newL = rest;
        }
        const bili = (newL * 100) / (w - 2 * k);
        onValueChanged(bili);
      }
    }
  };

  const onThumbUp = (/* e */) => {
    isMouseDownRef.current = false;

    const { moveEvt, endEvt } = eventRef.current;
    document.removeEventListener(moveEvt, onThumbMove); // 解绑移动事件
    document.removeEventListener(endEvt, onThumbUp);

    onDragging(false);
  };

  return (
    <div ref={sliderRef} className={styles.slider}>
      <div className={styles.tag}>
        <span className={styles.percent}>{`${value.toFixed(0)} %`}</span>
        <span className={styles.time}>{time}</span>
      </div>
      <div
        ref={trackRef}
        className={styles.track}
        onClick={onTrackClick}
        onMouseDown={onTrackDown}
        onTouchStart={onTrackDown}
      >
        <div ref={progressRef} className={styles.progress} style={value <= 0 ? { display: 'none' } : null} />
        <div className={styles.stamps}>
          {timePercent?.map((tp, idx) => {
            // eslint-disable-next-line react/no-array-index-key
            return <div className={styles.tp} key={idx} style={{ left: `${tp}%` }} />;
          })}
        </div>
        <div ref={thumbRef} className={styles.thumb} onMouseDown={onThumbDown} onTouchStart={onThumbDown} />
      </div>
    </div>
  );
};

FrameSlider.defaultProps = {
  value: 0,
  time: '',
  timePercent: [],
  onValueChanged: () => {},
  onDragging: () => {},
};

FrameSlider.propTypes = {
  value: PropTypes.number,
  time: PropTypes.string,
  timePercent: PropTypes.any,
  onValueChanged: PropTypes.func,
  onDragging: PropTypes.func,
};

export default FrameSlider;
