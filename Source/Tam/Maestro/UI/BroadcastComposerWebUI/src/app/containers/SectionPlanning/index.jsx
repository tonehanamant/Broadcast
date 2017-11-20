import React from 'react';
import PropTypes from 'prop-types';
import { Switch, Route } from 'react-router-dom';
import CSSModules from 'react-css-modules';

import AppBody from 'Components/body/AppBody';
import SectionPlanningHome from 'Containers/SectionPlanningHome';
import SectionPlanningProposalCreate from 'Containers/SectionPlanningProposalCreate';
import SectionPlanningProposal from 'Containers/SectionPlanningProposal';

import styles from './index.style.scss';

export const SectionPlanning = ({ match: { path } }) => (
  <div id="planning-section">
    <AppBody>
      <Switch>
        <Route path={`${path}/proposal/create`} component={SectionPlanningProposalCreate} />
        <Route path={`${path}/proposal/:id`} component={SectionPlanningProposal} />
        <Route path={`${path}`} component={SectionPlanningHome} />
      </Switch>
    </AppBody>
  </div>
);

SectionPlanning.propTypes = {
  match: PropTypes.object.isRequired,
};

export default CSSModules(SectionPlanning, styles);
