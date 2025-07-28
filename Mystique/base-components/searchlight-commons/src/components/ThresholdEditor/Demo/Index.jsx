import React, { useState, memo, useEffect, useRef } from 'react';
import PropTypes from 'prop-types';
import { Radio } from 'dui';
import ThresholdEditor from '../Index.jsx';
import styles from './index.module.less';

const ThresoldEditorDemo = () => {
  const [editorType, setEditorType] = useState('ffm');
  const [curThr, setCurThr] = useState();
  const [segments] = useState([
    {
      id: '123456',
      startFrequency: 88,
      stopFrequency: 108,
      stepFrequency: 25,
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
  const [demoData, setDemoData] = useState();
  const [mockSeg, setMockSeg] = useState();

  const [mscanLabels, setMscanLables] = useState();
  useEffect(() => {
    if (editorType === 'ffm') {
      const temp = [];
      for (let i = 0; i < 21; i += 1) {
        temp[i] = Math.round(Math.random() * 50);
      }
      setDemoData(temp);
      setMockSeg({
        startFrequency: 101.5,
        stopFrequency: 102.5,
        stepFrequency: 1000 / (temp.length - 1),
      });
    } else if (editorType === 'scan') {
      const temp = [];
      let offset = 0;
      segments.forEach((seg) => {
        const pc = Math.round(((seg.stopFrequency - seg.startFrequency) * 1000) / seg.stepFrequency) + 1;
        for (let i = 0; i < pc; i += 1) {
          temp[offset + i] = Math.round(Math.random() * 55);
        }
        offset += pc;
      });
      setDemoData(temp);
    } else if (editorType === 'mscan') {
      const lables = [];
      const demo = [];
      for (let i = 0; i < 36; i += 1) {
        lables[i] = 101.7 + i;
        demo[i] = Math.round(Math.random() * 55);
      }
      setMscanLables(lables);
      setDemoData(demo);
    } else {
      setDemoData(undefined);
    }
  }, [editorType]);

  return (
    <div
      style={{
        display: 'flex',
        flexDirection: 'column',
        // justifyContent: 'space-between',
        height: '-webkit-fill-available',
        marginBottom: '50px',
      }}
    >
      <Radio
        className={styles.select}
        options={[
          { label: 'FFM', value: 'ffm' },
          { label: 'SCAN', value: 'scan' },
          { label: 'SCAN-nodata', value: 'scan1' },
          { label: 'MSCAN', value: 'mscan' },
        ]}
        // options={[]}
        value={editorType}
        onChange={(val) => {
          setEditorType(val);
        }}
      />
      <ThresholdEditor
        classsName={styles.chart}
        // referenceData={editorType === 'scan1' ? undefined : demoData}
        customAxisX={false}
        referenceData={demoData}
        thresholdData={curThr}
        onEnsure={(e) => {
          setCurThr(e);
        }}
        axisXLabels={editorType !== 'mscan' ? undefined : mscanLabels}
        onCancel={(e) => {
          console.log('cancel');
        }}
        segments={editorType === 'ffm' ? [mockSeg] : editorType !== 'mscan' ? segments : undefined}
      />
    </div>
  );
};

export default ThresoldEditorDemo;
