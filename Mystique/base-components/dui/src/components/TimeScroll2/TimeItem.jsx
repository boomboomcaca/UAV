import React, { useRef, useEffect, useState, useMemo } from 'react';
import PropTypes from 'prop-types';
import BScroll from '@better-scroll/core';
import MouseWheel from '@better-scroll/mouse-wheel';
import styles from './style.module.less';

BScroll.use(MouseWheel);
export default function TimeItem(props) {
  const { onChange, data, value } = props;
  const [list, setList] = useState(data);
  const selectRef = useRef();
  const ref = useRef(null);

  useEffect(() => {
    ref.current = value;
  }, [value]);

  useEffect(() => {
    if (data.length > 0) {
      setArry();
    }
  }, [data]);

  useEffect(() => {
    if (selectRef.current) {
      const bs = new BScroll(selectRef.current, {
        mouseWheel: {
          speed: 0,
          invert: false,
          easeTime: 1000,
          discreteTime: 0,
          throttleTime: 0,
          dampingFactor: 0.0001,
        },
      });
      // bs.on('mousewheelMove', (e) => {
      //   setData(e);
      // });
      bs.on('mousewheelEnd', (e) => {
        setData(e);
      });
    }
  }, [selectRef.current]);

  const setData = (e) => {
    const { x, y } = e;
    const top = data[data.length - 1];
    if (top) {
      if (y > 0) {
        ref.current = ref.current === 0 ? data.length - 1 : ref.current - 1;
      } else {
        ref.current = ref.current <= data.length - 2 ? ref.current + 1 : data.length - 1 - ref.current;
      }
      onChange(ref.current);
    }
  };
  const setArry = () => {
    let t = [];
    const top = data[data.length - 1];
    if (ref.current === 0) {
      t = [top, data[ref.current], data[ref.current + 1]];
    } else if (ref.current >= data.length - 2) {
      const i = data.length - 1 - ref.current;
      t = [data[ref.current - 1], data[ref.current], data[i === 1 ? ref.current + i : i]];
    } else {
      t = [data[ref.current - 1], data[ref.current], data[ref.current + 1]];
    }
    setList(t);
  };
  useMemo(() => {
    setArry();
  }, [ref.current]);

  return (
    <div className={styles.box} ref={selectRef}>
      <div className={styles.boxwrapper}>
        {list.map((h) => {
          if (value === h?.value) {
            return (
              <span key={h?.key} className={styles.selectedItem} style={{ display: 'block' }}>
                {h?.value < 10 ? `0${h?.value}` : h?.value}
              </span>
            );
          }
          return (
            <span key={h?.key} className={styles.normalItem} style={{ display: 'block' }}>
              {h?.value < 10 ? `0${h?.value}` : h?.value}
            </span>
          );
        })}
      </div>
    </div>
  );
}

TimeItem.propTypes = {
  value: PropTypes.number.isRequired,
  data: PropTypes.array.isRequired,
  onChange: PropTypes.func.isRequired,
};
