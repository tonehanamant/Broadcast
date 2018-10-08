const webpack = require("webpack");
const UglifyJsPlugin = require("uglifyjs-webpack-plugin");
const MiniCssExtractPlugin = require("mini-css-extract-plugin");
const OptimizeCSSAssetsPlugin = require("optimize-css-assets-webpack-plugin");
const HtmlWebpackPlugin = require("html-webpack-plugin");

const { resolve } = require("path");
const LOADERS = require("./_loaders/_index.js");
const HELPERS = require("./_helpers/_index.js");

module.exports = {
  mode: "production",
  target: "web",
  entry: resolve(__dirname, "../src/index.jsx"),
  output: {
    filename: "[hash].js",
    path: resolve(__dirname, "../dist"),
    publicPath: "/broadcastreact/"
  },
  stats: HELPERS.stats,
  resolve: {
    alias: HELPERS.alias,
    extensions: [".js", ".jsx", ".scss"],
    mainFields: ["main", "module"]
  },
  optimization: {
    noEmitOnErrors: true,
    minimizer: [
      new UglifyJsPlugin({
        cache: true,
        parallel: true,
        sourceMap: true
      }),
      new OptimizeCSSAssetsPlugin({
        cssProcessorOptions: {
          safe: true,
          discardComments: { removeAll: true }
        },
        canPrint: true
      })
    ],
    splitChunks: {
      cacheGroups: {
        styles: {
          name: "styles",
          test: /\.css$/,
          chunks: "all",
          enforce: true
        }
      }
    }
  },
  plugins: [
    new webpack.optimize.ModuleConcatenationPlugin(),
    new webpack.optimize.OccurrenceOrderPlugin(),
    new webpack.LoaderOptionsPlugin({
      options: {}
    }),
    new webpack.DefinePlugin({
      __API_HOSTNAME__: '""',
      __API_NAME_AND_VERSION__: '"broadcast/api/"'
      // __API_NAME_AND_VERSION__: '"broadcast/api/v1/"'
    }),
    new MiniCssExtractPlugin({
      filename: "[contenthash].min.css",
      chunkFilename: "[contenthash].min.css"
    }),
    new HtmlWebpackPlugin({
      favicon: resolve(__dirname, "../src/assets/icons/favicon.ico"),
      template: resolve(__dirname, "../src/index.html"),
      title: "Broadcast | PROD"
    })
  ],
  module: {
    rules: [
      LOADERS.jsxBabel,
      LOADERS.image,
      LOADERS.svg,
      LOADERS.url,
      LOADERS.file,
      LOADERS.style({ isProduction: true })
    ]
  }
};
