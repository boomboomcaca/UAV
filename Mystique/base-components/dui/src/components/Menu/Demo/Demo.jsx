import React, { useState } from 'react';
import Menu from '../index';

export default function Demo() {
  const [value, setValue] = useState('');

  const sssetValue = (val, t) => {
    window.console.log(val);
    window.console.log(t);
    setValue(val);
  };

  return (
    <div style={{ width: '246px' }}>
      <Menu value={value} onClick={sssetValue}>
        <Menu.SubMenu title={<div>自定义</div>}>
          <Menu.Item key="1" transit="111">
            1111111111111111111111
          </Menu.Item>
          <Menu.Item key="2">2222222</Menu.Item>
          <Menu.SubMenu title="123" key="123">
            <Menu.Item key="11">自定义</Menu.Item>
            <Menu.Item key="12">自定义</Menu.Item>
            <Menu.Item key="13">自定义</Menu.Item>
          </Menu.SubMenu>
        </Menu.SubMenu>
        <Menu.SubMenu title={<div>123</div>}>
          <Menu.Item key="4">自定义</Menu.Item>
          <Menu.Item key="5">自定义</Menu.Item>
          <Menu.Item key="6">自定义</Menu.Item>
        </Menu.SubMenu>
        <Menu.SubMenu title={<div>123</div>}>
          <Menu.Item key="7">1111111111111111111111</Menu.Item>
          <Menu.Item key="8">2222222</Menu.Item>
          <Menu.Item key="9">3333333</Menu.Item>
        </Menu.SubMenu>
      </Menu>
      <br />
      <br />
      <br />
    </div>
  );
}
