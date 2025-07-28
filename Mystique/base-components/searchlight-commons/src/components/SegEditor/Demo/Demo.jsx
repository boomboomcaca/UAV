import React, { useState } from 'react';
import SegEditor from '../index';
import data from '../data.json';
import styles from './index.module.less';

const Demo = () => {
  const [segmentList, setSegmentList] = useState([
    {
      id: '1234',
      startFrequency: 88,
      stopFrequency: 1008,
      stepFrequency: 12.5,
    },
  ]);
  const limit = {
    min: 20,
    max: 8000,
    stepItems: [12.5, 25, 50, 100, 200, 500, 1000],
  };
  const getSegmentList = (e) => {
    setSegmentList(e);
  };
  return (
    <div className={styles.root}>
      <SegEditor
        disabled={false}
        segmentList={segmentList}
        limit={limit}
        getSegmentList={getSegmentList}
        data={data}
        maxLength={8}
      />
    </div>
  );
};

export default Demo;
