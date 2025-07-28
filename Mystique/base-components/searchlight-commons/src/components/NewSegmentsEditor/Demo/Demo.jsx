import React, { useState } from 'react';
import NewSegmentsEditor from '../index';
import NewSegmentViewX from '../../NewSegmentViewX';
import data from '../data.json';
import SegEditor from '../../SegEditor';
import styles from './index.module.less';

export default function Demo() {
  const [segmentList, setSegmentList] = useState([
    {
      id: '123456',
      name: 'seg1',
      startFrequency: 88,
      stopFrequency: 108,
      stepFrequency: 12.5,
    },
    {
      id: '123457',
      startFrequency: 109,
      stopFrequency: 160,
      stepFrequency: 25,
    },
    {
      id: '123458',
      startFrequency: 200,
      stopFrequency: 300,
      stepFrequency: 50,
    },
  ]);
  const limit = {
    min: 20,
    max: 8000,
    stepItems: [12.5, 25, 50, 100, 200, 500, 1000],
  };
  const [isSelect, setIsSelect] = useState({
    segment: {},
    flag: false,
  });
  const getSegmentList = (e) => {
    setSegmentList(e);
  };

  const deleteHandle = (e) => {
    setSegmentList(e);
  };
  const sigleHandle = (e) => {
    setIsSelect(e);
  };
  return (
    <div className={styles.root}>
      <NewSegmentsEditor
        selectSegment={isSelect}
        selectedChange={sigleHandle}
        editable={false}
        segmentList={segmentList}
        onlyName
      />
      <div className={styles.aaa}>
        <div className={styles.b}>
          <NewSegmentsEditor
            selectSegment={isSelect}
            selectedChange={sigleHandle}
            deleteSegmentFunc={deleteHandle}
            segmentList={segmentList}
          />
        </div>
        <SegEditor
          disabled={isSelect.flag}
          segmentList={segmentList}
          limit={limit}
          getSegmentList={getSegmentList}
          data={data}
        />
      </div>
      <div style={{ height: 32 }}>
        <NewSegmentViewX
          segmentList={segmentList}
          selectSegment={isSelect}
          onValueChange={(a, b) => {
            if (b) {
              setIsSelect(b);
            }
            setSegmentList(a);
          }}
          limit={limit}
        />
      </div>
      <div style={{ height: 32 }}>
        <NewSegmentViewX
          segmentList={segmentList}
          selectSegment={isSelect}
          editorDisable
          showStep
          onValueChange={(a, b) => {
            if (b) {
              setIsSelect(b);
            }
            setSegmentList(a);
          }}
          limit={limit}
        />
      </div>
    </div>
  );
}
