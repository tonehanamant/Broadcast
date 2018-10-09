module.exports = {
  test: /\.(js|jsx)$/,
  exclude: function(modulePath) {
    return /node_modules/.test(modulePath) && /node_modules(\/|\\)(?!react-table|react-dropzone)/.test(modulePath);
  },
  use: [
    {
      loader: "babel-loader",
      options: {
        presets: ["env", "react"]
      }
    },
    "eslint-loader"
  ]
};
