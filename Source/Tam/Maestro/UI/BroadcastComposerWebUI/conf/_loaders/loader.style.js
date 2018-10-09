const MiniCSSExtractPlugin = require("mini-css-extract-plugin");
const AutoPrefixer = require("autoprefixer");

const loaderCSS = (isProduction, extendOptions = {}) => ({
  loader: "css-loader",
  options: {
    sourceMap: true,
    importLoaders: 3,
    ...extendOptions
  }
});

const loaderSASS = (isProduction, extendOptions = {}) => ({
  loader: "sass-loader",
  options: {
    sourceMap: true,
    outputStyle: "expanded",
    ...extendOptions
  }
});

const loaderPOSTCSS = (isProduction, extendOptions = {}) => ({
  loader: "postcss-loader",
  options: {
    sourceMap: true,
    plugins: [
      AutoPrefixer({
        browsers: ["last 2 versions"]
      })
    ],
    ...extendOptions
  }
});

module.exports = ({ isProduction }) => ({
  test: /\.(css|scss)$/,
  oneOf: [
    {
      resourceQuery: /^\?raw$/,
      use: [
        "css-hot-loader",
        MiniCSSExtractPlugin.loader,
        loaderCSS(isProduction),
        loaderPOSTCSS(isProduction),
        loaderSASS(isProduction)
      ]
    },
    {
      use: [
        MiniCSSExtractPlugin.loader,
        loaderCSS(isProduction, {
          modules: true,
          context: __dirname,
          localIdentName: "[local]"
        }),
        loaderPOSTCSS(isProduction),
        loaderSASS(isProduction)
      ]
    }
  ]
});
