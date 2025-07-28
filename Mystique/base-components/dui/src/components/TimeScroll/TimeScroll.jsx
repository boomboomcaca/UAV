import React, { useState, useEffect } from 'react';
import PropTypes from 'prop-types';
// eslint-disable-next-line import/extensions
import { elementKey } from '@/lib/tools';
import TimeItem from './TimeItem.jsx';
import styles from './style.module.less';

export default function TimeScroll(props) {
  const { onChange, valueList,rangeSelection } = props;
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
    setMin(valueList.m);
    setHour1(valueList.h1);
    setMin1(valueList.m1);
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
    <div className={styles.container} style={{width:rangeSelection?"300px":"120px"}}>
      <TimeItem
        data={hourItems}
        value={hour}
        onChange={(val) => {
          setHour(val);
        }}
      />
      <span>:</span>
      <TimeItem
        data={minItems}
        value={min}
        onChange={(val) => {
          setMin(val);
        }}
      />
      {rangeSelection &&
      <>
      <span>至</span>
      <TimeItem
        data={hourItems}
        value={hour1}
        onChange={(val) => {
          setHour1(val);
        }}
      />
      <span>:</span>
      <TimeItem
        data={minItems}
        value={min1}
        onChange={(val) => {
          setMin1(val);
        }}
      /></>}
    </div>
  );
}
TimeScroll.defaultProps = {
  valueList: {
    h: 0,
    m: 0,
    h1: 0,
    m1: 0,
  },
  onChange: () => {},
  rangeSelection:true
};

TimeScroll.propTypes = {
  valueList: PropTypes.object,
  onChange: PropTypes.func,
  rangeSelection:PropTypes.bool
};
