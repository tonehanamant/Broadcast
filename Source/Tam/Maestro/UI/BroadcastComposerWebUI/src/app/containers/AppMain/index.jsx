import React from "react";
import PropTypes from "prop-types";
import { Switch, Route } from "react-router-dom";
import CSSModules from "react-css-modules";

import AppHeader from "Components/header/AppHeader";
import AppFooter from "Components/footer/AppFooter";
// import SectionHome from 'Containers/SectionHome';
import SectionPostPrePosting from "PostPrePosting";
import SectionPost from "Containers/SectionPost";
import SectionPlanning from "Containers/SectionPlanning";
import SectionTracker from "Containers/SectionTracker";
// import SectionRates from 'Containers/SectionRates';
import Toast from "Patterns/Toast";
import ErrorModal from "Patterns/ErrorModal";
import ConfirmModal from "Patterns/ConfirmModal";
import Overlay from "Patterns/Overlay";
import "font-awesome/css/font-awesome.min.css";

import styles from "./index.style.scss";

export const AppMain = ({ match: { path } }) => (
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

AppMain.propTypes = {
  match: PropTypes.object.isRequired
};

export default CSSModules(AppMain, styles);
