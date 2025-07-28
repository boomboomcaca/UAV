/* eslint-disable camelcase */
const _ = require('lodash');
const msgpack = require('msgpack-lite');
const { update, transactingAction } = require('../../db/dbHelper');
const { resSuccess, resError } = require('../../helper/handle');
const { getCurrentDate } = require('../../helper/common');
// 基础操作 添加删除 修改 基于数据库二次封装
const {
  getPageData,
  getLogger,
  sqlQuery,
  getSingle,
} = require('../../helper/repositoryBase');
const { getEdgeConn } = require('../../manager/edgeManager');
const { getGUID } = require('../../helper/common');

const planTableName = 'rmbt_plan';
const executorTableName = 'rmbt_plan_executor';
const logger = getLogger('plan');

const updateCrontab = {
  jsonrpc: '2.0',
  id: getGUID(),
  method: 'updateCrontab',
  params: null,
  // params: [{ tag: '' }],
};

exports.addSchema = {
  description: '添加计划',
  tags: [planTableName],
  summary: '添加计划',
  body: {
    type: 'object',
    properties: {
      id: { type: 'string', description: '计划id', maxLength: 36 },
      name: { type: 'string', description: '计划名称', maxLength: 100 },
      creator: { type: 'string', description: '计划创建者', maxLength: 30 },
      effective_time: { type: 'string', description: '计划生效时间' },
      expire_time: { type: 'string', description: '计划过期时间' },
      cron: {
        type: 'array',
        description: '计划调度规则',
        items: { type: 'string', description: 'cron表达式' },
      },
      rule: {
        type: 'array',
        description: '计划调度规则',
        items: {
          type: 'object',
          properties: {
            executors: {
              type: 'array',
              description: '计划执行者',
              items: {
                type: 'string',
                description: 'edgeid',
              },
            },
            duration: {
              type: 'array',
              description: '计划调度规则',
              items: { type: 'integer', description: '持续时长（s）' },
            },
            feature: { type: 'string', description: '功能' },
            dataStorage: {
              type: 'array',
              description: '数据保存',
              items: {
                type: 'string',
                description: '数据类型',
              },
            },
            parameters: { type: 'object', description: '参数' },
          },
        },
      },
      status: { type: 'integer', description: '计划状态' },
    },
  },
};

const sendMessageToEdge = (executors, message) => {
  executors.forEach(async (element) => {
    const edgeConn = await getEdgeConn(element);
    if (edgeConn) {
      const mes = message;
      mes.id = getGUID();
      edgeConn.send(msgpack.encode(mes));
    }
  });
};

exports.add = async (req, reply) => {
  const executors = [];
  const plan = req.body;
  await transactingAction(async (knex, trx) => {
    if (plan.rule && plan.rule.length > 0) {
      plan.rule.forEach((item) => {
        if (item.executors && item.executors.length > 0) {
          item.executors.forEach((edgeID) => {
            executors.push({ plan_id: plan.id, edge_id: edgeID });
          });
        }
      });
      const list = _.uniqWith(executors, _.isEqual);
      await knex(executorTableName).transacting(trx).insert(list);

      delete plan.executors;
      plan.create_time = getCurrentDate();
      plan.update_time = getCurrentDate();
      plan.cron = JSON.stringify(plan.cron);
      plan.rule = JSON.stringify(plan.rule);
      await knex(planTableName).transacting(trx).insert(plan);
    }
  });

  // updateCrontab.params[0].tag = plan.id;
  const executor111 = [];
  executors.forEach((element) => {
    executor111.push(element.edge_id);
  });
  sendMessageToEdge(Object.values(executor111), updateCrontab);
  resSuccess({ reply });
};

exports.updateSchema = {
  description: '修改计划',
  tags: [planTableName],
  summary: '修改计划',
  body: {
    type: 'object',
    properties: {
      id: { type: 'string', description: '计划id', maxLength: 36 },
      status: { type: 'integer', description: '计划状态' },
    },
  },
};

exports.update = async (req, reply) => {
  const { id, ...plan } = req.body;
  plan.update_time = getCurrentDate();
  const wheres = {};
  wheres.id = id;
  const tableName = planTableName;
  const mainData = plan;
  await update({ tableName, mainData, wheres });
  const executorsResult = await sqlQuery(
    `select edge_id from ${executorTableName} as a,rmbt_edge as b where a.edge_id = b.id and plan_id = '${id}'`
  );
  // updateCrontab.params[0].tag = id;
  const executors = [];
  executorsResult.forEach((element) => {
    executors.push(element.edge_id);
  });
  sendMessageToEdge(Object.values(executors), updateCrontab);
  resSuccess({ reply });
};

exports.deleteSchema = {
  description: `删除计划任务`,
  tags: [planTableName],
  summary: `删除计划任务`,
  body: {
    type: 'object',
    properties: {
      id: { type: 'string', description: 'ID' },
      // name: { type: 'string', description: '业务名称' },
      // isUserDefine: { type: 'string', format: 'number', description: '是否用户定义', },
    },
  },
};

exports.del = async (req, reply) => {
  const data = await getSingle({
    tableName: planTableName,
    wheres: { id: req.body.id },
  });
  if (!data) {
    resError({ message: '该条数据已被删除！' });
    return;
  }

  const executorsResult = await sqlQuery(
    `select edge_id from ${executorTableName} as a,rmbt_edge as b where a.edge_id = b.id and plan_id = '${req.body.id}'`
  );
  const executors = [];
  executorsResult.forEach((element) => {
    executors.push(element.edge_id);
  });

  await transactingAction(async (knex, trx) => {
    await knex(executorTableName)
      .transacting(trx)
      .where('plan_id', req.body.id)
      .del();
    await knex(planTableName).transacting(trx).where('id', req.body.id).del();
  });

  sendMessageToEdge(Object.values(executors), updateCrontab);

  resSuccess({ reply });
};

exports.getList = async (req, reply) => {
  await sqlQuery(
    `update ${planTableName} set status = 1 where expire_time <= '${getCurrentDate()}' and status = 0`
  );
  const tableName = planTableName;
  const { edge_id, ...body } = req.body;
  if (edge_id) {
    const executors = await sqlQuery(
      `select plan_id from ${executorTableName} where edge_id = '${req.body.edge_id}'`
    );
    body.id = executors.map((item) => {
      return item.plan_id;
    });
    req.body = body;
  }
  const plans = await getPageData({ req, reply, tableName, isReply: false });

  plans.rows.forEach((element) => {
    try {
      const ele = element;
      ele.cron = JSON.parse(element.cron);
      ele.rule = JSON.parse(element.rule);
    } catch (error) {
      logger.error(`计划规则转换失败${error}，规则字符串：${element.rule}`);
    }
  });

  resSuccess({ reply, result: plans });
};

exports.getExecutersList = async (req, reply) => {
  const data = await sqlQuery(
    `select name,edge_id from ${executorTableName} as a,rmbt_edge as b where a.edge_id = b.id and plan_id = '${req.body.plan_id}'`
  );

  resSuccess({ reply, result: data });
};
