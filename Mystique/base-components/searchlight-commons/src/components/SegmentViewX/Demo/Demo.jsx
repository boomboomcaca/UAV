import React, { useState, useCallback } from 'react';
import SegmentViewX from '..';
import SegmentsEditor from '../../SegmentsEditor';
import { treeData, tableData } from './demodata';
import styles from './index.module.less';

export default function Demo() {
  const [segments, setSegments] = useState([]);
  // 当前选中的频段
  const [selSegment, setSelSegment] = useState();

  const handleSegmentValueEdit = (e) => {
    const segs = JSON.parse(JSON.stringify(segments));
    const editSeg = segs[e.index];
    const newSeg = e.segment;
    segs[e.index] = { ...editSeg, ...newSeg };
    setSegments(segs);
  };

  const onViewChanged = useCallback(
    (dell) => {
      if (dell.segment) {
        const selSeg = segments.findIndex((s) => s.id === dell.segment.id);
        // TODO 设置选中频段
        setSelSegment(selSeg);
      } else {
        setSelSegment(undefined);
      }
    },
    [segments],
  );

  return (
    <div className={styles.root}>
      <SegmentsEditor
        treeData={treeData}
        tableData={tableData}
        editable
        onTreeSelect={([key], data) => {
          window.window.console.log(key, data);
        }}
        onViewChanged={onViewChanged}
        onSegChanged={(d) => {
          setSegments(JSON.parse(JSON.stringify(d.segment)));
        }}
      />
      <SegmentViewX
        onValueChange={handleSegmentValueEdit}
        segments={segments}
        minFrequency={20}
        maxFrequency={8000}
        editable={false}
        visibleSegments={selSegment !== undefined ? [selSegment] : undefined}
        // stepItems
      />
      <SegmentViewX
        onValueChange={handleSegmentValueEdit}
        segments={segments}
        minFrequency={20}
        maxFrequency={8000}
        visibleSegments={selSegment !== undefined ? [selSegment] : undefined}
        // stepItems
      />
    </div>
  );
}
