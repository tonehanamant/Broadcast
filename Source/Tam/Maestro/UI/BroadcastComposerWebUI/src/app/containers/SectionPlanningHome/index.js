import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import { getProposals } from "Ducks/planning";
import PageTitle from "Components/shared/PageTitle";
import PageHeaderContainer from "Components/planning/PageHeaderContainer";
import PlanningGrid from "Components/planning/PlanningGrid";

const mapStateToProps = () => ({});

const mapDispatchToProps = dispatch =>
  bindActionCreators({ getProposals }, dispatch);

export class SectionPlanningHome extends Component {
  constructor(props) {
    super(props);
    console.log(this);
  }

  componentWillMount() {
    this.props.getProposals();
  }

  render() {
    return (
      <div id="planning-section-proposal">
        <PageTitle title="Planning" />
        <PageHeaderContainer />
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
