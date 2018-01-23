import React from 'react';
import ReactDOM from 'react-dom';
import { Provider } from 'react-redux';
import { ConnectedRouter } from 'react-router-redux';
import { AppContainer } from 'react-hot-loader';
import 'Utils/element-closest'; // Element.closest polyfill

import AppRoot from 'Containers/AppRoot';

import configureStore, { history } from './index.store';

/* eslint-disable import/first */
import 'react-select/dist/react-select.css';
import 'react-dates/initialize';
import 'react-dates/lib/css/_datepicker.css';
// import 'antd/dist/antd.css';
import './index.css';
import './index.scss';
import 'antd/dist/antd.css';


require('./assets/icons/favicon.png');

const store = configureStore();

const render = (Component) => {
  ReactDOM.render(
    <AppContainer>
        <Provider store={store}>
          <ConnectedRouter history={history}>
            <Component />
          </ConnectedRouter>
        </Provider>
    </AppContainer>,
    document.getElementById('root'),
  );
};

render(AppRoot);

if (module.hot) {
  module.hot.accept('./app/containers/AppRoot', () => { render(AppRoot); });
}
