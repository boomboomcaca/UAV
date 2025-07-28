/* eslint-disable react/no-array-index-key */
import React, { useMemo, useState, useEffect } from 'react';
import PropTypes from 'prop-types';
import { SignOutIcon, ArrowLeftIcon, ArrowRightIcon, DelIcon } from './Icons.jsx';
import styles from './style.module.less';

const NewSegmentsEditor = (props) => {
  const { segmentList, deleteSegmentFunc, selectSegment, selectedChange, editable, onlyName } = props;
  const [num, setNum] = useState(0);
  const [flag, setFlag] = useState(false);
  const translateXCss = useMemo(() => {
    return `translateX(-${num * (100 / 6)}%)`;
  }, [num]);

  const clickLeftArrow = () => {
    if (num === 0) {
      return;
    }
    setNum(num - 1);
  };

  const clickRightArrow = () => {
    if (num === segmentList.length - 6) {
      return;
    }
    setNum(num + 1);
  };

  useEffect(() => {
    if (flag) {
      setFlag(false);
      return;
    }
    setNum(0);
  }, [segmentList]);

  return (
    <div className={styles.segRoot}>
      {segmentList.length > 6 && !selectSegment.flag && (
        <div className={styles.leftArrow} onClick={clickLeftArrow}>
          <ArrowLeftIcon style={{ fontSize: '18px' }} />
        </div>
      )}
      {segmentList.length > 6 && !selectSegment.flag && (
        <div className={styles.rightArrow} onClick={clickRightArrow}>
          <ArrowRightIcon style={{ fontSize: '18px' }} />
        </div>
      )}
      {!selectSegment.flag ? (
        <div className={styles.seg} style={{ transform: translateXCss }}>
          {segmentList.map((segment, index) => {
            return (
              <div key={segment.id} className={styles.segitem} style={{ flex: 1 }}>
                <div
                  onClick={() => {
                    if (segmentList.length === 1) return;
                    selectedChange({
                      flag: true,
                      segment,
                      segmentIndex: index,
                    });
                  }}
                  className={styles.buttonSegment}
                >
                  {!onlyName && (
                    <div
                      className={styles.valueColor}
                    >{`${segment.startFrequency}MHz-${segment.stopFrequency}MHz`}</div>
                  )}
                  <div className={onlyName ? styles.valueColor : styles.labelColor}>{segment.name}</div>
                  {editable && segmentList.length > 1 && (
                    <div
                      className={styles.icon}
                      onClick={(e) => {
                        e.stopPropagation();
                        const arr = segmentList.filter((it) => {
                          return it.id !== segment.id;
                        });
                        setFlag(true);
                        clickLeftArrow();
                        deleteSegmentFunc(arr);
                      }}
                    >
                      <DelIcon />
                    </div>
                  )}
                </div>
              </div>
            );
          })}
        </div>
      ) : (
        <div className={styles.segitem} style={{ width: '100%', padding: '0 8px' }}>
          <div className={styles.buttonSegment1}>
            {!onlyName && (
              <div className={styles.valueColor}>
                {`${selectSegment.segment.startFrequency}MHz-${selectSegment.segment.stopFrequency}MHz`}
              </div>
            )}
            <div className={onlyName ? styles.valueColor : styles.labelColor}>{selectSegment.segment.name}</div>
            <div className={styles.icon}>
              <SignOutIcon
                onClick={() => {
                  selectedChange({
                    segment: {},
                    flag: false,
                  });
                }}
                iconSize={20}
                color="var(--theme-font-100)"
              />
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

NewSegmentsEditor.defaultProps = {
  editable: true,
  segmentList: [],
  selectSegment: {},
  deleteSegmentFunc: () => {},
  selectedChange: () => {},
  onlyName: false,
};

NewSegmentsEditor.propTypes = {
  editable: PropTypes.bool,
  segmentList: PropTypes.array,
  selectSegment: PropTypes.object,
  deleteSegmentFunc: PropTypes.func,
  selectedChange: PropTypes.func,
  onlyName: PropTypes.func,
};

export default NewSegmentsEditor;
