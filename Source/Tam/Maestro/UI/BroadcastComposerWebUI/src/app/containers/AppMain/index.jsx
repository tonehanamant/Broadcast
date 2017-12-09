import React from 'react';
import PropTypes from 'prop-types';
import { Switch, Route } from 'react-router-dom';
import CSSModules from 'react-css-modules';

import AppHeader from 'Components/header/AppHeader';
import AppFooter from 'Components/footer/AppFooter';
// import SectionHome from 'Containers/SectionHome';
import SectionPost from 'Containers/SectionPost';
import SectionPlanning from 'Containers/SectionPlanning';
// import SectionRates from 'Containers/SectionRates';
import Toast from 'Components/shared/Toast';
import ErrorModal from 'Components/shared/ErrorModal';
import ConfirmModal from 'Components/shared/ConfirmModal';
import Overlay from 'Components/shared/Overlay';

import styles from './index.style.scss';

/* eslint-disable no-unused-vars */
export const AppMain = ({ match: { path } }) => (
  <div styleName="main-container">
    <Toast />
    <ErrorModal />
    <ConfirmModal />
    <AppHeader />
    <Overlay type="loading" />
    <Overlay type="processing" />
    <Switch>
      <Route exact path={path} component={SectionPost} />
      <Route path={`${path}/post`} component={SectionPost} />
      <Route path={`${path}/planning`} component={SectionPlanning} />
    </Switch>
    <AppFooter />
  </div>
);

AppMain.propTypes = {
  match: PropTypes.object.isRequired,
};

export default CSSModules(AppMain, styles);
