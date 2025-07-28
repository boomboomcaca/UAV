import React, { useState, useEffect, useRef } from 'react';
import PropTypes from 'prop-types';
import { elementKey } from '../utils';
import styles from './style.module.less';

/**
 * 函数组件
 * 下拉选择面板
 */
const timeItems = (len) => {
  const h = [];
  for (let i = 0; i < len; i += 1) {
    h.push({ key: elementKey(), value: i });
  }
  return h;
};
const TimeSelector = (props) => {
  const { onChange, value } = props;
  const [showPopover, setShowPopover] = useState(false);
  const [selHour, setSelHour] = useState(value.h);
  const [selMinute, setSelMinute] = useState(value.m);
  const hourItems = timeItems(24);
  const minItems = timeItems(60);
  const ref1 = useRef();
  const ref2 = useRef();

  useEffect(() => {
    setSelHour(value.h);
    setSelMinute(value.m);
  }, [value]);

  useEffect(() => {
    const a = (e) => {
      if (showPopover) {
        if (e.target !== ref1.current) setShowPopover(false);
      }
    };
    document.addEventListener('click', a);
    return () => {
      document.removeEventListener('click', a);
    };
  }, [showPopover]);

  return (
    <div className={styles.timeSelector}>
      <div
        className={styles.valuelabel}
        ref={ref1}
        onClick={(e) => {
          e.stopPropagation();
          setShowPopover(!showPopover);
        }}
      >
        <span>{selHour < 10 ? `0${selHour}` : selHour}</span>
        <span>:</span>
        <span>{selMinute < 10 ? `0${selMinute}` : selMinute}</span>
      </div>
      {showPopover && (
        <div className={styles.popPanel} ref={ref2}>
          <div className={styles.selItem}>
            <span className={styles.label}>时</span>
            <div>
              {hourItems.map((h) => {
                return (
                  <span
                    key={h.key}
                    className={selHour === h.value ? styles.selectedItem : styles.normalItem}
                    onClick={(e) => {
                      e.stopPropagation();
                      // setSelHour(h.value);
                      onChange({
                        h: h.value,
                        m: selMinute,
                      });
                    }}
                    onTouchEnd={() => {
                      onChange({
                        h: h.value,
                        m: selMinute,
                      });
                      // setSelHour(h.value);
                    }}
                    style={{ display: 'block' }}
                  >
                    {h.value < 10 ? `0${h.value}` : h.value}
                  </span>
                );
              })}
            </div>
          </div>
          <div className={styles.selItem}>
            <span className={styles.label}>分</span>
            <div>
              {minItems.map((m) => (
                <span
                  key={m.key}
                  className={selMinute === m.value ? styles.selectedItem : styles.normalItem}
                  onClick={(e) => {
                    e.stopPropagation();
                    onChange({
                      h: selHour,
                      m: m.value,
                    });
                  }}
                  onTouchEnd={() => {
                    onChange({
                      h: selHour,
                      m: m.value,
                    });
                    setSelMinute(m.value);
                  }}
                  style={{ display: 'block' }}
                >
                  {m.value < 10 ? `0${m.value}` : m.value}
                </span>
              ))}
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

TimeSelector.defaultProps = {
  value: { h: 0, m: 0 },
  onChange: () => {},
};

TimeSelector.propTypes = {
  value: PropTypes.object,
  onChange: PropTypes.func,
};

export default TimeSelector;
