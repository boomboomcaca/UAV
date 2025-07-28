import { useState, useEffect, useRef } from 'react';
import Socket from 'json-rpc-websocket';
import { createGUID } from '../../lib/random';

function useDataCenter(callback) {
  const [key, setKey] = useState();

  const taskReplay = useRef({ task: null, id: '', url: '' }).current;

  const taskData = useRef({ task: null, id: '', url: '' }).current;

  const beatTimer = useRef({ replay: null, data: null }).current;

  const InitTask = (url) => {
    if (url === '') {
      callback({
        event: 'main.nourl',
        result: '没有连接',
      });
      return;
    }
    const id = createGUID();
    taskReplay.id = id;
    taskReplay.url = url;
    taskReplay.task = new Socket({
      url,
      protocols: id,
      onopen: (e) => {
        beatTimer.replay = setInterval(() => {
          beat(taskReplay.task);
        }, 1000);
        callback({ event: 'main.onopen', result: e });
      },
      onerror: (e) => {
        resetReplayBeat();
        resetDataBeat();
        callback({ event: 'main.onerror', result: e });
      },
      onclose: (e) => {
        resetReplayBeat();
        resetDataBeat();
        callback({
          event: 'main.onclose',
          result: e,
        });
      },
    });
  };

  const beat = (task, id) => {
    if (task) {
      task.send({
        method: 'heartbeat',
        id: '1',
        callback: (e) => {
          // if (id === 0) {
          //   window.console.log(e);
          // }
        },
      });
    }
  };

  const resetReplayBeat = () => {
    if (beatTimer.replay) {
      clearInterval(beatTimer.replay);
      beatTimer.replay = null;
    }
  };

  const resetDataBeat = () => {
    if (beatTimer.data) {
      clearInterval(beatTimer.data);
      beatTimer.data = null;
    }
  };

  const StartTask = (fileID, param) => {
    if (taskReplay.task) {
      taskReplay.task.send({
        method: 'cloud.replayManager.presetReplay',
        params: { fileId: fileID },
        callback: (e) => {
          window.console.log('presetReplay', e, param);
          if (e.result && e.result.uri) {
            initTaskData(e.result, param, callback);
            const { timestampCollection } = e.result;
            if (timestampCollection) {
              callback({ event: 'main.timestamp', result: timestampCollection });
            }
          } else if (e.error) {
            callback({ event: 'main.notsync', result: e });
          }
        },
      });
    }
  };

  const initTaskData = (args, param) => {
    const { replayId, uri } = args;
    if (replayId && uri) {
      taskData.id = replayId;
      taskData.url = uri;
      taskData.task = new Socket({
        url: uri,
        protocols: replayId,
        onmessage: (res) => {
          // window.console.log(res);
          // if (res?.id === 0 && res?.result) {
          //   callback({
          //     event: 'data.onmessage',
          //     data: res.result,
          //   });
          // }
          const { method, params } = res;
          if (method === 'channel.dataHandler.notify' && params) {
            // window.console.log(params);
            callback({
              event: 'data.onmessage',
              data: params,
            });
          }
        },
        onopen: (e) => {
          beatTimer.data = setInterval(() => {
            beat(taskData.task, 0);
          }, 1000);
          callback({ event: 'data.onopen', result: e });
          startTaskData(param, callback);
        },
        onerror: (e) => {
          resetDataBeat();
          callback({ event: 'data.onerror', result: e });
        },
        onclose: (e) => {
          resetDataBeat();
          callback({ event: 'data.onclose', result: e });
        },
      });
    }
  };

  const startTaskData = (param) => {
    if (taskReplay.task) {
      taskReplay.task.send({
        method: 'cloud.replayManager.startReplay',
        params: {
          id: taskData.id,
          playIndex: 1,
          playTimeSpeed: 1,
          ...param,
        },
        callback: (e) => {
          window.console.log('startReplay', e, param);
          callback({ event: 'main.start', result: e });
        },
      });
    }
  };

  const PlayControl = (method) => {
    if (taskReplay.task) {
      taskReplay.task.send({
        method: `cloud.replayManager.${method}`,
        params: { id: taskData.id },
        callback: (e) => {
          window.console.log(e);
          callback({ event: 'main.control.play', result: e });
        },
      });
    }
  };

  const GetFrame = (index) => {
    if (taskReplay.task) {
      // window.console.log(index);
      taskReplay.task.send({
        method: 'cloud.replayManager.singleReplay',
        params: { id: taskData.id, playIndex: index },
        callback: (e) => {
          // window.console.log(e);
          callback({ event: 'main.control.single', result: e });
        },
      });
    }
  };

  const SetParam = (params) => {
    if (taskReplay.task) {
      taskReplay.task.send({
        method: 'cloud.replayManager.setReplayParameters',
        params: { id: taskData.id, ...params },
        callback: (e) => {
          // window.console.log(e);
          callback?.({ event: 'main.setparam', result: e });
        },
      });
    }
  };

  const StopTask = () => {
    if (taskReplay.task) {
      taskReplay.task.send({
        method: 'cloud.replayManager.stopReplay',
        params: { id: taskData.id },
        callback: (e) => {
          // window.console.log(e);
          callback?.({ event: 'main.stop', result: e });
        },
      });
    }
    taskData.task = null;
    taskData.id = '';
    taskData.url = '';
    resetReplayBeat();
    resetDataBeat();
  };

  const CloseTask = () => {
    if (taskReplay.task) {
      taskReplay.task.close();
    }
    taskReplay.task = null;
    taskReplay.id = '';
    taskReplay.url = '';
    resetReplayBeat();
    resetDataBeat();
  };

  useEffect(() => {}, []);

  return { key, setKey, InitTask, StartTask, PlayControl, GetFrame, SetParam, StopTask, CloseTask };
}

export default useDataCenter;
