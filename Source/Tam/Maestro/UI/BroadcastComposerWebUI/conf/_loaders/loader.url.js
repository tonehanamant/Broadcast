module.exports = {
  test: /\.(png|jpg|woff|woff2|eot|ttf|svg)(\?v=[0-9]\.[0-9]\.[0-9])?$/,
  use: [
    {
      loader: "url-loader",
      options: {
        limit: 10000,
        mimetype: "application/font-woff"
      }
    }
  ]
};
