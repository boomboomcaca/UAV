/**
 * 打包版本写入node程序
 */

'use strict';

// const fs = require('fs');
import fs from 'fs';

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
const pathNPM = './dist/package.json';

const pack = JSON.parse(fs.readFileSync(path));
// import pack from './package.json';
// import pathNPM from './dist/package.json';
// const distVer =  ;
// import pack from 'path';
const packNPM = isExists(pathNPM) ? JSON.parse(fs.readFileSync(pathNPM)) : pack;

// 读取打包状态
const { name, description, version, module: moduleName, author, license } = pack;
const { version: versionNPM } = packNPM;

// 对比变更情况
const [major, minor, patch] = version.split('.');
const [majorNPM, minorNPM, patchNPM] = versionNPM.split('.');

let isChanged = false;
if (majorNPM !== major || minorNPM !== minor) isChanged = true;

// 重新配置
const newVersion = `${major}.${minor}.${isChanged ? 1 : Number(patchNPM || 0) + 1}`;
console.log('new ver:::', newVersion);
const newPack = {
  name,
  description,
  module: moduleName,
  author,
  license,
  version: newVersion,
};

// 写入配置
fs.writeFileSync(pathNPM, JSON.stringify(newPack, null, '\t'));
