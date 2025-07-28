import React, { useRef, useEffect } from 'react';
import PropTypes from 'prop-types';
import styles from './style.module.less';

export default function SelectItem(props) {
  const { onChange, data, value } = props;
  const selectRef = useRef();
  useEffect(() => {
    if (selectRef.current) {
      selectRef.current.scrollIntoView({
        behavior: 'smooth',
        block: 'center',
      });
    }
  }, [value]);
  return (
    <div className={styles.box}>
      <span className={styles.top} />
      {data.map((h) => {
        if (value === h.value) {
          return (
            <span key={h.key} className={styles.selectedItem} ref={selectRef} style={{ display: 'block' }}>
              {h.value < 10 ? `0${h.value}` : h.value}
            </span>
          );
        }
        return (
          <span
            key={h.key}
            className={styles.normalItem}
            onClick={() => {
              onChange(h.value);
            }}
            style={{ display: 'block' }}
          >
            {h.value < 10 ? `0${h.value}` : h.value}
          </span>
        );
      })}
      <span className={styles.top} />
    </div>
  );
}

SelectItem.propTypes = {
  value: PropTypes.number.isRequired,
  data: PropTypes.array.isRequired,
  onChange: PropTypes.func.isRequired,
};
