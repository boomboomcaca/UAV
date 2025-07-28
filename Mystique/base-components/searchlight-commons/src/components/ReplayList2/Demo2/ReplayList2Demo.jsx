import React from 'react';
import ReplayList2 from '../ReplayList2.jsx';
import request from '../../../api/request';
import styles from './index.module.less';

const ReplayList2Demo = () => {
  const onSelectChange = (a, b, c) => {
    window.console.log(a, b, c);
  };

  // b6fd7a70-b2ea-4927-8cb9-0bb3927e7a62

  return (
    <ReplayList2
      appConfig={{
        wsTaskUrl: 'ws://192.168.102.99:12001/control',
        wsNotiUrl: 'ws://192.168.102.99:12001/notify',
        // appid: '92f79c59-3d63-438d-9756-6b6b3018b654',
        // appid: '92f79c59-3d63-438d-9756-6b6b3018b654',
        appid: '56ba4d7d-235f-4d18-bcf6-95b58e01ec82',
      }}
      request={request}
      // listType="segments"
      // listType="fdf"
      // listType="fbsdec"
      // functionName="wbdf"
      // listType="wbdf"
      // listType="bsdec"
      // listType="iqretri"
      showPlay
      listType="ffm"
      onSelectChange={onSelectChange}
      footerClassName={styles.foot}
    />
  );
};

ReplayList2Demo.defaultProps = {};

ReplayList2Demo.propTypes = {};

export default ReplayList2Demo;
