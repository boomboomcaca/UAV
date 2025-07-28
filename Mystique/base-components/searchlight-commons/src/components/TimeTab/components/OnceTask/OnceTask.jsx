import React, { useEffect } from 'react';
import { Calendar } from 'dui';
import PropTypes from 'prop-types';
import dayjs from 'dayjs';
import TimeSelector from '../TimeSelector/TimeSelector.jsx';
import styles from './once.module.less';

const OnceTask = (props) => {
  const { value, ruleChange, onceMinMax } = props;
  // 更新当前任务规则，通知外面
  useEffect(() => {
    if (dayjs(value.startDate[0]).valueOf() > dayjs(onceMinMax[1]).valueOf()) {
      ruleChange({ ...value, startDate: [dayjs(onceMinMax[1])] });
    }
    if (dayjs(value.startDate[0]).valueOf() < dayjs(onceMinMax[0]).valueOf()) {
      ruleChange({ ...value, startDate: [dayjs(onceMinMax[0])] });
    }
  }, [onceMinMax]);
  return (
    <div className={styles.comRoot}>
      <div className={styles.configItem}>
        <span className={styles.tipLabel}>执行日期</span>
        <div className={styles.setting}>
          <Calendar
            minDate={onceMinMax[0]}
            maxDate={onceMinMax[1]}
            value={value.startDate[0]}
            onChange={(ddd) => ruleChange({ ...value, startDate: [ddd] })}
          />
        </div>
      </div>
      <div className={styles.configItem}>
        <span className={styles.tipLabel}>开始时间</span>
        <div className={styles.valueDiv}>
          <TimeSelector
            value={{
              h: value.startHour[0],
              m: value.startMin[0],
            }}
            onChange={(e) => ruleChange({ ...value, startHour: [e.h], startMin: [e.m] })}
          />
        </div>
      </div>
    </div>
  );
};

OnceTask.defaultProps = {
  value: {},
  ruleChange: () => {},
};

OnceTask.propTypes = {
  value: PropTypes.object,
  ruleChange: PropTypes.func,
  onceMinMax: PropTypes.any.isRequired,
};

export default OnceTask;
