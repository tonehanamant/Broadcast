import React from "react";
import ReactDOM from "react-dom";
import { Provider } from "react-redux";
import { Router } from "react-router-dom";
import { store, history, configureIcons, StoreContext } from "AppConfigs";

// third party libraries here:
import "react-dates/initialize";
import "react-dates/lib/css/_datepicker.css?raw";
import "react-select/dist/react-select.css?raw";
import "react-table/react-table.css?raw";
import "react-contexify/dist/ReactContexify.css?raw";
import "./styles/imports/bootstrap/stylesheets/bootstrap-3.3.5/css/bootstrap.css?raw";
import "./styles/imports/bootstrap/stylesheets/bootstrap-3.3.5/css/bootstrap-theme.css?raw";
import "./index.scss";

/* Legacy Broadcast CSS */
import "./styles/imports/broadcast-css/app.css?raw";
import "./styles/imports/broadcast-css/appPost.css?raw";
import "antd/dist/antd.css?raw";

import "./assets/icons/favicon.ico";

import AppRoot from "./app";

configureIcons();

ReactDOM.render(
  <Provider store={store}>
    <StoreContext.Provider value={store}>
      <Router history={history}>
        <AppRoot />
      </Router>
    </StoreContext.Provider>
  </Provider>,
  document.getElementById("root")
);
