import React, { useCallback, forwardRef, useImperativeHandle, useEffect, useRef, useState } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { useKeyPress } from 'ahooks';
import langT from 'dc-intl';
import { IconButton } from 'dui'; /*  */
import { PlayIcon, StopIcon, VoiceIcon, CameraIcon, RecIcon, /* ReplayListIcon, */ IQRecordIcon } from 'dc-icon';
import ComboList from '../ComboList';
import EdgeInfo from '../EdgeInfo';
import TaskDeviceInfo from './TaskDeviceInfo';
import ExpandR2Button from './ExpandR2Button';
import styles from './index.module.less';

const Footer = forwardRef((props, ref) => {
  const {
    className,
    children,
    onClick,
    checkedStatesDef,
    disableStatesDef,
    loadingStatesDef,
    visibleStatesDef,
    message,
    edgeInfo,
    antennaInfo,
    avaliable,
    showPopup,
    popupContent,
    hasAudio,
  } = props;

  const getColor = useCallback((bo) => {
    return bo ? '#2ee8d5' : 'var(--theme-font-100)';
  }, []);

  const checkedStatesRef = useRef({ audio: false, runState: false, record: false, recordIQ: false });
  const [checkedStates, setCheckedState] = useState(checkedStatesRef.current);
  useEffect(() => {
    checkedStatesRef.current = { ...checkedStatesRef.current, ...checkedStatesDef };
    setCheckedState(checkedStatesRef.current);
  }, [checkedStatesDef]);

  const disableStatesRef = useRef({
    audio: false,
    capture: false,
    runState: false,
    record: false,
    recordIQ: false,
    data: false,
    info: false,
  });
  const [disableStates, setDisableStates] = useState(disableStatesRef.current);
  useEffect(() => {
    disableStatesRef.current = { ...disableStatesRef.current, ...disableStatesDef };
    setDisableStates(disableStatesRef.current);
  }, [disableStatesDef]);

  const loadingStatesRef = useRef({ running: false, capturing: false });
  const [loadingStates, setLoadingStates] = useState(loadingStatesRef.current);
  useEffect(() => {
    loadingStatesRef.current = { ...loadingStatesRef.current, ...loadingStatesDef };
    setLoadingStates(loadingStatesRef.current);
  }, [loadingStatesDef]);

  const visibleStatesRef = useRef({ station: true, recordIQ: false });
  const [visibleStates, setVisibleStates] = useState(visibleStatesRef.current);
  useEffect(() => {
    visibleStatesRef.current = { ...visibleStatesRef.current, ...visibleStatesDef };
    setVisibleStates(visibleStatesRef.current);
  }, [visibleStatesDef]);

  useImperativeHandle(ref, () => ({
    updateCheckedStates: (key, value) => {
      checkedStatesRef.current[key] = value;
      setCheckedState({ ...checkedStatesRef.current });
    },
    updateDisablStates: (key, value) => {
      disableStatesRef.current[key] = value;
      setDisableStates({ ...disableStatesRef.current });
    },
    updateLoadingStates: (key, value) => {
      loadingStatesRef.current[key] = value;
      setLoadingStates({ ...loadingStatesRef.current });
    },
    updateVisibleStates: (key, value) => {
      visibleStatesRef.current[key] = value;
      setVisibleStates({ ...visibleStatesRef.current });
    },
  }));

  useKeyPress('space', () => {
    if (!window.banblankstart) {
      onClick(checkedStatesRef.current.runState, 'runState');
    }
  });

  const [audioType, setAudioType] = useState(null);

  return (
    <div className={classnames(styles.container, className)}>
      <div className={styles.left}>{children}</div>
      <div className={styles.center}>
        {visibleStates.audio === false ? (
          <div className={styles.hideButton} />
        ) : (
          <IconButton
            tag="audio"
            text={langT('commons', 'audio')}
            checked={checkedStates.audio}
            disabled={disableStates.audio}
            onClick={onClick}
          >
            <VoiceIcon color={getColor(checkedStates.audio)} />
          </IconButton>
        )}
        <IconButton
          tag="capture"
          text={langT('commons', 'screenShoot')}
          disabled={loadingStates.capture || disableStates.capture}
          loading={loadingStates.capture}
          onClick={onClick}
        >
          <CameraIcon color={getColor(checkedStates.capture)} />
        </IconButton>
        <div
          className={classnames(
            checkedStates.runState === true ? styles.mainBtnBackChecked : styles.mainBtnBack,
            disableStates.runState ? (checkedStates.runState === true ? styles.disable2 : styles.disable1) : null,
          )}
        >
          <IconButton
            tag="runState"
            disabled={disableStates.runState}
            loading={loadingStates.runState}
            text={checkedStates.runState !== true ? langT('commons', 'start') : langT('commons', 'stop')}
            className={classnames(checkedStates.runState === true ? styles.mainBtnChecked : styles.mainBtn)}
            style={{ marginLeft: 4, marginRight: 4, boxSizing: 'content-box' }}
            onClick={() => {
              onClick(checkedStates.runState, 'runState');
            }}
          >
            {checkedStates.runState !== true ? <PlayIcon color="#2ee8d5" /> : <StopIcon color="#FFD118" />}
          </IconButton>
        </div>
        {visibleStates.record === false ? (
          <div className={styles.hideButton} />
        ) : hasAudio ? (
          <ExpandR2Button
            checked={checkedStates.record}
            disabled={!checkedStates.runState || disableStates.record}
            content={
              <>
                <RecIcon color={getColor(checkedStates.record)} />
                <span>{langT('commons', 'record')}</span>
              </>
            }
            onChange={(e) => {
              e.option && setAudioType(e.option);
              onClick(!e.checked, 'record', e);
            }}
            value={audioType?.value || 'raw'}
            options={[
              { value: 'raw', tag: 1, label: '原始数据' },
              { value: 'wav', tag: 3, label: 'WAV音频' },
              { value: 'mp3', tag: 4, label: 'MP3音频' },
              { value: 'wma', tag: 5, label: 'WMA音频' },
            ]}
          />
        ) : (
          <IconButton
            tag="record"
            text={langT('commons', 'record')}
            checked={checkedStates.record}
            disabled={!checkedStates.runState || disableStates.record}
            onClick={onClick}
          >
            <RecIcon color={getColor(checkedStates.record)} />
          </IconButton>
        )}
        {visibleStates.recordIQ === false ? (
          <div className={styles.hideButton} />
        ) : (
          <IconButton
            tag="recordIQ"
            text={langT('commons', 'record')}
            checked={checkedStates.recordIQ}
            disabled={!checkedStates.runState || disableStates.recordIQ}
            onClick={onClick}
          >
            <IQRecordIcon color={getColor(checkedStates.recordIQ)} />
          </IconButton>
        )}
        {/* <IconButton tag="data" text={langT('commons', 'data')} disabled={disableStates.data} onClick={onClick}>
          <ReplayListIcon />
        </IconButton> */}
      </div>
      <div className={styles.right}>
        <div className={styles.info}>
          <ComboList value={message} />
        </div>
        {visibleStates.station === false ? (
          <div className={styles.edgeInfoNull} />
        ) : (
          <EdgeInfo
            className={styles.edgeInfo}
            disable={checkedStates.runState !== false || disableStates.station === true}
            avaliable={avaliable}
            edgeInfo={edgeInfo}
            onClick={() => {
              onClick(undefined, 'station');
            }}
          >
            {popupContent ||
              (showPopup && edgeInfo ? <TaskDeviceInfo selFeature={edgeInfo} selAntenna={antennaInfo} /> : null)}
          </EdgeInfo>
        )}
      </div>
    </div>
  );
});

Footer.defaultProps = {
  className: null,
  children: null,
  onClick: () => {},
  checkedStatesDef: { audio: false, runState: false, record: false, recordIQ: false },
  disableStatesDef: { audio: false, capture: false, runState: false, record: false, recordIQ: false, data: false },
  loadingStatesDef: { runnState: false, capture: false },
  visibleStatesDef: { audio: true, station: true, record: true, recordIQ: false },
  message: null,
  edgeInfo: null,
  antennaInfo: null,
  avaliable: true,
  showPopup: false,
  popupContent: null,
  hasAudio: false,
};

Footer.propTypes = {
  className: PropTypes.any,
  children: PropTypes.any,
  onClick: PropTypes.func,
  checkedStatesDef: PropTypes.any,
  disableStatesDef: PropTypes.any,
  loadingStatesDef: PropTypes.any,
  visibleStatesDef: PropTypes.any,
  message: PropTypes.any,
  edgeInfo: PropTypes.any,
  antennaInfo: PropTypes.any,
  avaliable: PropTypes.bool,
  showPopup: PropTypes.bool,
  popupContent: PropTypes.any,
  hasAudio: PropTypes.bool,
};

export default Footer;
