const path = require("path");
// const nodeExternals = require("webpack-node-externals");
const CopyPlugin = require("copy-webpack-plugin");

module.exports = {
  mode: "production",
  entry: "./tileServer.js",
  output: {
    filename: "bundle.js",
    path: path.resolve(__dirname, "dist"),
  },
  externalsPresets: { node: true }, // 为了忽略诸如path、fs等内置模块。
  externals: [
    //     function ({context, request}, cb) => {
    //       if (/^node-gyp$/.test(request)) {
    //         return callback(null, "commonjs " + request);
    //       }
    //       callback();
    //     }),

    function ({ context, request }, callback) {
      if (/node-pre-gyp$/.test(request)) {
        return callback(null, "commonjs " + request);
      }
      if (/^node-gyp$/.test(request)) {
        // Externalize to a commonjs module using the request path
        return callback(null, "commonjs " + request);
      }

      // Continue without externalizing the import
      callback();
    },
    {
      // 可以手动排除
      //  "package-name": "commonjs package-name",
    },
  ],
  plugins: [
    new CopyPlugin({
      patterns: [
        {
          from: path.resolve(__dirname, "node_modules/node-gyp"),
          to: path.resolve(__dirname, "dist/node-gyp"),
        },
      ],
    }),
  ],
  //   externals: [nodeExternals()], // 以忽略节点\模块文件夹中的所有模块
};
