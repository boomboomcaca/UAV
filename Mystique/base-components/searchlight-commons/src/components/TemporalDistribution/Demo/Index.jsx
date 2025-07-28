import React, { useState, useEffect } from 'react';
import { Radio, Select } from 'dui';
import TemporalDistribution from '../TemporalDistribution.jsx';
import styles from './index.module.less';

const { Option } = Select;

export default () => {
  const radioOption = [
    { label: '黑广播', value: 'normal' },
    { label: '水上检测', value: 'aump' },
  ];
  const timeOption = [
    { label: '时间1', value: { start: '2021-11-18 23:00:00', end: '2021-11-20 13:59:59' } },
    { label: '时间2', value: { start: '2021-11-19 23:00:00', end: '2021-11-21 13:59:59' } },
  ];
  const [purpose, setPurpose] = useState('aump');
  const [rangeTime, setRangeTime] = useState({ start: '2021-11-18 23:00:00', end: '2021-11-20 13:59:59' });
  const [timeData, setTimeData] = useState([
    { startTime: '2021-11-19 00:00:00', endTime: '2021-11-19 01:59:59' },
    { startTime: '2021-11-19 03:00:00', endTime: '2021-11-19 06:59:59' },
    { startTime: '2021-11-19 09:00:00', endTime: '2021-11-19 09:59:59' },
    { startTime: '2021-11-19 13:00:00', endTime: '2021-11-19 13:59:59' },
    { startTime: '2021-11-19 17:00:00', endTime: '2021-11-19 17:59:59' },
    { startTime: '2021-11-19 21:00:00', endTime: '2021-11-19 21:59:59' },
  ]);
  const onValueChanged = (val) => {
    setPurpose(val);
  };
  const onTimeValueChanged = (val) => {
    setRangeTime(val);
  };
  return (
    <div>
      <Radio options={radioOption} value={purpose} onChange={onValueChanged} />
      <Select value={rangeTime} onChange={onTimeValueChanged}>
        {timeOption.map((item) => (
          <Option value={item.value} key={item.value}>
            {item.label}
          </Option>
        ))}
      </Select>
      <TemporalDistribution
        key={`TemporalDistribution-${purpose}`}
        interval={48}
        timeData={timeData}
        purpose={purpose}
        rangeTime={rangeTime}
      />
    </div>
  );
};
