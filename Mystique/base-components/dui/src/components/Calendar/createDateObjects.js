import dayjs from 'dayjs';

export default function createDateObjects(date, weekOffset = 1, daysCount = 42) {
  const startOfMonth = dayjs(date).startOf('month');

  let diff = startOfMonth.day() - weekOffset;
  if (diff < 0) diff += 7;

  const prevMonthDays = [];
  for (let i = 0; i < diff; i += 1) {
    prevMonthDays.push({
      day: startOfMonth.subtract(diff - i, 'day'),
      classNames: 'prevMonth',
    });
  }
  const currentMonthDays = [];
  for (let i = 1; i < date.daysInMonth() + 1; i += 1) {
    currentMonthDays.push({
      day: dayjs().year(date.year()).month(date.month()).date(i),
    });
  }

  const daysAdded = prevMonthDays.length + currentMonthDays.length - 1;
  const nextMonthDays = [];
  let i = 1;
  while (daysCount - daysAdded - i > 0) {
    nextMonthDays.push({
      day: currentMonthDays[currentMonthDays.length - 1].day.clone().add(i, 'day'),
      classNames: 'nextMonth',
    });

    i += 1;
  }
  return [...prevMonthDays, ...currentMonthDays, ...nextMonthDays];
}

export const createNum = (number) => {
  const arr = [];
  for (let index = 0; index < number; index += 1) {
    arr.push(index < 10 ? `0${index}` : index);
  }
  return arr;
};
