import DataCenter from '../componets/DataCenter';

const gotoSync = (replayParam, wsurl, callback) => {
  DataCenter.InitTask(wsurl, (res1) => {
    window.console.log(res1);
    if (res1.event === 'main.onopen') {
      DataCenter.StartTask(replayParam.id, {}, (res2) => {
        window.console.log(res2);
        if (res2.event === 'main.notsync') {
          DataCenter.CloseTask();
          callback(false);
        }
        if (res2.event === 'data.onopen') {
          DataCenter.StopTask();
          callback(true);
        }
      });
    }
  });
};

export default gotoSync;
