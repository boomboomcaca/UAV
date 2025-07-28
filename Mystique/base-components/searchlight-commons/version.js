/*
 * @Author: XYQ
 * @Date: 2022-01-24 18:09:27
 * @LastEditors: XYQ
 * @LastEditTime: 2022-06-15 15:20:43
 * @Description: file content
 */
/**
 * 打包版本写入node程序
 */

'use strict';

const fs = require('fs');
const pub = require('./pub.config.json');
/**
 * 判断文件是否存在:因为执行了build所以dist一定存在
 */
const isExists = (path) => {
  try {
    fs.accessSync(path, fs.F_OK);
    return true;
  } catch (e) {
    return false;
  }
};
// 读取项目配置
const path = './package.json';
const exec = require('child_process').exec;
const type = process.env.npm_config_key;
exec('npm view searchlight-commons version', function (error, stdout, stderr) {
  console.log('npm view searchlight-commons error ::', error);
  console.log('npm view searchlight-commons stdout ::', stdout);
  console.log('npm view searchlight-commons stderr ::', stderr);
  const verArray = String(stdout.trim()).split('.');

  const pack = require(path);
  const { name, description, version, module: moduleName, author, license } = pack;

  const [major, minor, patch] = version.trim().split('.');
  console.log('package.json version:::', version.trim().split('.'), verArray);

  let isChanged = false;
  if (verArray[0] !== major || verArray[1] !== minor) isChanged = true;
  // 重新配置
  const newVersion = `${isChanged ? major : verArray[0]}.${isChanged ? minor : verArray[1]}.${
    isChanged ? patch : Number(verArray[2] || 0) + 1
  }`;
  console.log('newVersion ver::', newVersion);
  try {
    if (type * 1 === 1 || type * 1 === 2) {
      const files = fs.readdirSync(pub.folder);
      files.forEach((it) => {
        const newPack = {
          name: `@${name}/${it.toLowerCase()}`,
          description,
          module: moduleName,
          author,
          license,
          version: newVersion,
        };
        const pubPackJson = `./${pub.folder}/${it}/package.json`;
        // 写入配置
        fs.writeFileSync(pubPackJson, JSON.stringify(newPack, null, '\t'));
        const cmd = `cd ${pub.folder}/${it} &&  npm publish -registry=http://192.168.1.163:8081/repository/own-npm/  && cd ../`;
        exec(cmd, function (error, stdout, stderr) {
          console.log('error-->', error);
          console.log('stdout-->', stdout);
          console.log('stderr-->', stderr);
        });
      });
    }
  } catch (error) {
    console.log('没有文件夹');
  }

  if (type * 1 === 0 || type * 1 === 2) {
    const newPack = {
      name,
      description,
      module: moduleName,
      author,
      license,
      version: newVersion,
    };
    const pubPackJson = './dist/package.json';
    // 写入配置
    fs.writeFileSync(pubPackJson, JSON.stringify(newPack, null, '\t'));
    const cmd = `cd dist &&  npm publish -registry=http://192.168.1.163:8081/repository/own-npm/  && cd ../`;
    exec(cmd, function (error, stdout, stderr) {
      console.log('error-->', error);
      console.log('stdout-->', stdout);
      console.log('stderr-->', stderr);
    });
  }
});
