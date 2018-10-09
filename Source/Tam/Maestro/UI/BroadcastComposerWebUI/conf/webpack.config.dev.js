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
    path: resolve(__dirname, '../../BroadcastComposerWeb/broadcastreact/'),
    filename: '[name]-bundle-[hash].js',
    publicPath: '/broadcastreact/',
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
      __API__: '"broadcast/api/"'
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
