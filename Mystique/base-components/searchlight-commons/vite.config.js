import { defineConfig } from 'vite';
import path from 'path';
import modifyVars from './src/config/modifyVars';
import image from '@rollup/plugin-image';

export default defineConfig({
  plugins: [image()],
  css: {
    preprocessorOptions: {
      less: {
        javascriptEnabled: true,
        modifyVars,
      },
    },
  },
  resolve: {
    alias: {
      '~': path.resolve(__dirname, './'),
      '@': path.resolve(__dirname, 'src'),
    },
  },
  server: {
    open: '/index.html',
    host: '0.0.0.0',
    port: 3099,
  },
  build: {
    outDir: 'build',
  },
});
