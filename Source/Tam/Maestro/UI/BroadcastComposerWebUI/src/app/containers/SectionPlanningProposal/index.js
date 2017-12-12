import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { bindActionCreators } from 'redux';

import { toggleModal } from 'Ducks/app';
import { getProposalLock, getProposalInitialData, getProposal, getProposalVersions, getProposalVersion, updateProposalEditForm, updateProposalEditFormDetail, deleteProposalDetail, saveProposal, deleteProposal, saveProposalAsVersion, modelNewProposalDetail } from 'Ducks/planning';

import ProposalHeader from 'Components/planning/ProposalHeader';
import ProposalActions from 'Components/planning/ProposalActions';
import ProposalSwitchVersionModal from 'Components/planning/ProposalSwitchVersionModal';
import ProposalDetails from 'Components/planning/ProposalDetails';


const mapStateToProps = ({ planning: { proposalLock }, planning: { initialdata }, planning: { proposal }, planning: { versions }, planning: { proposalEditForm } }) => ({
  proposalLock,
  initialdata,
  proposal,
  versions,
  proposalEditForm,
});

const mapDispatchToProps = dispatch => (
  bindActionCreators({ toggleModal, getProposalLock, getProposalInitialData, getProposal, getProposalVersions, getProposalVersion, updateProposalEditForm, updateProposalEditFormDetail, deleteProposalDetail, saveProposal, deleteProposal, saveProposalAsVersion, modelNewProposalDetail }, dispatch)
);

/* eslint-disable react/prefer-stateless-function */
export class SectionPlanningProposal extends Component {
  componentWillMount() {
    const id = this.props.match.params.id;
    const version = this.props.match.params.version;

    this.props.getProposalLock(id);

    this.props.getProposalInitialData();

    if (id && version) {
      this.props.getProposalVersion(id, version);
    } else if (id) {
      this.props.getProposal(id);
    }
  }

  render() {
    const { toggleModal, proposalLock, initialdata, proposal, versions, getProposalVersions, proposalEditForm, updateProposalEditForm, deleteProposalDetail, saveProposal, deleteProposal, saveProposalAsVersion, updateProposalEditFormDetail, modelNewProposalDetail } = this.props;
    return (
      <div id="planning-section-proposal" style={{ paddingBottom: 80 }}>
        {
          Object.keys(proposalLock).length > 0 &&
          !proposalLock.LockedUserId &&
          Object.keys(initialdata).length > 0 &&
          Object.keys(proposal).length > 0 &&
          Object.keys(proposalEditForm).length > 0 &&

          <div id="proposal-body">
            <ProposalSwitchVersionModal
              toggleModal={toggleModal}
              initialdata={initialdata}
              proposal={proposal}
              versions={versions}
            />
            <ProposalHeader
              isEdit
              toggleModal={toggleModal}
              initialdata={initialdata}
              proposal={proposal}
              proposalEditForm={proposalEditForm}
              updateProposalEditForm={updateProposalEditForm}
              getProposalVersions={getProposalVersions}
              deleteProposal={deleteProposal}
              saveProposalAsVersion={saveProposalAsVersion}
              versions={versions}
            />
            <ProposalDetails
              proposalEditForm={proposalEditForm}
              initialdata={initialdata}
              toggleModal={toggleModal}
              updateProposalEditFormDetail={updateProposalEditFormDetail}
              deleteProposalDetail={deleteProposalDetail}
              modelNewProposalDetail={modelNewProposalDetail}
            />
            <ProposalActions
              toggleModal={toggleModal}
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
SectionPlanningProposal.propTypes = {
  match: PropTypes.object.isRequired,
  proposalLock: PropTypes.object.isRequired,
  initialdata: PropTypes.object.isRequired,
  proposal: PropTypes.object.isRequired,
  proposalEditForm: PropTypes.object.isRequired,
  versions: PropTypes.array.isRequired,

  getProposalLock: PropTypes.func.isRequired,
  getProposalInitialData: PropTypes.func.isRequired,
  getProposal: PropTypes.func.isRequired,
  getProposalVersions: PropTypes.func.isRequired,
  getProposalVersion: PropTypes.func.isRequired,
  updateProposalEditForm: PropTypes.func.isRequired,

  updateProposalEditFormDetail: PropTypes.func.isRequired,
  deleteProposalDetail: PropTypes.func.isRequired,
  saveProposal: PropTypes.func.isRequired,
  deleteProposal: PropTypes.func.isRequired,
  saveProposalAsVersion: PropTypes.func.isRequired,
  modelNewProposalDetail: PropTypes.func.isRequired,

  toggleModal: PropTypes.func.isRequired,
};

export default connect(mapStateToProps, mapDispatchToProps)(SectionPlanningProposal);

