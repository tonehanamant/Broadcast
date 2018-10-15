module.exports = {
  test: /\.(png|jpg|gif|ico)$/,
  exclude: /node_modules/,
  use: [
    {
      loader: "file-loader",
      options: {
        name: "[hash].[ext]"
      }
    }
  ]
};
