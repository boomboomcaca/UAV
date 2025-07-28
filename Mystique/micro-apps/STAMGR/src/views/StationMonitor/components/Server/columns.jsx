import React from 'react';

const columns = [
  {
    key: 'process',
    name: <div style={{ width: '100%', textAlign: 'start', paddingLeft: 32 }}>进程</div>,
    style: { width: '30%' },
    render: (item) => {
      return <div style={{ width: '100%', textAlign: 'start', paddingLeft: 32 }}>{item.process}</div>;
    },
  },
  {
    key: 'cpu',
    name: 'CPU使用率',
  },
  {
    key: 'memory',
    name: '内存占用率',
  },
  {
    key: 'rwspeed',
    name: '磁盘读写速度',
  },
];

const data = [
  { id: 111, process: 'XX服务1', cpu: '5%', memory: '1.2Mb', rwspeed: '12.1bps' },
  { id: 112, process: 'XX服务2', cpu: '1%', memory: '1.2Mb', rwspeed: '12.1bps' },
  { id: 113, process: 'XX服务3', cpu: '2%', memory: '1.2Mb', rwspeed: '12.1bps' },
  { id: 114, process: 'XX服务4', cpu: '1.1%', memory: '1.2Mb', rwspeed: '12.1bps' },
  { id: 115, process: 'XX服务5', cpu: '0.2%', memory: '1.2Mb', rwspeed: '12.1bps' },
  { id: 120, process: 'XX服务6', cpu: '0.5%', memory: '1.2Mb', rwspeed: '12.1bps' },
  { id: 130, process: 'XX服务7', cpu: '5.6%', memory: '1.2Mb', rwspeed: '12.1bps' },
  { id: 140, process: 'XX服务8', cpu: '1%', memory: '1.2Mb', rwspeed: '12.1bps' },
  { id: 122, process: 'XX服务9', cpu: '2%', memory: '1.2Mb', rwspeed: '12.1bps' },
  { id: 190, process: 'XX服务a', cpu: '1.1%', memory: '1.2Mb', rwspeed: '12.1bps' },
  { id: 191, process: 'XX服务b', cpu: '1.2%', memory: '1.2Mb', rwspeed: '12.1bps' },
  { id: 192, process: 'XX服务c', cpu: '1.1%', memory: '1.2Mb', rwspeed: '12.1bps' },
  { id: 193, process: 'XX服务d', cpu: '0.3%', memory: '1.2Mb', rwspeed: '12.1bps' },
  { id: 195, process: 'XX服务e', cpu: '0.4%', memory: '1.2Mb', rwspeed: '12.1bps' },
  { id: 196, process: 'XX服务f', cpu: '0.1%', memory: '1.2Mb', rwspeed: '12.1bps' },
];

export default columns;
export { data };
