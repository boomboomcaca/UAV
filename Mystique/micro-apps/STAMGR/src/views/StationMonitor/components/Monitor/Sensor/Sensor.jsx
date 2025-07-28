import React, { useEffect } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { ListView, Empty } from 'dui';
import styles from './index.module.less';

const Sensor = (props) => {
  const { className, sensorData, alarmData } = props;

  const onHScroll = (e) => {
    const dom = e.currentTarget;
    const scrollWidth = 50;
    e.deltaY > 0 ? (dom.scrollLeft += scrollWidth) : (dom.scrollLeft -= scrollWidth);
    // e.preventDefault();
    // return false;
  };

  return (
    <div className={classnames(styles.root, className)}>
      <div>传感器</div>
      <ListView baseSize={{ width: '40%', height: 150 }}>
        <ListView.Item
          content={
            <div className={styles.system}>
              {sensorData && sensorData.length > 0 ? (
                <div className={styles.sensorItem} onWheel={onHScroll}>
                  {sensorData?.map((dinfo) => {
                    return (
                      <div className={styles.item1} key={dinfo.name}>
                        <div>
                          <div /* style={{ backgroundColor: '#FF4C2B' || '#35E065' }} */ />
                          未知
                        </div>
                        <div>
                          <span>{dinfo.value !== undefined ? dinfo.value.toFixed(2) : '--'}</span>
                          <span>{dinfo.unit}</span>
                        </div>
                        <div>{dinfo.display}</div>
                      </div>
                    );
                  })}
                </div>
              ) : (
                <Empty className={styles.empty} />
              )}
              <div className={styles.shadow} />
            </div>
          }
        />
        <ListView.Item
          content={
            <div className={styles.system}>
              {alarmData && alarmData.length > 0 ? (
                <div className={styles.alarmItem} onWheel={onHScroll}>
                  {alarmData?.map((dinfo) => {
                    return (
                      <div className={styles.item2} key={dinfo.name}>
                        <div>
                          <div style={{ backgroundColor: dinfo.value ? '#FF4C2B' : '#35E065' }} />
                          {dinfo.value === undefined ? '未配置' : dinfo.value === true ? '异常' : '正常'}
                        </div>
                        <div>{dinfo.display}</div>
                      </div>
                    );
                  })}
                </div>
              ) : (
                <Empty className={styles.empty} />
              )}
              <div className={styles.shadow} />
            </div>
          }
        />
      </ListView>
    </div>
  );
};

Sensor.defaultProps = {
  className: null,
  sensorData: null,
  alarmData: null,
};

Sensor.propTypes = {
  className: PropTypes.any,
  sensorData: PropTypes.any,
  alarmData: PropTypes.any,
};

export default Sensor;
