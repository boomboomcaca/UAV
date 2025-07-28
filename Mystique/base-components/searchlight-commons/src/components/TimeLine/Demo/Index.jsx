import React, { useState, useEffect } from 'react';
import TimeLine from '../TimeLine.jsx';

export default () => {
  const [rangeTime, setRangeTime] = useState({ start: '2021-11-18 23:00:00', end: '2021-11-20 13:59:59' });
  const [timeData, setTimeData] = useState([
    { startTime: '2021-11-19 00:00:00', endTime: '2021-11-19 01:59:59' },
    { startTime: '2021-11-19 03:00:00', endTime: '2021-11-19 06:59:59' },
    { startTime: '2021-11-19 09:00:00', endTime: '2021-11-19 09:59:59' },
    { startTime: '2021-11-19 13:00:00', endTime: '2021-11-19 13:59:59' },
    { startTime: '2021-11-19 17:00:00', endTime: '2021-11-19 17:59:59' },
    { startTime: '2021-11-19 21:00:00', endTime: '2021-11-19 21:59:59' },
  ]);
  return (
    <div>
      <TimeLine option={{ timeData, rangeTime }} />
    </div>
  );
};
