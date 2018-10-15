module.exports = {
  test: /\.(ttf|eot|svg)(\?v=[0-9]\.[0-9]\.[0-9])?$/,
  exclude: /node_modules/,
  use: ["file-loader"]
};
