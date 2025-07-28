/*
 * @Author: XYQ
 * @Date: 2022-04-20 10:12:00
 * @LastEditors: XYQ
 * @LastEditTime: 2022-06-15 15:17:55
 * @Description: file content
 */
import babel from 'rollup-plugin-babel';
import resolve from '@rollup/plugin-node-resolve';
import commonjs from '@rollup/plugin-commonjs';
import postcss from 'rollup-plugin-postcss';
import image from '@rollup/plugin-image';
import svgr from '@svgr/rollup';
import alias from '@rollup/plugin-alias';
import modifyVars from './src/config/modifyVars';
import url from '@rollup/plugin-url';
import json from '@rollup/plugin-json';
// import { terser } from 'rollup-plugin-terser';
import path from 'path';
import pub from './pub.config.json';
import { visualizer } from 'rollup-plugin-visualizer';

const plugins = [
  postcss({
    extract: false,
    sourceMap: false,
    inject: true,
    extensions: ['.less', '.css'],
    use: {
      less: { javascriptEnabled: true, modifyVars },
    },
  }),
  babel({
    exclude: '**/node_modules/**',
    runtimeHelpers: true,
  }),
  resolve(),
  commonjs(),
  image(),
  svgr(),
  url(),
  json(),
  alias({
    entries: [{ find: '@', replacement: path.resolve(__dirname, 'src') }],
  }),
  // terser(),
  visualizer(),
];

const external = [
  '@ant-design/icons',
  'ahooks',
  'async-validator',
  'classnames',
  'dc-icon',
  'dui',
  'json-rpc-websocket',
  'prop-types',
  'react',
  'react-dom',
  'react-router-dom',
  'react-transition-group',
  '@dc/charts',
  '@dc/theme',
  'dc-map',
  'dc-intl',
  'notifilter',
  'echarts',
];
let outFolders = [];

const type = process.env.npm_config_key;

if (type * 1 == 1 || type * 1 === 2) {
  outFolders = pub.pubcomponents.map((f) => {
    return {
      input: `src/extensions/${f}/index.js`,
      output: [
        {
          file: `commons/${f}/index.js`,
          format: 'esm',
        },
      ],
      plugins,
      external,
    };
  });
}
if (type * 1 == 0 || type * 1 === 2) {
  outFolders.push({
    input: `src/lib/index.js`,
    output: [{ file: `dist/lib/index.js`, format: 'esm' }],
    plugins,
    external,
  });
  outFolders.push({
    input: 'src/components/index.js',
    output: [{ file: 'dist/index.js', format: 'esm' }],
    plugins,
    external,
  });
}

export default outFolders;
