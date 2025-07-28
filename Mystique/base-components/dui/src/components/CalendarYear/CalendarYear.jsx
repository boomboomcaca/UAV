import React, { useState, useEffect, useRef } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { useClickAway } from 'ahooks';
import langT from 'dc-intl';
import Icon from '@ant-design/icons';
import { LeftSvg, RightSvg, CalendarSvg } from './svg.jsx';
import styles from './styles.module.less';

const CalendarYear = (props) => {
  const { value, onChange, data, style, width } = props;
  const SelectRef = useRef();
  const [open, setOpen] = useState(false);
  const [dateRange, setDateRange] = useState([]);
  const [valueC, setValueC] = useState();
  const currentYear = new Date().getFullYear();

  useClickAway(() => {
    cancel();
  }, SelectRef);

  const onPrevYear = () => {
    const a = dateRange.map((i) => {
      return i - 12;
    });
    setDateRange(a);
  };

  const onNextYear = () => {
    if (dateRange[dateRange.length - 1] === currentYear) {
      return;
    }
    const a = dateRange.map((i) => {
      return i + 12;
    });
    setDateRange(a);
  };

  useEffect(() => {
    setValueC(value);
  }, [value]);

  useEffect(() => {
    const a = [];
    for (let i = 11; i >= 0; i -= 1) {
      a.push(currentYear - i);
    }
    setDateRange(a);
  }, []);

  const changeValue = (i) => {
    if (data.length > 0) {
      if (i !== valueC && data.includes(i)) {
        setValueC(i);
      }
    } else if (i !== valueC) {
      setValueC(i);
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
        <div className={styles.calendartext}>{value}</div>
        <Icon component={CalendarSvg} style={{ marginLeft: '36px', fontSize: '18px' }} />
      </div>
      <div className={classnames(styles.calendarOpen, open ? styles.open : '')}>
        <div className={styles.calendarGridDay}>
          <div className={styles.CalendarHeader}>
            <div onClick={onPrevYear}>
              <Icon component={LeftSvg} className={styles.CalendarHeaderBtn} />
            </div>
            <div className={styles.CalendarHeaderText}>{`${dateRange[0]}-${dateRange[dateRange.length - 1]}`}</div>
            <div onClick={onNextYear}>
              <Icon component={RightSvg} className={styles.CalendarHeaderBtn} />
            </div>
          </div>
          <div className={styles.week}>
            {dateRange.map((i) => {
              let flag = true;
              if (data.length > 0) {
                flag = !data.includes(i) && i !== valueC;
              } else {
                flag = false;
              }
              return (
                <div
                  className={classnames(
                    styles.itemYear,
                    flag && styles.canSelect,
                    i === valueC ? styles.select : styles.unselect,
                  )}
                  key={`year_${i}`}
                  onClick={() => changeValue(i)}
                >
                  {i}
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

CalendarYear.defaultProps = {
  value: 2021,
  onChange: () => {},
  style: {},
  data: [],
  width: 256,
};

CalendarYear.propTypes = {
  width: PropTypes.any,
  value: PropTypes.any,
  onChange: PropTypes.func,
  data: PropTypes.array,
  style: PropTypes.object,
};

export default CalendarYear;
