import React, { useState } from 'react';
import InputNumber1 from '../index';

export default function Demo() {
  const [inputValue, setValue] = useState(50);
  return (
    <div>
      <InputNumber1
        minimum={20}
        maximum={1000}
        value={inputValue}
        //    digits={4}
        //    step={0.0001}
        suffix=""
        onChange={(val) => setValue(val)}
      />
    </div>
  );
}
