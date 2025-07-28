import Socket from 'json-rpc-websocket';
import { createGUID } from '../../../../lib/random';

const taskReplay = { task: null, id: '', url: '' };

const taskData = { task: null, id: '', url: '' };

const InitTask = (url, callback) => {
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
      callback({ event: 'main.onopen', result: e });
    },
    onerror: (e) => {
      callback({ event: 'main.onerror', result: e });
    },
    onclose: (e) => {
      callback({
        event: 'main.onclose',
        result: e,
      });
    },
  });
};

const StartTask = (fileID, param, callback) => {
  if (taskReplay.task) {
    taskReplay.task.send({
      method: 'cloud.replayManager.presetReplay',
      params: { fileId: fileID },
      callback: (e) => {
        window.console.log('presetReplay', e, param);
        if (e.result /* && e.result.uri */) {
          callback({ event: 'data.onopen', result: e });
        } else if (e.error) {
          callback({ event: 'main.notsync', result: e });
        }
      },
    });
  }
};

const StopTask = (callback) => {
  if (taskReplay.task) {
    taskReplay.task.send({
      method: 'cloud.replayManager.stopReplay',
      params: { id: taskData.id },
      callback: (e) => {
        callback?.({ event: 'main.stop', result: e });
      },
    });
  }
};

const CloseTask = () => {
  if (taskReplay.task) {
    taskReplay.task.close();
  }
};

export default {
  InitTask,
  StartTask,
  StopTask,
  CloseTask,
};
