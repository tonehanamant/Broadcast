import React from "react";
import { Route } from "react-router-dom";
// import AppMain from "Main/components";
import AppMain from "Main";

const AppRoot = () => <Route path="/broadcastreact" component={AppMain} />;

export default AppRoot;
