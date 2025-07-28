import React, { useState, useEffect } from 'react';
import FrequencyBandList from '../FrequencyBandList.jsx';
export default () => {
  const list = [
    {
      id: 0,
      choosed: true,
      startFrequency: 153,
      stopFrequency: 322,
    },
    {
      id: 1,
      choosed: true,
      startFrequency: 329,
      stopFrequency: 406,
    },
    {
      id: 2,
      choosed: true,
      startFrequency: 614,
      stopFrequency: 1000,
    },
    {
      id: 3,
      choosed: true,
      startFrequency: 1427,
      stopFrequency: 1606,
    },
    {
      id: 4,
      choosed: false,
      startFrequency: 1606,
      stopFrequency: 1723,
    },
    {
      id: 5,
      choosed: false,
      disabled: true,
      startFrequency: 2700,
      stopFrequency: 3300,
    },
    // {
    //   id: 6,
    //   choosed: false,
    //   startFrequency: 6600,
    //   stopFrequency: 6700,
    // },
    // {
    //   id: 7,
    //   choosed: false,
    //   startFrequency: 8600,
    //   stopFrequency: 8700,
    // },
    // {
    //   id: 8,
    //   choosed: false,
    //   startFrequency: 8700,
    //   stopFrequency: 12100,
    // },
  ];
  useEffect(() => {}, []);
  const onChange = (list) => {
    console.log(list);
  };
  return (
    <div style={{ width: '100%', height: '100%', paddingTop: 100 }}>
      <FrequencyBandList frequencyList={list} onChange={onChange} />
    </div>
  );
};
