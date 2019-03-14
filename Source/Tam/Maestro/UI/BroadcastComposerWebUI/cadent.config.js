const path = require("path");

// local api environment variables
const api = {
  global: {
    __API__: "https://jsonplaceholder.typicode.com"
  },
  local: {
    // __API__: "http://localhost:61722/api/"
    __API__: "http://devvmqa2.dev.crossmw.com/Broadcast/api/"
  }
};

module.exports = {
  app: {
    /**
     * @description the path that the application is going to be,
     * accessed from. Defaults to "/" if there is no
     * value provided
     * @default "/"
     */
    publicPath: "/broadcastreact",
    /**
     * @description the base application title that needs to be displayed,
     * at the home page. This will be displayed for every
     * page unless otherwise changed using the a React hook
     * @default "Cadent Web Application"
     *
     */
    title: "Broadcast",
    /**
     * 4 options
     * ["local", "development", "qa", "stage", "demo", "production"]
     */
    envVars: {
      local: {
        ...api.local
      },
      development: {
        ...api.global
      },
      qa: {
        ...api.global
      },
      demo: {
        ...api.global
      },
      stage: {
        ...api.global
      },
      production: {
        ...api.global
      }
    },
    /**
     * If you need to, you have the ability to specify files that you'll need
     * copied from certain directories into the final webpack builds. You can include
     * them in the copy configuration key. Each position in the array takes an
     * environment and then the file that needs to be copied. Use the node `path.resolve()`
     * method to ensure that you're always resolving the full path of the file regardless
     * of the environment that it's being built in.
     *
     * NOTE: environment must match one of the envVar keys that you have in the above.
     * If it does not, then the file won't be copied to those environments and you'll
     * get a build-time error. If you include no envVars, then the defaults will be
     * "development" and "production". If you want to include the same file in
     * all webpack builds, then use the key "all". At the moment, this does not support
     * globbing. Please add all files by explicit absolute path name
     */
    copy: [
      {
        environments: ["all"],
        location: path.resolve(__dirname, "./src/app-configs/Web.config")
      }
    ],
    /**
     * @description In order to simplify the import boilerplate,
     * writing these aliases and mapping them to the `src/app`
     * directory and below, allows them to be imported
     * much like node_module imports. Adding them here not
     * only maps them for webpack but also adds them to the
     * Jest import resolution file as well. You can only add
     * aliases for items inside of the `src/app` directory
     *
     * NOTE: Make sure that you put your deepest routes at top
     * and then the shortest routes at the bottom. This ensures
     * that the testing module picks up all of the aliases in
     * priority order
     * @default {}
     * @example `src/app/<ALIAS>`
     */
    alias: {
      Main: "features/Main",
      Post: "features/Post",
      Planning: "features/Planning",
      Proposal: "features/Proposal",
      PricingGuide: "features/PricingGuide",
      PostPrePosting: "features/PostPrePosting",
      Tracker: "features/Tracker",

      Patterns: "patterns",
      Ducks: "ducks",
      Sagas: "sagas",
      Utils: "utils",
      Lib: "lib",
      API: "api"
    }
  },
  test: {
    /**
     * @description Sets the required amount of coverage needed in all
     * application directories. If this threshold isn't met,
     * then all tests will fail. By default, this value will
     * be set to 50. You have the option of increasing this
     * threshold but cannot decrease it below 50%.
     * @default 50
     */
    requiredCoverage: 0,
    /**
     * @description Global test variables that you want to access during
     * testing. Defaults to an empty string.
     * You will however always receive a few defaults
     * @default {}
     */
    globalTestVars: {}
  }
};
