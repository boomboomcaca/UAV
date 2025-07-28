import React, { useState, useRef } from 'react';
import { SettingOutlined } from '@ant-design/icons';
import Input from '../index';

export default function Demo() {
  const [vvv, setvvv] = useState('');
  const inputRef = useRef();

  return (
    <div>
      <Input
        placeholder="请输入marker名称"
        style={{ width: 250 }}
        // suffix="dBμV"
        allowClear
        value={vvv}
        onSearch={(str) => window.console.log(`${str}`)}
        onPressEnter={(str) => window.console.log(str)}
        ref={inputRef}
        maxLength={5}
        onChange={(val) => {
          setvvv(val);
          window.console.log(val);
        }}
        rules={[
          {
            required: true,
            message: '请输入中频衰减',
          },
          {
            pattern: /^\d+$/,
            message: '请输入整数',
          },
        ]}
      />
      <br />
      <br />
      <br />
      <Input type="password" />
      <br />
      <br />
      <br />
      <Input size="large" suffix={<SettingOutlined />} />
      <br />
      <br />
      <br />
      <Input
        allowClear
        showSearch
        onSearch={(str) => window.console.log(`${str}`)}
        onPressEnter={(str) => window.console.log(str)}
        placeholder="请输入marker名称"
        style={{ width: 250 }}
        rules={[
          {
            required: true,
            message: '请输入中频衰减',
          },
          { type: 'integer', transform: Number, message: '请输入整数' },
        ]}
      />
    </div>
  );
}
