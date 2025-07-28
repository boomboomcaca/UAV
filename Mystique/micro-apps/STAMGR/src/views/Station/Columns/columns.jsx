import React from 'react';
import { PenEditIcon, CopyIcon } from 'dc-icon';
import { Switch, Button } from 'dui';
import { getState } from '@/hooks/dictKeys';
import styles from './column.module.less';

const getColumns = (onEdit, onCopy, onActive, onRestart) => {
  return [
    {
      key: 'mfid',
      name: '编号',
    },
    {
      key: 'name',
      name: '名称',
    },
    {
      key: 'categoryStr',
      name: '类型',
    },
    {
      key: 'zone',
      name: '行政区划',
    },
    {
      key: 'groupName',
      name: '分组',
    },
    {
      key: 'state',
      name: '状态',
      render: (dat) => {
        const state = getState(dat.state);
        return <div style={{ color: state.color }}>{state.tag}</div>;
      },
    },
    {
      key: 'operate',
      name: '操作',
      render: (dat) => {
        return (
          <div className={styles.operate}>
            <PenEditIcon
              iconSize={24}
              onClick={() => {
                onEdit?.(dat);
              }}
            />
            <CopyIcon
              iconSize={24}
              onClick={() => {
                onCopy?.(dat);
              }}
            />
          </div>
        );
      },
    },
    {
      key: 'isActive',
      name: '控制' /* '开启/关闭' */,
      render: (dat) => {
        return (
          <div className={styles.operate}>
            {/* <Switch
              selected={dat.isActive === true}
              onChange={() => {
                onActive?.(dat);
              }}
            /> */}
            <Button
              size="small"
              onClick={() => {
                onRestart?.(dat);
              }}
            >
              重启
            </Button>
          </div>
        );
      },
    },
  ];
};

export default getColumns;
