module.exports = {
  test: /\.(png|jpg|gif)$/,
  exclude: /node_modules/,
  use: [
    {
      loader: 'url-loader',
      options: {
        limit: 8192
      }
    }
  ]
};