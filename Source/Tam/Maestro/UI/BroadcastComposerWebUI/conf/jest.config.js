const { resolve } = require("path");

module.exports = {
  rootDir: resolve(__dirname, "../src"),
  modulePathIgnorePatterns: ["<rootDir>/app/patterns/"],
  moduleNameMapper: {
    // Resolves all files that aren't js handled by webpack
    "^.+\\.(jpg|jpeg|png|gif|eot|otf|webp|svg|ttf|woff|woff2|mp4|webm|wav|mp3|m4a|aac|oga)$":
      "<rootDir>/__mocks__/fileMock.js",
    // Resolves React CSS Modules correctly
    "^.+\\.(css|scss|css?raw)$": "identity-obj-proxy",
    // Take care of Aliases:
    // -- App
    "^Patterns(.*)$": "<rootDir>/app/patterns$1",
    "^Validation(.*)$": "<rootDir>/app/validation$1",
    "^Utils(.*)$": "<rootDir>/app/utils$1",
    "^API(.*)$": "<rootDir>/app/api$1",
    "^History(.*)$": "<rootDir>/index.jsx",
    // -- Features
    "^Features(.*)$": "<rootDir>/app/features$1",
    "^AppRoot(.*)$": "<rootDir>/app/features/AppRoot$1",
    "^Main(.*)$": "<rootDir>/app/features/Main$1",
    "^Generic(.*)$": "<rootDir>/app/features/Generic$1",
    "^Modal(.*)$": "<rootDir>/app/features/Modal$1",
    "^Plans(.*)$": "<rootDir>/app/features/Plans$1",
    "^Research(.*)$": "<rootDir>/app/features/Research$1",
    "^Dashboard(.*)$": "<rootDir>/app/features/Dashboard$1",
    "^Campaigns(.*)$": "<rootDir>/app/features/Campaigns$1",
    "^Proposal(.*)$": "<rootDir>/app/features/Proposal$1",
    // -- Assets
    "^Icons(.*)$": "<rootDir>/assets/icons$1",
    "^Logos(.*)$": "<rootDir>/assets/logos$1"
  },
  moduleFileExtensions: ["js", "jsx", "scss"],
  moduleDirectories: ["node_modules"],
  // Location of main config
  setupFiles: [resolve(__dirname, "../src/index.jest.js")],
  snapshotSerializers: ["enzyme-to-json/serializer"],
  collectCoverageFrom: ["**/*.{js,jsx}"],
  // Configure coverages
  coverageDirectory: "__coverage__",
  coveragePathIgnorePatterns: [
    // locations where coverage reports shouldn't be looking
    "/node_modules/",
    "<rootDir>/__coverage__/",
    "<rootDir>/__mocks__/",
    "<rootDir>/index.jest.js",
    "<rootDir>/index.store.js",
    "<rootDir>/assets/",
    "<rootDir>/styles/"
  ]
};
