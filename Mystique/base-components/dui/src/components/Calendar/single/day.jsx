import React, { useState, useEffect, useRef, useMemo } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import dayjs from 'dayjs';
import { useClickAway } from 'ahooks';
import Icon from '@ant-design/icons';
import isBetween from 'dayjs/plugin/isBetween';
import createDateObjects from '../createDateObjects';
import { LeftSvg, RightSvg, CalendarSvg } from '../svg.jsx';
import WeekBar from '../weekBar/index.jsx';

import styles from '../styles.module.less';

dayjs.extend(isBetween);

const Calendar = (props) => {
  const { value, onChange, disable, style, minDate, maxDate, position } = props;
  const SelectRef = useRef();

  const [open, setOpen] = useState(false);
  const [centerDate, setCenterDate] = useState(dayjs());

  useClickAway(() => {
    setOpen(false);
  }, SelectRef);

  const opengrid = () => {
    if (disable) {
      return;
    }
    setOpen(!open);
  };

  const onPrevMonth = () => {
    setCenterDate(centerDate.clone().subtract(1, 'months'));
  };

  const onNextMonth = () => {
    setCenterDate(centerDate.clone().add(1, 'months'));
  };

  const onPick = (day) => {
    if (minDate && day.isBefore(minDate, 'day')) {
      return;
    }
    if (maxDate && day.isAfter(maxDate, 'day')) {
      return;
    }
    onChange(value.clone().year(day.year()).month(day.month()).date(day.date()));
    setOpen(false);
  };

  useEffect(() => {
    setCenterDate(value);
  }, [value]);

  if (!value.format) {
    return 'value 错误';
  }

  const showDays = useMemo(() => {
    return createDateObjects(centerDate);
  }, [centerDate]);

  return (
    <div className={styles.Calendarnew} ref={SelectRef} style={style}>
      <div
        className={classnames(styles.calendaritem, disable && styles.disable, open && styles.focusmain)}
        onClick={opengrid}
      >
        <div className={styles.calendartext}>{value.format('YYYY-MM-DD')}</div>
        <Icon component={CalendarSvg} style={{ marginLeft: '36px', fontSize: '18px' }} />
      </div>
      <div className={classnames(styles.calendarOpen, open ? styles.open : '', styles[position])}>
        <div style={{ display: 'flex' }}>
          <div className={styles.calendarGridDay}>
            <div className={styles.CalendarHeader}>
              <div onClick={onPrevMonth}>
                <Icon component={LeftSvg} className={styles.CalendarHeaderBtn} />
              </div>
              <div className={styles.CalendarHeaderText}>{centerDate.format('YYYY-MM')}</div>
              <div onClick={onNextMonth}>
                <Icon component={RightSvg} className={styles.CalendarHeaderBtn} />
              </div>
            </div>
            <WeekBar />
            <div className={styles.grid}>
              {showDays.map(({ day, classNames }) => {
                return (
                  <div
                    key={dayjs(day).valueOf()}
                    className={classnames(
                      styles.griditem,
                      day.isSame(value, 'day') && styles.currentday,
                      minDate && day.isBefore(minDate, 'day') && styles.prevMonth,
                      maxDate && day.isAfter(maxDate, 'day') && styles.prevMonth,
                      styles[classNames],
                    )}
                    onClick={() => onPick(day)}
                  >
                    {day.format('D')}
                  </div>
                );
              })}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

Calendar.defaultProps = {
  value: dayjs(),
  disable: false,
  onChange: () => {},
  style: {},
  minDate: null,
  maxDate: null,
  position: 'left',
};

Calendar.propTypes = {
  value: PropTypes.any,
  disable: PropTypes.bool,
  onChange: PropTypes.func,
  style: PropTypes.object,
  minDate: PropTypes.any,
  maxDate: PropTypes.any,
  position: PropTypes.string,
};

export default Calendar;
