import { terser } from "rollup-plugin-terser";
import babel from "rollup-plugin-babel";
import postcss from "rollup-plugin-postcss";
import image from "@rollup/plugin-image";
import resolve from "@rollup/plugin-node-resolve";
import commonjs from "@rollup/plugin-commonjs";
import json from "@rollup/plugin-json";

export default {
  input: "./src/components/index.js",
  output: {
    file: "./dist/index.js",
  },

  plugins: [
    // terser(),
    // copy({
    //   targets: [{ src: "src/assets/*", dest: "dist/assets" }],
    // }),
    postcss({
      extract: false,
      sourceMap: false,
      inject: true,
      extensions: [".less", ".css"],
      use: {
        less: { javascriptEnabled: true },
      },
    }),
    babel({
      exclude: "**/node_modules/**",
      runtimeHelpers: true,
    }),
    image(),
    commonjs(),
    json(),
    resolve({
      // 将自定义选项传递给解析插件
      customResolveOptions: {
        moduleDirectory: ["node_modules"],
      },
    }),
    // 打包依赖到一起
    // resolve(),
  ],

  external: ["@ant-design/icons", "react", "mapbox-gl", "prop-types"],
};
