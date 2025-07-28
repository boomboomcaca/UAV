const {
  addList,
  getSingle,
  getList,
  del,
} = require('../helper/repositoryBase');
const { getGUID } = require('../helper/common');
const config = require('../data/config/config');

const segmentTable = 'rmbt_planning_segment';
const segmentTypeID = '718c1960-bdfb-11eb-a5ab-e91b13de939b';
const maxSegmentCount = 10;

// /**
//  * 对数组中的对象，按对象的key进行sortType排序
//  * @param key 数组中的对象为object,按object中的key进行排序
//  * @param sortType true为降序；false为升序
//  */
// function keySort(key, sortType) {
//   return (a, b) => {
//     if (a[key] > b[key]) {
//       return sortType ? 1 : -1;
//     }
//     if (a[key] < b[key]) {
//       return sortType ? -1 : 1;
//     }
//     return 0;
//   };
// }

exports.getModule = async (moduleID) => {
  const module = await getSingle({
    tableName: 'rmbt_device',
    wheres: { id: moduleID },
  });
  return module;
};

exports.addScanSegments = async (scanSegments) => {
  const addSegments = [];
  for (let i = 0; i < scanSegments.length; i++) {
    const newSegment = {
      id: getGUID(),
      segment_type_id: segmentTypeID,
      name: `频段${i + 1}`,
      start_freq: scanSegments[i].startFrequency,
      stop_freq: scanSegments[i].stopFrequency,
      freq_step: scanSegments[i].stepFrequency,
      bandwidth: scanSegments[i].stepFrequency,
    };
    if (addSegments.length < maxSegmentCount) {
      addSegments.push(newSegment);
    } else {
      break;
    }
  }
  if (addSegments.length < maxSegmentCount) {
    const existSegments = await getList({
      tableName: segmentTable,
      wheres: { segment_type_id: segmentTypeID },
    });
    if (existSegments && existSegments.length > 0) {
      let addLength = scanSegments.length;
      for (let i = 0; i < existSegments.length; i++) {
        let existed;
        for (let j = 0; j < scanSegments.length; j++) {
          if (
            scanSegments[j].startFrequency === existSegments[i].start_freq &&
            scanSegments[j].stopFrequency === existSegments[i].stop_freq &&
            scanSegments[j].stepFrequency === existSegments[i].freq_step
          ) {
            existed = true;
            break;
          }
        }
        if (!existed) {
          if (addSegments.length < maxSegmentCount) {
            existSegments[i].name = `频段${++addLength}`;
            delete existSegments[i].Remark;
            delete existSegments[i].is_user_define;
            delete existSegments[i].segment_id;
            delete existSegments[i].mode;
            addSegments.push(existSegments[i]);
          } else {
            break;
          }
        }
      }
    }
  }
  // 删除已有频段
  await del({
    tableName: segmentTable,
    wheres: { segment_type_id: segmentTypeID },
  });
  // 添加新频段
  if (config.DB === 'mysql') {
    addSegments.reverse();
  }
  await addList({ tableName: segmentTable, mainData: addSegments });
};
