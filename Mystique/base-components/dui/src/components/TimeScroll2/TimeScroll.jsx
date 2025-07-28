/*
 * @Author: XYQ
 * @Date: 2022-10-14 10:03:30
 * @LastEditors: XYQ
 * @LastEditTime: 2022-10-17 17:49:26
 * @Description: file content
 */
import React, { useState, useEffect } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { useSetState, useDebounceEffect } from 'ahooks';
// eslint-disable-next-line import/extensions
import { elementKey } from '@/lib/tools';
import TimeItem from './TimeItem.jsx';
import styles from './style.module.less';

export default function TimeScroll(props) {
  const { onChange, options } = props;

  const [hourItems, setHourItems] = useState([]);
  const [minItems, setMinItems] = useState([]);
  const [stems, setSItems] = useState([]);
  const [state, setState] = useSetState({
    sh: 0,
    sm: 0,
    ss: 0,
    eh: 0,
    em: 0,
    es: 0,
    item: 0,
  });
  const { sh, sm, ss, eh, em, es, item } = state;

  useEffect(() => {
    if (options.startTime) {
      const { startTime, endTime } = options;
      const [h, m, s] = startTime?.split(':');
      const [h1, m1, s1] = endTime?.split(':');
      setState({ sh: h * 1, sm: m * 1, ss: s * 1, eh: h1 * 1, em: m1 * 1, es: s1 * 1 });
    }
  }, [options]);

  const timeItems = (len) => {
    const h = [];
    for (let i = 0; i < len; i += 1) {
      h.push({ key: elementKey(), value: i });
    }
    return h;
  };

  useEffect(() => {
    const hs = timeItems(24);
    setHourItems(hs);
    const ms = timeItems(60);
    setMinItems(ms);
    const s = timeItems(60);
    setSItems(s);
  }, []);

  useDebounceEffect(
    () => {
      const json = {
        startTime: `${sh < 10 ? `0${sh}` : sh}:${sm < 10 ? `0${sm}` : sm}:${ss < 10 ? `0${ss}` : ss}`,
        endTime: `${eh < 10 ? `0${eh}` : eh}:${em < 10 ? `0${em}` : em}:${es < 10 ? `0${es}` : es}`,
      };
      onChange(json);
    },
    [sh, sm, ss, eh, em, es],
    {
      wait: 500,
    },
  );

  return (
    <div className={styles.tigercontainer}>
      <div className={styles.tigercontainer_center}>
        <div className={styles.tigercontainer_center_time}>
          <div className={styles.tigercontainer_time_head}>开始时间</div>
          <div className={styles.tigercontainer_center_time_1}>
            <TimeItem
              data={hourItems}
              value={sh}
              onChange={(val) => {
                setState({ sh: val });
              }}
            />
            <span>:</span>
            <TimeItem
              data={minItems}
              value={sm}
              onChange={(val) => {
                setState({ sm: val });
              }}
            />
            <span>:</span>
            <TimeItem
              data={stems}
              value={ss}
              onChange={(val) => {
                setState({ ss: val });
              }}
            />
          </div>
        </div>
        <div className={styles.tigercontainer_center_time_line}>
          <p />
        </div>
        <div className={styles.tigercontainer_center_time}>
          <div className={styles.tigercontainer_time_head}>结束时间</div>
          <div className={styles.tigercontainer_center_time_1}>
            <TimeItem
              data={hourItems}
              value={eh}
              onChange={(val) => {
                setState({ eh: val });
              }}
            />
            <span>:</span>
            <TimeItem
              data={minItems}
              value={em}
              onChange={(val) => {
                setState({ em: val });
              }}
            />
            <span>:</span>
            <TimeItem
              data={stems}
              value={es}
              onChange={(val) => {
                setState({ es: val });
              }}
            />
          </div>
        </div>
      </div>
      <div className={styles.tigercontainer_foot}>
        <div className={styles.tigercontainer_foot_1}>
          <div
            className={classnames(styles.tigercontainer_foot_btn, { [styles.tigercontainer_foot_btn_ac]: item === 1 })}
            onClick={(e) => {
              e.stopPropagation();
              if (item !== 1) {
                setState({ sh: 8, sm: 0, ss: 0, eh: 18, em: 0, es: 0, item: item === 1 ? 0 : 1 });
              } else {
                setState({ sh: 0, sm: 0, ss: 0, eh: 0, em: 0, es: 0, item: item === 1 ? 0 : 1 });
              }
            }}
          >
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path fill-rule="evenodd" clip-rule="evenodd" d="M12.5001 4.49951C12.5001 4.22337 12.2763 3.99951 12.0001 3.99951C11.724 3.99951 11.5001 4.22337 11.5001 4.49951V5.9996C11.5001 6.27574 11.724 6.4996 12.0001 6.4996C12.2763 6.4996 12.5001 6.27574 12.5001 5.9996V4.49951ZM12.5001 18.0003C12.5001 17.7242 12.2763 17.5003 12.0001 17.5003C11.724 17.5003 11.5001 17.7242 11.5001 18.0003V19.5004C11.5001 19.7766 11.724 20.0004 12.0001 20.0004C12.2763 20.0004 12.5001 19.7766 12.5001 19.5004V18.0003ZM17.6547 7.05008C17.8499 6.85481 17.8499 6.53823 17.6547 6.34297C17.4594 6.14771 17.1428 6.14771 16.9476 6.34297L15.8868 7.4037C15.6916 7.59896 15.6916 7.91555 15.8868 8.11081C16.0821 8.30607 16.3987 8.30607 16.5939 8.11081L17.6547 7.05008ZM8.1078 16.5966C8.30306 16.4014 8.30306 16.0848 8.1078 15.8895C7.91254 15.6943 7.59595 15.6943 7.40069 15.8895L6.33996 16.9503C6.1447 17.1455 6.1447 17.4621 6.33996 17.6574C6.53522 17.8526 6.8518 17.8526 7.04706 17.6574L8.1078 16.5966ZM20.0021 11.9998C20.0021 12.2759 19.7782 12.4998 19.5021 12.4998H18.002C17.7258 12.4998 17.502 12.2759 17.502 11.9998C17.502 11.7236 17.7258 11.4998 18.002 11.4998H19.5021C19.7782 11.4998 20.0021 11.7236 20.0021 11.9998ZM6.50012 11.9998C6.50012 12.2759 6.27626 12.4998 6.00012 12.4998H4.5C4.22386 12.4998 4 12.2759 4 11.9998C4 11.7236 4.22386 11.4998 4.5 11.4998H6.00012C6.27626 11.4998 6.50012 11.7236 6.50012 11.9998ZM16.9512 17.6568C17.1464 17.852 17.463 17.852 17.6583 17.6568C17.8535 17.4615 17.8535 17.1449 17.6583 16.9497L16.5976 15.8889C16.4023 15.6937 16.0857 15.6937 15.8904 15.8889C15.6952 16.0842 15.6952 16.4008 15.8904 16.596L16.9512 17.6568ZM7.40438 8.11063C7.59964 8.3059 7.91623 8.3059 8.11149 8.11063C8.30675 7.91537 8.30675 7.59879 8.11149 7.40353L7.05075 6.34279C6.85549 6.14753 6.53891 6.14753 6.34365 6.34279C6.14838 6.53805 6.14839 6.85464 6.34365 7.0499L7.40438 8.11063ZM12.002 17C14.7634 17 17.002 14.7614 17.002 12C17.002 9.23855 14.7634 6.99997 12.002 6.99997C9.24053 6.99997 7.00195 9.23855 7.00195 12C7.00195 14.7614 9.24053 17 12.002 17Z" fill={`${item === 1 ? '#3CE5D3' : 'rgba(255, 255, 255, 0.5)'}`} />
            </svg>
            <span>白天</span>
          </div>
        </div>
        <div className={styles.tigercontainer_foot_1}>
          <div
            className={classnames(styles.tigercontainer_foot_btn, { [styles.tigercontainer_foot_btn_ac]: item === 2 })}
            onClick={(e) => {
              e.stopPropagation();
              if (item !== 2) {
                setState({ sh: 20, sm: 0, ss: 0, eh: 6, em: 0, es: 0, item: item === 2 ? 0 : 2 });
              } else {
                setState({ sh: 0, sm: 0, ss: 0, eh: 0, em: 0, es: 0, item: item === 2 ? 0 : 2 });
              }
            }}
          >
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path fill-rule="evenodd" clip-rule="evenodd" d="M10.2614 18.9888C10.1753 18.9962 10.0881 19 10 19C8.34315 19 7 17.6569 7 16C7 14.3431 8.34315 13 10 13C10.7725 13 11.4768 13.292 12.0086 13.7716C12.1253 12.2214 13.42 11 15 11C16.6569 11 18 12.3431 18 14C18 14.0167 17.9999 14.0333 17.9996 14.0499C19.1409 14.2814 20 15.2904 20 16.5C20 17.8807 18.8807 19 17.5 19C17.3288 19 17.1616 18.9828 17 18.95C16.8384 18.9828 16.6712 19 16.5 19H10.5C10.4195 19 10.34 18.9962 10.2614 18.9888Z" fill={`${item === 2 ? '#3CE5D3' : 'rgba(255, 255, 255, 0.5)'}`} />
              <path fill-rule="evenodd" clip-rule="evenodd" d="M6.47003 11.4694C9.78374 11.4694 12.47 8.78308 12.47 5.46937C12.47 5.03236 12.4233 4.60626 12.3346 4.1958C14.2024 5.21259 15.47 7.19282 15.47 9.46913C15.47 9.82769 15.4386 10.1789 15.3783 10.5202C15.2538 10.5068 15.1274 10.4999 14.9993 10.4999C13.4421 10.4999 12.1226 11.5167 11.6683 12.9228C11.1721 12.6532 10.6034 12.4999 9.99933 12.4999C8.4884 12.4999 7.20109 13.4573 6.71096 14.7985C5.14838 13.9879 3.98752 12.5103 3.60547 10.7427C4.45671 11.2061 5.43263 11.4694 6.47003 11.4694Z" fill={`${item === 2 ? '#3CE5D3' : 'rgba(255, 255, 255, 0.5)'}`} />
            </svg>
            <span>夜晚</span>
          </div>
        </div>
      </div>
    </div>
  );
}
TimeScroll.defaultProps = {
  options: {
    startTime: '',
    endTime: '',
  },
  onChange: () => { },
};

TimeScroll.propTypes = {
  options: PropTypes.object,
  onChange: PropTypes.func,
};
