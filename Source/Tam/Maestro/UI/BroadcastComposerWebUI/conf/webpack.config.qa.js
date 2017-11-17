var webpack = require('webpack');
var ExtractTextPlugin = require('extract-text-webpack-plugin');
var HtmlWebpackPlugin = require('html-webpack-plugin');

var { resolve } = require('path');
var AutoPrefixer = require('autoprefixer');
var LOADERS = require('./_loaders/_index.js');
var HELPERS = require('./_helpers/_index.js');

var webpackConfig = {
  context: resolve(__dirname, '../src'),

  entry: {
    app: ['babel-polyfill', './index.jsx'],
    vendor: HELPERS.exclude,
  },

  output: {
    path: resolve(__dirname, '../dist'),
    filename: '[name]-bundle-[hash].js',
    publicPath: '/broadcastreact/',
  },

  resolve: {
    alias: HELPERS.alias,
    extensions: ['.js', '.jsx', '.scss']
  },

  node: {
    fs: "empty"
  },

  stats: HELPERS.stats,

  plugins: [
    new webpack.optimize.ModuleConcatenationPlugin(),
    new webpack.optimize.OccurrenceOrderPlugin(),
    new webpack.optimize.CommonsChunkPlugin({
      name: 'vendor'
    }),
    new webpack.NoEmitOnErrorsPlugin(),
    new webpack.DefinePlugin({
      'process.env.NODE_ENV': '"development"',
      __PRODUCTION__: false,
      __API__: HELPERS.api.createApi('qa'),
    }),
    new ExtractTextPlugin({
      filename: '[name]-style-[hash].css',
    }),
    new HtmlWebpackPlugin({
      favicon: resolve(__dirname, '../src/assets/icons/favicon.ico'),
      template: './index.html',
    }),
  ],

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
