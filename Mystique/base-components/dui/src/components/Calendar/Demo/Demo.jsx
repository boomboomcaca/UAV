import React, { useState } from 'react';
import dayjs from 'dayjs';
import Calendar from '../index';

export default function Demo() {
  const [date, setDate] = useState();
  const [date2, setDate2] = useState([]);
  const [date3, setDate3] = useState([]);
  return (
    <div>
      日历组件严格采用dayjs库
      <br />
      <br />
      <br />
      <Calendar value={date} maxDate={dayjs()} onChange={(ddd) => setDate(ddd)} />
      <br />
      <br />
      <Calendar
        value={date}
        onChange={(ddd) => {
          setDate(ddd);
        }}
        minDate={dayjs().subtract(3, 'months')}
        maxDate={dayjs()}
        type="daytime"
        theme="white"
      />
      <br />
      <Calendar value={date} onChange={(ddd) => setDate(ddd)} type="daytime" theme="white" />
      <br />
      <br />
      <br />
      <Calendar.Range value={date3} onChange={(ddd) => setDate3(ddd)} />
      <br />
      <br />
      <br />
      <Calendar.Range value={date3} onChange={(ddd) => setDate3(ddd)} type="daytime" position="bottomleft" />
      <br />
      <br />
      <br />
      <Calendar value={date} onChange={(ddd) => setDate(ddd)} type="daytime" />
    </div>
  );
}
