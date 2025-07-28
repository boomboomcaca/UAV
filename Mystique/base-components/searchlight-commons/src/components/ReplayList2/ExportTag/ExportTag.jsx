/* eslint-disable max-len */
import React, { useState, useRef, useCallback } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { Modal, InputNumber, Radio } from 'dui';
import { useClickAway } from 'ahooks';
import langT from 'dc-intl';
import icons from '../Icon';
import styles from './index.module.less';

const border = (
  <svg width="115" height="34" viewBox="0 0 115 34" fill="none" xmlns="http://www.w3.org/2000/svg">
    <path
      d="M6.78632 3C6.78632 1.89543 7.68176 1 8.78632 1H112C113.105 1 114 1.89543 114 3V31C114 32.1046 113.105 33 112 33H8.78633C7.68176 33 6.78632 32.1046 6.78632 31V21.2687C6.78632 20.6009 6.45305 19.9772 5.89792 19.606L2 17L5.89792 14.394C6.45305 14.0228 6.78632 13.3991 6.78632 12.7313V3Z"
      fill="black"
      fillOpacity="0.3"
    />
    <path
      d="M1.7221 16.5843L1.10039 17L1.7221 17.4157L5.62002 20.0217C6.03637 20.3001 6.28632 20.7678 6.28632 21.2687V31C6.28632 32.3807 7.40561 33.5 8.78633 33.5H112C113.381 33.5 114.5 32.3807 114.5 31V3C114.5 1.61929 113.381 0.5 112 0.5H8.78632C7.40561 0.5 6.28632 1.61929 6.28632 3V12.7313C6.28632 13.2322 6.03637 13.6999 5.62002 13.9783L1.7221 16.5843Z"
      stroke="var(--theme-font-100)"
      strokeOpacity="0.1"
    />
  </svg>
);

const ExportTag = (props) => {
  const { className, savedTypes, onClick } = props;

  const SelectRef = useRef(null);

  const [show, setShow] = useState(false);

  const [showDfind, setShowDfind] = useState(false);
  const [dfCondition, setDfCondition] = useState(null);

  const [showAudio, setShowAudio] = useState(false);
  const [audioCondition, setAudioCondition] = useState({ audioType: 'wav' });

  useClickAway(() => {
    setShow(false);
  }, SelectRef);

  const types = useRef(['dfind', 'iq', 'spectrum', 'scan', 'audio']).current;
  const hasTypes = useCallback((ts) => {
    let has = 0;
    if (ts) {
      types.forEach((t) => {
        const find = ts.find((x) => {
          return x === t;
        });
        if (find) {
          has += 1;
        }
      });
    }
    return has > 0;
  }, []);

  return (
    <div
      className={classnames(styles.root, className)}
      ref={SelectRef}
      onMouseMove={() => {
        setShow(true);
      }}
      onMouseLeave={() => {
        setShow(false);
      }}
    >
      <div
        onClick={() => {
          if (savedTypes && Array.isArray(savedTypes) && savedTypes.length > 0 && hasTypes(savedTypes)) {
            setShow(true);
          } else {
            // ?? 没有数据开关直接下载
            onClick('');
          }
        }}
      >
        {langT('commons', 'export')}
      </div>
      {savedTypes && Array.isArray(savedTypes) && savedTypes.length > 0 && hasTypes(savedTypes) ? (
        <div className={classnames(styles.pop, show ? styles.show : null)}>
          {border}
          <div className={styles.selects}>
            {savedTypes.includes('dfind') ? (
              <div
                className={!savedTypes.includes('dfind') ? styles.disable : styles.normal}
                onClick={() => {
                  savedTypes.includes('dfind') && setShowDfind(true);
                }}
              >
                {icons.dfdata}
              </div>
            ) : (
              <div
                className={!savedTypes.includes('iq') ? styles.disable : styles.normal}
                onClick={() => {
                  savedTypes.includes('iq') && onClick('iq');
                }}
              >
                {icons.iqdata}
              </div>
            )}
            <div
              className={
                !savedTypes.includes('spectrum') && !savedTypes.includes('scan') ? styles.disable : styles.normal
              }
              onClick={() => {
                savedTypes.includes('spectrum') && onClick('spectrum');
                savedTypes.includes('scan') && onClick('scan');
              }}
            >
              {icons.spectrum}
            </div>
            <div
              className={!savedTypes.includes('audio') ? styles.disable : styles.normal}
              onClick={() => {
                savedTypes.includes('audio') && setShowAudio(true);
              }}
            >
              {icons.audio}
            </div>
          </div>
        </div>
      ) : null}
      <Modal
        visible={showDfind || showAudio}
        title="数据导出"
        style={{ top: '50%', transform: 'translateY(-50%)', minWidth: '420px' }}
        onCancel={() => {
          if (showDfind) {
            setShowDfind(false);
          }
          if (showAudio) {
            setShowAudio(false);
          }
        }}
        onOk={() => {
          if (showDfind) {
            setShowDfind(false);
            savedTypes.includes('dfind') &&
              onClick('dfind', {
                levelThreshold:
                  dfCondition?.levelThreshold === undefined || dfCondition?.levelThreshold === ''
                    ? 60
                    : dfCondition.levelThreshold,
                qualityThreshold:
                  dfCondition?.qualityThreshold === undefined || dfCondition?.qualityThreshold === ''
                    ? 50
                    : dfCondition.qualityThreshold,
              });
          }
          if (showAudio) {
            setShowAudio(false);
            savedTypes.includes('audio') && onClick('audio', { audioType: audioCondition?.audioType || 'mp3' });
          }
        }}
      >
        {showDfind ? (
          <div className={styles.modal}>
            <div className={styles.title}>请输入电平门限和质量门限过滤数据</div>
            <div className={styles.item}>
              <div>电平门限</div>
              <InputNumber
                style={{ width: 260 }}
                digits={4}
                min={-40}
                max={128}
                suffix="dBμV"
                placeholder="请输入"
                defaultValue={60}
                onChange={(val) => {
                  setDfCondition({ ...dfCondition, levelThreshold: val });
                }}
              />
            </div>
            <div className={styles.item}>
              <div>质量门限</div>
              <InputNumber
                style={{ width: 260 }}
                digits={4}
                min={0}
                max={100}
                suffix="%"
                placeholder="请输入"
                defaultValue={50}
                onChange={(val) => {
                  setDfCondition({ ...dfCondition, qualityThreshold: val });
                }}
              />
            </div>
          </div>
        ) : null}
        {showAudio ? (
          <div className={styles.modal}>
            <div className={styles.title}>请选择导出的音频格式</div>
            <div className={styles.item}>
              {/* <div>音频格式</div> */}
              <Radio
                value={audioCondition?.audioType}
                options={[
                  { label: 'WAV音频', value: 'wav' },
                  { label: 'MP3音频', value: 'mp3' },
                  { label: 'WMA音频', value: 'wma' },
                ]}
                onChange={(val) => {
                  setAudioCondition({ audioType: val });
                }}
              />
            </div>
          </div>
        ) : null}
      </Modal>
    </div>
  );
};

ExportTag.defaultProps = {
  className: null,
  savedTypes: null,
  onClick: () => {},
};

ExportTag.propTypes = {
  className: PropTypes.any,
  savedTypes: PropTypes.any,
  onClick: PropTypes.func,
};

export default ExportTag;
