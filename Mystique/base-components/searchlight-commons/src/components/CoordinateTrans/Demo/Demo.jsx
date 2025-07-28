import React, { useEffect, useState } from 'react';
import { Button, InputNumber } from 'dui';
import CoordinateTrans from '../CoordinateTrans.jsx';
import { gps2Degree, degree2DMS, degree2DM } from '../weapon';

export default function Demo() {
  const [val1, setVal1] = useState(39.928889);
  const [val2, setVal2] = useState(39.928889);

  const [val3, setVal3] = useState('');
  const [val4, setVal4] = useState('');
  const [val5, setVal5] = useState('');
  const [val6, setVal6] = useState('');
  const [val7, setVal7] = useState('');
  const [val8, setVal8] = useState('');

  const [test, setTest] = useState('');

  useEffect(() => {
    setVal3(gps2Degree('S 39°55′44″'));
    setVal4(gps2Degree('N 39°55′44″'));
    setVal5(gps2Degree('W 39°55′44″'));
    setVal6(gps2Degree('E 39°55′44″'));
    setVal7(degree2DMS('39.928889', 'n'));
    setVal8(degree2DM('39.928889', 'n'));
  }, []);

  useEffect(() => {
    if (val1 !== '') {
      setVal7(degree2DMS(val1.toFixed(6), 'n'));
    }
  }, [val1]);

  return (
    <>
      <CoordinateTrans
        maximum={180}
        minimum={-180}
        units={['W', 'E']}
        value={val1}
        onChange={(val) => {
          setVal1(val);
        }}
      />
      <br />
      <CoordinateTrans
        maximum={90}
        minimum={-90}
        value={val2}
        units={['S', 'N']}
        onChange={(val) => {
          setVal2(val);
        }}
      />
      <br />
      <div>经度：{val1}</div>
      <div>纬度：{val2}</div>
      <br />
      <div>S 39°55′44″：{val3}</div>
      <div>N 39°55′44″：{val4}</div>
      <div>W 39°55′44″：{val5}</div>
      <div>E 39°55′44″：{val6}</div>
      <br />
      <div>
        {val1} n：{val7}
      </div>
      <div>
        {val1} n：{val8}
      </div>
      <br />
      <Button
        onClick={() => {
          setVal1('');
        }}
      >
        ???
      </Button>
      <Button
        onClick={() => {
          setVal1(39.928889);
        }}
      >
        DDD
      </Button>
      <InputNumber value={test} />
      <Button
        onClick={() => {
          setTest(null);
        }}
      >
        null
      </Button>
      <Button
        onClick={() => {
          setTest(39.928889);
        }}
      >
        test
      </Button>
      <Button
        onClick={() => {
          setTest('');
        }}
      >
        empty
      </Button>
    </>
  );
}
