import nodeResolve from '@rollup/plugin-node-resolve';
import typescript from 'rollup-plugin-typescript2';
import { terser } from 'rollup-plugin-terser';
import ts from 'typescript';

export default {
  input: './src/tools.ts',
  output: {
    file: './dist/tools.js',
    format: 'esm',
  },
  plugins: [
    nodeResolve(),
    typescript({
      exclude: 'node_modules/**',
      typescript: ts,
    }),
    terser(),
  ],
  external: [],
};
