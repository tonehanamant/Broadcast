import React, { Component } from "react";
import PropTypes from "prop-types";

import { ButtonToolbar, Button, Row, Col } from "react-bootstrap";

export default class ProposalActions extends Component {
  constructor(props) {
    super(props);
    this.state = {};
    this.checkValid = this.checkValid.bind(this);
    this.save = this.save.bind(this);
    this.cancel = this.cancel.bind(this);
  }

  componentDidUpdate() {
    const {
      proposalEditForm,
      toggleModal,
      updateProposalEditForm,
      saveProposal
    } = this.props;
    if (
      proposalEditForm.ValidationWarning &&
      proposalEditForm.ValidationWarning.HasWarning
    ) {
      toggleModal({
        modal: "confirmModal",
        active: true,
        properties: {
          titleText: "Warning",
          bodyText: proposalEditForm.ValidationWarning.Message,
          closeButtonText: "Cancel",
          actionButtonText: "Continue",
          actionButtonBsStyle: "warning",
          action: () => {
            updateProposalEditForm({
              key: "ForceSave",
              value: true
            });
            updateProposalEditForm({
              key: "ValidationWarning",
              value: null
            });
            saveProposal({
              proposal: proposalEditForm,
              force: true
            });
          },
          dismiss: () => {}
        }
      });
    }
  }

  checkValid() {
    const {
      isValidProposalForm,
      isValidProposalDetails,
      isValidProposalDetailGrids,
      setProposalValidationState
    } = this.props;
    const formValid = isValidProposalForm();
    const detailValid = isValidProposalDetails();
    const detailGridsValid = isValidProposalDetailGrids();

    setProposalValidationState({
      type: "FormInvalid",
      state: !formValid
    });
    setProposalValidationState({
      type: "DetailInvalid",
      state: !detailValid
    });
    setProposalValidationState({
      type: "DetailGridsInvalid",
      state: !detailGridsValid
    });

    return formValid && detailValid && detailGridsValid;
  }

  save() {
    const { proposalEditForm, saveProposal, createAlert } = this.props;
    if (this.checkValid()) {
      saveProposal({ proposal: proposalEditForm });
    } else {
      createAlert({
        type: "danger",
        headline: "Proposal Cannot Be Saved",
        message: "Required Inputs Incomplete (in red)"
      });
    }
  }

  cancel() {
    const { isCreate, getProposalUnlock, proposal } = this.props;
    if (!isCreate) {
      getProposalUnlock(proposal.Id);
    }
    setTimeout(() => {
      window.location = "/broadcastreact/planning";
    }, 500);
  }

  render() {
    return (
      <div id="proposal-actions">
        <Row>
          <Col md={12}>
            <hr />
            <ButtonToolbar style={{ float: "right" }}>
              <Button bsStyle="default" onClick={this.cancel}>
                Cancel
              </Button>
              <Button bsStyle="success" onClick={this.save}>
                Save
              </Button>
            </ButtonToolbar>
          </Col>
        </Row>
      </div>
    );
  }
}

ProposalActions.defaultProps = {
  getProposalUnlock: () => {}
};

ProposalActions.propTypes = {
  proposal: PropTypes.object.isRequired,
  proposalEditForm: PropTypes.object.isRequired,
  updateProposalEditForm: PropTypes.func.isRequired,
  saveProposal: PropTypes.func.isRequired,
  getProposalUnlock: PropTypes.func,
  isCreate: PropTypes.bool.isRequired,
  toggleModal: PropTypes.func.isRequired,
  createAlert: PropTypes.func.isRequired,
  setProposalValidationState: PropTypes.func.isRequired,
  isValidProposalForm: PropTypes.func.isRequired,
  isValidProposalDetails: PropTypes.func.isRequired,
  isValidProposalDetailGrids: PropTypes.func.isRequired
};
