import React, { useState } from 'react';
import dayjs from 'dayjs';
import { Calendar, Button } from 'dui';
import TimeTab from '../index';
import styles from './index.module.less';

const Demo = () => {
  const [value, setValue] = useState({
    // 是否同步
    advanced: false,
    startDate: ['5', '8', '9', '11', '12', '14', '15'],
    startHour: [3, 3, 3, 3, 3, 3, 4],
    startMin: [3, 3, 3, 3, 3, 3, 4],
    type: 'everymonth',
  });
  const [dateRange, setDateRange] = useState([dayjs(), dayjs().add(30, 'day')]);

  const getCrone = () => {
    const { type, startDate, startHour, startMin } = value;
    const crone = [];
    if (type === 'everymonth' || type === 'everyweek') {
      const y1 = dateRange[0].get('year');
      const m1 = dateRange[0].get('month') + 1;
      const y2 = dateRange[1].get('year');
      const m2 = dateRange[1].get('month') + 1;
      // 构造重复规则为每周的 没有的值
      let monthOfEveryWeek = '';
      if (y1 < y2) {
        // 跨年的情况
        for (let i = m1; i <= 12; i += 1) {
          monthOfEveryWeek = monthOfEveryWeek.length > 0 ? `${monthOfEveryWeek},${i}` : String(i);
        }
        for (let i = 1; i <= m2; i += 1) {
          monthOfEveryWeek = monthOfEveryWeek.length > 0 ? `${monthOfEveryWeek},${i}` : String(i);
        }
      } else {
        for (let i = m1; i <= m2; i += 1) {
          monthOfEveryWeek = monthOfEveryWeek.length > 0 ? `${monthOfEveryWeek},${i}` : String(i);
        }
      }
      startDate.forEach((d, indx) => {
        let h = 0;
        let m = 0;
        h = startHour[indx];
        m = startHour[indx];
        h = startHour[indx];
        m = startMin[indx];
        if (type === 'everyweek') {
          crone.push(`0 ${m} ${h} ? ${monthOfEveryWeek} ${d} *`);
        }
        if (type === 'everymonth') {
          crone.push(`0 ${m} ${h} ${d} * ? *`);
        }
      });
    } else {
      switch (type) {
        case 'once':
          {
            const h = startHour[0];
            const m = startMin[0];
            crone.push(
              `0 ${m} ${h} ${startDate[0].get('date')} ${startDate[0].get('month') + 1} ? ${startDate[0].get('year')}`,
            );
          }
          break;
        case 'everyday':
          {
            const h = startHour[0];
            const m = startMin[0];
            crone.push(`0 ${m} ${h} * * ? *`);
          }
          break;
        case 'freetime':
          crone.push('');
          break;
        default:
          break;
      }
    }
    console.log(crone);
  };

  return (
    <div className={styles.root}>
      <div className={styles.step}>
        <span>计划周期</span>
        <Calendar.Range minDate={dayjs()} value={dateRange} onChange={(d) => setDateRange(d)} />
      </div>
      <div className={styles.timeTab}>
        <TimeTab
          onceMinMax={dateRange}
          value={value}
          getValue={(e) => {
            setValue(e);
          }}
        />
      </div>
      <div>
        <Button onClick={getCrone}>crons转换</Button>
      </div>
    </div>
  );
};

export default Demo;
