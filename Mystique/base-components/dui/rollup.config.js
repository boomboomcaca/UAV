import babel from 'rollup-plugin-babel';
import nodeResolve from '@rollup/plugin-node-resolve';
import commonjs from '@rollup/plugin-commonjs';
import postcss from 'rollup-plugin-postcss';
import alias from '@rollup/plugin-alias';
import modifyVars from './src/config/modifyVars.js';
import image from '@rollup/plugin-image';
import { terser } from 'rollup-plugin-terser';
import path from 'path';
import { visualizer } from 'rollup-plugin-visualizer';

export default {
  input: 'src/components/index.js',
  output: {
    // file: 'dist/index.js',
    dir: 'dist',
    format: 'esm',
    // manualChunks: {
    //   'better-scroll-core': ['@better-scroll/core'],
    //   'async-validator': ['async-validator'],
    // },
  },
  external: [
    // 'prop-types',
    // 'react',
    // 'react-dom',
    // 'ahooks',
    // '@ant-design/icons',
    // 'classnames',
    // '@dc/theme',
    // 'dc-intl',
    // 'dayjs',
    // '@better-scroll/core',
  ],
  plugins: [
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
    // nodeResolve(),
    commonjs(),
    alias({
      entries: [{ find: '@', replacement: path.resolve(__dirname, 'src') }],
    }),
    image(),
    terser(),
    visualizer(),
  ],
};
