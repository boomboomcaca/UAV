/* eslint-disable react/no-array-index-key */
import React, { useState, useEffect } from 'react';
import PropTypes from 'prop-types';
import SingleSegment from './SingleSegment/SingleSegment.jsx';
import SingleLabel from './SingleLabel/SingleLabel.jsx';
import styles from './style.module.less';

const NewSegmentViewX = (props) => {
  const { segmentList, selectSegment, limit, onValueChange, editorDisable, showStep } = props;

  const [allPoint, setAllPoint] = useState(0);
  const [type, setType] = useState(0);

  useEffect(() => {
    let num = 0;
    segmentList.forEach((item) => {
      num = num + ((item.stopFrequency - item.startFrequency) * 1000) / item.stepFrequency + 1;
    });
    setAllPoint(num);
  }, [segmentList]);

  const changeHandle = (e, a) => {
    if (a === 'true') {
      const arr = segmentList.map((i) => {
        if (i.id === selectSegment.segment.id) {
          return {
            ...i,
            ...e.segment,
          };
        }
        return i;
      });
      onValueChange(arr, {
        ...selectSegment,
        segment: {
          ...selectSegment.segment,
          ...e.segment,
        },
      });
    } else {
      const arr = [
        {
          ...segmentList[0],
          ...e.segment,
        },
      ];
      onValueChange(arr);
    }
  };

  useEffect(() => {
    if (editorDisable) {
      if (selectSegment.flag) {
        if (segmentList.length === 1) {
          setType(2);
        } else {
          setType(4);
        }
      } else if (segmentList.length === 1) {
        setType(2);
      } else {
        setType(3);
      }
    } else if (selectSegment.flag) {
      if (segmentList.length === 1) {
        setType(0);
      } else {
        setType(1);
      }
    } else if (segmentList.length === 1) {
      setType(0);
    } else {
      setType(3);
    }
  }, [selectSegment, editorDisable, segmentList]);

  return (
    <div className={styles.srootnew}>
      {segmentList.length > 0 && (
        <>
          {type === 1 ? (
            <SingleSegment
              key={`edit_${selectSegment.segment.id}`}
              startFrequency={selectSegment.segment.startFrequency}
              stopFrequency={selectSegment.segment.stopFrequency}
              stepFrequency={selectSegment.segment.stepFrequency}
              minFrequency={limit.min}
              maxFrequency={limit.max}
              stepItems={limit.stepItems}
              onChange={(e) => changeHandle(e, 'true')}
            />
          ) : type === 0 ? (
            <SingleSegment
              key={`edit_${segmentList[0]?.id}`}
              startFrequency={segmentList[0].startFrequency}
              stopFrequency={segmentList[0].stopFrequency}
              stepFrequency={segmentList[0].stepFrequency}
              minFrequency={limit.min}
              maxFrequency={limit.max}
              stepItems={limit.stepItems}
              onChange={(e) => changeHandle(e, 'false')}
            />
          ) : type === 2 ? (
            <SingleLabel
              key={`see_${segmentList[0]?.id}`}
              startFrequency={segmentList[0].startFrequency}
              stopFrequency={segmentList[0].stopFrequency}
              stepFrequency={segmentList[0].stepFrequency}
              showStep={showStep}
            />
          ) : type === 3 ? (
            segmentList.map((s, idx) => {
              const num = ((((s.stopFrequency - s.startFrequency) * 1000) / s.stepFrequency + 1) * 100) / allPoint;
              return (
                <SingleLabel
                  key={`see_${s.id}_${idx}`}
                  startFrequency={s.startFrequency}
                  stopFrequency={s.stopFrequency}
                  width={num}
                  stepFrequency={s.stepFrequency}
                  showStep={showStep}
                />
              );
            })
          ) : (
            <SingleLabel
              key={`see_${selectSegment.segment.id}`}
              startFrequency={selectSegment.segment.startFrequency}
              stopFrequency={selectSegment.segment.stopFrequency}
              stepFrequency={selectSegment.segment.stepFrequency}
              showStep={showStep}
            />
          )}
        </>
      )}
    </div>
  );
};

NewSegmentViewX.defaultProps = {
  segmentList: [],
  selectSegment: {},
  onValueChange: () => {},
  limit: {
    min: 20,
    max: 8000,
    stepItems: [12.5, 25, 50, 100, 200, 500, 1000],
  },
  editorDisable: false,
  showStep: false,
};

NewSegmentViewX.propTypes = {
  segmentList: PropTypes.array,
  selectSegment: PropTypes.object,
  onValueChange: PropTypes.func,
  limit: PropTypes.object,
  editorDisable: PropTypes.bool,
  showStep: PropTypes.bool,
};

export default NewSegmentViewX;
