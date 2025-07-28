import React, { useState } from 'react';
import SegmentList from '../SegmentList.jsx';
// eslint-disable-next-line import/extensions
import limit from './limit.json';
import styles from './index.module.less';

export default () => {
  const [segmentList, setSegmentList] = useState([
    {
      stepFrequency: 25,
      stopFrequency: 108,
      startFrequency: 87,
    },
  ]);
  const onValueChange = (e) => {
    setSegmentList(e);
  };
  return (
    <div className={styles.container}>
      <SegmentList limit={limit} onValueChange={onValueChange} segmentList={segmentList} />
    </div>
  );
};
