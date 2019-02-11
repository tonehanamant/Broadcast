import "babel-polyfill";

import React from "react";
import ReactDOM from "react-dom";
import { Provider } from "react-redux";
import { Router } from "react-router-dom";
import { AppContainer } from "react-hot-loader";
import createHistory from "history/createBrowserHistory";

// import AppRoot from "AppRoot/components";
// import AppRoot from "Containers/AppRoot";

// import "Icons/favicon.ico";

// third party libraries here:
import "react-dates/initialize";
import "react-dates/lib/css/_datepicker.css?raw";
import "react-select/dist/react-select.css?raw";
import "./index.css?raw";
import "./index.scss";
// eslint-disable-next-line import/first
import "antd/dist/antd.css?raw";
import AppRoot from "./app";

import configureStore from "./index.store";
// import configureIcons from "./index.icons";

import "./assets/icons/favicon.ico";

const history = createHistory();
const store = configureStore();
// configureIcons();

const render = Component => {
  ReactDOM.render(
    <Provider store={store}>
      <Router history={history}>
        <AppContainer>
          <Component />
        </AppContainer>
      </Router>
    </Provider>,
    document.getElementById("root")
  );
};

render(AppRoot);

if (module.hot) {
  // module.hot.accept("./app/containers/AppRoot", () => {
  module.hot.accept("./app", () => {
    render(AppRoot);
  });
}

export default history;
