const AsyncLock = require('async-lock');

const { knex } = require('../helper/repositoryBase');
const config = require('../data/config/config');
const { getJson, setJson } = require('../helper/cacheManager');
// const { getLogger } = require('../helper/log4jsHelper');

// const logger = getLogger('logRecorder');

const lock = new AsyncLock();
const chunkSize = 100;
let timeObj = null;
let isRunning = false;
let execCount = 0;

const saveLogs = async () => {
  execCount++;
  lock.acquire('log_operate', async () => {
    const logs = await getJson(config.cacheKey.logBusinessList);
    const needSave = execCount >= 10;
    if (needSave) {
      execCount = 0;
    }
    if (!logs) {
      return;
    }
    if (logs.length < chunkSize && !needSave) {
      return;
    }
    await knex.batchInsert('log_business', logs, chunkSize);
    await setJson(config.cacheKey.logBusinessList, []);
    execCount = 0;
  });
};

const init = () => {
  isRunning = true;
  timeObj = setInterval(saveLogs, 1000);
};

const addLog = async (logEntity) => {
  if (!isRunning) {
    return;
  }
  lock.acquire('log_operate', async () => {
    let logs = await getJson(config.cacheKey.logBusinessList);
    if (logs) {
      logs.push(logEntity);
    } else {
      logs = [logEntity];
    }
    await setJson(config.cacheKey.logBusinessList, logs);
  });
};

const stop = async () => {
  isRunning = false;
  if (timeObj) {
    clearInterval(this.timeObj);
    timeObj = null;
  }
  await saveLogs(true);
};

module.exports = {
  init,
  addLog,
  stop,
};
