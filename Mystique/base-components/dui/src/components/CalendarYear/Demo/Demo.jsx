import React, { useState } from 'react';
// import dayjs from 'dayjs';
import CalendarYear from '../index';

export default function Demo() {
  const [date, setDate] = useState(2021);
  return (
    <div>
      <CalendarYear
        data={[1995, 2012, 2021, 2022, 2011, 2018, 2006, 2008, 2007, 1993, 1986]}
        value={date}
        onChange={(ddd) => setDate(ddd)}
      />
      <CalendarYear value={date} onChange={(ddd) => setDate(ddd)} />
    </div>
  );
}
