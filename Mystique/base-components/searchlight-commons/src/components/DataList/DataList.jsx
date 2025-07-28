/*
 * @Author: dengys
 * @Date: 2022-03-17 16:42:32
 * @LastEditors: dengys
 * @LastEditTime: 2022-04-11 10:46:32
 */
import React, { useState, useEffect } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { PopUp } from 'dui';
import langT from 'dc-intl';
import ElectButton from './ElectButton';
import { CaptureList2 } from '../CaptureList';
import ReplayList2 from '../ReplayList2';
import styles from './index.module.less';

const DataList = (props) => {
  const { className, appConfig, request, listType, functionName, feature, visible, showType, showPlay, onPlay } = props;

  const [type, setType] = useState('replay');

  const bo = showType.includes('replay') && showType.includes('capture');

  useEffect(() => {
    if (showType?.length === 1 && showType[0] === 'capture') {
      setType('capture');
    }
  }, [showType]);

  return (
    <PopUp visible={visible} popupTransition="rtg-fade" usePortal={false} mask={false} popStyle={{ top: '72px' }}>
      <div className={classnames(styles.root, className)}>
        {bo ? (
          <ElectButton
            className={styles.elect}
            options={[
              { key: 'replay', value: langT('commons', 'recordFile') },
              { key: 'capture', value: langT('commons', 'screenshootFile') },
            ]}
            value={type}
            onChange={(e) => {
              setType(e.key);
            }}
          />
        ) : null}
        {showType.includes('replay') ? (
          <ReplayList2
            className={type !== 'replay' ? styles.hide : bo ? styles.normal1 : styles.normal2}
            appConfig={appConfig}
            request={request}
            listType={feature !== null && feature !== undefined && feature !== '' ? feature : listType}
            onSelectChange={onPlay}
            showPlay={showPlay}
          />
        ) : null}
        {showType.includes('capture') ? (
          <CaptureList2
            className={type !== 'capture' ? styles.hide : bo ? styles.normal1 : styles.normal2}
            appConfig={appConfig}
            request={request}
            functionName={feature !== null && feature !== undefined && feature !== '' ? feature : functionName}
          />
        ) : null}
      </div>
    </PopUp>
  );
};

DataList.defaultProps = {
  className: null,
  appConfig: null,
  request: null,
  listType: 'segments',
  functionName: 'scan',
  feature: '',
  visible: false,
  showType: ['replay', 'capture'],
  showPlay: false,
  onPlay: () => {},
};

DataList.propTypes = {
  className: PropTypes.any,
  appConfig: PropTypes.any,
  request: PropTypes.any,
  listType: PropTypes.string,
  functionName: PropTypes.string,
  feature: PropTypes.string,
  visible: PropTypes.bool,
  showType: PropTypes.any,
  showPlay: PropTypes.bool,
  onPlay: PropTypes.func,
};

export default DataList;
