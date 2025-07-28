function PrefixInteger(num, n) {
  return (Array(n).join(0) + num).slice(-n);
}

const getTimePercent = (timeStr1, timeStr2, timestamp) => {
  if (timeStr1 === undefined || timeStr2 === undefined || timeStr1 === null || timeStr2 === null) {
    return '';
  }
  const date1 = new Date(timeStr1.replace(/-/g, '/')).getTime();
  const date2 = new Date(timeStr2.replace(/-/g, '/')).getTime();
  if (timestamp >= date1 && timestamp <= date2) {
    const percent = (timestamp - date1) / (date2 - date1);
    return percent * 100;
  }
  return -1;
};

const getTimeSpanHMS = (timeStr1, timeStr2, percent) => {
  if (timeStr1 === undefined || timeStr2 === undefined || timeStr1 === null || timeStr2 === null) {
    return '';
  }
  const date1 = new Date(timeStr1.replace(/-/g, '/'));
  const date2 = new Date(timeStr2.replace(/-/g, '/'));
  const total = Math.ceil((((date1.getTime() - date2.getTime()) / 1000) * percent) / 100);
  const span = {
    hours: parseInt(total / 3600, 10),
    minutes: parseInt((total % 3600) / 60, 10),
    seconds: total % 60,
  };
  return `${PrefixInteger(span.hours, 2)}:${PrefixInteger(span.minutes, 2)}:${PrefixInteger(span.seconds, 2)}`;
};

const getTimeSpan = (timeStr1, timeStr2) => {
  if (timeStr1 === undefined || timeStr2 === undefined || timeStr1 === null || timeStr2 === null) {
    return '';
  }
  const date1 = new Date(timeStr1.replace(/-/g, '/'));
  const date2 = new Date(timeStr2.replace(/-/g, '/'));
  const total = (date1.getTime() - date2.getTime()) / 1000;

  if (total < 60) {
    const span = { seconds: total };
    return `00'${PrefixInteger(span.seconds, 2)}"`;
  }
  if (total < 3600) {
    const span = { minutes: parseInt(total / 60, 10), seconds: total % 60 };
    return `${PrefixInteger(span.minutes, 2)}'${PrefixInteger(span.seconds, 2)}"`;
  }
  if (total < 216000) {
    const span = {
      hours: parseInt(total / 3600, 10),
      minutes: parseInt((total % 3600) / 60, 10),
      seconds: total % 216000,
    };
    return `${PrefixInteger(span.hours, 2)}h${PrefixInteger(span.minutes, 2)}'${PrefixInteger(span.seconds, 2)}"`;
  }

  return '';
};

const getTimeStamp = (timeStr) => {
  if (timeStr) {
    const date = new Date(timeStr.replace(/-/g, '/'));
    return date.getTime();
  }
  return null;
};

export default {};
export { getTimePercent, getTimeSpan, getTimeStamp, getTimeSpanHMS };
