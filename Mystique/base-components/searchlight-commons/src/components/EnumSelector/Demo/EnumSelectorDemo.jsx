/* eslint-disable */
import React, { useEffect, useState } from 'react';
import { Button } from 'dui';
import EnumSelector from '../EnumSelector.jsx';
import testData from './data.js';

const EnumSelectorDemo = () => {
  const data1 = [
    { value: 10, display: '10 kHz' },
    { value: 15, display: '15 kHz' },
    { value: 40, display: '40 kHz' },
    { value: 100, display: '100 kHz' },
    { value: 50, display: '50 kHz' },
    { value: 125, display: '125 kHz' },
    { value: 150, display: '150 kHz' },
    { value: 200, display: '200 kHz' },
    { value: 500, display: '500 kHz' },
    { value: 800, display: '800 kHz' },
    { value: 20000, display: '20 MHz' },
    { value: 40000, display: '40 MHz' },
  ];

  const [subItems, setSubItems] = useState(testData);

  const [items, setItems] = useState(data1);

  const [selValue1, setSelValue1] = useState(100);
  const [selValue2, setSelValue2] = useState(100);
  const [selValue3, setSelValue3] = useState(100);
  const [isLightUp, setIsLightUp] = useState(true);

  return (
    <div style={{ display: 'flex' }}>
      <div style={{ width: '360px', backgroundColor: '#181B37', color: 'white' }}>
        带宽设置
        <br />
        <div style={{ height: 100 }}>
          <EnumSelector
            position="left"
            caption="中频带宽"
            items={items}
            value={selValue1}
            onValueChanged={(index, value) => {
              console.log('bandwidth changed', value);
              setSelValue1(value);
            }}
          />
        </div>
        <br />
        <Button
          onClick={() => {
            setItems([]);
          }}
        >
          更新值
        </Button>
        <Button onClick={() => setSelValue1(200)}>更新值1</Button>
        <div
          style={{
            width: '600px',
            height: '200px',
            position: 'absolute',
            bottom: '40px',
            left: '300px',
            background: 'rgb(24, 27, 55)',
          }}
        >
          <EnumSelector
            position="bottom"
            keyBoardType="simple"
            caption="步进"
            items={items}
            value={selValue3}
            onValueChanged={(index, value) => {
              console.log('bandwidth changed', value);
              setSelValue3(value);
            }}
          />
        </div>
      </div>
      <div style={{ width: '390px', marginLeft: '200px', backgroundColor: '#181B37', color: 'white' }}>
        业务频段和信道中心频率设置
        <br />
        <div style={{ height: 100 }}>
          <EnumSelector
            position="center"
            lightUp={false}
            levelItems={subItems}
            caption="信道"
            value={selValue2}
            onValueChanged={(index, value) => {
              console.log('level---->', index, value);
              setSelValue2(value);
            }}
            onCaptionValueChanged={(index, value) => {
              console.log('Caption---->', index, value);
            }}
            onClickSearch={(e) => {
              window.console.log(e);
            }}
          />
        </div>
        <br />
        <div>
          <Button
            onClick={() => {
              // setSubItems([]);
              setSelValue2(20);
              // setLevelSel([]);
            }}
          >
            更新值
          </Button>
        </div>
      </div>
      <br />
      <div style={{ width: '390px', backgroundColor: '#181B37', color: 'white' }}>
        自定义图标
        <br />
        {/* <EnumSelector
          caption="带宽"
          items={data1}
          onValueChanged={(index, value) => {
            console.log('frequency changed', value);
          }}
          options={{ leftIcon: leftArrow, rightIcon: rightArrow }}
        /> */}
      </div>
    </div>
  );
};

export default EnumSelectorDemo;
