const CONF = {
  // 开启服务的端口
  // 单机版 10000 8080
  // port: '10001',
  // wsPort: '8080',
  // domainName: '192.168.102.123',
  port: process.env.HTTP_PORT,
  wsPort: process.env.WS_PORT,
  domainName: process.env.HOST_IP,
  // 使用mysql或sqlite3
  DB: process.env.DATABASE_TYPE,
  mysql: {
    host: process.env.MYSQL_HOST,
    port: process.env.MYSQL_PORT,
    user: process.env.MYSQL_USER,
    database: process.env.MYSQL_DATABASE,
    password: process.env.MYSQL_PASSWORD,
    // char: 'utf8mb4',
  },
  sqlite3: {
    // 默认放在config里面
    filename: 'cloud.sqlite',
  },
  pool: {
    min: 1,
    max: 32,
  },
  redisStream: {
    useRedis: process.env.IS_REDIS_STREAM,
    redisPort: process.env.REDISSTREAM_PORT,
    redisHost: process.env.REDISSTREAM_HOST,
    redisAuth: process.env.REDISSTREAM_PASSWORD,
    expire: 30, // 单位秒
    streamsKey: 'prod_stream',
  },
  redisCache: {
    useRedis: process.env.IS_REDIS_CACHE,
    redisPort: process.env.IS_REDIS_PORT,
    redisHost: process.env.IS_REDIS_HOST,
    redisAuth: process.env.IS_REDIS_PASSWORD,
    expire: 30, // 单位秒
  },
  SyncSystem: {
    // true，需要同步到本地path；false，不需要同步，文件保存在本机path目录（单机版）
    // useSync: false,
    // host: 'localhost',
    // port: '8085',
    // path: '/DCData',
    useSync: process.env.IS_SYNC,
    host: process.env.SYNC_IP,
    port: process.env.SYNC_PORT,
    path: '/opt/DCData',
  },
  rebootCmd: 'node restart server.js',
};

module.exports = CONF;
