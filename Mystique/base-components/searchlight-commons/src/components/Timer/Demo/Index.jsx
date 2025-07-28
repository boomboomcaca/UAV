/*
 * @Author: XYQ
 * @Date: 2022-06-07 10:22:22
 * @LastEditors: XYQ
 * @LastEditTime: 2022-06-07 15:55:59
 * @Description: file content
 */
import React, { useState, useEffect } from 'react';
import { Button, message, TimeSelect } from 'dui';
import Timer from '../Timer.jsx';

export default () => {
  const [ss, setS] = useState(false);
  const [key, setkey] = useState(1);
  const [value, setValue] = useState({
    h: 0,
    m: 0,
  });
  const onChange = (val) => {
    setValue(val);
  };

  return (
    <div>
      <TimeSelect valueList={value} onChange={onChange} />
      <Timer
        time={value}
        onChange={(e) => {
          // true 开始 false 停止
          if (!e) {
            message.info('2222');
            setS(false);
          }
        }}
        onStart={ss}
        initialize={key}
      />
      <Button onClick={(e) => setS(true)}>开始</Button>
      <Button onClick={(e) => setS(false)}>停止</Button>
      <Button onClick={(e) => setkey(Math.random())}>重置</Button>
    </div>
  );
};
