// eslint-disable-next-line import/no-extraneous-dependencies
const Client = require('ssh2-sftp-client');

const sftp = new Client();

const config = {
  host: '192.168.102.167',
  port: '22',
  username: 'root',
  password: '8ae91a21',
};

console.log(process.argv);

const local = process.argv[2];
const remote = process.argv[3];

/**
 * 上传文件到sftp
 * @param { Object } config    sftp 链接配置参数
 * @param { String } config.host sftp 主机地址
 * @param { String } config.port sftp 端口号
 * @param { String } config.username sftp 用户名
 * @param { String } config.password sftp 密码
 *
 * @param { Object } options 配置参数
 * @param { String } localStatic // 本地静态资源文件夹路径
 * @param { String } remoteStatic // 服务器静态资源文件夹路径
 * @param { String } localFile // 本地html页面
 * @param { String } remoteFile // 服务器html页面
 */
function upload(conf, options) {
  sftp.connect(conf).then(() => {
    console.log('sftp链接成功');
    sftp
      .rmdir(options.remote, true)
      .then(() => {
        console.log('目录已删除::', options.remote);
      })
      .catch((er) => {
        console.log('目录删除失败:::', er);
      })
      .finally(() => {
        console.log('文件上传中');
        sftp
          .uploadDir(options.local, options.remote)
          .then((data) => {
            console.log('文件上传成功');
            sftp.end();
          })
          .catch((err) => {
            console.log('上传失败', err);
            sftp.end();
          });
      });
  });
}

// 上传文件
upload(config, {
  local, // 本地文件夹路径
  remote, // 服务器文件夹路径器
});
