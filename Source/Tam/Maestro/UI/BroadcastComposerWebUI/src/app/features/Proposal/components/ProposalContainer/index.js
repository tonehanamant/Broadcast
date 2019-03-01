import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";

import _ from "lodash";

import { toggleModal, createAlert } from "Main/redux/index.ducks";

import {
  getProposalLock,
  getProposalUnlock,
  getProposalInitialData,
  getProposal,
  getProposalVersions,
  getProposalVersion,
  updateProposalEditForm,
  updateProposalEditFormDetail,
  updateProposal,
  deleteProposalDetail,
  saveProposal,
  deleteProposal,
  saveProposalAsVersion,
  modelNewProposalDetail,
  updateProposalEditFormDetailGrid,
  unorderProposal,
  generateScx,
  setProposalValidationState
} from "Ducks/planning";

import ProposalHeader from "Proposal/components/ProposalHeader";
import ProposalActions from "Proposal/components/ProposalActions";
import ProposalSwitchVersionModal from "Proposal/components/ProposalSwitchVersionModal";
import ProposalDetails from "Proposal/components/ProposalDetails";

const mapStateToProps = ({
  app: { employee },
  planning: {
    proposalLock,
    initialdata,
    proposal,
    versions,
    proposalEditForm,
    proposalValidationStates
  }
}) => ({
  employee,
  proposalLock,
  initialdata,
  proposal,
  versions,
  proposalEditForm,
  proposalValidationStates
});

const mapDispatchToProps = dispatch =>
  bindActionCreators(
    {
      toggleModal,
      createAlert,
      getProposalLock,
      getProposalUnlock,
      getProposalInitialData,
      getProposal,
      getProposalVersions,
      getProposalVersion,
      updateProposalEditForm,
      updateProposal,
      updateProposalEditFormDetail,
      deleteProposalDetail,
      saveProposal,
      deleteProposal,
      saveProposalAsVersion,
      modelNewProposalDetail,
      updateProposalEditFormDetailGrid,
      unorderProposal,
      setProposalValidationState,
      generateScx
    },
    dispatch
  );

/* eslint-disable react/prefer-stateless-function */
export class SectionPlanningProposal extends Component {
  constructor(props) {
    super(props);
    this.isValidProposalForm = this.isValidProposalForm.bind(this);
    this.isValidProposalDetails = this.isValidProposalDetails.bind(this);
    this.isValidProposalDetailGrids = this.isValidProposalDetailGrids.bind(
      this
    );
    this.isDirty = this.isDirty.bind(this);
    this.isLocked = this.isLocked.bind(this);
    this.onUnload = this.onUnload.bind(this);
  }

  onUnload() {
    this.props.getProposalUnlock(this.props.proposal.Id);
  }

  componentDidMount() {
    window.addEventListener("beforeunload", this.onUnload);
  }

