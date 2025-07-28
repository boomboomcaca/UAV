import babel from 'rollup-plugin-babel';
import resolve from '@rollup/plugin-node-resolve';
import commonjs from '@rollup/plugin-commonjs';
import postcss from 'rollup-plugin-postcss';
import alias from '@rollup/plugin-alias';
import { terser } from 'rollup-plugin-terser';
import copy from 'rollup-plugin-copy';
import path from 'path';

export default {
  input: 'src/Icons/index.jsx',
  output: [{ file: 'dist/index.js', format: 'esm' }],
  plugins: [
    postcss({
      extract: false,
      sourceMap: false,
      inject: true,
      extensions: ['.less', '.css'],
    }),
    copy({
      targets: [
        { src: 'index.d.ts', dest: 'dist' },
        { src: 'src/Icons/style.css', dest: 'dist/style' },
      ],
    }),
    babel({
      exclude: '**/node_modules/**',
      runtimeHelpers: true,
    }),
    resolve(),
    commonjs(),
    alias({
      entries: [{ find: '@', replacement: path.resolve(__dirname, 'src') }],
    }),
    terser(),
  ],
  external: ['prop-types', 'react', 'react-dom'],
};
