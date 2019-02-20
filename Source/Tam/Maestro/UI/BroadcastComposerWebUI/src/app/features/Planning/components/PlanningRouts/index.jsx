import React from "react";
import PropTypes from "prop-types";
import { Switch, Route } from "react-router-dom";
import CSSModules from "react-css-modules";

import AppBody from "Patterns/layout/Body";
import SectionPlanningProposalCreate from "Containers/SectionPlanningProposalCreate";
import SectionPlanningProposal from "Containers/SectionPlanningProposal";
import PlanningHome from "../PlanningHome";

import styles from "./index.style.scss";

export const SectionPlanning = ({ match: { path } }) => (
  <div id="planning-section">
    <AppBody>
      <Switch>
        <Route
          path={`${path}/proposal/create`}
          component={SectionPlanningProposalCreate}
        />
        <Route
          path={`${path}/proposal/:id/version/:version`}
          component={SectionPlanningProposal}
        />
        <Route
          path={`${path}/proposal/:id`}
          component={SectionPlanningProposal}
        />
        <Route path={`${path}`} component={PlanningHome} />
      </Switch>
    </AppBody>
  </div>
);

SectionPlanning.propTypes = {
  match: PropTypes.object.isRequired
};

export default CSSModules(SectionPlanning, styles);