  componentWillUnmount() {
    window.removeEventListener("beforeunload", this.onUnload);
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

  componentDidUpdate() {
    if (this.isLocked()) {
      this.props.toggleModal({
        modal: "confirmModal",
        active: true,
        properties: {
          titleText: "Proposal Locked",
          bodyText: `This Proposal is currently in use by ${
            this.props.proposalLock.LockedUserName
          }. Please try again later.`,
          closeButtonText: "Cancel",
          closeButtonDisabled: true,
          actionButtonText: "Ok",
          actionButtonBsStyle: "primary",
          action: () =>
            window.open(
              `${window.location.origin}/broadcastreact/planning`,
              "_self"
            ),
          dismiss: () =>
            window.open(
              `${window.location.origin}/broadcastreact/planning`,
              "_self"
            )
        }
      });
    }
  }

  isValidProposalForm() {
    const { proposalEditForm } = this.props;
    const { ProposalName, AdvertiserId, GuaranteedDemoId } = proposalEditForm;

    // Proposal Form
    const validProposalName = value => {
      // const alphanumeric = /^[A-Za-z0-9- ]+$/i;
      const valid = {
        required: value !== "" || null,
        // alphaNumeric: (alphanumeric.test(value) || value === ''),
        maxChar100: value && value.length <= 100
      };
      return valid.required && valid.maxChar100;
    };

    return validProposalName(ProposalName) && AdvertiserId && GuaranteedDemoId;
  }

  isValidProposalDetails() {
    const { proposalEditForm } = this.props;
    const { Details } = proposalEditForm;

    // Proposal Details
    const validDetails = [];
    Details.forEach(detail => {
      const validSpothLength = value => {
        const valid = {
          required: value !== null
        };
        return valid.required;
      };

      const validDaypart = value => {
        const valid = {
          required: value !== null
        };
        return valid.required;
      };

      const validDaypartCode = value => {
        const alphanumeric = /^[A-Za-z0-9- ]+$/i;
        const valid = {
          required: value !== "" && value !== null,
          alphaNumeric: alphanumeric.test(value) || value === "",
          maxChar10: value && value.length <= 10
        };
        return valid.required && valid.alphaNumeric && valid.maxChar10;
      };

      const validNti = (value, postType) => {
        const valid = {
          required:
            postType === 2
              ? !isNaN(value) && value !== "" && value !== null
              : true
        };
        return valid.required;
      };

      const validDetail =
        validSpothLength(detail.SpothLengthId) &&
        validNti(detail.NtiConversionFactor, proposalEditForm.PostType) &&
        validDaypart(detail.Daypart) &&
        validDaypartCode(detail.DaypartCode);
      validDetails.push(validDetail);
    });

    return !validDetails.includes(null || false);
  }

  isValidProposalDetailGrids() {
    const { proposalEditForm } = this.props;
    const { Details } = proposalEditForm;

    // Proposal Detail Grids
    let validDetailQuarters = true;
    let validDetailQuarterWeeks = true;

    Details.forEach(detail => {
      const isAdu = detail.Adu;
      detail.Quarters.forEach(quarter => {
        // handle detail ADU
        if (isAdu !== true && quarter.Cpm === 0) validDetailQuarters = false;
        if (quarter.ImpressionGoal === 0) validDetailQuarters = false;
        return quarter.Weeks.forEach(week => {
          if (week.IsHiatus === false && week.Impressions === 0)
            validDetailQuarterWeeks = false;
        });
      });
    });
    return validDetailQuarters && validDetailQuarterWeeks;
  }

  isDirty() {
    return !_.isEqual(this.props.proposalEditForm, this.props.proposal);
  }

  isLocked() {
    const { proposalLock, employee } = this.props;

    const proposalLockResolved = proposalLock && proposalLock.Key;
    const employeeResolved = employee && employee._Accountdomainsid;

    if (proposalLockResolved && proposalLock.Success === true) {
      return false;
    } // Proposal not locked
    if (proposalLockResolved && proposalLock.Success === false) {
      if (
        employeeResolved &&
        employee._Accountdomainsid === proposalLock.LockedUserId
      ) {
        return false; // Proposal locked by user; not locked to user
      }
      return true; // Propsal lock by another user
    }
    return false; // Assume not locked
  }

  render() {
    const {
      toggleModal,
      createAlert,
      initialdata,
      proposal,
      versions,
      getProposalVersions,
      proposalEditForm,
      updateProposalEditForm,
      updateProposal,
      deleteProposalDetail,
      getProposalUnlock,
      saveProposal,
      deleteProposal,
      saveProposalAsVersion,
      updateProposalEditFormDetail,
      modelNewProposalDetail,
      updateProposalEditFormDetailGrid,
      unorderProposal,
      proposalValidationStates,
      setProposalValidationState,
      generateScx
    } = this.props;
    const isReadOnly =
      proposal.Status != null
        ? proposal.Status === 3 || proposal.Status === 4
        : false;
    return (
      <div id="planning-section-proposal" style={{ paddingBottom: 80 }}>
        {this.props.proposalLock.Success &&
          !this.isLocked() &&
          Object.keys(initialdata).length > 0 &&
          Object.keys(proposal).length > 0 &&
          Object.keys(proposalEditForm).length > 0 && (
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
                generateScx={generateScx}
              />
              <ProposalDetails
                proposalEditForm={proposalEditForm}
                initialdata={initialdata}
                toggleModal={toggleModal}
                updateProposalEditFormDetail={updateProposalEditFormDetail}
                updateProposalEditFormDetailGrid={
                  updateProposalEditFormDetailGrid
                }
                updateProposal={updateProposal}
                deleteProposalDetail={deleteProposalDetail}
                modelNewProposalDetail={modelNewProposalDetail}
                isReadOnly={isReadOnly}
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
                getProposalUnlock={getProposalUnlock}
                isCreate={false}
                setProposalValidationState={setProposalValidationState}
                isValidProposalForm={this.isValidProposalForm}
                isValidProposalDetails={this.isValidProposalDetails}
                isValidProposalDetailGrids={this.isValidProposalDetailGrids}
                isDirty={this.isDirty}
                // toggleEditIsciClass={this.props.toggleEditIsciClass}
                // toggleEditGridCellClass={this.props.toggleEditGridCellClass}
              />
            </div>
          )}
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
  proposalEditForm: {}
};

SectionPlanningProposal.propTypes = {
  match: PropTypes.object.isRequired,
  employee: PropTypes.object.isRequired,
  proposalLock: PropTypes.object.isRequired,
  initialdata: PropTypes.object.isRequired,
  proposal: PropTypes.object.isRequired,
  proposalEditForm: PropTypes.object.isRequired,
  versions: PropTypes.array.isRequired,

  proposalValidationStates: PropTypes.object.isRequired,
  setProposalValidationState: PropTypes.func.isRequired,

  // restorePlanningProposal: PropTypes.func.isRequired,

  getProposalLock: PropTypes.func.isRequired,
  getProposalUnlock: PropTypes.func.isRequired,
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
  generateScx: PropTypes.func.isRequired,

  toggleModal: PropTypes.func.isRequired,
  createAlert: PropTypes.func.isRequired
  // toggleEditIsciClass: PropTypes.func.isRequired,
  // toggleEditGridCellClass: PropTypes.func.isRequired,
};

export default connect(
  mapStateToProps,
  mapDispatchToProps
)(SectionPlanningProposal);
