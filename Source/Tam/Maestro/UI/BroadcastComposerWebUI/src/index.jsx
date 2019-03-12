import React from "react";
import ReactDOM from "react-dom";
import { Provider } from "react-redux";
import { Router } from "react-router-dom";
import { store, history, configureIcons } from "AppConfigs";

// third party libraries here:
import "react-dates/initialize";
import "react-dates/lib/css/_datepicker.css?raw";
import "react-select/dist/react-select.css?raw";
import "./index.scss";
import "antd/dist/antd.css?raw";

import "./assets/icons/favicon.ico";

import AppRoot from "./app";

configureIcons();

ReactDOM.render(
  <Provider store={store}>
    <Router history={history}>
      <AppRoot />
    </Router>
  </Provider>,
  document.getElementById("root")
);
