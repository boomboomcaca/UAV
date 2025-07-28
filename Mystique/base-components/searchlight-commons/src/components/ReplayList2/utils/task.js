import Socket from 'json-rpc-websocket';
import langT from 'dc-intl';
import { createGUID } from '../../../lib';

class Task {
  #taskReplay = { task: null, id: '', url: '' };

  #callback = () => {};

  constructor(url, callback) {
    this.#taskReplay.url = url;
    this.#callback = callback;

    this.InitTask();
  }

  InitTask = () => {
    if (this.#taskReplay.url === '') {
      this.#callback?.({
        event: 'main.nourl',
        result: langT('commons', 'noConnection'),
      });
      return;
    }
    const id = createGUID();
    this.#taskReplay.id = id;
    this.#taskReplay.task = new Socket({
      url: this.#taskReplay.url,
      protocols: id,
      onopen: (e) => {
        this.#callback?.({ event: 'main.onopen', result: e });
      },
      onerror: (e) => {
        this.#callback?.({ event: 'main.onerror', result: e });
      },
      onclose: (e) => {
        this.#callback?.({
          event: 'main.onclose',
          result: e,
        });
      },
    });
  };

  GotoSync = (fileId) => {
    if (this.#taskReplay.task) {
      this.#taskReplay.task.send({
        method: 'cloud.replayManager.presetReplay',
        params: { fileId },
        callback: (e) => {
          if (e.result) {
            this.#callback?.({ event: 'data.onsync', result: e });
          } else if (e.error) {
            this.#callback?.({ event: 'main.notsync', result: e });
          }
        },
      });
    }
  };

  DowloadData = (fileId, type) => {
    if (this.#taskReplay.task) {
      this.#taskReplay.task.send({
        method: 'cloud.replayManager.downloadData',
        params: { fileId, dataType: type },
        callback: (e) => {
          if (e.result) {
            this.#callback?.({ event: 'data.ondownload', result: e });
          } else if (e.error) {
            this.#callback?.({ event: 'main.nodata', result: e });
          }
        },
      });
    }
  };

  CloseTask = () => {
    if (this.#taskReplay.task) {
      this.#taskReplay.task.close();
    }
  };
}

export default Task;
