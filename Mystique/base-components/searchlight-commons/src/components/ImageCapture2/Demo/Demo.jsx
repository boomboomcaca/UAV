import React, { useRef } from 'react';
import { Button } from 'dui';
import ImageCapture2 from '..';
import request from '../../../api/request';
import styles from './index.module.less';
import nikki from './nikki';

export default function Demo() {
  const capRef = useRef(null);
  return (
    <div className={styles.root} id="test">
      <Button
        onClick={() => {
          // capRef.current.onCapture('test', (e) => {
          //   window.console.log(e);
          // });
          capRef.current.onCaptureTest(nikki.uri, (e) => {
            window.console.log(e);
          });
        }}
      >
        截图
      </Button>
      <ImageCapture2 ref={capRef} axios={request} feature="ffm" selectedModule={{ edgeId: 142857 }} />
    </div>
  );
}
