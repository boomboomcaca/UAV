import React, { useState, useEffect } from 'react';
import classnames from 'classnames';
import PropTypes from 'prop-types';
import { Checkbox } from 'dui';
import TimeSelector from '../TimeSelector/TimeSelector.jsx';
import { MinusIcon } from '../icon.jsx';
import styles from './style.module.less';

const EveryMonthTask = (props) => {
  const { value, ruleChange } = props;
  const [startTime, setStartTime] = useState({
    h: 0,
    m: 0,
  });
  const mouthDay = [];
  for (let i = 1; i < 32; i += 1) {
    mouthDay.push({
      label: i,
      value: i.toString(),
    });
  }
  // 高级模式下的开始时间
  const [startTimeAD, setStartTimeAD] = useState([]);
  const [checkedDay, setCheckedDay] = useState([]);
  const [visible, setVisible] = useState(false);
  const [advanced, setAdvanced] = useState(true);
  const [checkedData, setCheckedData] = useState([]);
  const [key, setKey] = useState('');
  const [options, setOptions] = useState([]);
  useEffect(() => {
    setAdvanced(value.advanced);
    setCheckedDay(value.startDate);
    setCheckedData(value.startDate);
    const arr = value.startDate.map((it, index) => {
      return {
        date: it,
        h: value.startHour[index],
        m: value.startMin[index],
      };
    });
    setStartTimeAD(arr);
  }, [value]);

  useEffect(() => {
    const arr = [];
    mouthDay.forEach((item) => {
      if (checkedDay.includes(item.value)) {
        arr.push(item);
      }
    });
    setOptions(arr);
    if (arr.length > 0) selectKey(arr[0].value);
  }, [checkedDay]);

  const startTimeChange = (e, date) => {
    let adHourSettings = startTimeAD;
    // 数据同步
    if (advanced) {
      adHourSettings = [];
      checkedDay.forEach((item) => {
        adHourSettings.push({
          date: item,
          h: e.h,
          m: e.m,
        });
      });
    } else {
      const exist = adHourSettings.find((h) => h.date === date);
      if (exist) {
        exist.h = e.h;
        exist.m = e.m;
      } else {
        adHourSettings.push({
          date,
          h: e.h,
          m: e.m,
        });
      }
    }
    setStartTime({
      h: e.h,
      m: e.m,
    });
    ruleChange({
      ...value,
      startDate: checkedData,
      startHour: adHourSettings.map((s) => s.h),
      startMin: adHourSettings.map((s) => s.m),
    });
  };

  const okhandle = () => {
    setVisible(false);
    const arr = [];
    // 数据同步
    if (advanced) {
      checkedData.forEach((item) => {
        arr.push({
          date: item,
          h: startTime.h,
          m: startTime.m,
        });
      });
    } else {
      checkedData.forEach((item) => {
        const empty = [...startTimeAD].filter((it) => {
          return it.date === item;
        });
        if (empty.length > 0) {
          arr.push(...empty);
        } else {
          arr.push({
            date: item,
            h: 0,
            m: 0,
          });
        }
      });
    }
    ruleChange({
      ...value,
      startDate: checkedData,
      startHour: arr.map((s) => s.h),
      startMin: arr.map((s) => s.m),
    });
  };

  const subWeek = (e) => {
    const arr1 = checkedDay.filter((item) => {
      return item !== e;
    });
    const arr2 = startTimeAD.filter((item) => {
      return item.date !== e;
    });
    ruleChange({
      ...value,
      startDate: arr1,
      startHour: arr2.map((s) => s.h),
      startMin: arr2.map((s) => s.m),
    });
  };

  const selectKey = (val) => {
    setKey(val);
    const arr = startTimeAD.filter((item) => {
      return item.date === val;
    });
    if (arr.length > 0) {
      setStartTime({
        h: arr[0].h,
        m: arr[0].m,
      });
    }
  };

  return (
    <div className={styles.top}>
      <div className={styles.left}>
        <div className={styles.dateAdd} style={{ marginBottom: 20 }}>
          <span>日期</span>
          <div className={styles.addBtn} onClick={() => setVisible(true)} />
        </div>
        {visible ? (
          <div className={styles.card}>
            <div>
              <Checkbox.Group value={checkedData} onChange={(val) => setCheckedData(val)} options={mouthDay} />
            </div>
            <div className={styles.cardFoot}>
              <span
                onClick={() => {
                  setCheckedData(checkedDay);
                  setVisible(false);
                }}
              >
                取消
              </span>
              <span onClick={() => okhandle()}>确定</span>
            </div>
          </div>
        ) : null}
        <div className={styles.weekBottom}>
          {options.map((item) => {
            return (
              <div key={item.value} className={styles.dateAdd}>
                <span
                  className={classnames(styles.checkedweek, key === item.value ? styles.active : '')}
                  key={item.value}
                  onClick={() => selectKey(item.value)}
                >
                  {`每月${item.label}号`}
                </span>
                <div onClick={() => subWeek(item.value)}>
                  <MinusIcon />
                </div>
              </div>
            );
          })}
        </div>
      </div>
      <div className={styles.center} />
      <div className={styles.right}>
        <div className={styles.configItem}>
          <span className={styles.tipLabel}>开始时间</span>
          <div className={styles.valueDiv}>
            <TimeSelector value={startTime} onChange={(e) => startTimeChange(e, key)} />
          </div>
        </div>
        <div className={styles.head}>
          <Checkbox.Traditional
            checked={advanced}
            onChange={(e) => {
              const adHourSettings = [];
              if (e) {
                checkedDay.forEach((item) => {
                  adHourSettings.push({
                    date: item,
                    h: startTime.h,
                    m: startTime.m,
                  });
                });
                ruleChange({
                  ...value,
                  advanced: e,
                  startHour: adHourSettings.map((s) => s.h),
                  startMin: adHourSettings.map((s) => s.m),
                });
              } else {
                ruleChange({
                  ...value,
                  advanced: e,
                });
              }
            }}
          >
            同步到所有已选日期
          </Checkbox.Traditional>
        </div>
      </div>
    </div>
  );
};

EveryMonthTask.defaultProps = {
  ruleChange: () => {},
};

EveryMonthTask.propTypes = {
  value: PropTypes.object.isRequired,
  ruleChange: PropTypes.func,
};

export default EveryMonthTask;
