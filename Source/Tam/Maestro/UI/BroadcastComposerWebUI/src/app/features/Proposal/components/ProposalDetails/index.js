import React, { Component } from "react";
import PropTypes from "prop-types";

import ProposalDetail from "Proposal/sub-features/Detail";

export default class ProposalDetails extends Component {
  constructor(props) {
    super(props);
    this.state = {};
    this.onUpdateProposal = this.onUpdateProposal.bind(this);
  }

  onUpdateProposal() {
    const { updateProposal } = this.props;
    updateProposal();
  }

  render() {
    const {
      proposalEditForm,
      initialdata,
      updateProposalEditFormDetail,
      deleteProposalDetail,
      toggleModal,
      createAlert,
      modelNewProposalDetail,
      updateProposalEditFormDetailGrid,
      proposalValidationStates
    } = this.props;
    return (
      <div id="proposal-details">
        <h5 style={{ textAlign: "center", color: "#1e5fa8" }}>
          <strong>Proposal Details</strong>
        </h5>
        {proposalEditForm.Details.map(detail => (
          <ProposalDetail
            key={detail.Id}
            proposalEditForm={proposalEditForm}
            detail={detail}
            initialdata={initialdata}
            updateProposalEditFormDetail={updateProposalEditFormDetail}
            updateProposalEditFormDetailGrid={updateProposalEditFormDetailGrid}
            onUpdateProposal={this.onUpdateProposal}
            deleteProposalDetail={deleteProposalDetail}
            toggleModal={toggleModal}
            isReadOnly={this.props.isReadOnly}
            isDirty={this.props.isDirty}
            proposalValidationStates={proposalValidationStates}
            createAlert={createAlert}
          />
        ))}
        <ProposalDetail
          initialdata={initialdata}
          proposalEditForm={proposalEditForm}
          modelNewProposalDetail={modelNewProposalDetail}
          isReadOnly={this.props.isReadOnly}
          isDirty={this.props.isDirty}
        />
      </div>
    );
  }
}

ProposalDetails.defaultProps = {};

ProposalDetails.propTypes = {
  proposalEditForm: PropTypes.object.isRequired,
  initialdata: PropTypes.object.isRequired,
  updateProposalEditFormDetail: PropTypes.func.isRequired,
  updateProposalEditFormDetailGrid: PropTypes.func.isRequired,
  updateProposal: PropTypes.func.isRequired,
  modelNewProposalDetail: PropTypes.func.isRequired,
  deleteProposalDetail: PropTypes.func.isRequired,
  toggleModal: PropTypes.func.isRequired,
  isReadOnly: PropTypes.bool.isRequired,
  isDirty: PropTypes.func.isRequired,

  createAlert: PropTypes.func.isRequired,
  proposalValidationStates: PropTypes.object.isRequired
};
