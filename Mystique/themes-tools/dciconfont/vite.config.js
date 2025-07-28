/*
 * @Author: wangXueDong
 * @Date: 2021-12-07 11:44:35
 * @LastEditors: wangXueDong
 * @LastEditTime: 2022-10-21 15:28:35
 */
import { defineConfig } from 'vite';
import reactRefresh from '@vitejs/plugin-react-refresh';
import path from 'path';

export default defineConfig({
  plugins: [reactRefresh()],
  resolve: {
    alias: {
      '~': path.resolve(__dirname, './'),
      '@': path.resolve(__dirname, 'src'),
    },
  },
  server: {
    open: '/',
    host: '0.0.0.0',
    port: 3100,
  },
  build: {
    outDir: 'build',
  },
});
