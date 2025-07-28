/**
 * 拷贝style.css：多了dist是因为引入的目录多一层dist
 */

'use strict';

const fs = require('fs');
const path = require('path');

const from = path.resolve(__dirname, './src/Icons/style.css');
const to = path.resolve(__dirname, './dist/dist/style.css');

// fs.rmdirSync('./dist');
// fs.mkdirSync('./dist/dist');

fs.copyFile(from, to, 0, () => {
  console.log('copy style.css, succeeded');
});
