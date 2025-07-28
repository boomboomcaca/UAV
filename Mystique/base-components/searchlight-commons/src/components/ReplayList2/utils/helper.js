import Task from './task';

export const freq = 'frequency';
export const ifbw = ['ifBandwidth', 'span', 'dfBandwidth'];

export const findParam = (parameters, filter) => {
  let find = null;
  if (filter instanceof Array) {
    for (let index = 0; index < filter.length; index += 1) {
      const element = filter[index];
      find = parameters.find((p) => {
        return p.name === element;
      });
      if (find) break;
    }
  } else {
    find = parameters.find((p) => {
      return p.name === filter;
    });
  }
  return find;
};

export const gotoSync = (id, wsurl, callback) => {
  const task = new Task(wsurl, (res) => {
    window.console.log(res);
    if (res.event === 'main.onopen') {
      task.GotoSync(id);
    }
    if (res.event === 'main.notsync' || res.event === 'main.onerror') {
      callback(false);
      task.CloseTask();
    }
    if (res.event === 'data.onsync') {
      callback(true);
      task.CloseTask();
    }
  });
};

export const gotoDownload = (id, type, wsurl, callback) => {
  const task = new Task(wsurl, (res) => {
    window.console.log(res);
    if (res.event === 'main.onopen') {
      task.DowloadData(id, type);
    }
    if (res.event === 'main.nodata' || res.event === 'main.onerror') {
      callback(res.result);
      task.CloseTask();
    }
    if (res.event === 'data.ondownload') {
      callback(res.result);
      task.CloseTask();
    }
  });
};
