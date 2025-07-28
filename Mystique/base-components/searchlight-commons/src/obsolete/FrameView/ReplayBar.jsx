import React, { useState, useEffect, useRef } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { VoiceIcon, CameraIcon } from 'dc-icon';
import { IconButton } from 'dui';
import { getTimeSpanHMS, getTimePercent } from '../../lib/timeHelper';
import StatusControlBar, { Null } from '../StatusControlBar';
import icons from './utils/icons.jsx';
import FrameSlider, { useFrameTimer } from './componets/FrameSlider';
import useDataCenter from './useDataCenter';
import Selection from './componets/Selection';
import styles from './index.module.less';

const ReplayBar = (props) => {
  const {
    className,
    wsurl,
    replayParam,
    menu,
    action,
    hideActions,
    onDataCallback,
    onRunState,
    onCapture,
    onVoice,
    onSliderDrag,
  } = props;

  const [initialized, setInitialized] = useState(false);

  const [capturing, setCapturing] = useState(false);

  const [audioChk, setAudioChk] = useState(true);

  const [speedValue, setSpeedValue] = useState(1);
  const speedValueRef = useRef(1);

  const [indexValue, setIndexValue] = useState(0);
  const indexValueRef = useRef(0);
  const draggingRef = useRef(false);

  const [timePercent, setTimePercent] = useState([]);

  const [runState, setRunState] = useState(false);
  const runStateRef = useRef(false);
  useEffect(() => {
    runStateRef.current = runState;
    onRunState?.(runStateRef.current, indexValueRef.current);
    if (!runState && indexValueRef.current === 100) {
      // setIndexValue(0);
      indexValueRef.current = 0;
    }
  }, [runState]);

  const playOverRef = useRef(false);

  const { Enqueue, StartTimer, ExitTimer } = useFrameTimer();

  const onCallback = (res) => {
    if (res.event === 'main.onopen') {
      setInitialized(() => {
        return true;
      });
      setRunState(() => {
        return true;
      });
      onGet();
    }
    if (res.event === 'main.onclose') {
      setInitialized(() => {
        return false;
      });
    }
    if (res.event === 'main.timestamp') {
      setTimePercent(
        res.result.map((rr, idx) => {
          if (idx !== 0) return getTimePercent(replayParam?.dataStartTime, replayParam?.dataStopTime, rr[0] / 1e6);
          return -1;
        }),
      );
    }
    if (res.data) {
      const { index } = res.data;
      if (!draggingRef.current && runStateRef.current && index >= 0) {
        // window.console.log(draggingRef.current, runStateRef.current, index);
        const percent = getPercentage(index + 1);
        setIndexValue(percent);
        indexValueRef.current = percent;
        if (percent === 100) {
          setRunState(false);
        }
      }
      // window.console.log(index);
      // window.console.log(res);
      onDataCallback(res, speedValueRef.current);
    } else if (res.event === 'main.notsync' || res.event === 'data.onopen') {
      onDataCallback(res);
    } else if (res.event === 'data.onclose') {
      playOverRef.current = true;
    }
  };

  const { InitTask, StartTask, PlayControl, GetFrame, SetParam, StopTask, CloseTask } = useDataCenter(onCallback);

  // #region 状态控制条

  const onSpeedValueChanged = (val) => {
    setSpeedValue(val);
    speedValueRef.current = val;
    SetParam({ playIndex: getIndex(indexValueRef.current), playTimeSpeed: val });
  };

  const onIndexValueChanged = (val, clicked) => {
    setIndexValue(val);
    indexValueRef.current = val;
    if (runState && !draggingRef.current) {
      SetParam({ playIndex: getIndex(val), playTimeSpeed: speedValueRef.current });
    } else if (clicked) {
      setTimeout(() => {
        onGetFrame(getIndex(val));
      }, 200);
    } else if (draggingRef.current) {
      Enqueue(getIndex(val));
    }
  };

  const onSliderDragging = (bo) => {
    onSliderDrag(bo);
    draggingRef.current = bo;
    if (bo) {
      if (runStateRef.current) {
        onPlay('pauseReplay');
      }
      StartTimer((val) => {
        // window.console.log(val);
        onGetFrame(val);
      });
    } else {
      if (runStateRef.current) {
        onPlay('continueReplay');
      }
      ExitTimer();
    }
  };

  const onVoiceClick = (e) => {
    setAudioChk(e);
    onVoice(e);
  };

  const onCaptureClick = () => {
    setCapturing(true);
    onCapture(setCapturing);
  };

  // #endregion

  // #region 数据处理

  useEffect(() => {
    setIndexValue(0);
    indexValueRef.current = 0;
    if (replayParam) {
      InitTask(wsurl);
    }

    return () => {
      StopTask();
      CloseTask();
    };
  }, [replayParam]);

  const onGet = () => {
    if (!replayParam) return;
    StartTask(replayParam.id, { playIndex: getIndex(indexValueRef.current), playTimeSpeed: speedValueRef.current });
  };

  const getIndex = (val) => {
    let frameIndex = Math.round((val * (replayParam.recordCount - 1)) / 100);
    if (frameIndex < 1) frameIndex = 1;
    if (frameIndex > replayParam.recordCount - 1) {
      frameIndex = replayParam.recordCount - 1;
    }
    return frameIndex;
  };

  const getPercentage = (val) => {
    if (val < 1) return 0;
    if (val >= replayParam.recordCount - 1) return 100;
    return (val / (replayParam.recordCount - 1)) * 100;
  };

  const onGetFrame = (idx) => {
    if (!replayParam) return;
    GetFrame(idx);
  };

  const onPlay = (method) => {
    // window.console.log(method);
    PlayControl(method);
  };
  // #endregion

  const onIconClick = (checked, tag) => {
    const chk = !checked;
    switch (tag) {
      case 'start':
        if (initialized) {
          onPlay(runState ? 'pauseReplay' : 'continueReplay');
          setRunState(!runState);
        }
        break;
      case 'audio':
        onVoiceClick(chk);
        break;
      case 'capture':
        onCaptureClick();
        break;

      default:
        break;
    }
  };

  const getIconColor = (bo) => {
    return bo ? '#2ee8d5' : 'white';
  };

  const getTime = () => {
    // window.console.log(replayParam);
    return getTimeSpanHMS(replayParam?.dataStopTime, replayParam?.dataStartTime, indexValue);
  };

  return (
    <StatusControlBar className={classnames(styles.bottom, className)}>
      {menu ? <StatusControlBar.Main>{menu}</StatusControlBar.Main> : <Null />}
      <StatusControlBar.Message>
        <div className={styles.playcontrol}>
          <IconButton tag="start" className={styles.trigger} onClick={onIconClick}>
            {!runState ? icons.play : icons.pause}
          </IconButton>
          <FrameSlider
            value={indexValue}
            time={getTime()}
            timePercent={timePercent}
            onValueChanged={onIndexValueChanged}
            onDragging={onSliderDragging}
          />
          <Selection value={speedValue} onValueChange={onSpeedValueChanged} />
        </div>
      </StatusControlBar.Message>
      <StatusControlBar.Action>
        {!hideActions?.includes('audio') ? (
          <IconButton tag="audio" checked={audioChk} onClick={onIconClick}>
            <VoiceIcon color={getIconColor(audioChk)} />
          </IconButton>
        ) : null}
        {!hideActions?.includes('capture') ? (
          <IconButton tag="capture" disabled={capturing} loading={capturing} onClick={onIconClick}>
            <CameraIcon color={getIconColor(false)} />
          </IconButton>
        ) : null}
        {action || <Null />}
      </StatusControlBar.Action>
    </StatusControlBar>
  );
};

ReplayBar.defaultProps = {
  className: null,
  wsurl: '',
  replayParam: null,
  menu: null,
  action: null,
  hideActions: [],
  onDataCallback: () => {},
  onRunState: () => {},
  onCapture: () => {},
  onVoice: () => {},
  onSliderDrag: () => {},
};

ReplayBar.propTypes = {
  className: PropTypes.any,
  wsurl: PropTypes.string,
  replayParam: PropTypes.any,
  menu: PropTypes.element,
  action: PropTypes.any,
  hideActions: PropTypes.any,
  onDataCallback: PropTypes.func,
  onRunState: PropTypes.func,
  onCapture: PropTypes.func,
  onVoice: PropTypes.func,
  onSliderDrag: PropTypes.func,
};

export default ReplayBar;
