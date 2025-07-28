import React, { useState, useEffect } from 'react';
import PropTypes from 'prop-types';
import ImageCapture from '../ImageCapture';
import ReplayBar from './ReplayBar.jsx';
import player from './utils/voicePlayer';
import styles from './index.module.less';

const { showImage } = ImageCapture;
const FrameView = (props) => {
  const { wsurl, replayParam, children, menu, action, hideActions, onDataCallback, onRunState, onSliderDrag } = props;

  const [captureUrl, setCaptureUrl] = useState(null);

  const onCaptureClick = (setCapturing) => {
    setCapturing(true);
    showImage('frame', (imgUri) => {
      const item = { url: imgUri, timestamp: new Date().getTime() };
      setCaptureUrl(item);
      setCapturing(false);
    });
  };

  const onVoiceClick = (e) => {
    player.play(e);
  };

  const onDataBack = (res, speedValue) => {
    if (res.data) {
      res.data.dataCollection.forEach((rdd) => {
        if (rdd.type === 'audio') {
          player.playData(speedValue !== 1 ? { ...rdd, samplingRate: rdd.samplingRate * speedValue } : rdd);
        }
      });
      onDataCallback(res.data, speedValue);
    } else {
      onDataCallback(res);
    }
  };

  useEffect(() => {
    if (replayParam) {
      player.initPlayer(/* 'replay' */);
      player.play(true);
    } else {
      player.play(false);
      player.killPlayer(/* 'replay' */);
    }
    return () => {
      player.play(false);
      player.killPlayer(/* 'replay' */);
    };
  }, [replayParam]);

  return (
    <div id="frame" className={styles.root}>
      <div className={styles.content}>
        <div className={styles.chart}>{children}</div>
      </div>
      {replayParam ? (
        <ReplayBar
          wsurl={wsurl}
          replayParam={replayParam}
          menu={menu}
          action={action}
          hideActions={hideActions}
          onDataCallback={onDataBack}
          onRunState={onRunState}
          onCapture={onCaptureClick}
          onVoice={onVoiceClick}
          onSliderDrag={onSliderDrag}
        />
      ) : null}
      <ImageCapture imgURL={captureUrl} />
    </div>
  );
};

FrameView.defaultProps = {
  wsurl: '',
  replayParam: null,
  children: null,
  menu: null,
  action: null,
  hideActions: [],
  onDataCallback: () => {},
  onRunState: () => {},
  onSliderDrag: () => {},
};

FrameView.propTypes = {
  wsurl: PropTypes.string,
  replayParam: PropTypes.any,
  children: PropTypes.element,
  menu: PropTypes.element,
  action: PropTypes.any,
  hideActions: PropTypes.any,
  onDataCallback: PropTypes.func,
  onRunState: PropTypes.func,
  onSliderDrag: PropTypes.func,
};

export default FrameView;
