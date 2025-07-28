import React from 'react';
import PropTypes from 'prop-types';
import TimeSelector from '../TimeSelector/TimeSelector.jsx';
import styles from './style.module.less';

const EveryDayTask = (props) => {
  const { ruleChange, value } = props;

  return (
    <div className={styles.comRoot}>
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

EveryDayTask.defaultProps = {
  value: {},
  ruleChange: () => {},
};

EveryDayTask.propTypes = {
  value: PropTypes.object,
  ruleChange: PropTypes.func,
};

export default EveryDayTask;
