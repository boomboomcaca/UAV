import React, { useEffect, useState } from 'react';
import PropTypes from 'prop-types';
import SignalTable from '@/components/SignalTable';
import styles from './SignalTableDemo.module.less';

const SignalTableDemo = () => {
  const [dataSource, setDataSource] = useState([]);
  useEffect(() => {
    const data = [
      {
        signalType: 0,
        frequency: 102,
        bandwidth: 50,
        edgeName: '站点1',
      },
      {
        signalType: 1,
        frequency: 88,
        bandwidth: 35,
        edgeName: '站点2',
      },
      {
        signalType: 2,
        frequency: 107,
        bandwidth: 25,
        edgeName: '站点3',
      },
    ];
    setDataSource([...data]);
    return () => {};
  }, []);
  const columns = [
    {
      key: 'signalType',
      name: '',
      options: [
        {
          label: '全部信号',
          value: '',
          color: null,
        },
        {
          label: '未知信号',
          value: 0,
          color: '#FFD118',
        },
        {
          label: '合法信号',
          value: 1,
          color: '#40A9FF',
        },
        {
          label: '新信号',
          value: 2,
          color: '#A0D911',
        },
      ],
    },
    {
      key: 'frequency',
      name: '频率',
      sort: true,
    },
    {
      key: 'bandwidth',
      name: '估测带宽',
      sort: true,
    },
    {
      key: 'edgeName',
      name: '台站名称',
    },
    {
      key: '',
      name: '操作',
      action: true,
    },
  ];
  return (
    <div className={styles.container}>
      <SignalTable showSelection={false} columns={columns} data={dataSource} />
    </div>
  );
};

SignalTableDemo.defaultProps = {};

SignalTableDemo.propTypes = {};

export default SignalTableDemo;
