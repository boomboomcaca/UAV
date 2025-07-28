import React, { useState, useEffect, useRef, useMemo } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { useClickAway } from 'ahooks';
import dayjs from 'dayjs';
import langT from 'dc-intl';
import Icon from '@ant-design/icons';
import isBetween from 'dayjs/plugin/isBetween';
import createDateObjects from '../createDateObjects';
import { LeftSvg, RightSvg, CalendarSvg, toSvg } from '../svg.jsx';
import WeekBar from '../weekBar/index.jsx';

import styles from '../styles.module.less';

dayjs.extend(isBetween);

const Calendar = (props) => {
  const { value, onChange, disable, style, minDate, maxDate, position } = props;
  const SelectRef = useRef();

  const [open, setOpen] = useState(false);
  const [centerLeft, setCenterLeft] = useState(dayjs());
  const [centerRight, setCenterRight] = useState(dayjs().add(1, 'months'));
  const [leftDate, setLeftDate] = useState(null);
  const [rightDate, setRightDate] = useState(null);

  useClickAway(() => {
    setOpen(false);
  }, SelectRef);

  const opengrid = () => {
    if (disable) {
      return;
    }
    setOpen(!open);
  };

  const onPrevMonth1 = () => {
    setCenterLeft(centerLeft.clone().subtract(1, 'months'));
  };

  const onNextMonth1 = () => {
    setCenterLeft(centerLeft.clone().add(1, 'months'));
  };

  const onPrevMonth2 = () => {
    setCenterRight(centerRight.clone().subtract(1, 'months'));
  };

  const onNextMonth2 = () => {
    setCenterRight(centerRight.clone().add(1, 'months'));
  };

  // 同时处理起止
  const onPickDay = (day) => {
    // 早于minDate 拒绝
    if (minDate && day.isBefore(minDate, 'day')) {
      return;
    }
    if (maxDate && day.isAfter(maxDate, 'day')) {
      return;
    }
    // 起止相同 选择一个 同时取消
    if (day.isSame(leftDate, 'day') && day.isSame(rightDate, 'day')) {
      setLeftDate(null);
      setRightDate(null);
      return;
    }
    if (day.isSame(leftDate, 'day')) {
      setLeftDate(null);
      return;
    }
    if (day.isSame(rightDate, 'day')) {
      setRightDate(null);
      return;
    }
    // 起止相同 选前赋起 选后赋止
    if (leftDate && rightDate && leftDate.isSame(rightDate, 'day')) {
      if (day.isBefore(leftDate, 'day')) {
        setLeftDate(day);
      }
      if (day.isAfter(rightDate, 'day')) {
        setRightDate(day);
      }
      return;
    }
    // 起止都有 选前赋起 选后赋止 选中判近
    if (leftDate && rightDate) {
      if (day.isBefore(leftDate, 'day')) {
        setLeftDate(day);
        return;
      }
      if (day.isAfter(rightDate, 'day')) {
        setRightDate(day);
        return;
      }
      const leftDiff = day.diff(leftDate, 'day');
      const rightDiff = rightDate.diff(day, 'day');
      if (leftDiff < rightDiff) {
        setLeftDate(day);
      } else if (leftDiff > rightDiff) {
        setRightDate(day);
      } else {
        setRightDate(day);
      }
      return;
    }
    // 先赋值起 再赋值止
    if (!leftDate) {
      if (rightDate && day.isAfter(rightDate, 'day')) {
        return;
      }
      setLeftDate(day);
    }
    if (leftDate && !rightDate) {
      if (day.isBefore(leftDate, 'day')) {
        return;
      }
      setRightDate(day);
    }
  };

  const onCancel = () => {
    setOpen(false);
  };

  const onOk = () => {
    setOpen(false);
    const snap = [];
    if (leftDate) {
      snap[0] = leftDate;
    }
    if (rightDate) {
      snap[1] = rightDate;
    } else if (leftDate) {
      snap[1] = leftDate;
    }
    onChange(snap);
  };

  useEffect(() => {
    if (value[0]) {
      setLeftDate(value[0]);
    }
    if (value[1]) {
      setRightDate(value[1]);
    }
  }, [value]);

  if (value.length > 0) {
    if (value[0] && !value[0].format) {
      return 'value 错误';
    }
    if (value[1] && !value[1].format) {
      return 'value 错误';
    }
  }

  if (value.length > 1 && value[1] && !value[1].format) {
    return 'value 错误';
  }

  const showDaysLeft = useMemo(() => {
    return createDateObjects(centerLeft);
  }, [centerLeft]);

  const showDaysRight = useMemo(() => {
    return createDateObjects(centerRight);
  }, [centerRight]);

  return (
    <div className={styles.Calendarnew} ref={SelectRef} style={style}>
      <div className={classnames(styles.calendaritem, disable && styles.disable, open && styles.focusmain)}>
        <div className={styles.calendartext}>
          <div style={{ minWidth: '75px' }} onClick={() => opengrid('left')}>
            {value[0] ? value[0].format('YYYY-MM-DD') : langT('dui', 'StartTime')}
          </div>
          <Icon component={toSvg} style={{ margin: '0 16px' }} onClick={() => opengrid('left')} />
          <div style={{ minWidth: '75px' }} onClick={() => opengrid('right')}>
            {value[1] ? value[1]?.format('YYYY-MM-DD') : langT('dui', 'EndTime')}
          </div>
        </div>
        <Icon
          component={CalendarSvg}
          style={{ marginLeft: '16px', fontSize: '18px' }}
          onClick={() => opengrid('left')}
        />
      </div>
      <div className={classnames(styles.calendarOpen, open ? styles.open : '', styles[position])}>
        <div style={{ display: 'flex' }}>
          <div className={styles.calendarGridDay}>
            <div className={styles.CalendarHeader}>
              <div onClick={onPrevMonth1}>
                <Icon component={LeftSvg} className={styles.CalendarHeaderBtn} />
              </div>
              <div className={styles.CalendarHeaderText}>{centerLeft.format('YYYY-MM')}</div>
              <div onClick={onNextMonth1}>
                <Icon component={RightSvg} className={styles.CalendarHeaderBtn} />
              </div>
            </div>
            <WeekBar />
            <div className={styles.grid}>
              {showDaysLeft.map(({ day, classNames }) => (
                <div
                  key={dayjs(day).valueOf()}
                  className={classnames(
                    styles.griditem,
                    (day.isSame(leftDate, 'day') || day.isSame(rightDate, 'day')) && styles.currentday,
                    day.isBetween(leftDate, rightDate) && styles.rangeday,
                    rightDate === null && day.isBefore(leftDate, 'day') && styles.prevMonth,
                    leftDate === null && day.isAfter(rightDate, 'day') && styles.nextMonth,
                    minDate && day.isBefore(minDate, 'day') && styles.prevMonth,
                    maxDate && day.isAfter(maxDate, 'day') && styles.prevMonth,
                    styles[classNames],
                  )}
                  onClick={() => onPickDay(day)}
                >
                  {day.format('D')}
                </div>
              ))}
            </div>
          </div>
          <div className={styles.calendarGridDay}>
            <div className={styles.CalendarHeader}>
              <div onClick={onPrevMonth2}>
                <Icon component={LeftSvg} className={styles.CalendarHeaderBtn} />
              </div>
              <div className={styles.CalendarHeaderText}>{centerRight.format('YYYY-MM')}</div>
              <div onClick={onNextMonth2}>
                <Icon component={RightSvg} className={styles.CalendarHeaderBtn} />
              </div>
            </div>
            <WeekBar />
            <div className={styles.grid}>
              {showDaysRight.map(({ day, classNames }) => (
                <div
                  key={dayjs(day).valueOf()}
                  className={classnames(
                    styles.griditem,
                    (day.isSame(leftDate, 'day') || day.isSame(rightDate, 'day')) && styles.currentday,
                    day.isBetween(leftDate, rightDate) && styles.rangeday,
                    rightDate === null && day.isBefore(leftDate, 'day') && styles.prevMonth,
                    leftDate === null && day.isAfter(rightDate, 'day') && styles.nextMonth,
                    minDate && day.isBefore(minDate, 'day') && styles.prevMonth,
                    maxDate && day.isAfter(maxDate, 'day') && styles.prevMonth,
                    styles[classNames],
                  )}
                  onClick={() => onPickDay(day)}
                >
                  {day.format('D')}
                </div>
              ))}
            </div>
          </div>
        </div>
        <div className={styles.footer}>
          <div className={styles.footeritem} onClick={onCancel}>
            {langT('dui', 'Cancel')}
          </div>
          <div className={styles.footeritem} onClick={onOk}>
            {langT('dui', 'Sure')}
          </div>
        </div>
      </div>
    </div>
  );
};

Calendar.defaultProps = {
  value: [],
  disable: false,
  onChange: () => {},
  minDate: null,
  maxDate: null,
  style: {},
  position: 'left',
};

Calendar.propTypes = {
  value: PropTypes.array,
  disable: PropTypes.bool,
  onChange: PropTypes.func,
  style: PropTypes.object,
  minDate: PropTypes.any,
  maxDate: PropTypes.any,
  position: PropTypes.string,
};

export default Calendar;
