/*
 * @Author: wangXueDong
 * @Date: 2022-02-17 16:14:39
 * @LastEditors: wangXueDong
 * @LastEditTime: 2022-07-08 14:33:32
 */
/*
 * @Author: dengys
 * @Date: 2021-09-28 10:48:00
 * @LastEditors: dengys
 * @LastEditTime: 2022-01-27 17:47:11
 */
import React, { useState } from 'react';
import ColumnScaleSliderBar from '../index';

const ColumnScaleSliderBarTest = () => {
  const [sliderVal, setSliderVal] = useState(0.5);
  const [sliderVal1, setSliderVal1] = useState(30);
  const [scaleData] = useState([0, 0.001, 0.01, 0.02, 0.05, 0.1, 0.2]);
  const [labelData] = useState(['0s', '1ms', '10ms', '20ms', '50ms', '100ms', '200ms']);
  return (
    <>
      <div
        style={{
          width: '104px',
          padding: '10px 10px',
          height: '300px',
          background: 'var(--theme-background-primary)',
          display: 'inline-block',
          marginRight: '50px',
        }}
      >
        <ColumnScaleSliderBar
          // minimum={-20}
          // maximum={120}
          labelData={labelData}
          scaleData={scaleData}
          value={sliderVal}
          unitName="dB"
          width="12px"
          onSliderValueChange={(e) => {
            setSliderVal(e.value);
          }}
        />
      </div>
      <div
        style={{
          width: '104px',
          padding: '10px 10px',
          height: '300px',
          background: 'var(--theme-background-primary)',
          display: 'inline-block',
          marginRight: '50px',
        }}
      >
        <ColumnScaleSliderBar
          // disable
          title="保持时间"
          isSet
          minimum={0}
          maximum={300}
          // labelData={labelData}
          // scaleData={scaleData}
          value={sliderVal1}
          unitName="dB"
          width="12px"
          onSliderValueChange={(e) => {
            setSliderVal1(e.value);
          }}
        />
      </div>
    </>
  );
};

export default ColumnScaleSliderBarTest;
