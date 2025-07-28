/*
 * @Author: wangXueDong
 * @Date: 2022-02-17 16:14:39
 * @LastEditors: wangXueDong
 * @LastEditTime: 2022-03-23 17:40:29
 */
/*
 * @Author: dengys
 * @Date: 2021-09-28 10:48:00
 * @LastEditors: dengys
 * @LastEditTime: 2022-01-27 17:47:11
 */
import React, { useState } from 'react';
import Popover from '../index';

const PopoverTest = () => {
  const [value, setValue] = useState(120);
  const content = (
    <div style={{ width: '400px' }}>
      <p>Content</p>
      <p>Content</p>
    </div>
  );
  return (
    <div
      style={{
        width: '900px',
        padding: '10px 10px',
        height: '104px',
        background: 'var(--theme-background-primary)',
        display: 'flex',
        justifyContent: 'center',
        marginRight: '50px',
      }}
    >
      <Popover content={content} position="center">
        <div style={{ width: '180px', height: '50px', lineHeight: '50px', textAlign: 'center', background: 'red' }}>
          PopoverTest
        </div>
      </Popover>
    </div>
  );
};

export default PopoverTest;
