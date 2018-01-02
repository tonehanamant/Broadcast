import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { bindActionCreators } from 'redux';

import { toggleModal, createAlert } from 'Ducks/app';
import { getProposalLock, getProposalInitialData, getProposal, getProposalVersions, getProposalVersion, updateProposalEditForm, updateProposalEditFormDetail, updateProposal, deleteProposalDetail, saveProposal, deleteProposal, saveProposalAsVersion, modelNewProposalDetail, updateProposalEditFormDetailGrid, unorderProposal, setProposalValidationState } from 'Ducks/planning';

import ProposalHeader from 'Components/planning/ProposalHeader';
import ProposalActions from 'Components/planning/ProposalActions';
import ProposalSwitchVersionModal from 'Components/planning/ProposalSwitchVersionModal';
import ProposalDetails from 'Components/planning/ProposalDetails';


const mapStateToProps = ({ planning: { proposalLock, initialdata, proposal, versions, proposalEditForm, proposalValidationStates } }) => ({
  proposalLock,
  initialdata,
  proposal,
  versions,
  proposalEditForm,
  proposalValidationStates,
});

const mapDispatchToProps = dispatch => (
  bindActionCreators({ toggleModal, createAlert, getProposalLock, getProposalInitialData, getProposal, getProposalVersions, getProposalVersion, updateProposalEditForm, updateProposal, updateProposalEditFormDetail, deleteProposalDetail, saveProposal, deleteProposal, saveProposalAsVersion, modelNewProposalDetail, updateProposalEditFormDetailGrid, unorderProposal, setProposalValidationState }, dispatch)
);

/* eslint-disable react/prefer-stateless-function */
export class SectionPlanningProposal extends Component {
  constructor(props) {
    super(props);
    this.isValidProposalForm = this.isValidProposalForm.bind(this);
    this.isValidProposalDetails = this.isValidProposalDetails.bind(this);
    this.isValidProposalDetailGrids = this.isValidProposalDetailGrids.bind(this);
  }

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

  isValidProposalForm() {
    const { proposalEditForm } = this.props;
    const { ProposalName, AdvertiserId } = proposalEditForm;

    // Proposal Form
    const validProposalName = (value) => {
      const alphanumeric = /^[A-Za-z0-9- ]+$/i;
      const valid = {
        required: (value !== '' || null),
        alphaNumeric: (alphanumeric.test(value) || value === ''),
        maxChar100: (value && value.length <= 100),
      };
      return valid.required && valid.alphaNumeric && valid.maxChar100;
    };

    const validAdvertiserId = (value) => {
      const valid = {
        required: value !== null,
      };
      return valid.required;
    };

    return validProposalName(ProposalName) && validAdvertiserId(AdvertiserId);
  }

  isValidProposalDetails() {
    const { proposalEditForm } = this.props;
    const { Details } = proposalEditForm;

    // Proposal Details
    const validDetails = [];
    Details.forEach((detail) => {
      const validSpothLength = (value) => {
        const valid = {
          required: value !== null,
        };
        return valid.required;
      };

      const validDaypart = (value) => {
        const valid = {
          required: value !== null,
        };
        return valid.required;
      };

      const validDaypartCode = (value) => {
        const alphanumeric = /^[A-Za-z0-9- ]+$/i;
        const valid = {
          required: (value !== '' || null),
          alphaNumeric: (alphanumeric.test(value) || value === ''),
          maxChar10: (value && value.length <= 10),
        };
        return valid.required && valid.alphaNumeric && valid.maxChar10;
      };

      const validDetail = validSpothLength(detail.SpothLengthId) && validDaypart(detail.Daypart) && validDaypartCode(detail.DaypartCode);
      validDetails.push(validDetail);
    });
    // console.log('VALID DETAILS', validDetails);

    return !validDetails.includes(null || false);
  }

