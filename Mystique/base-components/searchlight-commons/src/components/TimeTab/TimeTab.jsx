import React from 'react';
import PropTypes from 'prop-types';
import dayjs from 'dayjs';
import RuleOnce from './components/OnceTask/OnceTask.jsx';
import EveryDayTask from './components/EveryDayTask/EveryDayTask.jsx';
import EveryWeekTask from './components/EveryWeekTask/EveryWeekTask.jsx';
import EveryMonthTask from './components/EveryMonthTask/EveryMonthTask.jsx';
import FreetimeTask from './components/FreetimeTask/FreetimeTask.jsx';
import styles from './index.module.less';

const TimeTab = (props) => {
  const { title, value, getValue, onceMinMax } = props;
  const option = [
    {
      value: '单次执行',
      key: 'once',
    },
    {
      value: '每天执行',
      key: 'everyday',
    },
    {
      value: '每周执行',
      key: 'everyweek',
    },
    {
      value: '每月执行',
      key: 'everymonth',
    },
    {
      value: '空闲执行',
      key: 'freetime',
    },
  ];

  const add = (type) => {
    switch (type) {
      case 'once':
        return <RuleOnce onceMinMax={onceMinMax} value={value} ruleChange={(e) => getValue(e)} />;
      case 'everyday':
        return <EveryDayTask value={value} ruleChange={(e) => getValue(e)} />;
      case 'everyweek':
        return <EveryWeekTask value={value} ruleChange={(e) => getValue(e)} />;
      case 'everymonth':
        return <EveryMonthTask value={value} ruleChange={(e) => getValue(e)} />;
      default:
        return <FreetimeTask value={value} ruleChange={(e) => getValue(e)} />;
    }
  };

  const typeChange = (e) => {
    if (e === 'once') {
      const { $H, $m } = dayjs().add(15, 'minute');
      getValue({
        startDate: [dayjs()],
        startHour: [$H],
        startMin: [$m],
        type: e,
      });
    }
    if (e === 'freetime') {
      getValue({
        type: e,
      });
    }
    if (e === 'everyday') {
      getValue({
        startHour: [0],
        startMin: [0],
        type: e,
      });
    }
    if (e === 'everyweek' || e === 'everymonth') {
      getValue({
        advanced: true,
        startDate: [],
        startHour: [0],
        startMin: [0],
        type: e,
      });
    }
  };
  return (
    <div className={styles.timeTab} id="timeTab">
      {title && <div className={styles.title}>{title}</div>}
      <div className={styles.tabs}>
        {option.map((it) => {
          return (
            <div
              className={it.key !== value.type ? styles.tab : styles.activeTab}
              onClick={() => typeChange(it.key)}
              key={it.key}
            >
              {it.value}
              <div
                className={styles.tabBar}
                style={{ backgroundColor: it.key !== value.type ? 'transparent' : '#3CE5D3' }}
              />
            </div>
          );
        })}
      </div>
      <div className={styles.content}>{add(value.type)}</div>
    </div>
  );
};

TimeTab.defaultProps = {
  title: null,
};

TimeTab.propTypes = {
  title: PropTypes.any,
  value: PropTypes.object.isRequired,
  getValue: PropTypes.func.isRequired,
  onceMinMax: PropTypes.any.isRequired,
};

export default TimeTab;
