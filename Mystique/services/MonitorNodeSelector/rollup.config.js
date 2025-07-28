import { terser } from 'rollup-plugin-terser';
import resolve from '@rollup/plugin-node-resolve';
import commonjs from '@rollup/plugin-commonjs';

// eslint-disable-next-line import/no-anonymous-default-export
export default {
  input: './src/index.js',
  output: {
    file: './dist/index.js',
    format: 'esm',
  },
  external: ['parametersnamekey', '@dc/previousparameters'],
  plugins: [
    // terser(),
    commonjs(),
    resolve(),
  ],
};
