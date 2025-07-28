import React, { memo, useEffect, useRef } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import scrollIntoView from 'scroll-into-view-if-needed';
import dayjs from 'dayjs';
import isBetween from 'dayjs/plugin/isBetween';
import { createNum } from '../createDateObjects';

import styles from './styles.module.less';

dayjs.extend(isBetween);

const Calendar = (props) => {
  const { value, num, onPickTime } = props;

  const LiRef = useRef(new Map());

  // 滚动 有问题 暂不开放
  useEffect(() => {
    if (value !== null) {
      const activeEle = LiRef.current.get(value);
      if (activeEle) {
        scrollIntoView(activeEle, {
          behavior: 'smooth',
          block: 'center',
        });
      }
    }
  }, [value]);

  return (
    <div className={styles.column}>
      {createNum(num).map((item) => (
        <div
          key={item}
          className={classnames(styles.columnitem, {
            [styles.active]: Number(item) === value,
          })}
          onClick={() => onPickTime(item)}
          ref={(element) => {
            if (element) {
              LiRef.current.set(Number(item), element);
            }
          }}
        >
          {item}
        </div>
      ))}
    </div>
  );
};

Calendar.defaultProps = {
  value: null,
  num: 60,
  onPickTime: () => {},
};

Calendar.propTypes = {
  value: PropTypes.number,
  num: PropTypes.number,
  onPickTime: PropTypes.func,
};

export default memo(Calendar);
