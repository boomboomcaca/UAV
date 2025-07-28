import React, { useEffect, useState } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import styles from './TimeFrame.module.less';
/**
 * 日月报分析 时间刻度展示
 */

const TimeFrame = (props) => {
  const { className, recall, timeData, type, timeRange, disable } = props;
  const [choose, setChoose] = useState(100);

  const clickHandle = (i) => {
    if (!disable && timeData.includes(i) && i !== choose) {
      setChoose(i);
      recall(i);
    }
  };
  useEffect(() => {
    setChoose(100);
  }, [timeData]);

  return (
    <div className={classnames(styles.timeFrame, className)}>
      {timeRange.map((i, index) => {
        const flag = timeData.includes(i);
        let flag1 = false;
        if (choose === i) {
          flag1 = true;
        }
        return (
          <div
            key={`${index + 1}-item`}
            onClick={() => clickHandle(i)}
            className={classnames(
              styles.bottom_item,
              flag1 && styles.bottom_item_choose,
              flag && styles.bottom_item_has,
              disable && styles.disableTimeFrame,
            )}
          >
            <span>{`${i}${type}`}</span>
          </div>
        );
      })}
    </div>
  );
};
TimeFrame.defaultProps = {
  className: null,
  recall: null,
  timeData: [],
  type: '号',
  timeRange: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12],
  disable: false,
};

TimeFrame.propTypes = {
  className: PropTypes.any,
  recall: PropTypes.func,
  timeData: PropTypes.array,
  type: PropTypes.string,
  timeRange: PropTypes.array,
  disable: PropTypes.bool,
};
export default TimeFrame;
