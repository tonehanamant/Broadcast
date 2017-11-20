import React, { Component } from 'react';
import { connect } from 'react-redux';
import { bindActionCreators } from 'redux';

import ProposalHeader from 'Components/planning/ProposalHeader';

const mapStateToProps = ({ routing }) => ({
  routing,
});

const mapDispatchToProps = dispatch => (
  bindActionCreators({}, dispatch)
);

/* eslint-disable react/prefer-stateless-function */
export class SectionPlanningProposalCreate extends Component {
  render() {
    return (
      <div id="planning-section-proposal">
          <ProposalHeader />
      </div>
    );
  }
}

export default connect(mapStateToProps, mapDispatchToProps)(SectionPlanningProposalCreate);

