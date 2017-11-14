module.exports = {
  test: /\.(ttf|eot|svg|woff|woff2|)(\?v=[0-9]\.[0-9]\.[0-9])?$/,
  exclude: /node_modules/,
  use: ['file-loader']
};