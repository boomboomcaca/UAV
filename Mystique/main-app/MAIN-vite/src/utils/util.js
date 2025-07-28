/* eslint-disable */

export function throttle(fn, wait = 500) {
  let flag = false;
  let timer = null;
  return function anymous(...args) {
    if (flag === false) {
      flag = true;
      timer = setTimeout(() => {
        flag = false;
        clearTimeout(timer);
        fn.call(this, ...args);
      }, wait);
    }
  };
}

export function debounce(fn, wait = 500) {
  let timer = null;

  return function anymous(...args) {
    timer = setTimeout(() => {
      clearTimeout(timer);
      timer = null;
      return fn.call(this, ...args);
    }, wait);
  };
}

/**
 *
 * @param {*} maxNumber
 * @param {*} promises
 */
export function asyncLimit(maxNumber, promises) {
  let index = 0;
  const tasks = [];
  const runningTasks = [];
  function excute() {
    if (index === promises.length) {
      return Promise.resolve();
    }
    let task = Promise.resolve().then(() => {
      return promises[index++];
    });

    tasks.push(task);

    let p = task.then(() => {
      runningTasks.splice(runningTasks.indexOf(p), 1);
    });

    runningTasks.push(p);

    let result = Promise.resolve();

    if (runningTasks.length >= maxNumber) {
      result = Promise.race(runningTasks);
    }

    // 递归执行
    return result.then(() => {
      excute();
    });
  }

  return excute().then(() => {
    Promise.all(tasks);
  });
}

export function timeOut(data) {
  const time = new Date(data).getTime();
  const timestamp = new Date().getTime();
  return time - timestamp < 7 * 60 * 60 * 24 * 1000;
}
