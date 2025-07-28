import React, { useState, useEffect } from 'react';
import AxisXLabels from './axisesX/AxisXLabels.jsx';
import AxisXNumber from './axisesX/AxisXNumber.jsx';
import AxisXSegments from './axisesX/AxisXSegments.jsx';
import PropTypes from 'prop-types';

const AxisX = (props) => {
  const { labels, segments, selectedSegment, zoomStart, zoomStop, unit } = props;

  const [singleSeg, setSingleSeg] = useState();
  // 是否使用数字型X轴
  const [useNumber, setUseNumber] = useState(false);
  const [startValue, setStartValue] = useState(0);
  const [stopValue, setStopValue] = useState(10);
  useEffect(() => {
    let oneSeg;
    if (selectedSegment) {
      oneSeg = selectedSegment;
    } else if (segments && segments.length === 1) {
      oneSeg = segments[0];
    }
    if (oneSeg) {
      setSingleSeg(oneSeg);
      setUseNumber(true);
      setStartValue(oneSeg.startFrequency);
      setStopValue(oneSeg.startFrequency);
    } else {
      setUseNumber(false);
    }
  }, [selectedSegment, segments]);

  useEffect(() => {
    if (singleSeg) {
      const { startFrequency, stepFrequency, startIndex } = singleSeg;
      // 根据缩放索引位置计算起始结束频率
      const startFreq = startFrequency + (stepFrequency * (zoomStart - startIndex)) / 1000;
      const stopFreq = startFrequency + (stepFrequency * (zoomStop - startIndex)) / 1000;
      setStartValue(startFreq);
      setStopValue(stopFreq);
    }
  }, [singleSeg, zoomStart, zoomStop]);

  return (
    <>
      {labels && <AxisXLabels labels={labels} zoomStart={zoomStart} zoomStop={zoomStop} unit={unit} />}
      {segments && !useNumber && (
        <AxisXSegments segments={segments} zoomStart={zoomStart} zoomStop={zoomStop} unit={unit} />
      )}
      {useNumber && <AxisXNumber zoomStart={startValue} zoomStop={stopValue} unit={unit} />}
    </>
  );
};

AxisX.defaultProps = {
  labels: undefined,
  segments: undefined,
  selectedSegment: undefined,
  // 如果设置了labels，则这里为索引，否则为值
  zoomStart: 88,
  // 如果设置了labels，则这里为索引，否则为值
  zoomStop: 108,
  //   minimumY: -20,
  //   maximumY: 80,
  unit: 'MHz',
};

AxisX.propTypes = {
  labels: PropTypes.array,
  segments: PropTypes.any,
  selectedSegment: PropTypes.any,
  zoomStart: PropTypes.number,
  zoomStop: PropTypes.number,
  unit: PropTypes.string,
};

export default AxisX;
