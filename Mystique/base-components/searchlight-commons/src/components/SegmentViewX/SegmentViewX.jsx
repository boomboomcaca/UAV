/* eslint-disable react/no-array-index-key */
import React, { useMemo } from 'react';
import PropTypes from 'prop-types';
import langT from 'dc-intl';
import SingleSegment from './SingleSegment/SingleSegment.jsx';
import SingleLabel from './SingleLabel/SingleLabel.jsx';
import { createGUID } from '../../lib/random';
import styles from './style.module.less';

//dys

const SegmentViewX = (props) => {
  const {
    segments,
    onValueChange,
    minFrequency,
    maxFrequency,
    stepItems,
    disable,
    visibleSegments,
    padding,
    editable,
  } = props;

  const [paddingTop, paddingRight, paddingBottom, paddingLeft] = padding;

  const data = useMemo(() => {
    let ss = segments;
    if (visibleSegments && visibleSegments.length > 0) {
      const visibleS = [];
      for (let i = 0; i < segments.length; i += 1) {
        const item = segments[i];
        item.kkkindex = i;
        if (visibleSegments.includes(i)) {
          visibleS.push(item);
        }
      }
      ss = visibleS;
    }
    return ss;
  }, [segments, visibleSegments]);

  return (
    <div className={styles.sroot} style={{ paddingTop, paddingRight, paddingBottom, paddingLeft }}>
      {(segments || []).length === 0 ? <span style={{ color: 'lightgray' }}>{langT('commons', 'addseg')}</span> : null}

      {data.length < 7 &&
        data.map((s, index) =>
          editable && data.length < 4 ? (
            <SingleSegment
              key={`edit_${s.id}_${index}`}
              startFrequency={s.startFrequency}
              stopFrequency={s.stopFrequency}
              stepFrequency={s.stepFrequency}
              minFrequency={minFrequency}
              maxFrequency={maxFrequency}
              stepItems={stepItems}
              disable={disable}
              onChange={(e) => {
                // 外界入参的index 避免删除后错位
                e.index = s.kkkindex === undefined ? index : s.kkkindex;
                if (e.name) {
                  e.id = createGUID();
                  delete e.name;
                }
                e.id = e.id || createGUID();
                onValueChange(e);
              }}
              showStep={!(segments.length > 5)}
            />
          ) : (
            <SingleLabel
              key={`see_${s.id}_${index}`}
              startFrequency={s.startFrequency}
              stopFrequency={s.stopFrequency}
              stepFrequency={s.stepFrequency}
              showStep={!(segments.length > 5)}
            />
          ),
        )}
    </div>
  );
};

SegmentViewX.defaultProps = {
  minFrequency: 20,
  maxFrequency: 8000,
  stepItems: [12.5, 25, 50, 100, 200, 500, 1000],
  segments: [],
  disable: false,
  onValueChange: () => {},
  visibleSegments: [],
  padding: [0, 0, 0, 88],
  editable: true,
};

SegmentViewX.propTypes = {
  minFrequency: PropTypes.number,
  maxFrequency: PropTypes.number,
  stepItems: PropTypes.array,
  segments: PropTypes.array,
  disable: PropTypes.bool,
  onValueChange: PropTypes.func,
  visibleSegments: PropTypes.array,
  padding: PropTypes.array,
  editable: PropTypes.bool,
};

export default SegmentViewX;
