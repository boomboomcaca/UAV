const commonjs = require("rollup-plugin-commonjs");
const resolve = require("rollup-plugin-node-resolve");
const { terser } = require("rollup-plugin-terser");
// import babel from 'rollup-plugin-babel';

module.exports = {
  input: "./src/tileServer.js", // 入口文件
  output: {
    file: "dist/bundle.js", // 输出文件名
    format: "cjs", // 输出模块格式
  },
  plugins: [
    resolve(), // 解析 node_modules 中的依赖
    commonjs(), // 将 CommonJS 模块转换为 ES6 模块
    terser(),
    //     babel({ babelrc: false, presets: [['@babel/preset-env', { targets: { node: 'current' } }]] }), // 转换 ES6 代码
  ],
  external: [
    "https",
    "cors",
    "express",
    "fs",
    "http",
    "util",
    "path",
    "better-sqlite3",
  ], // 指定不打包的外部依赖模块
};
