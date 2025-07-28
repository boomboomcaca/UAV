import React, { memo } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { RIGHTGXSVG, LEFTGXSVG } from '../svg.jsx';
import styles from './index.module.less';

const SegList = (props) => {
  const { segments, activeIdx, maxLength, onSelectSeg, onDeleteSeg } = props;

  return (
    <div className={styles.SegList}>
      <div className={styles.righthead}>
        <div className={styles.leftgx}>{LEFTGXSVG}</div>
        <span>当前选择频段</span>
        <div className={styles.rightgx}>{RIGHTGXSVG}</div>
      </div>
      <div className={styles.tstext}>选中的卡片可修改和切换频段</div>
      <div className={styles.rightcontent}>
        {segments.map((item, idx) => (
          <div
            className={classnames(styles.segitem, { [styles.active]: activeIdx === idx })}
            key={item.id}
            style={{ justifyContent: maxLength === 1 ? 'center' : 'space-between' }}
            onClick={() => {
              if (maxLength === 1) {
                return;
              }
              onSelectSeg(idx);
            }}
          >
            {maxLength !== 1 && <div className={styles.segidx}>{idx + 1}</div>}
            <div className={styles.seginfo}>
              <div className={styles.infoitem}>
                <div className={styles.infohead}>
                  <span>{item.startFrequency}</span>
                  <span style={{ margin: '0 4px' }}>~</span>
                  <span>{item.stopFrequency}</span>
                  <span className={styles.unit}>MHz</span>
                </div>
                <div className={styles.unit}>
                  <span>@</span>
                  <span>{item.stepFrequency}</span>
                  <span>kHz</span>
                </div>
              </div>
              {item.name && item.name !== '' && (
                <div className={styles.infoitem}>
                  <div className={styles.segname} title={item.name}>
                    {item.name}
                  </div>
                </div>
              )}
            </div>
            {maxLength !== 1 && (
              <svg
                width="24"
                height="24"
                viewBox="0 0 24 24"
                fill="none"
                xmlns="http://www.w3.org/2000/svg"
                onClick={(evt) => {
                  evt.stopPropagation();
                  onDeleteSeg(idx);
                }}
              >
                <path
                  fillRule="evenodd"
                  clipRule="evenodd"
                  d="M16.7812 8.28033C17.074 7.98744 17.074 7.51256 16.7812 7.21967C16.4883 6.92678 16.0134 6.92678 15.7205 7.21967L12.0004 10.9398L8.28033 7.21967C7.98744 6.92678 7.51256 6.92678 7.21967 7.21967C6.92678 7.51256 6.92678 7.98744 7.21967 8.28033L10.9398 12.0004L7.21967 15.7205C6.92678 16.0134 6.92678 16.4883 7.21967 16.7812C7.51256 17.074 7.98744 17.074 8.28033 16.7812L12.0004 13.0611L15.7205 16.7812C16.0134 17.074 16.4883 17.074 16.7812 16.7812C17.074 16.4883 17.074 16.0134 16.7812 15.7205L13.0611 12.0004L16.7812 8.28033Z"
                  fill="white"
                  fillOpacity="0.3"
                />
              </svg>
            )}
          </div>
        ))}
      </div>
    </div>
  );
};

SegList.propTypes = {
  segments: PropTypes.array.isRequired,
  activeIdx: PropTypes.object.isRequired,
  maxLength: PropTypes.number.isRequired,
  onSelectSeg: PropTypes.func.isRequired,
  onDeleteSeg: PropTypes.func.isRequired,
};

export default memo(SegList);
