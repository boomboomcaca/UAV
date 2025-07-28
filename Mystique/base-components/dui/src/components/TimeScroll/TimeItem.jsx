import React, { useRef, useEffect } from 'react';
import PropTypes from 'prop-types';
import styles from './style.module.less';

export default function TimeItem(props) {
  const { onChange, data, value } = props;
  const selectRef = useRef();
  useEffect(() => {
    if (selectRef.current) {
      selectRef.current.scrollTo({
        top: value * 24,
        behavior: 'smooth',
      });
    }
  }, [value]);
  return (
    <div
      className={styles.box}
      ref={selectRef}
      onScroll={(e) => {
        const a = Math.round(e.target.scrollTop / 24);
        onChange(a);
      }}
    >
      <span className={styles.top} />
      {data.map((h) => {
        if (value === h.value) {
          return (
            <span key={h.key} className={styles.selectedItem} style={{ display: 'block' }}>
              {h.value < 10 ? `0${h.value}` : h.value}
            </span>
          );
        }
        return (
          <span key={h.key} className={styles.normalItem} style={{ display: 'block' }}>
            {h.value < 10 ? `0${h.value}` : h.value}
          </span>
        );
      })}
      <span className={styles.top} />
    </div>
  );
}

TimeItem.propTypes = {
  value: PropTypes.number.isRequired,
  data: PropTypes.array.isRequired,
  onChange: PropTypes.func.isRequired,
};
