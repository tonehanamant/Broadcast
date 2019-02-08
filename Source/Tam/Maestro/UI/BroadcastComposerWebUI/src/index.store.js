import { createStore, applyMiddleware } from "redux";
import { createLogger } from "redux-logger";
import createMiddlewareSaga from "redux-saga";
import { rootSaga, rootReducer } from "AppConfigs";

// Reducers
import oldSagas from "./app/sagas/index.js";

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

export default function configureStore(initialState) {
  const configuredStore = createStore(
    rootReducer,
    initialState,
    createStoreWithMiddleware
  );

  mwSaga.run(oldSagas);
  mwSaga.run(rootSaga);

  if (module.hot) {
    // Enable Webpack hot module replacement for reducers
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
