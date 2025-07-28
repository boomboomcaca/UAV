/*
 * @Author: dengys
 * @Date: 2022-03-17 16:42:32
 * @LastEditors: dengys
 * @LastEditTime: 2022-03-17 16:52:15
 */
import React, { useState } from 'react';
import { Button } from 'dui';
import DataList from '..';
import request from '../../../api/request';
import styles from './index.module.less';

const { Type, Func } = DataList;

const Demo = () => {
  const [visible, setVisible] = useState(true);
  return (
    <>
      <DataList
        visible={visible}
        appConfig={{
          apiBaseUrl: 'http://192.168.102.16:12001',
          // appid: '7284525d-1b81-4301-96fd-cc5ba46e731d', // fdf
          // appid: 'ed37cd3d-f6d4-f952-6bb7-8d70664e0357', // wbdf
          appid: '92f79c59-3d63-438d-9756-6b6b3018b654', // bsdec
          wsTaskUrl: 'ws://192.168.102.16:12001/control',
          wsNotiUrl: 'ws://192.168.102.16:12001/notify',
        }}
        request={request}
        // listType={Type.Single}
        // listType="fdf"
        // functionName="fdf"
        // feature="iqretri"
        feature="bsdec"
        showPlay={false}
        showType={['capture', 'replay']}
        // showType={['replay']}
        // showType={['capture']}
      />
      <div className={styles.float}>
        <Button
          onClick={() => {
            setVisible(false);
          }}
        >
          返回
        </Button>
        <Button
          onClick={() => {
            setVisible(true);
          }}
        >
          数据
        </Button>
      </div>
    </>
  );
};

export default Demo;
