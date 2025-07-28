import React, { useState } from 'react';
import MultipleSelect from '../index';

const { Option } = MultipleSelect;

export default function Demo() {
  const [value, setValue] = useState([]);
  return (
    <div>
      <MultipleSelect value={value} onChange={(val) => setValue(val)}>
        <Option value="aaa">111</Option>
        <Option value="bbb">222</Option>
        <Option value="ccc">333</Option>
        <Option value="ddd">444</Option>
        <Option value="eee">555</Option>
        <Option value="fff">666</Option>
        <Option value="ggg">777</Option>
      </MultipleSelect>
    </div>
  );
}
