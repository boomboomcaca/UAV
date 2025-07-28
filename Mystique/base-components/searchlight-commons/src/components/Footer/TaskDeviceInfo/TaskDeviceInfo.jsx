import React from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import styles from './index.module.less';

const TaskDeviceInfo = (props) => {
  const { className, selFeature, selAntenna } = props;

  return (
    <div className={classnames(styles.root, className)}>
      <div className={styles.edgeInfoItem}>
        <div className={styles.lighttext}>设备信息</div>
        <div className={styles.infosubclauses}>
          <div className={styles.subclausesitem}>
            <div>型号</div>
            <div className={styles.lighttext}>{selFeature.deviceName || selFeature.featureName}</div>
          </div>
          <div className={styles.subclausesitem}>
            <div>频率范围</div>
            <div>
              <span className={styles.lighttext}>{selFeature.frequency?.minimum || '--'}</span>
              <span style={{ margin: '0 4px' }}>~</span>
              <span className={styles.lighttext}>{selFeature.frequency?.maximum || '--'}</span>
              <span>MHz</span>
            </div>
          </div>
          <div className={styles.subclausesitem}>
            <div>中频带宽</div>
            {selFeature.ifBandwidth > 10000 ? (
              <div>
                <span className={styles.lighttext}>{selFeature.ifBandwidth / 1000}</span>
                <span>MHz</span>
              </div>
            ) : (
              <div>
                <span className={styles.lighttext}>{selFeature.ifBandwidth}</span>
                <span>kHz</span>
              </div>
            )}
          </div>
        </div>
      </div>
      {selAntenna?.isActive > 0 && (
        <div className={styles.edgeInfoItem}>
          <div className={styles.lighttext}>天线信息</div>
          <div className={styles.infosubclauses}>
            <div className={styles.subclausesitem}>
              <div>型号</div>
              <div className={styles.lighttext}>{selAntenna.model || ''}</div>
            </div>
            <div className={styles.subclausesitem}>
              <div>频率范围</div>
              <div>
                <span className={styles.lighttext}>{selAntenna.startFrequency}</span>
                <span style={{ margin: '0 4px' }}>~</span>
                <span className={styles.lighttext}>{selAntenna.stopFrequency}</span>
                <span>MHz</span>
              </div>
            </div>
            <div className={styles.subclausesitem}>
              <div>模式</div>
              <div>
                <span className={styles.lighttext}>
                  {selAntenna.polarization === 'horizontal' ? '水平极化' : '垂直极化'}
                </span>
                <span style={{ margin: '0 4px' }}>/</span>
                <span className={styles.lighttext}>{selAntenna.lowisActive ? '有源' : '无源'}</span>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

TaskDeviceInfo.defaultProps = {
  className: null,
  selFeature: null,
  selAntenna: null,
};

TaskDeviceInfo.propTypes = {
  className: PropTypes.any,
  selFeature: PropTypes.any,
  selAntenna: PropTypes.any,
};

export default TaskDeviceInfo;
