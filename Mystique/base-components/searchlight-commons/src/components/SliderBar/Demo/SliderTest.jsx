/*
 * @Author: dengys
 * @Date: 2021-09-28 10:48:00
 * @LastEditors: dengys
 * @LastEditTime: 2022-03-04 10:00:41
 */
import React, { useState } from 'react';
import SliderBar from '../index';

const SliderTest = () => {
  const [sliderVal, setSliderVal] = useState(20);

  const change = (e) => {
    if (e.end) {
      window.console.log(e);
      setSliderVal(e.value);
    }
  };

  return (
    <>
      <div
        style={{
          width: '104px',
          padding: '0 10px',
          height: '320px',
          display: 'inline-block',
          marginRight: '50px',
        }}
      >
        <SliderBar value={sliderVal} sliderValue={sliderVal} stitle="电平门限" onSliderValueChange={change} />
      </div>

      <div
        style={{
          width: '104px',
          padding: '0 10px',
          height: '320px',
          display: 'inline-block',
          marginRight: '50px',
        }}
      >
        <SliderBar
          minimum={0}
          maximum={100}
          value={sliderVal}
          sliderValue={sliderVal}
          stitle="质量门限"
          unitName="%"
          colorOptions={{
            gradientColors: ['#2AD1C0'],
          }}
          onSliderValueChange={(e) => setSliderVal(e.value)}
          decimal={1}
        />
      </div>
    </>
  );
};

export default SliderTest;
