import React, { useEffect, useState } from 'react';
import SegmentsEditor from '../index';
import SegmentViewX from '../../SegmentViewX';
import { treeData, tableData } from './demodata';
import styles from './index.module.less';

export default function Demo() {
  const [defaultSeg, setDefaultSeg] = useState([
    { id: 'dfault_1', startFrequency: 87, stopFrequency: 108, stepFrequency: 25 },
  ]);
  const [segments, setSegments] = useState([]);
  // 当前选中的频段
  const [selSegIndex, setSelSegIndex] = useState();

  const [initSegs2, setInitSegs2] = useState({
    id: 'dfault_1',
    startFrequency: 87,
    stopFrequency: 108,
    stepFrequency: 25,
  });

  useEffect(() => {
    console.log('reaload on demo');
    // setTimeout(() => {
    //   setDefaultSeg([
    //     { id: 'dfault_1', startFrequency: 87, stopFrequency: 108, stepFrequency: 25 },
    //     { id: 'dfault_2', startFrequency: 85, stopFrequency: 108, stepFrequency: 25 },
    //   ]);
    // }, 10000);
  }, []);

  return (
    <div className={styles.root}>
      <div style={{ display: 'flex', justifyContent: 'center' }}>
        <SegmentsEditor.SingleSeg
          segmentData={initSegs2}
          treeData={treeData}
          tableData={tableData}
          onTreeSelect={(id, data) => {
            window.window.console.log(id, data);
          }}
          callbackseg={(e) => {
            setInitSegs2(e);
            console.log('dgsggsgsgsgsd');
          }}
        />
      </div>
      <SegmentViewX
        onValueChange={(e) => {
          const newSeg = { ...e.segment };
          console.log(e);
          setInitSegs2(newSeg);
        }}
        segments={defaultSeg}
        minFrequency={20}
        maxFrequency={8000}
      />
      <SegmentsEditor
        treeData={treeData}
        tableData={tableData}
        initSegmentData={defaultSeg}
        editable={segments.length < 5}
        onTreeSelect={(id, data) => {
          window.window.console.log(id, data);
        }}
        onViewChanged={(dell) => {
          if (dell.segment) {
            const selSeg = segments.findIndex((s) => s.id === dell.segment.id);
            setSelSegIndex(selSeg);
          } else {
            setSelSegIndex(undefined);
          }
        }}
        onSegChanged={(e) => {
          console.log('onSegChanged', e);
          const newSegs = e.segment.map((s) => {
            return { ...s };
          });
          setSegments(newSegs);
        }}
      />
      <SegmentViewX
        onValueChange={(e) => {
          const segs = [...segments];
          segs[e.index] = { ...e.segment };
          setSegments(segs);
        }}
        segments={segments}
        minFrequency={20}
        maxFrequency={8000}
        visibleSegments={selSegIndex !== undefined ? [selSegIndex] : undefined}
      />
    </div>
  );
}
