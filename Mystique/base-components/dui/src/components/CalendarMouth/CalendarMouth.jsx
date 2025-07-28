import React, { useState, useEffect, useRef } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { useClickAway } from 'ahooks';
import langT from 'dc-intl';
import Icon from '@ant-design/icons';
import { LeftSvg, RightSvg, CalendarSvg } from './svg.jsx';
import styles from './styles.module.less';

const CalendarMouth = (props) => {
  const { value, onChange, data, style, width } = props;
  const SelectRef = useRef();
  const [open, setOpen] = useState(false);
  const [dateRange, setDateRange] = useState({ year: 2021, mouth: [] });
  const [valueC, setValueC] = useState({
    year: 2017,
    mouth: 7,
  });
  const mouthList = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12];

  useClickAway(() => {
    cancel();
  }, SelectRef);

  const onPrevYear = () => {
    if (data.length > 0) {
      const a = data.filter((i) => {
        return i.year === dateRange.year - 1;
      });
      if (a[0]) {
        setDateRange(...a);
      } else {
        setDateRange({
          year: dateRange.year - 1,
          mouth: [],
        });
      }
    } else {
      setDateRange({
        year: dateRange.year - 1,
        mouth: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12],
      });
    }
  };

  const onNextYear = () => {
    if (dateRange.year === new Date().getFullYear()) {
      return;
    }
    if (data.length > 0) {
      const a = data.filter((i) => {
        return i.year === dateRange.year + 1;
      });
      if (a[0]) {
        setDateRange(...a);
      } else {
        setDateRange({
          year: dateRange.year + 1,
          mouth: [],
        });
      }
    } else {
      setDateRange({
        year: dateRange.year + 1,
        mouth: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12],
      });
    }
  };

  useEffect(() => {
    setValueC(value);
    if (data.length > 0) {
      const a = data.filter((i) => {
        return i.year === value.year;
      });
      setDateRange(...a);
    } else {
      setDateRange({
        year: value.year,
        mouth: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12],
      });
    }
  }, [value]);

  const changeValue = (i) => {
    if (dateRange.year === valueC.year && i === valueC.mouth) return;
    if (dateRange.mouth.includes(i)) {
      setValueC({
        year: dateRange.year,
        mouth: i,
      });
    }
  };

  const onOk = () => {
    onChange(valueC);
    setOpen(false);
  };

  const cancel = () => {
    setValueC(value);
    setOpen(false);
  };

  return (
    <div className={styles.Calendar} ref={SelectRef} style={style}>
      <div
        style={{ width }}
        className={classnames(styles.calendaritem, open && styles.focusmain)}
        onClick={() => {
          if (open) {
            cancel();
          } else {
            setOpen(true);
          }
        }}
      >
        <div className={styles.calendartext}>{`${value.year}-${value.mouth}`}</div>
        <Icon component={CalendarSvg} style={{ marginLeft: '36px', fontSize: '18px' }} />
      </div>
      <div className={classnames(styles.calendarOpen, open ? styles.open : '')}>
        <div className={styles.calendarGridDay}>
          <div className={styles.CalendarHeader}>
            <div onClick={onPrevYear}>
              <Icon component={LeftSvg} className={styles.CalendarHeaderBtn} />
            </div>
            <div className={styles.CalendarHeaderText}>{dateRange?.year}</div>
            <div onClick={onNextYear}>
              <Icon component={RightSvg} className={styles.CalendarHeaderBtn} />
            </div>
          </div>
          <div className={styles.week}>
            {mouthList.map((i) => {
              const flag = !dateRange?.mouth.includes(i);
              return (
                <div
                  className={classnames(
                    styles.itemYear,
                    flag && styles.canSelect,
                    i === valueC.mouth && dateRange?.year === valueC.year && dateRange?.mouth.includes(valueC.mouth)
                      ? styles.select
                      : styles.unselect,
                  )}
                  key={`mouth_${i}`}
                  onClick={() => changeValue(i)}
                >
                  {`${i}月`}
                </div>
              );
            })}
          </div>
          <div className={styles.footer}>
            <div className={styles.footeritem} onClick={cancel}>
              {langT('dui', 'Cancel')}
            </div>
            <div className={styles.footeritem} onClick={onOk}>
              {langT('dui', 'Sure')}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

CalendarMouth.defaultProps = {
  value: 2021,
  onChange: () => {},
  style: {},
  data: [],
  width: 256,
};

CalendarMouth.propTypes = {
  width: PropTypes.any,
  value: PropTypes.any,
  onChange: PropTypes.func,
  data: PropTypes.array,
  style: PropTypes.object,
};

export default CalendarMouth;
