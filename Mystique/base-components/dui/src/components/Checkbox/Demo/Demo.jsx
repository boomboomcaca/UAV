import React, { useState } from 'react';
import { StepForwardOutlined } from '@ant-design/icons';
import Checkbox from '../index';

export default function Demo() {
  const [checked1, setChecked1] = useState(false);
  const [checked2, setChecked2] = useState(false);
  const [checked3, setChecked3] = useState(false);

  const [checkedList, setCheckedList] = useState(['sc']);

  const options = [
    {
      label: '四川',
      value: 'sc',
      // icon: <StepForwardOutlined />,
    },
    {
      label: '云南',
      value: 'yn',
      icon: <StepForwardOutlined />,
    },
    {
      label: '贵州',
      value: 'gz',
      icon: <StepForwardOutlined />,
    },
  ];

  return (
    <div>
      <Checkbox>参数1</Checkbox>
      <Checkbox checked={checked1} onChange={(bl) => setChecked1(bl)}>
        参数2
      </Checkbox>

      <br />
      <br />
      <br />
      <Checkbox.Traditional checked={checked2} onChange={(bl) => setChecked2(bl)}>
        参数3
      </Checkbox.Traditional>
      <br />
      <br />
      <br />
      <Checkbox.Traditional checked={checked3} onChange={(bl) => setChecked3(bl)} disabled>
        全选
      </Checkbox.Traditional>
      <br />
      <br />
      <br />

      <Checkbox.Group
        options={options}
        value={checkedList}
        sort={false}
        onChange={(e) => {
          if (e.length > 2) {
            setCheckedList([e[0], e[2]]);
          } else {
            setCheckedList(e);
          }
        }}
      />
    </div>
  );
}
