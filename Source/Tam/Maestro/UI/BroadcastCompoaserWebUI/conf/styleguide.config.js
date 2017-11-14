var path = require('path');

module.exports = {
  title: 'one2one React Component Library',
  components: '../src/app/**/*.jsx',
  highlightTheme: 'monokai',
  webpackConfig: require('./webpack.config.js'),
  dangerouslyUpdateWebpackConfig: function(webpackConfig) {
    webpackConfig.entry = ['babel-polyfill'].concat(webpackConfig.entry);
    return webpackConfig;
  },
  styleguideDir: 'one2one Pattern Library',
  sections: [
    {
      name: 'Presentaitonal',
      components: () => [
        '../src/app/components/presentation/Typography/Heading/index.jsx',
        '../src/app/components/presentation/Typography/Paragraph/index.jsx',
        '../src/app/components/presentation/FontIcon/index.jsx',
        '../src/app/components/presentation/LoadingSpinner/index.jsx',
      ]
    },
    {
      name: 'Input Controls',
      components: () => [
        '../src/app/components/input-controls/Button/index.jsx',
        '../src/app/components/input-controls/InputText/index.jsx',
        '../src/app/components/input-controls/InputNumber/index.jsx',
        '../src/app/components/input-controls/InputTextarea/index.jsx',
        '../src/app/components/input-controls/InputSelect/index.jsx',
        '../src/app/components/input-controls/InputPassword/index.jsx',
        '../src/app/components/input-controls/InputSearch/index.jsx',
        '../src/app/components/input-controls/InputUploadBase64/index.jsx',
      ]
    },
    {
      name: 'Navigational',
      components: () => [
        '../src/app/components/navigation/Tabs/index.jsx',
        '../src/app/components/navigation/Link/index.jsx',
      ]
    },
    {
      name: 'Informational',
      components: () => [
        '../src/app/components/information/ModalLightbox/index.example.jsx',
        '../src/app/components/information/ModalDialog/index.example.jsx',
        '../src/app/components/information/ModalLoading/index.example.jsx',
      ]
    },
    {
      name: 'Collections',
      components: () => [
        '../src/app/components/collections/CreateAccount/index.jsx',
      ]
    },
  ],
  styleguideComponents: {
    Wrapper: path.join(__dirname, 'styleguide.config.wrapper.jsx')
  }
};
