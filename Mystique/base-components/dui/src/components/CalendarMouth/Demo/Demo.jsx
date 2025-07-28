import React, { useState } from 'react';
import CalendarMouth from '../index';

export default function Demo() {
  const [date1, setDate1] = useState({
    year: 2019,
    mouth: 4,
  });
  const [date, setDate] = useState({
    year: 2018,
    mouth: 9,
  });
  const data = [
    {
      year: 2021,
      mouth: [4, 5, 9, 8, 3, 7],
    },
    {
      year: 2019,
      mouth: [4, 1, 9, 7, 3, 1, 6],
    },
    {
      year: 2018,
      mouth: [4, 5, 9],
    },
    {
      year: 2016,
      mouth: [4, 1, 10, 8, 3],
    },
  ];
  return (
    <div>
      <CalendarMouth data={data} value={date1} onChange={(ddd) => setDate1(ddd)} />
      <CalendarMouth value={date} onChange={(ddd) => setDate(ddd)} />
    </div>
  );
}
