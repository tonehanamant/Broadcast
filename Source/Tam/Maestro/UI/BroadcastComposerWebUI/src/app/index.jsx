import React from "react";
import { Route } from "react-router-dom";
import AppMain from "Main";
import { hot } from "react-hot-loader";

const AppRoot = () => <Route path="/" component={AppMain} />;

export default hot(module)(AppRoot);
