import React, { useState, useEffect } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { message } from 'dui';
import Circle from '@/components/Circle';
import LogList from '@/components/LogList';
import { getIconByType, getData } from './data';
import check from './icons/check.png';
import styles from './index.module.less';

const { Item } = LogList;

const Check = (props) => {
  const { className, dataSource, onLoading } = props;

  const [progress, setProgress] = useState(0);

  const [running, setRunning] = useState(false);

  const [logs, setLogs] = useState([]);

  const [round, setRound] = useState(0);

  const updateLogs = () => {
    const timespan = Math.round(500 + 500 * Math.random());
    window.console.log(timespan);
    setTimeout(() => {
      const { ret: d, index } = getData(dataSource, round);
      setProgress(Math.round((index / dataSource.length) * 100));

      if (d) {
        const find = logs.find((l) => {
          return l.id === d.id;
        });
        if (find) {
          logs.splice(logs.indexOf(find), 1, d);
        } else {
          logs.push(d);
        }
        setLogs([...logs]);
        updateLogs();
      } else {
        setRunning(false);
        onLoading(false);
      }
    }, timespan);
  };

  return (
    <div className={classnames(styles.root, className)}>
      <Circle className={styles.circle} progress={progress} lineWidth={50}>
        {running ? (
          <div
            className={styles.checking}
            onClick={() => {
              setRunning(true);
              onLoading(true);
              updateLogs();
            }}
          >
            <span>正在自检</span>
            <div>
              <span>{progress}</span>
              <span>%</span>
            </div>
          </div>
        ) : (
          <div
            className={styles.check}
            onClick={() => {
              if (dataSource.length > 0) {
                setRunning(true);
                onLoading(true);
                setRound(round + 1);
                updateLogs();
              } else {
                message.info({ key: 'tip', content: '没有设备需要自检' });
              }
            }}
          >
            <img width={36} height={36} alt="" src={check} />
            <span>设备自检</span>
          </div>
        )}
      </Circle>
      <LogList className={styles.log}>
        {logs.map((d) => {
          return (
            <Item>
              <div className={styles.item}>
                <div className={styles.prog}>
                  <img
                    className={d.type === 0 ? styles.loading : null}
                    width={16}
                    height={16}
                    alt=""
                    src={getIconByType(d.type)}
                  />
                  <span>{d.msg}</span>
                </div>
                <span>{d.device}</span>
                <span>{d.model}</span>
                <span>{d.time}</span>
              </div>
            </Item>
          );
        })}
      </LogList>
      <div className={styles.shadow} />
    </div>
  );
};

Check.defaultProps = {
  className: null,
  dataSource: null,
  onLoading: () => {},
};

Check.propTypes = {
  className: PropTypes.any,
  dataSource: PropTypes.any,
  onLoading: PropTypes.func,
};

export default Check;