  isValidProposalDetailGrids() {
    const { proposalEditForm } = this.props;
    const { Details } = proposalEditForm;

    // Proposal Detail Grids
    let validDetailQuarters = true;
    let validDetailQuarterWeeks = true;

    Details.forEach((detail) => {
      detail.Quarters.forEach((quarter) => {
        if (quarter.Cpm === 0) validDetailQuarters = false;
        if (quarter.ImpressionGoal === 0) validDetailQuarters = false;
        return quarter.Weeks.forEach((week) => {
          if (week.IsHiatus === false && week.Impressions === 0) validDetailQuarterWeeks = false;
        });
      });
    });
    // console.log('VALID DETAIL GRIDS', validDetailQuarters, validDetailQuarterWeeks);

    return validDetailQuarters && validDetailQuarterWeeks;
  }


  render() {
    const { toggleModal, createAlert, initialdata, proposal, versions, getProposalVersions, proposalEditForm, updateProposalEditForm, updateProposal, deleteProposalDetail, saveProposal, deleteProposal, saveProposalAsVersion, updateProposalEditFormDetail, modelNewProposalDetail, updateProposalEditFormDetailGrid, unorderProposal, proposalValidationStates, setProposalValidationState } = this.props;
    const isReadOnly = proposal.Status != null ? (proposal.Status === 3 || proposal.Status === 4) : false;
    // console.log('proposal is read only', proposal, isReadOnly);
    return (
      <div id="planning-section-proposal" style={{ paddingBottom: 80 }}>
        {
          // Object.keys(proposalLock).length > 0 &&
          // !proposalLock.LockedUserId &&
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
              unorderProposal={unorderProposal}
              versions={versions}
              isReadOnly={isReadOnly}
              proposalValidationStates={proposalValidationStates}
            />
            <ProposalDetails
              proposalEditForm={proposalEditForm}
              initialdata={initialdata}
              toggleModal={toggleModal}
              updateProposalEditFormDetail={updateProposalEditFormDetail}
              updateProposalEditFormDetailGrid={updateProposalEditFormDetailGrid}
              updateProposal={updateProposal}
              deleteProposalDetail={deleteProposalDetail}
              modelNewProposalDetail={modelNewProposalDetail}
              isReadOnly={isReadOnly}
              proposalValidationStates={proposalValidationStates}
            />
            <ProposalActions
              toggleModal={toggleModal}
              createAlert={createAlert}
              proposal={proposal}
              proposalEditForm={proposalEditForm}
              updateProposalEditForm={updateProposalEditForm}
              saveProposal={saveProposal}
              setProposalValidationState={setProposalValidationState}
              isValidProposalForm={this.isValidProposalForm}
              isValidProposalDetails={this.isValidProposalDetails}
              isValidProposalDetailGrids={this.isValidProposalDetailGrids}
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
SectionPlanningProposal.defaultProps = {
  proposalLock: {},
  initialdata: {},
  proposal: {},
  proposalEditForm: {},
};

SectionPlanningProposal.propTypes = {
  match: PropTypes.object.isRequired,
  // proposalLock: PropTypes.object.isRequired,
  initialdata: PropTypes.object.isRequired,
  proposal: PropTypes.object.isRequired,
  proposalEditForm: PropTypes.object.isRequired,
  versions: PropTypes.array.isRequired,

  proposalValidationStates: PropTypes.object.isRequired,
  setProposalValidationState: PropTypes.func.isRequired,

  getProposalLock: PropTypes.func.isRequired,
  getProposalInitialData: PropTypes.func.isRequired,
  getProposal: PropTypes.func.isRequired,
  getProposalVersions: PropTypes.func.isRequired,
  getProposalVersion: PropTypes.func.isRequired,
  updateProposalEditForm: PropTypes.func.isRequired,

  updateProposalEditFormDetail: PropTypes.func.isRequired,
  updateProposalEditFormDetailGrid: PropTypes.func.isRequired,
  updateProposal: PropTypes.func.isRequired,
  deleteProposalDetail: PropTypes.func.isRequired,
  saveProposal: PropTypes.func.isRequired,
  deleteProposal: PropTypes.func.isRequired,
  saveProposalAsVersion: PropTypes.func.isRequired,
  unorderProposal: PropTypes.func.isRequired,
  modelNewProposalDetail: PropTypes.func.isRequired,

  toggleModal: PropTypes.func.isRequired,
  createAlert: PropTypes.func.isRequired,
};

export default connect(mapStateToProps, mapDispatchToProps)(SectionPlanningProposal);

