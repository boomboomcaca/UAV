import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import svgLoader from "vite-svg-loader";
import vitePluginImp from "vite-plugin-imp";
import svgr from "vite-plugin-svgr";
import { visualizer } from "rollup-plugin-visualizer";

import path from "path";

// // 统计 node_modules 依赖的大小，以500kb为阈值拆离
// let chunkSize = 0;

/**
 * vite打包拆包处理
 * @param {String} id
 */
const manualChunks1 = (id) => {
  // 2022-11-10 liujian 此数组可根据项目引用情况进行拆分，比如这里的react-dom大家统一使用的cdn的话就可以不用管它
  const split = [
    "dui",
    "searchlight-commons",
    "@dc/charts",
    "ccesocket-webworker",
    "dc-icon",
    "json-rpc-websocket",
    "react-dom",
    "echarts",
    "gpu.js",
    "mapbox-gl",
    "html2image",
    "recharts",
    "qiankun",
    "searchlight-settings",
  ];
  let key = null;
  split.forEach((item) => {
    if (id.includes(item)) {
      key = item;
    }
  });
  // 2022-11-10 liujian 这里可能会有坑，先不要
  // if (id.includes('node_modules') && id.endsWith('.js')) {
  //   const size = fs.statSync(id).size;
  //   chunkSize += size;
  //   key = `chunk${Math.round(chunkSize / 500000)}`;
  // }

  return key;
};
console.log(process.argv);
// https://vitejs.dev/config/
export default defineConfig({
  plugins: [
    react(),
    // svgLoader(),
    svgr(),
    vitePluginImp({
      // libList: [
      //   {
      //     libName: 'antd',
      //     style: (file) => `antd/es/${file}/style`,
      //   },
      // ],
    }),
    // createStyleImportPlugin({
    //   resolves: [ElementPlusResolve()],
    //   libs: [
    //     {
    //       libraryName: 'antd',
    //       resolveStyle: (file) => `antd/es/${file}/style`,
    //     },
    //   ],
    // }),
  ],
  css: {
    preprocessorOptions: {
      less: {
        javascriptEnabled: true,
        // modifyVars,
      },
    },
  },
  resolve: {
    alias: {
      "~": path.resolve(__dirname, "./"),
      "@": path.resolve(__dirname, "./src"),
    },
  },
  build: {
    minify: "terser",
    terserOptions: {
      // keep_fnames: true,
      compress: {
        // keep_fnames: true,
        // keep_fargs: true,
        // drop_console: true,
        drop_debugger: true,
        // pure_funcs: [
        //   "Math.floor",
        //   "Math.ceil",
        //   "Math.min",
        //   "Math.max",
        //   "Math.round",
        //   "Math.random",
        //   "renderWidthFill_chart",
        //   "renderWidthPixel_chart",
        //   "getMaximum_chart",
        // ],
      },
      // mangle: { keep_fnames: true },
    },
    outDir: "build",
    rollupOptions: {
      output: {
        // manualChunks: manualChunks1,
        manualChunks: {
          "dc-intl": ["dc-intl"],
          capacitor: [
            "@better-scroll/mouse-wheel",
            "@capacitor-community/http",
            "@capacitor/app",
            "@capacitor/device",
            "@capacitor/filesystem",
            "@capacitor/status-bar",
            "@capacitor/toast",
          ],
          "async-validator": ["async-validator"],
          "better-scroll-core": ["@better-scroll/core"],
          "reduce-css-calc": ["reduce-css-calc"],
          "reat-dom": ["react-dom"],
          "dc-icon": ["dc-icon"],
          dui: ["dui"],
          // cartesian: ["cartesian"],
          recharts: ["recharts"],
          "searchlight-commons": ["searchlight-commons"],
          searchsettings: ["searchlight-settings"],
          "ccesocket-webworker": ["ccesocket-webworker"],
          "json-rpc-websocket": ["json-rpc-websocket"],
          gpujs: ["gpu.js"],
          "mapbox-gl": ["mapbox-gl"],
          html2image: ["html2image"],
          qiankun: ["qiankun"],
          jmuxer: ["jmuxer"],
          pixi: ["pixi.js"],
        },
      },
      plugins: [visualizer()],
    },
    assetsInlineLimit: 1000 * 1000, // 小于此阈值(kb)的导入或引用资源将内联为 base64 编码
    manifest: true,
  },
  optimizeDeps: {
    // exclude: ["gpu.js"],
  },
  server: {
    port: 9501,
  },
  define: {
    global: 'window',
  },
});
