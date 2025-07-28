import React, { useState } from 'react';
import { Button } from 'dui';
import FilterDrawer from '../index';

const Demo = () => {
  const [show, setShow] = useState(false);
  const [timeRange, setTimeRange] = useState([]);
  const [options, setOptions] = useState([
    {
      name: '频段',
      visible: false,
      type: 'scan',
      // 每行显示个数，占时最多4种
      child: 4,
      option: [
        {
          label: '四川',
          value: 'sc1',
        },
        {
          label: '云南',
          value: 'yn2',
        },
        {
          label: '贵州',
          value: 'gz3',
        },
        {
          label: '四川',
          value: 'sc2',
        },
        {
          label: '云南',
          value: 'yn3',
        },
        {
          label: '贵州',
          value: 'gz4',
        },
      ],
      value: [],
    },
    {
      name: '频段',
      visible: true,
      type: 'scan1',
      // 每行显示个数，占时最多4种
      child: 3,
      option: [
        {
          label: '四川',
          value: 'sc1',
        },
        {
          label: '云南',
          value: 'yn2',
        },
        {
          label: '贵州',
          value: 'gz3',
        },
        {
          label: '四川',
          value: 'sc2',
        },
        {
          label: '云南',
          value: 'yn3',
        },
      ],
      value: [],
    },
    {
      name: '站点',
      visible: true,
      type: 'station',
      child: 2,
      option: [
        {
          label: '四川',
          value: 'sc1',
        },
        {
          label: '云南',
          value: 'yn2',
        },
        {
          label: '贵州',
          value: 'gz3',
        },
        {
          label: '四川',
          value: 'sc2',
        },
        {
          label: '云南',
          value: 'yn3',
        },
        {
          label: '贵州',
          value: 'gz4',
        },
      ],
      value: [],
    },
    {
      name: '站点',
      visible: true,
      child: 1,
      type: 'station1',
      option: [
        {
          label: '四川',
          value: 'sc1',
        },
        {
          label: '云南',
          value: 'yn2',
        },
        {
          label: '贵州',
          value: 'gz3',
        },
      ],
      value: [],
    },
  ]);

  return (
    <>
      <FilterDrawer
        // drawer宽度
        width={400}
        // drawer标题
        title="筛选"
        isShow={show}
        visibleHandle={(e) => setShow(e)}
        options={options}
        okHandle={(e) => {
          setOptions(e);
        }}
        contentLeft={<div style={{ marginRight: '8px' }}>有删除条</div>}
        contentRight={<Button>新增</Button>}
      />
      <span>---------------</span>
      <FilterDrawer
        // drawer宽度
        width={400}
        // drawer标题
        title="筛选"
        enableDelete={false}
        timeRange={timeRange}
        isShow={show}
        visibleHandle={(e) => setShow(e)}
        options={options}
        contentLeft={<div style={{ marginRight: '8px' }}>没有删除条</div>}
        okHandle={(e, a) => {
          setOptions(e);
          setTimeRange(a);
        }}
      />
    </>
  );
};

export default Demo;
