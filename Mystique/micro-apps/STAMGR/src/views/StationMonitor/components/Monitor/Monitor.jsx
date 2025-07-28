import React from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import useMonitor from './useMonitor';
import Power from './Power';
import Sensor from './Sensor';
import Camera from './Camera';
import styles from './index.module.less';

const Monitor = (props) => {
  const { className, edgeID, disabled } = props;

  const { mainData, powerData, sensorData, alarmData, cameraData } = useMonitor(edgeID);

  return (
    <div className={classnames(styles.root, className)}>
      <Power main={mainData} data={powerData} disabled={disabled} />
      <Sensor sensorData={sensorData} alarmData={alarmData} />
      <Camera data={cameraData} />
    </div>
  );
};

Monitor.defaultProps = {
  className: null,
  edgeID: null,
  disabled: false,
};

Monitor.propTypes = {
  className: PropTypes.any,
  edgeID: PropTypes.any,
  disabled: PropTypes.bool,
};

export default Monitor;
