import { createStore, applyMiddleware, combineReducers } from "redux";
import { createLogger } from "redux-logger";
import createMiddlewareSaga from "redux-saga";
import { Reducers as gridReducers } from "react-redux-grid";

// Reducers
// import * as reducers from "./index.ducks.js";
// import sagas from "./index.sagas.js";
import * as reducers from "./app/ducks/index.js";
import sagas from "./app/sagas/index.js";

// broadcast specific:
import { saveLocalStorageState } from "./index.store.localstorage.js";

const mwSaga =
  typeof createMiddlewareSaga === "function"
    ? createMiddlewareSaga()
    : createMiddlewareSaga.default();
const mwLogger = createLogger();
const createStoreWithMiddleware =
  process.env.NODE_ENV === "production"
    ? applyMiddleware(mwSaga)
    : applyMiddleware(mwSaga, mwLogger);
const rootReducer = combineReducers({
  ...gridReducers,
  ...reducers
});

export default function configureStore(initialState) {
  const configuredStore = createStore(
    rootReducer,
    initialState,
    createStoreWithMiddleware
  );

  mwSaga.run(sagas);

  if (module.hot) {
    // Enable Webpack hot module replacement for reducers
    // module.hot.accept("./index.ducks.js", () => {
    module.hot.accept("./app/ducks/index.js", () => {
      const nextReducer = rootReducer;
      configuredStore.replaceReducer(nextReducer);
    });
  }

  // broadcast specific:
  configuredStore.subscribe(() => {
    saveLocalStorageState({
      app: configuredStore.getState().app,
      planning: configuredStore.getState().planning
    });
  });

  return configuredStore;
}
