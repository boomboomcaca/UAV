import React, { useState } from 'react';
import { Button, message } from 'dui';
import FrameView from '@/components/FrameView';
import ReplayBar from '@/components/FrameView/ReplayBar.jsx';
import replayParam1 from './4005';
import replayParam2 from './4006';
import styles from './index.module.less';

const wsurl = 'ws://192.168.102.103:12001/control';

const FrameViewDemo = () => {
  const [data, setData] = useState('hello world!');
  const [voice, setVoice] = useState('hello world!');
  const [visible, setVisible] = useState(false);

  const [rParam, setRParam] = useState(replayParam1);

  return (
    <div className={styles.root}>
      {visible ? (
        <FrameView
          wsurl={wsurl}
          replayParam={replayParam1}
          onDataCallback={(res) => {
            setData(JSON.stringify(res));
          }}
          menu={<Button onClick={() => setVisible(false)}> ReplayBar </Button>}
          action={<Button onClick={() => message.info('action message')}> action message </Button>}
          // hideActions={['audio', 'capture']}
          hideActions={['audio']}
        >
          <div
            style={{
              wordBreak: 'break-all',
              width: '30%',
              height: '30%',
              position: 'absolute',
              right: 200,
              bottom: 200,
              overflowY: 'auto',
              overflowX: 'hidden',
              color: 'white',
              fontSize: 10,
            }}
          >
            {data}
          </div>
        </FrameView>
      ) : (
        <ReplayBar
          wsurl={wsurl}
          replayParam={rParam}
          hideActions={['audio']}
          onDataCallback={(res) => {
            // window.console.log(res);
            setData(JSON.stringify(res));
            if (res.data) {
              res.data.dataCollection.forEach((rdd) => {
                if (rdd.type === 'audio') {
                  setVoice(JSON.stringify(rdd));
                }
              });
            }
          }}
          onVoice={(e) => {
            window.console.log(e);
          }}
          menu={
            <>
              <Button onClick={() => setVisible(true)}> FrameView </Button>
              <Button onClick={() => setRParam(replayParam1)}> 4005.json </Button>
              <Button onClick={() => setRParam(replayParam2)}> 4006.json </Button>
            </>
          }
        />
      )}
      {!visible ? (
        <>
          <div
            style={{
              wordBreak: 'break-all',
              width: '30%',
              height: '30%',
              position: 'absolute',
              right: 200,
              bottom: 200,
              overflowY: 'auto',
              overflowX: 'hidden',
              color: 'white',
              fontSize: 10,
            }}
          >
            {data}
          </div>
          <div
            style={{
              wordBreak: 'break-all',
              width: '30%',
              height: '30%',
              position: 'absolute',
              let: 0,
              bottom: 200,
              overflowY: 'auto',
              overflowX: 'hidden',
              color: 'gray',
              fontSize: 10,
            }}
          >
            {voice}
          </div>
        </>
      ) : null}
    </div>
  );
};

FrameViewDemo.defaultProps = {};

FrameViewDemo.propTypes = {};

export default FrameViewDemo;
