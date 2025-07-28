import React, { useState } from 'react';
import Select from '../index';

const { Option } = Select;

export default function Demo() {
  const [value, setValue] = useState(0);
  return (
    <div>
      <Select value={value} onChange={(val) => setValue(val)}>
        {/* <Option value="1">222</Option> */}
        {[].map((item) => (
          <Option value={item} key={item}>
            所有类型
          </Option>
        ))}
        <Option value="">所有类型</Option>
        <Option value={0}>111</Option>
        <Option value="1">222</Option>
        <Option value="ccc">333</Option>
        <Option value="ddd">444</Option>
        <Option value="eee">555</Option>
        <Option value="fff">666</Option>
        <Option value="ggg">777</Option>
      </Select>
    </div>
  );
}
