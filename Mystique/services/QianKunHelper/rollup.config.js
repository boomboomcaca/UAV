// import { terser } from "rollup-plugin-terser";

// eslint-disable-next-line import/no-anonymous-default-export
export default {
  input: "./src/index.js",
  output: {
    file: "./dist/index.js",
    format: "esm",
    name: "skywaver-qiankun-helper",
  },
  // plugins: [terser()],
  external: ["dui", "react", "react-dom", "qiankun"],
};
