module.exports = {
  test: /\.woff2?(\?v=[0-9]\.[0-9]\.[0-9])?$/,
  use: [
    {
      loader: 'url-loader',
      options: {
        limit: 10000,
        mimetype: 'application/font-woff'
      }
    }
  ]
};