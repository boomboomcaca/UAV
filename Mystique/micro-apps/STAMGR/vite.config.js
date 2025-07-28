import { defineConfig } from 'vite';
import refresh from '@vitejs/plugin-react-refresh';
import qiankun from 'vite-plugin-qiankun';
import useCDN from '@dc/vite-plugin-cdn';
import createPath, { config2script } from '@dc/vite-plugin-path';
import path from 'path';
import modifyVars from './src/config/modifyVars';
import { microConfig } from './src/config';
import { name } from './package';

export default ({ mode }) => {
  // dev接入qiankun与热更新插件冲突，请使用变量切换
  const useDevMode = false;
  // 生成base、在html.head中注入引用文件ico和config、index.html必须添加<%- injectHead %>
  const { base, injectHead } = createPath(mode, name, config2script(microConfig));

  return defineConfig({
    base,
    plugins: [
      ...(useDevMode ? [] : [refresh()]),
      qiankun(name, {
        useDevMode,
      }),
      injectHead(),
    ],
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
        '@': path.resolve(__dirname, './src'),
        ...useCDN(mode),
      },
    },
    server: {
      open: base,
      port: 3030,
    },
    build: {
      // minify: false,
      minify: 'terser',
      terserOptions: {
        compress: {
          drop_console: true,
          drop_debugger: true,
        },
      },
      outDir: 'build',
      assetsInlineLimit: 1000 * 1000, // 小于此阈值(kb)的导入或引用资源将内联为 base64 编码
      manifest: true,
    },
  });
};
