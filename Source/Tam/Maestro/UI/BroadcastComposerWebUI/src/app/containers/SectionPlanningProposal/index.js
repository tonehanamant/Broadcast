import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { bindActionCreators } from 'redux';

// import { toggleModal, createAlert, setOverlayLoading } from 'Ducks/app';
import { getProposalInitialData, getProposal, getProposalVersions, getProposalVersion } from 'Ducks/planning';

import ProposalHeader from 'Components/planning/ProposalHeader';

const mapStateToProps = ({ planning: { initialdata }, planning: { proposal }, planning: { versions } }) => ({
  initialdata,
  proposal,
  versions,
});

const mapDispatchToProps = dispatch => (
  bindActionCreators({ getProposalInitialData, getProposal, getProposalVersions, getProposalVersion }, dispatch)
);

/* eslint-disable react/prefer-stateless-function */
export class SectionPlanningProposal extends Component {
  componentWillMount() {
    const id = this.props.match.params.id;
    const version = this.props.match.params.version;
    console.log('PARAMS', this.props.match, 'ID', this.props.match.params.version, 'VERSION', this.props.match.params.version);

    this.props.getProposalInitialData();

    if (id && version) {
      this.props.getProposalVersion(id, version);
      this.props.getProposalVersions(id);
    } else if (id) {
      this.props.getProposal(id);
      this.props.getProposalVersions(id);
    }
  }

  render() {
    return (
      <div id="planning-section-proposal">
          <ProposalHeader />
      </div>
    );
  }
}

/* ////////////////////////////////// */
/* // PROPTYPES
/* ////////////////////////////////// */
SectionPlanningProposal.propTypes = {
  match: PropTypes.object.isRequired,
  // initialdata: PropTypes.object.isRequired,
  // proposal: PropTypes.object.isRequired,
  // versions: PropTypes.object.isRequired,

  getProposalInitialData: PropTypes.func.isRequired,
  getProposal: PropTypes.func.isRequired,
  getProposalVersions: PropTypes.func.isRequired,
  getProposalVersion: PropTypes.func.isRequired,
};

export default connect(mapStateToProps, mapDispatchToProps)(SectionPlanningProposal);

