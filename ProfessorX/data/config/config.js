/* eslint-disable global-require */
/* eslint-disable import/no-dynamic-require */
const CONF = {
  isPrintSql: false,
  pageDefault: {
    page: 1,
    rows: 10,
    maxRows: 1000,
    sort: '',
    order: 'desc',
  },
  security: [
    {
      authorization: [],
    },
  ],
  jwt: {
    secret: 'cloud',
    sign: {
      audience: 'cloud',
      issuer: 'cloud',
      expiresIn: 5000000,
    },
    verify: {
      audience: 'cloud',
      issuer: 'cloud',
    },
    enable: true,
  },
  license: {
    // license内容
    statusResultKey: 'prod:licenseStatusResult',
    enable: false,
    checkMachineCode: true,
    checkExpirationDate: true,
  },
  // 当前的路由信息
  routes: [],
  whitelistedRoutes: [
    '/doc',
    '/User/login',
    '/License/getLicenseInfo',
    '/Runtime/get',
  ],
  log4jsconfigure: {
    appenders: {
      app: {
        type: 'dateFile',
        filename: 'log/app_cloud.log',
        maxLogSize: 10485760,
        keepFileExt: true,
        daysToKeep: 30,
        layout: {
          type: 'pattern',
          pattern: '%x{lokiStr} host=%h ',
        },
      },
      console: {
        type: 'console',
      },
    },
    categories: {
      default: {
        appenders: ['app', 'console'],
        level: 'debug',
      },
    },
  },
  cacheKey: {
    // 用户权限缓存Key
    userPermission: 'prod:userPermission:',
    // 失效tokenKey
    userCancelTokens: 'prod:userCancelTokens:',

    dic: 'prod:dic:',
    // 设施监控告警门限Key
    resourceThreshold: 'prod:resourceThreshold:',
    // 日志缓存
    logBusinessList: 'prod:logBussinessList',
  },
  requirePath: {
    // control 文件夹下面具体文件 依赖的repositoryBase 路径
    repositoryBasePath: '../../helper/repositoryBase',
  },
  user: {
    initPwd: '123456',
    // 系统管理员Id
    adminUserId: 1,
    cancelTokenLength: 100,
  },
  // 表名称前缀
  tablePrefix: ['sys_', 'test_', 'rmbt_', 'log_'],
  // 权限规则
  permissionRole: {
    // 创建用户访问 表示这些Url 匹配这些规则创建用户访问
    // 我如果就想要限制操作ProjectName 为 XX 的数据//
    // ProjectName
    // ProjectId
    // 直接程序内置Project_Id //同时也表示对Project表和Project明细表进行限制访问
    createUserAccess: ['/project/queryTest', '/device/queryTest'],
    // 如果这个数据
  },
  // 不进行通用格式转换字段
  notConvertField: ['remark', 'db_sql'],
  // 定义操作符
  operator: [
    {
      key: 'gt',
      value: '>',
    },
    {
      key: 'lt',
      value: '<',
    },
    {
      key: 'gte',
      value: '>=',
    },
    {
      key: 'lte',
      value: '<=',
    },
    {
      key: 'lk',
      value: 'like',
    },
  ],
  edge: {
    // 间隔多久进行一次边缘端在线检测，单位 s
    interval: 3,
    // 合理的心跳间隔时间限值，单位 s
    timeLimit: 5,
    edgesCacheKey: 'prod:edges:',
    taskCacheKey: 'prod:tasks:',
    // 环境控制Redis缓存
    controlsCacheKey: 'prod:controlEdges:',
    // 环境控制自定义ID（重要：至少含有一个，否则报错!!!）
    controlEdgeID: ['prod:controlEdgeID'],
    virtualEdge: true,
  },
  intersectionPositioning: {
    pointLen: 10,
    mindistance: 0.1,
    useTestData: false,
    signleLocateDataMaxCount: 100,
    signleLocateDrawtimeInterval: 3000,
  },
  tdoa: {
    // 是否记录IQ数据
    recordIQData: false,
    // 保存IQ数据文件夹
    iqDataFolder: 'iqData',
    // 是否为模拟边缘端
    virtualEdge: true,
    // 缓存边缘端IQ数据数量
    edgeDataCacheCount: 100,
    // 计算间隔时间，单位ms
    calculationInterval: 100,
  },
  poa: {
    // 缓存边缘端电平数据数量
    edgeDataCacheCount: 100,
    // 计算间隔时间，单位ms
    calculationInterval: 1000,
  },
  // 更新系统运行时长间隔，单位 s
  updateRuntimeInterval: 3600,
  frequencyTemplateImagePath: 'public/frequencyImg',
  frequencyTemplateImageWidth: 320,
  frequencyTemplateImageHeight: 320,
  // 间隔多少次TDOA计算后进行手动GC
  manualGCInterval: 1,
};

const model = 'prod';

const conf = require(`${process.cwd()}/data/config/config.${model}`);
const config = Object.assign(CONF, conf);

module.exports = config;
