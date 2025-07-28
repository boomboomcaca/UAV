import { loadMicroApp } from 'qiankun';
import MicroController from './microController.js';

const uuid = function () {
  const temp = URL.createObjectURL(new Blob());
  const id = temp.toString();
  URL.revokeObjectURL(temp);
  return id.substring(id.lastIndexOf('/') + 1);
};

/**
 * 加载子应用
 * @param {{name:String, entry:String,loadType:String, container:String, more:Object, messagePort:Function}} options
 * @returns {Promise<{id: String, controller:MicroController, unmount: Function, unmountPromise: Promise}>}
 */
const loadMicro = (options) => {
  const { name, entry, loadType, container, more, messagePort, tag } = options;
  const messageChannel = (e) => {
    messagePort(e);
  };
  // 创建子应用id
  const microId = uuid();
  // 子应用回调
  const controller = new MicroController(microId, messageChannel);
  return new Promise((resolve, reject) => {
    const app = loadMicroApp({
      name,
      entry,
      container: `#${container}`,
      props: {
        id: microId,
        master: controller,
        loadType,
        options: more,
      },
    });
    if (app) {
      app.loadPromise
        .then(() => {
          resolve({
            id: microId,
            controller,
            unmount: app.unmount,
            unmountPromise: app.unmountPromise,
          });
        })
        .catch((ex) => {
          reject(ex);
        });
    } else {
      reject('');
    }
  });
};

export default loadMicro;
