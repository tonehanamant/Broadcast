const webpack = require("webpack");
const MiniCSSExtractPlugin = require("mini-css-extract-plugin");
const HtmlWebpackPlugin = require("html-webpack-plugin");

const { resolve } = require("path");
const LOADERS = require("./_loaders/_index.js");
const HELPERS = require("./_helpers/_index.js");

module.exports = {
  mode: "development",
  target: "web",
  entry: resolve(__dirname, "../src/index.jsx"),
  output: {
    filename: "dev-bundle.js",
    path: resolve(__dirname, "../dist"),
    publicPath: "/broadcastreact/"
  },
  resolve: {
    alias: HELPERS.alias,
    extensions: [".js", ".jsx", ".scss"]
  },
  optimization: {
    noEmitOnErrors: true,
    concatenateModules: true
  },
  plugins: [
    new webpack.optimize.OccurrenceOrderPlugin(),
    new webpack.LoaderOptionsPlugin({
      options: {}
    }),
    new webpack.DefinePlugin({
      __API_HOSTNAME__: '""',
      __API_NAME_AND_VERSION__: '"broadcast/api/"'
      // __API_NAME_AND_VERSION__: '"broadcast/api/v1/"'
    }),
    new MiniCSSExtractPlugin("dev-style.css"),
    new HtmlWebpackPlugin({
      favicon: resolve(__dirname, "../src/assets/icons/favicon.ico"),
      template: resolve(__dirname, "../src/index.html"),
      title: "Broadcast | Dev"
    })
  ],
  module: {
    rules: [
      LOADERS.jsxBabel,
      LOADERS.image,
      LOADERS.svg,
      LOADERS.url,
      LOADERS.file,
      LOADERS.style({ isProduction: false })
    ]
  }
};
