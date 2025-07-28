import React, { useState } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import useStationMonitor from './useStationMonitor';
import Detail from './components/Detail';
import Device from './components/Device';
import Driver from './components/Driver';
import Monitor from './components/Monitor';
import Server from './components/Server';
import Test from './Test';
import styles from './index.module.less';

const showTest = false;

const StationMonitor = (props) => {
  const { className, edgeID, disabled } = props;

  const [tabIndex, setTabIndex] = useState(0); // 9

  const { station, devices, drivers } = useStationMonitor(edgeID);

  const getContent = (idx) => {
    switch (idx) {
      case 0:
        return <Detail data={station} />;
      case 1:
        return <Device data={devices} />;
      case 2:
        return <Driver data={drivers} attach={devices} />;
      case 3:
        return <Monitor edgeID={edgeID} disabled={disabled} />;
      case 4:
        return <Server data={station} />;
      default:
        return showTest ? <Test /> : null;
    }
  };

  return (
    <div className={classnames(styles.root, className)}>
      <div className={styles.tab}>
        {['基本信息', '监测设备', '监测功能', '环境监控', '服务器监控' /* , 'test' */].map((str, idx) => {
          const istest = str === 'test';
          return (
            <div
              key={str}
              onClick={() => {
                setTabIndex(istest ? 9 : idx);
              }}
              className={classnames(
                styles.tabItem,
                (istest && tabIndex === 9) || idx === tabIndex ? styles.tabItemChecked : null,
              )}
            >
              {str}
            </div>
          );
        })}
      </div>
      <div className={styles.pan}>{getContent(tabIndex)}</div>
    </div>
  );
};

StationMonitor.defaultProps = {
  className: null,
  edgeID: null,
  disabled: false,
};

StationMonitor.propTypes = {
  className: PropTypes.any,
  edgeID: PropTypes.any,
  disabled: PropTypes.bool,
};

export default StationMonitor;
