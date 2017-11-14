var ExtractTextPlugin = require('extract-text-webpack-plugin');
var AutoPrefixer = require('autoprefixer');

module.exports = function(isDev) {
  return {
    test: /\.(scss)$/,
    exclude: /node_modules/,
    use: ExtractTextPlugin.extract({
      fallback: 'style-loader',
      use: [
        {
          loader: 'css-loader',
          options: {
            sourceMap: isDev,
            minimize: !isDev,
            modules: true,
            importLoader: 2,
            // localIdentName: '[local]___[hash:base64:5]'
            localIdentName: '[local]'
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
        },
        {
          loader: 'sass-loader',
          options: {
            sourceMap: isDev,
          }
        }
      ]
    })
  };
}
