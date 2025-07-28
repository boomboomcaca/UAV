import { defineConfig } from 'vite';
import reactRefresh from '@vitejs/plugin-react-refresh';
import path from 'path';
import modifyVars from './src/config/modifyVars.js';
import json from '@rollup/plugin-json';
import { visualizer } from 'rollup-plugin-visualizer';

export default defineConfig({
  plugins: [reactRefresh(), json()],
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
    port: '9521',
  },
  build: {
    outDir: 'build',
    rollupOptions: {
      output: {
        manualChunks(id) {
          if (id.includes('node_modules')) {
            return id.toString().split('node_modules/')[1].split('/')[0].toString();
          }
        },
      },
      external: [
        'prop-types',
        'react',
        'react-dom',
        'ahooks',
        '@ant-design/icons',
        'classnames',
        '@dc/theme',
        'dc-intl',
        'dayjs',
      ],
      plugins: [visualizer()],
    },
  },
});
