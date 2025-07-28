import React, { useState, useEffect, useRef, useMemo } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import langT from 'dc-intl';
import dayjs from 'dayjs';
import { useClickAway } from 'ahooks';
import Icon from '@ant-design/icons';
import isBetween from 'dayjs/plugin/isBetween';
import createDateObjects from '../createDateObjects';
import { LeftSvg, RightSvg, CalendarSvg } from '../svg.jsx';
import TimeColumn from '../timecolumn/index.jsx';
import WeekBar from '../weekBar/index.jsx';

import styles from '../styles.module.less';

dayjs.extend(isBetween);

const Calendar = (props) => {
  const { value, onChange, disable, style, minDate, position, theme, maxDate } = props;
  const SelectRef = useRef();

  const [open, setOpen] = useState(false);
  const [centerDate, setCenterDate] = useState(dayjs());
  const [localDate, setlocalDate] = useState(dayjs());

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

  const onPickTime = (ttype, num) => {
    if (ttype === 'hh') {
      setlocalDate(localDate.clone().hour(num));
    }
    if (ttype === 'mm') {
      setlocalDate(localDate.clone().minute(num));
    }
    if (ttype === 'ss') {
      setlocalDate(localDate.clone().second(num));
    }
  };

  const onPick = (day) => {
    if (minDate && day.isBefore(minDate, 'day')) {
      return;
    }
    if (maxDate && day.isAfter(maxDate, 'day')) {
      return;
    }
    setlocalDate(localDate.clone().year(day.year()).month(day.month()).date(day.date()));
  };

  const onOk = () => {
    onChange(localDate);
    setOpen(false);
  };

  useEffect(() => {
    setCenterDate(value);
    setlocalDate(value);
  }, [value]);

  if (!value.format) {
    return 'value 错误';
  }

  const showDays = useMemo(() => {
    return createDateObjects(centerDate);
  }, [centerDate]);

  return (
    <div className={classnames(styles.Calendarnew, styles[theme])} ref={SelectRef} style={style}>
      <div
        className={classnames(styles.calendaritem, disable && styles.disable, open && styles.focusmain)}
        onClick={opengrid}
      >
        <div className={styles.calendartext}>{value.format('YYYY-MM-DD HH:mm:ss')}</div>
        <Icon component={CalendarSvg} style={{ marginLeft: '36px', fontSize: '18px' }} />
      </div>
      <div className={classnames(styles.calendarOpen, styles[position], open ? styles.open : '')}>
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
                      day.isSame(localDate, 'day') && styles.currentday,
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
          <div className={styles.line} />
          <div className={styles.calendarGridTime}>
            <div className={styles.timeheader}>{localDate?.format('YYYY-MM-DD HH:mm:ss')}</div>
            <div className={styles.timearea}>
              <TimeColumn value={localDate?.hour()} num={24} onPickTime={(item) => onPickTime('hh', item)} />
              <div className={styles.column}>
                <TimeColumn value={localDate?.minute()} num={60} onPickTime={(item) => onPickTime('mm', item)} />
              </div>
              <div className={styles.column}>
                <TimeColumn value={localDate?.second()} num={60} onPickTime={(item) => onPickTime('ss', item)} />
              </div>
            </div>
          </div>
        </div>
        <div className={classnames(styles.footer, styles.topborder)} style={{ justifyContent: 'end' }}>
          <div className={styles.footeritem} onClick={onOk}>
            {langT('dui', 'Sure')}
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
  theme: 'black',
};

Calendar.propTypes = {
  value: PropTypes.any,
  disable: PropTypes.bool,
  onChange: PropTypes.func,
  style: PropTypes.object,
  minDate: PropTypes.any,
  maxDate: PropTypes.any,
  position: PropTypes.string,
  theme: PropTypes.string,
};

export default Calendar;
