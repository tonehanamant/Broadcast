import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { bindActionCreators } from "react-redux/node_modules/redux";
import { getProposals } from "Ducks/planning";
import PageTitle from "Patterns/PageTitle";
import PlanningGrid from "../PlanningGrid";

const mapStateToProps = () => ({});

const mapDispatchToProps = dispatch =>
  bindActionCreators({ getProposals }, dispatch);

export class SectionPlanningHome extends Component {
  componentWillMount() {
    const { getProposals } = this.props;
    getProposals();
  }

  render() {
    return (
      <div id="planning-section-proposal">
        <PageTitle title="Planning" />
        <PlanningGrid />
      </div>
    );
  }
}

SectionPlanningHome.propTypes = {
  getProposals: PropTypes.func.isRequired
};

export default connect(
  mapStateToProps,
  mapDispatchToProps
)(SectionPlanningHome);
