const config = require('../data/config/config');
const { getLogger } = require('../helper/log4jsHelper');
const { add, getSingle, update } = require('../helper/repositoryBase');

const logger = getLogger('runtimeManager');
const tableName = 'sys_runtime';

exports.init = async () => {
  const runtimeData = await getSingle({ tableName });
  const currentTime = new Date().getTime();
  if (runtimeData !== null) {
    await update({
      tableName,
      mainData: { lastStartTime: currentTime },
      wheres: { id: runtimeData.id },
    });
  } else {
    await add({
      tableName,
      mainData: { lastStartTime: currentTime, totalRuntime: 0 },
    });
  }
};

const updateRuntime = async () => {
  const runtimeData = await getSingle({ tableName });
  const currentTime = new Date().getTime();
  let totalRuntime = 0;
  if (runtimeData !== null) {
    totalRuntime = runtimeData.totalRuntime;
    const newRuntime = currentTime - runtimeData.lastStartTime;
    totalRuntime += newRuntime;
    await update({
      tableName,
      mainData: { lastStartTime: currentTime, totalRuntime },
      wheres: { id: runtimeData.id },
    });
  } else {
    await add({
      tableName,
      mainData: { lastStartTime: currentTime, totalRuntime },
    });
  }
  return Math.round(totalRuntime / 60000.0);
};

exports.updateRuntime = updateRuntime;

/**
 * 定时更新系统运行时长
 */
setInterval(async () => {
  try {
    await updateRuntime();
  } catch (err) {
    logger.error(`更新系统运行时长出错:${err}`);
  }
}, config.updateRuntimeInterval * 1000);
