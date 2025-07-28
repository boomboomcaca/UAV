const { getList } = require('../../db/dbHelper');
const { sqlQuery, resSuccess } = require('../../helper/repositoryBase');

const tableNameChannel = 'rmbt_channel_division';
exports.getChannelPlanningSchema = {
  summary: '查询信道规划信息',
  description: '查询信道规划信息',
  tags: [tableNameChannel],
  query: {
    freq: {
      type: 'number',
      description: '频率',
    },
  },
};
const queryColumnChannel = ['name', 'freq'];

exports.getChannelPlanning = async (request, reply) => {
  const sql = `
    SELECT
      business.name,
      seg.* 
    FROM
      rmbt_planning_business business
      JOIN (
      SELECT
        type.name AS typeName,
        type.business_id,
        segment.*
      FROM
        rmbt_planning_segment_type type
        JOIN (
        SELECT
          id AS segment_id,
          segment_type_id,
          name AS segmentName,
          start_freq AS startFreq,
          bandwidth,
          stop_freq AS stopFreq,
          freq_step AS freqStep 
        FROM
          rmbt_planning_segment 
        WHERE
          start_freq <= ${request.body.freq} and stop_freq >= ${request.body.freq} 
        ) AS segment 
      WHERE
      type.id = segment.segment_type_id 
      ) AS seg ON business.id = seg.business_id`;
  const segments = await sqlQuery(sql);

  if (segments !== undefined && segments.length > 0) {
    const promises = segments.map(async (element) => {
      const Channels = await getList({
        wheres: { segment_id: element.segment_id },
        tableName: tableNameChannel,
        queryColumn: queryColumnChannel,
      });
      element.Channels = Channels;
      return element;
    });
    await Promise.all(promises);
  }
  resSuccess({ reply, result: segments });
};
