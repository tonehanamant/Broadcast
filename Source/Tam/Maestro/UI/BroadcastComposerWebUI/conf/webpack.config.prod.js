var webpack = require('webpack');
var ExtractTextPlugin = require('extract-text-webpack-plugin');
var HtmlWebpackPlugin = require('html-webpack-plugin');
require ('babel-polyfill');
require ('react');
require ('react-dom');

var { resolve } = require('path');
var AutoPrefixer = require('autoprefixer');
var LOADERS = require('./_loaders/_index.js');
var HELPERS = require('./_helpers/_index.js');

var webpackConfig = {
  context: resolve(__dirname, '../src'),

  entry: {
    app: [
      'babel-polyfill',
      'react',
      'react-dom',
      './index.jsx'
    ],
    vendor: [
      'babel-polyfill',
      'react',
      'react-dom',
    ].concat(HELPERS.exclude),
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
    new webpack.optimize.UglifyJsPlugin({
      sourceMap: false,
      output: {
        comments: false
      },
      compress: {
        unused: true,
        dead_code: true,
        warnings: false
      }
    }),
    new webpack.NoEmitOnErrorsPlugin(),
    new webpack.DefinePlugin({
      'process.env.NODE_ENV': '"production"',
      __PRODUCTION__: true,
      __API__: HELPERS.api.createApi('production'),
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
      LOADERS.style(false),  // true for development, false for prod, default to true
      LOADERS.css(false),
    ]
  },
};

module.exports = webpackConfig;
