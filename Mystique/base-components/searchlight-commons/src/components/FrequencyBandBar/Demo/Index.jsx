import React, { useState, useEffect } from 'react';
import { Button } from 'dui';
import FrequencyBandBar from '../FrequencyBandBar.jsx';
export default () => {
  // const list = [
  //   {
  //     id: 0,
  //     choosed: true,
  //     startFrequency: 153,
  //     stopFrequency: 322,
  //   },
  //   {
  //     id: 1,
  //     choosed: true,
  //     startFrequency: 329,
  //     stopFrequency: 406,
  //   },
  //   {
  //     id: 2,
  //     choosed: true,
  //     startFrequency: 614,
  //     stopFrequency: 1000,
  //   },
  //   {
  //     id: 3,
  //     choosed: true,
  //     startFrequency: 1427,
  //     stopFrequency: 1606,
  //   },
  //   {
  //     id: 4,
  //     choosed: false,
  //     startFrequency: 1606,
  //     stopFrequency: 1723,
  //   },
  //   {
  //     id: 5,
  //     choosed: false,
  //     startFrequency: 2700,
  //     stopFrequency: 3300,
  //   },
  //   {
  //     id: 6,
  //     choosed: false,
  //     startFrequency: 6600,
  //     stopFrequency: 6700,
  //   },
  //   {
  //     id: 7,
  //     choosed: false,
  //     startFrequency: 8600,
  //     stopFrequency: 8700,
  //   },
  //   {
  //     id: 8,
  //     choosed: false,
  //     startFrequency: 8700,
  //     stopFrequency: 12100,
  //   },
  // ];
  const [freList, setFreList] = useState([]);
  const [isShow, setIsShow] = useState(true);
  useEffect(() => {
    setTimeout(() => {
      setFreList([
        {
          id: 143321326419970,
          segmentId: 1,
          startFrequency: 70,
          stopFrequency: 150,
          referenceLevel: -120,
          attenuation: 30,
          resolutionBandwidth: 30,
          videoBandwidth: 100,
          preAmpSwitch: 1,
          integrationTime: 10,
          repeatTimes: 1000,
          scanTime: 20,
          choosed: true,
        },
        {
          id: 143321326419971,
          segmentId: 2,
          startFrequency: 150,
          stopFrequency: 300,
          referenceLevel: -120,
          attenuation: 30,
          resolutionBandwidth: 30,
          videoBandwidth: 100,
          preAmpSwitch: 1,
          integrationTime: 10,
          repeatTimes: 1000,
          scanTime: 20,
          choosed: true,
        },
        {
          id: 143321326419972,
          segmentId: 3,
          startFrequency: 300,
          stopFrequency: 800,
          referenceLevel: -120,
          attenuation: 30,
          resolutionBandwidth: 30,
          videoBandwidth: 100,
          preAmpSwitch: 1,
          integrationTime: 10,
          repeatTimes: 1000,
          scanTime: 20,
          choosed: true,
        },
        {
          id: 143321326419973,
          segmentId: 10,
          startFrequency: 153,
          stopFrequency: 322,
          referenceLevel: -120,
          attenuation: 30,
          resolutionBandwidth: 30,
          videoBandwidth: 100,
          preAmpSwitch: 1,
          integrationTime: 10,
          repeatTimes: 1000,
          scanTime: 20,
          choosed: true,
        },
      ]);
    }, 1000);
  }, []);
  const onCheckChange = (list) => {
    // 选中列表项
    console.log('选中列表项--->list', list);
  };
  return (
    <div style={{ width: '100%', height: '100%', paddingTop: 100 }}>
      <div>
        <Button onClick={() => setIsShow(!isShow)}>显示/隐藏</Button>
      </div>
      {isShow && <FrequencyBandBar frequencyList={freList} onCheckChange={onCheckChange} />}
    </div>
  );
};
