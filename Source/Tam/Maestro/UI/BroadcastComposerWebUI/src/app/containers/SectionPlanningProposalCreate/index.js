/* eslint-disable no-unused-vars */
import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { bindActionCreators } from 'redux';

import _ from 'lodash';

import { toggleModal, createAlert } from 'Ducks/app';
import { getProposalInitialData, getProposal, updateProposalEditForm, updateProposalEditFormDetail, updateProposalEditFormDetailGrid, updateProposal, deleteProposalDetail, saveProposal, modelNewProposalDetail, setProposalValidationState } from 'Ducks/planning';

import ProposalHeader from 'Components/planning/ProposalHeader';
import ProposalActions from 'Components/planning/ProposalActions';
import ProposalDetails from 'Components/planning/ProposalDetails';
// import { toggleEditIsciClass, toggleEditGridCellClass } from '../../ducks/planning';


const mapStateToProps = ({ planning: { initialdata, proposal, proposalEditForm, proposalValidationStates } }) => ({
  initialdata,
  proposal,
  proposalEditForm,
  proposalValidationStates,
});

const mapDispatchToProps = dispatch => (
  bindActionCreators({ toggleModal, createAlert, getProposalInitialData, getProposal, updateProposalEditForm, updateProposal, updateProposalEditFormDetail, updateProposalEditFormDetailGrid, deleteProposalDetail, saveProposal, modelNewProposalDetail, setProposalValidationState }, dispatch)
);

/* eslint-disable react/prefer-stateless-function */
export class SectionPlanningProposalCreate extends Component {
  constructor(props) {
    super(props);
    this.isValidProposalForm = this.isValidProposalForm.bind(this);
    this.isValidProposalDetails = this.isValidProposalDetails.bind(this);
    this.isValidProposalDetailGrids = this.isValidProposalDetailGrids.bind(this);
    this.isDirty = this.isDirty.bind(this);
  }

  componentWillMount() {
    this.props.getProposalInitialData();
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
          required: value !== '' && value !== null,
          alphaNumeric: (alphanumeric.test(value) || value === ''),
          maxChar10: (value && value.length <= 10),
        };
        return valid.required && valid.alphaNumeric && valid.maxChar10;
      };

      const validNti = (value) => {
        const valid = {
          required: !isNaN(value),
        };
        return valid.required;
      };

      const validDetail = validSpothLength(detail.SpothLengthId) && validNti(detail.NtiConversionFactor) && validDaypart(detail.Daypart) && validDaypartCode(detail.DaypartCode);
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
      const isAdu = detail.Adu;
      detail.Quarters.forEach((quarter) => {
        // handle detail ADU
        if (isAdu !== true && quarter.Cpm === 0) validDetailQuarters = false;
        if (quarter.ImpressionGoal === 0) validDetailQuarters = false;
        return quarter.Weeks.forEach((week) => {
          if (week.IsHiatus === false && week.Impressions === 0) validDetailQuarterWeeks = false;
        });
      });
    });
    // console.log('VALID DETAIL GRIDS', validDetailQuarters, validDetailQuarterWeeks);

    return validDetailQuarters && validDetailQuarterWeeks;
  }

  isDirty() {
    return !_.isEqual(this.props.proposalEditForm, this.props.proposal);
  }

  render() {
    const { toggleModal, createAlert, initialdata, proposal, proposalEditForm, updateProposalEditForm, updateProposal, deleteProposalDetail, saveProposal, updateProposalEditFormDetail, updateProposalEditFormDetailGrid, modelNewProposalDetail, proposalValidationStates, setProposalValidationState } = this.props;
    const isCreate = true;
    return (
      <div id="planning-section-proposal" style={{ paddingBottom: 80 }}>
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
              proposalValidationStates={proposalValidationStates}
              setProposalValidationState={setProposalValidationState}
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
              isReadOnly={false}
              isDirty={this.isDirty}
              proposalValidationStates={proposalValidationStates}
              createAlert={createAlert}
            />
            <ProposalActions
              toggleModal={toggleModal}
              createAlert={createAlert}
              proposal={proposal}
              proposalEditForm={proposalEditForm}
              updateProposalEditForm={updateProposalEditForm}
              saveProposal={saveProposal}
              isCreate
              setProposalValidationState={setProposalValidationState}
              isValidProposalForm={this.isValidProposalForm}
              isValidProposalDetails={this.isValidProposalDetails}
              isValidProposalDetailGrids={this.isValidProposalDetailGrids}
              isDirty={this.isDirty}
              // toggleEditIsciClass={this.props.toggleEditIsciClass}
              // toggleEditGridCellClass={this.props.toggleEditGridCellClass}
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

  proposalValidationStates: PropTypes.object.isRequired,
  setProposalValidationState: PropTypes.func.isRequired,

  getProposalInitialData: PropTypes.func.isRequired,
  updateProposalEditForm: PropTypes.func.isRequired,

  updateProposalEditFormDetail: PropTypes.func.isRequired,
  updateProposalEditFormDetailGrid: PropTypes.func.isRequired,
  updateProposal: PropTypes.func.isRequired,
  deleteProposalDetail: PropTypes.func.isRequired,
  saveProposal: PropTypes.func.isRequired,
  modelNewProposalDetail: PropTypes.func.isRequired,

  toggleModal: PropTypes.func.isRequired,
  createAlert: PropTypes.func.isRequired,
  // toggleEditIsciClass: PropTypes.func.isRequired,
  // toggleEditGridCellClass: PropTypes.func.isRequired,
};

export default connect(mapStateToProps, mapDispatchToProps)(SectionPlanningProposalCreate);

