var ExtractTextPlugin = require('extract-text-webpack-plugin');
var AutoPrefixer = require('autoprefixer');

module.exports = function(isDev) {
  return {
    test: /\.(css)$/,
    // exclude: /node_modules/,
    use: ExtractTextPlugin.extract({
      fallback: 'style-loader',
      use: [
        {
          loader: 'css-loader',
          options: {
            sourceMap: isDev,
            minimize: !isDev,
            modules: false,
            importLoader: 2,
            localIdentName: '[local]___[hash:base64:5]'
          }
        },
        {
          loader: 'postcss-loader',
          options: {
            sourceMap: isDev,
            plugins: [
              AutoPrefixer({ browsers: ['last 2 versions'] })
            ]
          }
        }
      ]
    })
  };
}
