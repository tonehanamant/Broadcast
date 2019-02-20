import React from "react";
import PropTypes from "prop-types";
import { Switch, Route } from "react-router-dom";
import CSSModules from "react-css-modules";

// TO CHANGE
import SectionTracker from "Containers/SectionTracker";

// NEW STRUCTURE
import SectionPostPrePosting from "PostPrePosting";
import SectionPost from "Post";
import SectionPlanning from "Planning";

import Toast from "Patterns/Toast";
import ErrorModal from "Patterns/ErrorModal";
import ConfirmModal from "Patterns/ConfirmModal";
import Overlay from "Patterns/Overlay";
import "font-awesome/css/font-awesome.min.css";

import AppFooter from "../MainFooter";
import AppHeader from "../MainHeader";

import styles from "./index.style.scss";

export const Main = ({ match: { path } }) => (
  <div styleName="main-container">
    <Toast />
    <ErrorModal />
    <ConfirmModal />
    <AppHeader />
    <Overlay type="loading" />
    <Overlay type="processing" />
    <Switch>
      <Route exact path={path} component={SectionPostPrePosting} />
      <Route path={`${path}/post`} component={SectionPost} />
      <Route
        path={`${path}/post-pre-posting`}
        component={SectionPostPrePosting}
      />
      <Route path={`${path}/planning`} component={SectionPlanning} />
      <Route path={`${path}/tracker`} component={SectionTracker} />
    </Switch>
    <AppFooter />
  </div>
);

Main.propTypes = {
  match: PropTypes.object.isRequired
};

export default CSSModules(Main, styles);
