import React, { useState, useEffect } from 'react';
import PropTypes from 'prop-types';
// eslint-disable-next-line import/extensions
import { elementKey } from '@/lib/tools';
import SelectItem from './SelectItem.jsx';
import styles from './style.module.less';

export default function TimeSelect(props) {
  const { onChange, valueList } = props;
  const [hour, setHour] = useState(0);
  const [hour1, setHour1] = useState(0);
  const [min, setMin] = useState(0);
  const [min1, setMin1] = useState(0);
  const [hourItems, setHourItems] = useState([]);
  const [minItems, setMinItems] = useState([]);

  const timeItems = (len) => {
    const h = [];
    for (let i = 0; i < len; i += 1) {
      h.push({ key: elementKey(), value: i });
    }
    return h;
  };

  useEffect(() => {
    const hs = timeItems(24);
    setHourItems(hs);
    const ms = timeItems(60);
    setMinItems(ms);
    setHour(valueList.h);
    setTimeout(() => {
      setMin(valueList.m);
    }, 500);
    setTimeout(() => {
      setHour1(valueList.h1);
    }, 1500);
    setTimeout(() => {
      setMin1(valueList.m1);
    }, 2000);
  }, []);

  useEffect(() => {
    onChange({
      h: hour,
      m: min,
      h1: hour1,
      m1: min1,
    });
  }, [hour, hour1, min, min1]);

  return (
    <div className={styles.container}>
      <SelectItem
        data={hourItems}
        value={hour}
        onChange={(val) => {
          setHour(val);
          if (val * 60 + min > hour1 * 60 + min1) {
            setTimeout(() => {
              setHour1(val);
            }, 200);
          }
        }}
      />
      <span>:</span>
      <SelectItem
        data={minItems}
        value={min}
        onChange={(val) => {
          setMin(val);
          if (hour * 60 + val > hour1 * 60 + min1) {
            setTimeout(() => {
              setMin1(val);
            }, 200);
          }
        }}
      />
      <span>至</span>
      <SelectItem
        data={hourItems}
        value={hour1}
        onChange={(val) => {
          setHour1(val);
          if (hour * 60 + min > val * 60 + min1) {
            setTimeout(() => {
              setHour(val);
            }, 200);
          }
        }}
      />
      <span>:</span>
      <SelectItem
        data={minItems}
        value={min1}
        onChange={(val) => {
          setMin1(val);
          if (hour * 60 + min > hour1 * 60 + val) {
            setTimeout(() => {
              setMin(val);
            }, 200);
          }
        }}
      />
    </div>
  );
}
TimeSelect.defaultProps = {
  valueList: {
    h: 0,
    m: 0,
    h1: 0,
    m1: 0,
  },
  onChange: () => {},
};

TimeSelect.propTypes = {
  valueList: PropTypes.object,
  onChange: PropTypes.func,
};
