var { resolve } = require('path');

module.exports = {
  SRCRoot: resolve(__dirname, '../../src'),
  Components: resolve(__dirname, '../../src/app/components'),
  Containers: resolve(__dirname, '../../src/app/containers'),
  Ducks: resolve(__dirname, '../../src/app/ducks'),
  Sagas: resolve(__dirname, '../../src/app/sagas'),
  Utils: resolve(__dirname, '../../src/app/utils'),
  Lib: resolve(__dirname, '../../src/app/lib'),
  Assets: resolve(__dirname, '../../src/assets'),
};