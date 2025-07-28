import React, { useState, useEffect } from 'react';
import { Button, Calendar } from 'dui';
import TimeSlidingBlock from '../TimeSlidingBlock.jsx';
import styles from './index.module.less';

export default () => {
  const [purpose, setPurpose] = useState('aump');
  const [rangeTime, setRangeTime] = useState();
  const [date, setDate] = useState();
  const [timeData, setTimeData] = useState([]);
  const presetTime = () => {
    setRangeTime({ start: '2022-05-18 23:00:00', end: '2022-05-20 13:59:59' });
    setTimeData([
      { startTime: '2022-05-18 01:00:00', endTime: '2022-05-18 01:59:59' },
      { startTime: '2022-05-18 03:00:00', endTime: '2022-05-18 06:59:59' },
      { startTime: '2022-05-19 09:00:00', endTime: '2022-05-19 09:59:59' },
      { startTime: '2022-05-20 13:00:00', endTime: '2022-05-20 13:59:59' },
      { startTime: '2022-05-21 17:00:00', endTime: '2022-05-21 17:59:59' },
      { startTime: '2022-05-22 21:00:00', endTime: '2022-05-22 21:40:59' },
    ]);
  };
  const onCalendarChange = (ddd) => {
    setDate(ddd);
    setRangeTime({
      start: `${ddd[0].$y}-${ddd[0].$M + 1}-${ddd[0].$D} 00:00:00`,
      end: `${ddd[1].$y}-${ddd[1].$M + 1}-${ddd[1].$D} 00:00:00`,
    });
  };
  return (
    <div className={styles.container}>
      <div>
        <Button onClick={presetTime}>设置rangeTime</Button>
        <Calendar.Range value={date} onChange={onCalendarChange} />
      </div>
      <TimeSlidingBlock
        faultTolerant={false}
        timeData={timeData}
        rangeTime={rangeTime}
        resetStyle={{
          bgColor: '#555',
          dataColor: '#464673',
          coverColor: 'red',
        }}
        recall={(e) => {
          console.log('recall--->', e);
        }}
      />
    </div>
  );
};
