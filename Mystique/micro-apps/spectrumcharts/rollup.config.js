import { terser } from "rollup-plugin-terser";
import babel from "rollup-plugin-babel";
import postcss from "rollup-plugin-postcss";
import image from "@rollup/plugin-image";
import resolve from "@rollup/plugin-node-resolve";
import svgr from "vite-plugin-svgr";

export default {
  input: "./src/components/index.js",
  output: {
    file: "./dist/index.js",
  },

  plugins: [
    // terser(),
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
    svgr(),
    // 打包依赖到一起
    // resolve(),
  ],

  external: [
    "@ant-design/icons",
    "react",
    "mapbox-gl",
    "prop-types",
    "react-dom",
    "react-router",
    "react-router-dom",
    "less",
    "dayjs",
    "ahooks",
    "dui",
  ],
};
