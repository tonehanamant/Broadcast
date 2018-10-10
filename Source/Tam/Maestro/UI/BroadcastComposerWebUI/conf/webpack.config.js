const webpack = require("webpack");
const MiniCSSExtractPlugin = require("mini-css-extract-plugin");
const HtmlWebpackPlugin = require("html-webpack-plugin");

const { resolve } = require("path");
const LOADERS = require("./_loaders/_index.js");
const HELPERS = require("./_helpers/_index.js");

module.exports = env => ({
  mode: "development",
  target: "web",
  entry: [
    "react-hot-loader/patch",
    `webpack-dev-server/client?http://localhost:${env || 8080}`,
    "webpack/hot/only-dev-server",
    resolve(__dirname, "../src/index.jsx")
  ],
  output: {
    filename: "bundle.js",
    path: resolve(__dirname, "../dist"),
    publicPath: "/broadcastreact/"
  },
  devtool: '#source-map',
  devServer: {
    hot: true,
    port: env || 8080,
    contentBase: resolve(__dirname, "../src"),
    publicPath: "/broadcastreact",
    historyApiFallback: {
      index: "/broadcastreact/"
    },
    stats: HELPERS.stats,
    proxy: {
      "/api/**": {
        target: "http://localhost:61722/",
        secure: false,
        changeOrigin: true,
        pathRewrite: {
          "http://localhost:61722/": ""
        }
      }
    }
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
    new webpack.HotModuleReplacementPlugin(),
    new webpack.LoaderOptionsPlugin({
      options: {}
    }),
    new webpack.DefinePlugin({
      __API_HOSTNAME__: `"http://localhost:${env || 8080}"`,
      __API_NAME_AND_VERSION__: '"api/"',
      __API__: `"http://localhost:${env || 8080}/api/"`,
    }),
    new MiniCSSExtractPlugin("style.css"),
    new HtmlWebpackPlugin({
      favicon: resolve(__dirname, "../src/assets/icons/favicon.ico"),
      template: resolve(__dirname, "../src/index.html"),
      title: "Broadcast | Local"
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
});
