const { resolve } = require("path");

module.exports = {
  SRCRoot: resolve(__dirname, '../../src'),
  Components: resolve(__dirname, '../../src/app/components'),
  Containers: resolve(__dirname, '../../src/app/containers'),
  Patterns: resolve(__dirname, '../../src/app/patterns'),
  Ducks: resolve(__dirname, '../../src/app/ducks'),
  Sagas: resolve(__dirname, '../../src/app/sagas'),
  Utils: resolve(__dirname, '../../src/app/utils'),
  Lib: resolve(__dirname, '../../src/app/lib'),
  Assets: resolve(__dirname, '../../src/assets'),
  API: resolve(__dirname, '../../src/app/api'),
  AppConfigs: resolve(__dirname, '../../src/app/app-configs'),
  Main: resolve(__dirname, '../../src/app/features/Main'),
  Post: resolve(__dirname, '../../src/app/features/Post'),
  PostPrePosting: resolve(__dirname, '../../src/app/features/PostPrePosting')
};