import Socket from 'json-rpc-websocket';
import getConfig from '@/config';
import { createGUID } from './random';

const { wsTaskUrl: wsUrl } = getConfig();

const task = { task: null, id: '', url: '' };

let beatTimer = null;

const InitTask = (callback) => {
  if (wsUrl === '') {
    callback({
      event: 'main.nourl',
      result: '没有连接',
    });
    return;
  }
  const id = createGUID();
  task.id = id;
  task.url = wsUrl;
  task.task = new Socket({
    url: wsUrl,
    protocols: id,
    onopen: (e) => {
      beatTimer = setInterval(() => {
        beat();
      }, 5000);
      callback({ event: 'main.onopen', result: e });
    },
    onmessage: (e) => callback({ event: 'main.message', result: e.result }),
    onerror: (e) => {
      resetBeat();
      callback({ event: 'main.onerror', result: e });
    },
    onclose: (e) => {
      resetBeat();
      callback({
        event: 'main.onclose',
        result: e,
      });
    },
  });
};

const beat = () => {
  if (task.task) {
    task.task.send({
      method: 'heartbeat',
      id: '1',
      callback: (e) => {
        window.console.log(e);
      },
    });
  }
};

const resetBeat = () => {
  if (beatTimer) {
    clearInterval(beatTimer);
    beatTimer = null;
  }
};

const SetParam = (params, callback) => {
  if (task.task) {
    task.task.send({
      method: 'cloud.remoteControlManager.setSwitches',
      params: { /* requestType: 'environment',  */ ...params },
      callback: (e) => {
        window.console.log(e);
        callback({ event: 'main.setparam', result: e });
      },
    });
  }
};

const CloseTask = () => {
  if (task.task) {
    resetBeat();
    task.task.close();
  }
};

export default {
  InitTask,
  SetParam,
  CloseTask,
};
