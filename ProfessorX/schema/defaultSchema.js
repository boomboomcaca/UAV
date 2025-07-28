// 返回数据不再进行验证
// jsonSchema 参考网站
// https://blog.csdn.net/swinfans/article/details/89231247
exports.loginSchema = {
  description: '登录',
  tags: ['user'],
  summary: '登录',
  body: {
    type: 'object',
    properties: {
      userName: { type: 'string' },
      password: { type: 'string' },
    },
  },
};
// 返回数据不再进行验证
// 这样就不能对数据长度等有效性进行验证有力必然有弊
// 如果有需要验证的可以验证！
exports.getAddSchema = ({ description, tags, summary }) => {
  const defaultSchema = {
    description,
    tags: [tags],
    summary: summary || description,
    body: {
      type: 'object',
      properties: {
        mainData: { type: 'object' },
      },
    },
  };
  return defaultSchema;
};
exports.getUpdateSchema = ({ description, tags, summary }) => {
  const defaultSchema = {
    description,
    tags: [tags],
    summary: summary || description,
    body: {},
    required: ['id'],
  };
  return defaultSchema;
};
// 普通的可以动态生成呀？
exports.getDeleteSchema = ({
  description,
  tags,
  summary,
  primaryKeyType = 'integer',
}) => {
  const defaultSchema = {
    description,
    tags: [tags],
    summary: summary || description,
    body: {
      type: 'object',
      properties: {
        id: { type: primaryKeyType },
      },
      required: ['id'],
    },
  };
  return defaultSchema;
};
exports.getDeleteListSchema = ({
  description,
  tags,
  summary,
  primaryKeyType = 'integer',
}) => {
  const defaultSchema = {
    description,
    tags: [tags],
    summary: summary || description,
    body: {
      type: 'object',
      properties: {
        id: {
          type: 'array',
          description: 'id数组',
          items: { type: primaryKeyType },
        },
      },
      required: ['id'],
    },
  };
  return defaultSchema;
};
exports.getDefaultSchema = ({ description, tags, summary }) => {
  const defaultSchema = {
    description,
    tags: [tags],
    summary: summary || description,
    query: {},
    body: {},
  };
  return defaultSchema;
};
// exports.getDefaultArraySchema = ({ description, tags, summary }) => {
//   const defaultSchema = {
//     description,
//     tags: [tags],
//     summary: summary || description,
//   };
//   return defaultSchema;
// };
// 难道是由于我多了一个属性故导出不认
exports.getPageDataSchema = ({ description, tags, summary, post = true }) => {
  const defaultSchema = {
    description,
    tags: [tags],
    summary: summary || description,
    post,
    query: {
      page: { description: '页数', type: 'integer' },
      rows: { description: '条数', type: 'integer' },
      sort: { description: '排序', type: 'string' },
      order: { description: '升序降序', type: 'string' },
    },
  };
  return defaultSchema;
};
// type: 'array',
//     items: {
//       type: 'object',
//       properties: {
//         // id: { type: 'string', description: 'ID' },
//         segmentID: { type: 'string', description: '规划频段ID' },
//         channelID: { type: 'string', description: '上行/下行关联信道ID' },
//         name: { type: 'string', description: '规划信道名称' },
//         freq: { type: 'number', description: '中心频率' },
//         bandwidth: { type: 'number', description: '带宽' },
//         mode: { type: 'number', description: '未知/上行/下行' },
//         // isUserDefine: { type: 'string', format: 'number', description: '是否用户定义', },
//       },
//     },
// exports.getPagePostDataSchema = ({ description, tags, summary }) => {
//   const defaultSchema = {
//     description,
//     tags: [tags],
//     summary: summary || description,
//     body: {
//       type: 'object',
//       properties: {
//         page: { type: 'integer' },
//         rows: { type: 'integer' },
//         sort: { type: 'string' },
//         order: { type: 'string' },
//       },
//     },
//   };
//   return defaultSchema;
// };
