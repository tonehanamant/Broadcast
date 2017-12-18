/* eslint-disable no-unused-vars */
import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { bindActionCreators } from 'redux';

import { toggleModal, createAlert } from 'Ducks/app';
import { getProposalInitialData, getProposal, updateProposalEditForm, updateProposalEditFormDetail, updateProposal, deleteProposalDetail, saveProposal, modelNewProposalDetail } from 'Ducks/planning';

import ProposalHeader from 'Components/planning/ProposalHeader';
import ProposalActions from 'Components/planning/ProposalActions';
import ProposalDetails from 'Components/planning/ProposalDetails';


const mapStateToProps = ({ planning: { initialdata }, planning: { proposal }, planning: { proposalEditForm } }) => ({
  initialdata,
  proposal,
  proposalEditForm,
});

const mapDispatchToProps = dispatch => (
  bindActionCreators({ toggleModal, createAlert, getProposalInitialData, getProposal, updateProposalEditForm, updateProposal, updateProposalEditFormDetail, deleteProposalDetail, saveProposal, modelNewProposalDetail }, dispatch)
);

/* eslint-disable react/prefer-stateless-function */
export class SectionPlanningProposalCreate extends Component {
  componentWillMount() {
    this.props.getProposalInitialData();
  }

  render() {
    const { toggleModal, createAlert, initialdata, proposal, proposalEditForm, updateProposalEditForm, updateProposal, deleteProposalDetail, saveProposal, updateProposalEditFormDetail, modelNewProposalDetail } = this.props;
    return (
      <div id="planning-section-proposal">
        {
          // Object.keys(proposalLock).length > 0 &&
          // !proposalLock.LockedUserId &&
          Object.keys(initialdata).length > 0 &&
          Object.keys(proposal).length > 0 &&
          Object.keys(proposalEditForm).length > 0 &&
          <div id="proposal-body">
            <ProposalHeader
              isEdit={false}
              toggleModal={toggleModal}
              initialdata={initialdata}
              proposal={proposal}
              proposalEditForm={proposalEditForm}
              updateProposalEditForm={updateProposalEditForm}
            />
            <ProposalDetails
              proposalEditForm={proposalEditForm}
              initialdata={initialdata}
              updateProposalEditFormDetail={updateProposalEditFormDetail}
              updateProposal={updateProposal}
              deleteProposalDetail={deleteProposalDetail}
              modelNewProposalDetail={modelNewProposalDetail}
              toggleModal={toggleModal}
            />
            <ProposalActions
              toggleModal={toggleModal}
              createAlert={createAlert}
              proposal={proposal}
              proposalEditForm={proposalEditForm}
              updateProposalEditForm={updateProposalEditForm}
              saveProposal={saveProposal}
            />
          </div>
        }
      </div>
    );
  }
}

/* ////////////////////////////////// */
/* // PROPTYPES
/* ////////////////////////////////// */
SectionPlanningProposalCreate.defaultProps = {
  proposalLock: {},
  initialdata: {},
  proposal: {},
  proposalEditForm: {},
};

SectionPlanningProposalCreate.propTypes = {
  initialdata: PropTypes.object.isRequired,
  proposal: PropTypes.object.isRequired,
  proposalEditForm: PropTypes.object.isRequired,

  getProposalInitialData: PropTypes.func.isRequired,
  updateProposalEditForm: PropTypes.func.isRequired,

  updateProposalEditFormDetail: PropTypes.func.isRequired,
  updateProposal: PropTypes.func.isRequired,
  deleteProposalDetail: PropTypes.func.isRequired,
  saveProposal: PropTypes.func.isRequired,
  modelNewProposalDetail: PropTypes.func.isRequired,

  toggleModal: PropTypes.func.isRequired,
  createAlert: PropTypes.func.isRequired,
};

export default connect(mapStateToProps, mapDispatchToProps)(SectionPlanningProposalCreate);

