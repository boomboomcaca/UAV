/*
 * @Author: wangXueDong
 * @Date: 2022-02-17 16:14:39
 * @LastEditors: wangXueDong
 * @LastEditTime: 2022-04-19 17:24:03
 */
/*
 * @Author: dengys
 * @Date: 2021-09-28 10:48:00
 * @LastEditors: dengys
 * @LastEditTime: 2022-01-27 17:47:11
 */
import React, { useState } from 'react';
import BubbleSelector from '../index';

const BubbleSelectorTest = () => {
  const [dataSource, setDataSource] = useState([
    {
      value: 3.125,
      display: '3.125kHz',
    },
    {
      value: 6.25,
      display: '6.25kHz',
    },
    {
      value: 12.5,
      display: '12.5kHz',
    },
    {
      value: 25,
      display: '25kHz',
    },
    {
      value: 50,
      display: '50kHz',
    },
    {
      value: 100,
      display: '100kHz',
    },
    {
      value: 120,
      display: '120kHz',
    },
    {
      value: 220,
      display: '220kHz',
    },
    {
      value: 210,
      display: '210kHz',
    },
    {
      value: 550,
      display: '550kHz',
    },
    {
      value: 1000,
      display: '1MHz',
    },
    {
      value: 5000,
      display: '5MHz',
    },
    {
      value: 10000,
      display: '10MHz',
    },
    {
      value: 20000,
      display: '20MHz',
    },
    {
      value: 40000,
      display: '40MHz',
    },
  ]);
  const [value, setValue] = useState(120);
  return (
    <div
      style={{
        width: '900px',
        padding: '10px 10px',
        height: '104px',
        display: 'flex',
        justifyContent: 'center',
        marginRight: '50px',
      }}
    >
      <BubbleSelector
        width="190px"
        dataSource={dataSource}
        value={value}
        position="center"
        onValueChange={(e) => {
          setValue(e.value);
        }}
        // keyBoardType="simple"
      />
    </div>
  );
};

export default BubbleSelectorTest;
