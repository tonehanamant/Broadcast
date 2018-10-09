const webpackConfiguration = require("./webpack.config");
const path = require("path");

module.exports = {
  title: "Broadcast React JS Styleguide & Component Pattern Library",
  components: "../src/app/**/*.jsx",
  webpackConfig: {
    ...webpackConfiguration(),
    devServer: {
      publicPath: "/"
    }
  },
  editorConfig: {
    theme: "monokai"
  },
  styleguideDir: path.join(__dirname, "../docs/js/styleguide"),
  assetsDir: path.join(__dirname, "../docs/js/assets"),
  dangerouslyUpdateWebpackConfig(webpackConfig) {
    webpackConfig.entry = ["babel-polyfill"].concat(webpackConfig.entry);
    return webpackConfig;
  },
  require: [path.join(__dirname, "./styleguide.config.css")],
  getComponentPathLine(componentPath) {
    const dir = path.dirname(componentPath);
    const componentName = dir.split("/")[dir.split("/").length - 1];
    const componentDir = `Patterns/${dir.split("patterns/")[1]}`;
    return `import ${componentName} from '${componentDir}';`;
  },
  showUsage: true,
  theme: {
    sidebarWidth: 350
  },
  sections: [
    {
      name: "Introduction",
      content: "../docs/js/introduction.md"
    },
    {
      name: "React & JS Styleguide/Standards",
      sections: [
        {
          name: "Getting Started",
          content: "../docs/js/getting-started.md"
        },
        {
          name: "Build & Asset Pipeline",
          content: "../docs/js/builds.md"
        },
        {
          name: "Version Control",
          content: "../docs/js/version-control.md"
        },
        {
          name: "Respository",
          content: "../docs/js/repository.md"
        },
        {
          name: "Features",
          content: "../docs/js/features.md"
        },
        {
          name: "Components",
          content: "../docs/js/components.md"
        },
        {
          name: "Styles",
          content: "../docs/js/styles.md"
        },
        {
          name: "Assets",
          content: "../docs/js/assets.md"
        },
        {
          name: "Redux & Redux Saga",
          content: "../docs/js/redux.md"
        },
        {
          name: "Api",
          content: "../docs/js/api.md"
        },
        {
          name: "Linting & Code Formatting",
          content: "../docs/js/linting.md"
        },
        {
          name: "Style Guide",
          content: "../docs/js/style-guide.md"
        },
        {
          name: "Testing",
          content: "../docs/js/testing.md"
        }
      ]
    },
    {
      name: "Patterns & Components",
      sections: [
        {
          name: "Typography",
          components: () => [
            "../src/app/patterns/typography/Heading/index.jsx",
            "../src/app/patterns/typography/Paragraph/index.jsx"
          ]
        },
        {
          name: "Buttons & Links",
          components: () => [
            "../src/app/patterns/buttons-links/Button/index.jsx",
            "../src/app/patterns/buttons-links/ButtonDropdown/index.jsx",
            "../src/app/patterns/buttons-links/ButtonIcon/index.jsx"
          ]
        },
        {
          name: "Inputs",
          content: "../docs/patterns/inputs.md",
          sections: [
            {
              name: "Text Inputs",
              content: "../docs/patterns/inputs_text.md",
              components: () => [
                "../src/app/patterns/inputs/InputText/index.jsx",
                "../src/app/patterns/inputs/InputTextarea/index.jsx",
                "../src/app/patterns/inputs/InputTextMask/index.jsx",
                "../src/app/patterns/inputs/InputPassword/index.jsx",
                "../src/app/patterns/inputs/InputSearch/index.jsx"
              ]
            },
            {
              name: "Number Inputs",
              content: "../docs/patterns/inputs_number.md",
              components: () => [
                "../src/app/patterns/inputs/InputNumber/index.jsx"
                // '../src/app/patterns/inputs/InputPercent/index.jsx',
                // '../src/app/patterns/inputs/InputMoney/index.jsx',
              ]
            },
            {
              name: "Selection Inputs",
              content: "../docs/patterns/inputs_select.md",
              components: () => [
                "../src/app/patterns/inputs/InputSelect/index.jsx",
                "../src/app/patterns/inputs/InputPill/index.jsx"
              ]
            },
            {
              name: "Collection Inputs",
              components: () => [
                "../src/app/patterns/inputs/InputCheckbox/index.jsx",
                "../src/app/patterns/inputs/InputRadio/index.jsx"
                // '../src/app/patterns/inputs/InputSwitch/index.jsx',
              ]
            }
            // {
            //   name: "File Inputs",
            //   components: () => [
            //     '../src/app/patterns/inputs/InputImageBase64/index.jsx',
            //   ]
            // },
            // {
            //   name: 'Input Groups',
            //   components: () => [
            //     '../src/app/patterns/layouts/FlexGrid/index.jsx',
            //     '../src/app/patterns/inputs/InputGroup/index.jsx',
            //   ]
            // }
          ]
        },
        {
          name: "Iconography",
          components: () => [
            "../src/app/patterns/icons-images/Icon/index.jsx"
            // '../src/app/patterns/icons-images/ImagePlaceholder/index.jsx',
          ]
        },
        {
          name: "Dates",
          components: () => [
            "../src/app/patterns/dates/DatePickerRange/index.jsx",
            "../src/app/patterns/dates/DatePickerSingle/index.jsx"
            // '../src/app/patterns/dates/Calendar/index.jsx',
          ]
        },
        {
          name: "Lists",
          components: () => [
            "../src/app/patterns/lists/ListFormatted/index.jsx"
            // '../src/app/patterns/lists/ListLedger/index.jsx',
            // '../src/app/patterns/lists/ListSummary/index.jsx',
          ]
        },
        {
          name: "Navigation",
          components: () => [
            "../src/app/patterns/navigation/Breadcrumbs/index.jsx",
            "../src/app/patterns/navigation/Link/index.jsx",
            "../src/app/patterns/navigation/Tabs/index.jsx"
            // '../src/app/patterns/navigation/Wizard/index.js',
          ]
        },
        {
          name: "Progress",
          components: () => [
            "../src/app/patterns/progress/Loading/index.jsx"
            // '../src/app/patterns/progress/StatusBar/index.jsx',
          ]
        },
        {
          name: "Dialogs",
          components: () => ["../src/app/patterns/dialogs/Tooltip/index.jsx"]
        }
      ]
    }
  ]
};
