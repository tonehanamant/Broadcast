import 'babel-polyfill';

import React from 'react';

/* eslint-disable import/first */

// Needed if React@16.0.0
// react-data-grid (Uses React.PropTypes) https://github.com/adazzle/react-data-grid/issues/744
// react-bootstrap (Breaks Modal) https://github.com/react-bootstrap/react-bootstrap/issues/2812

    // import './shims/r15-prop-types-shim'; // fixes React.PropTypes error for 'prop-type'
    // import './shims/r15-create-class-shim'; // fixes React.creatClass error for 'create-react-class'

import ReactDOM from 'react-dom';
import { Provider } from 'react-redux';
import { ConnectedRouter } from 'react-router-redux';
import { IntlProvider } from 'react-intl';
import { AppContainer } from 'react-hot-loader';

import AppRoot from 'Containers/AppRoot';

import configureStore, { history } from './index.store';

import 'react-select/dist/react-select.css';
import './index.css';
import './index.scss';

require('./assets/icons/favicon.png');

const store = configureStore();
const locale = 'en';


const render = (Component) => {
  ReactDOM.render(
    <AppContainer>
      <IntlProvider locale={locale}>
        <Provider store={store}>
          <ConnectedRouter history={history}>
            <Component />
          </ConnectedRouter>
        </Provider>
      </IntlProvider>
    </AppContainer>,
    document.getElementById('root'),
  );
};

render(AppRoot);

if (module.hot) {
  module.hot.accept('./app/containers/AppRoot', () => { render(AppRoot); });
}
