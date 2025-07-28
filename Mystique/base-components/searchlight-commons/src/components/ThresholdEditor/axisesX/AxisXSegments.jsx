import React, { useState, memo, useEffect, useRef } from 'react';
import PropTypes from 'prop-types';

const AxisXSegments = (props) => {
  const { segments, zoomStart, zoomStop, unit } = props;
  const [ticks, setTicks] = useState([]);
  const [segmentScale, setScale] = useState([]);

  useEffect(() => {
    if (segments && zoomStart !== null && zoomStop !== null) {
      const zoomLen = zoomStop - zoomStart + 1;
      // 显示段的两端
      const ticks = [];
      const scale = [];
      segments.forEach((seg, index) => {
        const { startFrequency, stopFrequency, startIndex, stepFrequency, pointCount } = seg;
        // 段起始位置
        let flag = 0;
        if ((zoomStart >= startIndex && zoomStart < startIndex + pointCount) || startIndex >= zoomStart) {
          if (startIndex >= zoomStart) {
            ticks.push(startFrequency.toFixed(4));
            flag = startIndex;
          } else {
            // 计算起始频率
            const indexGap = zoomStart - startIndex;
            const startFreq = startFrequency + (stepFrequency * indexGap) / 1000;
            ticks.push(startFreq.toFixed(4));
            flag = zoomStart;
          }
        }
        if ((zoomStop > startIndex && zoomStop <= startIndex + pointCount) || startIndex + pointCount <= zoomStop) {
          if (startIndex + pointCount <= zoomStop) {
            ticks.push(stopFrequency.toFixed(4));
            scale.push(((startIndex + pointCount - flag) * 100) / zoomLen);
          } else {
            const indexGap = zoomStop - startIndex;
            const stopFreq = startFrequency + (stepFrequency * indexGap) / 1000;
            ticks.push(stopFreq.toFixed(4));
            scale.push(((zoomStop - flag) * 100) / zoomLen);
          }
        }
        if (flag === 0) {
          scale.push(0);
          ticks.push('');
          ticks.push('');
        }
      });
      console.log(ticks);
      setTicks(ticks);
      setScale(scale);
    }
  }, [segments, zoomStart, zoomStop]);

  return (
    <div
      style={{
        display: 'flex',
        flexDirection: 'row',
        justifyContent: 'space-between',
        marginLeft: '45px',
      }}
    >
      {zoomStart != null &&
        zoomStop !== null &&
        segmentScale.map((s, index) => {
          return (
            <div
              style={{
                width: `${s}%`,
                display: 'flex',
                flexDirection: 'row',
                justifyContent: 'space-between',
                fontSize: '12px',
                color: 'var(--theme-font-50)',
              }}
            >
              <span>{ticks[index * 2]}</span>
              <span>{ticks[index * 2 + 1]}</span>
            </div>
          );
        })}
    </div>
  );
};

AxisXSegments.defaultProps = {
  segments: [],
  zoomInfo: null,
  // 如果设置了labels，则这里为索引，否则为值
  zoomStart: null,
  // 如果设置了labels，则这里为索引，否则为值
  zoomStop: null,
  unit: 'MHz',
};

AxisXSegments.propTypes = {
  segments: PropTypes.arrayOf(
    PropTypes.shape({
      startFrequency: PropTypes.number,
      stopFrequency: PropTypes.number,
      stepFrequency: PropTypes.number,
      pointCount: PropTypes.number,
      startIndex: PropTypes.number,
    }),
  ),
  zoomInfo: PropTypes.shape({
    startSegIndex: PropTypes.number,
    startOffset: PropTypes.number,
    stopSegIndex: PropTypes.number,
    stopOffset: PropTypes.number,
  }),
  zoomStart: PropTypes.number,
  zoomStop: PropTypes.number,
  unit: PropTypes.string,
};

export default AxisXSegments;
