var webpack = require('webpack');
var ExtractTextPlugin = require('extract-text-webpack-plugin');
var HtmlWebpackPlugin = require('html-webpack-plugin');

var { resolve } = require('path');
var AutoPrefixer = require('autoprefixer');
var LOADERS = require('./_loaders/_index.js');
var HELPERS = require('./_helpers/_index.js');

var webpackConfig = {
  context: resolve(__dirname, '../src'),

  entry: [
    'react-hot-loader/patch',
    'webpack-dev-server/client?http://localhost:8080',
    'webpack/hot/only-dev-server',
    './index.jsx',
  ],

  output: {
    filename: 'bundle.js',
    path: resolve(__dirname, '../dist'),
    publicPath: '/broadcastreact',
  },

  devServer: {
    hot: true,
    contentBase: resolve(__dirname, '../build'),
    publicPath: '/broadcastreact/',
    stats: HELPERS.stats,
    proxy: HELPERS.api.proxy
  },

  resolve: {
    alias: HELPERS.alias,
    extensions: ['.js', '.jsx', '.scss']
  },

  plugins: [
    new webpack.optimize.OccurrenceOrderPlugin(),
    new webpack.optimize.ModuleConcatenationPlugin(),
    new webpack.NoEmitOnErrorsPlugin(),
    new webpack.HotModuleReplacementPlugin(),
    new webpack.DefinePlugin({
      'process.env.NODE_ENV': '"development"',
      __PRODUCTION__: false,
      __API__: HELPERS.api.createApi('development'),
    }),
    new ExtractTextPlugin({
      filename: 'style.css',
    }),
    new HtmlWebpackPlugin({
      template: './index.html',
    }),
  ],
  devtool: '#source-map',
  module: {
    rules: [
      LOADERS.jsxBabel,
      LOADERS.image,
      LOADERS.svg,
      LOADERS.url,
      LOADERS.file,
      LOADERS.style(true),  // true for development, false for prod, default to true
      LOADERS.css(true),
    ]
  },
};

module.exports = webpackConfig;
