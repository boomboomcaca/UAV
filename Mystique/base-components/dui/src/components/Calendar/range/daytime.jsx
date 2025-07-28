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
import message from '../../Message';
import TimeColumn from '../timecolumn/index.jsx';
import WeekBar from '../weekBar/index.jsx';

import styles from '../styles.module.less';

dayjs.extend(isBetween);

const Calendar = (props) => {
  const { value, onChange, disable, style, minDate, position } = props;
  const SelectRef = useRef();

  const [open, setOpen] = useState(false);
  const [openMain, setOpenMain] = useState('');
  const [centerDay, setCenterDay] = useState(dayjs());

  const [leftDate, setLeftDate] = useState(null);
  const [rightDate, setRightDate] = useState(null);

  useClickAway(() => {
    setOpen(false);
    setLeftDate(null);
    setRightDate(null);
  }, SelectRef);

  const opengrid = (dirc) => {
    if (disable) {
      return;
    }
    setOpen(true);
    if (dirc === 'auto') {
      if (leftDate && rightDate === null) {
        setOpenMain('right');
      }
      if (leftDate === null && rightDate) {
        setOpenMain('left');
      }
      if (leftDate && rightDate) {
        setOpenMain('left');
        setCenterDay(leftDate);
      }
    } else {
      setOpenMain(dirc);
      if (dirc === 'left' && leftDate) {
        setOpenMain('left');
        setCenterDay(leftDate);
      }
      if (dirc === 'right' && rightDate) {
        setOpenMain('right');
        setCenterDay(rightDate);
      }
    }
  };

  const onPrevMonth = () => {
    setCenterDay(centerDay.clone().subtract(1, 'months'));
  };

  const onNextMonth = () => {
    setCenterDay(centerDay.clone().add(1, 'months'));
  };

  // 只处理起或者止
  const onPickDay2 = (day) => {
    if (openMain === 'left' && day.isAfter(rightDate, 'day')) {
      return;
    }
    if (openMain === 'right' && day.isBefore(leftDate, 'day')) {
      return;
    }
    const fun = openMain === 'right' ? setRightDate : setLeftDate;
    if (snapData === null) {
      // fun(dayjs().year(day.year()).month(day.month()).date(day.date()));
      fun(day.clone().hour(0).minute(0).second(0));
    } else {
      fun(snapData.clone().year(day.year()).month(day.month()).date(day.date()));
    }
  };

  const onPickTime = (ttype, num) => {
    const fun = openMain === 'right' ? setRightDate : setLeftDate;
    const olddate = snapData === null ? dayjs() : snapData;
    if (ttype === 'hh') {
      fun(olddate.clone().hour(num));
    }
    if (ttype === 'mm') {
      fun(olddate.clone().minute(num));
    }
    if (ttype === 'ss') {
      fun(olddate.clone().second(num));
    }
  };

  const onOk = () => {
    const snap = [];
    if (leftDate) {
      snap[0] = leftDate;
    }
    if (rightDate) {
      snap[1] = rightDate;
    }
    if (leftDate && rightDate && leftDate.isAfter(rightDate)) {
      message.info(langT('dui', 'TimeNotLater'));
      return;
    }
    setOpen(false);
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

  const showDays = useMemo(() => {
    return createDateObjects(centerDay);
  }, [centerDay]);

  const snapData = useMemo(() => {
    return openMain === 'left' ? leftDate : rightDate;
  }, [openMain, leftDate, rightDate]);

  return (
    <div className={styles.Calendarnew} ref={SelectRef} style={style}>
      <div className={classnames(styles.calendaritem, disable && styles.disable, open && styles.focusmain)}>
        <div className={styles.calendartext}>
          <div
            // style={{ minWidth: '75px', color: open && openMain === 'left' && '#3CE5D3' }}
            style={{ minWidth: '75px' }}
            onClick={() => opengrid('left')}
          >
            {value[0] ? (
              value[0].format('YYYY-MM-DD HH:mm:ss')
            ) : (
              <span style={{ color: 'var(--theme-font-50)' }}>{langT('dui', 'StartTime')}</span>
            )}
          </div>
          <Icon component={toSvg} style={{ margin: '0 16px' }} onClick={() => opengrid('auto')} />
          <div
            // style={{ minWidth: '75px', color: open && openMain === 'right' && '#3CE5D3' }}
            style={{ minWidth: '75px' }}
            onClick={() => opengrid('right')}
          >
            {value[1] ? (
              value[1]?.format('YYYY-MM-DD HH:mm:ss')
            ) : (
              <span style={{ color: 'var(--theme-font-50)' }}>{langT('dui', 'EndTime')}</span>
            )}
          </div>
        </div>
        <Icon
          component={CalendarSvg}
          style={{ marginLeft: '16px', fontSize: '18px' }}
          onClick={() => opengrid('auto')}
        />
      </div>
      <div className={classnames(styles.calendarOpen, open ? styles.open : '', styles[position])}>
        <div style={{ display: 'flex' }}>
          <div className={styles.calendarGridDay}>
            <div className={styles.CalendarHeader}>
              <div onClick={onPrevMonth}>
                <Icon component={LeftSvg} className={styles.CalendarHeaderBtn} />
              </div>
              <div className={styles.CalendarHeaderText}>{centerDay.format('YYYY-MM')}</div>
              <div onClick={onNextMonth}>
                <Icon component={RightSvg} className={styles.CalendarHeaderBtn} />
              </div>
            </div>
            <WeekBar />
            <div className={styles.grid}>
              {showDays.map(({ day, classNames }) => (
                <div
                  key={dayjs(day).valueOf()}
                  className={classnames(
                    styles.griditem,
                    leftDate && openMain === 'left' && day.isSame(leftDate, 'day') && styles.currentday,
                    rightDate && openMain === 'right' && day.isSame(rightDate, 'day') && styles.currentday,
                    // day.isBetween(leftDate, rightDate) && styles.rangeday,
                    openMain === 'right' && day.isBefore(leftDate, 'day') && styles.prevMonth,
                    openMain === 'left' && day.isAfter(rightDate, 'day') && styles.nextMonth,
                    minDate && day.isBefore(minDate, 'day') && styles.prevMonth,
                    styles[classNames],
                  )}
                  onClick={() => onPickDay2(day)}
                >
                  {day.format('D')}
                </div>
              ))}
            </div>
          </div>
          <div className={styles.line} />
          <div className={styles.calendarGridTime}>
            <div className={styles.timeheader}>{snapData?.format('YYYY-MM-DD HH:mm:ss')}</div>
            <div className={styles.timearea}>
              <TimeColumn value={snapData?.hour()} num={24} onPickTime={(item) => onPickTime('hh', item)} />
              <div className={styles.column}>
                <TimeColumn value={snapData?.minute()} num={60} onPickTime={(item) => onPickTime('mm', item)} />
              </div>
              <div className={styles.column}>
                <TimeColumn value={snapData?.second()} num={60} onPickTime={(item) => onPickTime('ss', item)} />
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
  value: [],
  disable: false,
  onChange: () => {},
  minDate: null,
  style: {},
  position: 'left',
};

Calendar.propTypes = {
  value: PropTypes.array,
  disable: PropTypes.bool,
  onChange: PropTypes.func,
  style: PropTypes.object,
  minDate: PropTypes.any,
  position: PropTypes.string,
};

export default Calendar;
