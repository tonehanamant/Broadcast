import { createStore, applyMiddleware, combineReducers } from 'redux';
import { routerReducer, routerMiddleware } from 'react-router-redux';
import { createLogger } from 'redux-logger';
import createmiddlewareSaga from 'redux-saga';
import createHistory from 'history/createBrowserHistory';

import { Reducers as gridReducers } from 'react-redux-grid';

import * as reducers from './app/ducks';
import sagas from './app/sagas';

export const history = createHistory();
const mwSaga = createmiddlewareSaga();
const mwLogger = createLogger();
const mwHistory = routerMiddleware(history);
const createStoreWithMiddleware = applyMiddleware(mwSaga, mwLogger, mwHistory);

const rootReducer = combineReducers({
  ...gridReducers,
  ...reducers,
  routing: routerReducer,
});

export default function configureStore(initialState) {
  const configuredStore = createStore(
    rootReducer,
    initialState,
    createStoreWithMiddleware,
  );

  mwSaga.run(sagas);

  if (module.hot) {
    // Enable Webpack hot module replacement for reducers
    module.hot.accept('./app/ducks/index.js', () => {
      const nextReducer = rootReducer;
      configuredStore.replaceReducer(nextReducer);
    });
  }

  return configuredStore;
}
