import React, { Component } from "react";
import PropTypes from "prop-types";

import {
  Row,
  Col,
  Form,
  FormGroup,
  ControlLabel,
  DropdownButton,
  MenuItem
} from "react-bootstrap";
import Select from "react-select";

export default class ProposalHeaderActions extends Component {
  constructor(props) {
    super(props);
    this.state = {};
    this.onChangeStatus = this.onChangeStatus.bind(this);
    this.onSaveVersion = this.onSaveVersion.bind(this);
    this.onSwitchVersions = this.onSwitchVersions.bind(this);
    this.onDeleteProposal = this.onDeleteProposal.bind(this);
    this.onUnorder = this.onUnorder.bind(this);
    this.onGenerateSCX = this.onGenerateSCX.bind(this);
  }

  onChangeStatus(value) {
    const { updateProposalEditForm } = this.props;
    updateProposalEditForm({
      key: "Status",
      value: value ? value.Id : null
    });
  }

  onSaveVersion() {
    const { saveProposalAsVersion, proposalEditForm } = this.props;
    saveProposalAsVersion(proposalEditForm);
  }

  onSwitchVersions() {
    const { getProposalVersions, proposalEditForm } = this.props;
    getProposalVersions(proposalEditForm.Id);
  }

  onDeleteProposal() {
    const { proposalEditForm, toggleModal, deleteProposal } = this.props;
    toggleModal({
      modal: "confirmModal",
      active: true,
      properties: {
        titleText: "Delete Proposal",
        bodyText:
          "Are you sure you want to delete this proposal? Any reserved inventory will be lost.",
        closeButtonText: "Cancel",
        actionButtonText: "Continue",
        actionButtonBsStyle: "danger",
        action: () => deleteProposal(proposalEditForm.Id),
        dismiss: () => {}
      }
    });
  }

  onUnorder() {
    const { proposalEditForm, toggleModal, unorderProposal } = this.props;
    toggleModal({
      modal: "confirmModal",
      active: true,
      properties: {
        titleText: "Unorder Proposal",
        bodyText:
          "Operation will Archive contracted version of the proposal and create new version for editing. Select Continue to complete. Select Cancel to cancel.",
        closeButtonText: "Cancel",
        actionButtonText: "Continue",
        actionButtonBsStyle: "warning",
        action: () => unorderProposal(proposalEditForm.Id),
        dismiss: () => {}
      }
    });
  }

  onGenerateSCX() {
    const { proposal, generateScx } = this.props;
    const detailIds = proposal.Details.map(dt => dt.Id);
    generateScx(detailIds, false);
  }

  render() {
    const { initialdata, proposalEditForm, isReadOnly } = this.props;
    const copyStatuses = [...initialdata.Statuses];
    const statusOptions =
      proposalEditForm.Status !== 4
        ? copyStatuses.filter(item => item.Id !== 4)
        : copyStatuses;

    return (
      <Row>
        <Col md={10}>
          <Form horizontal id="proposal-header-actions">
            <FormGroup controlId="proposalStatus" style={{ marginBottom: 0 }}>
              <Col componentClass={ControlLabel} sm={2}>
                <strong>Status</strong>
              </Col>
              <Col sm={10}>
                <Select
                  name="proposalStatus"
                  disabled={isReadOnly}
                  value={proposalEditForm.Status}
                  options={statusOptions}
                  labelKey="Display"
                  valueKey="Id"
                  onChange={this.onChangeStatus}
                  clearable={false}
                />
              </Col>
            </FormGroup>
          </Form>
        </Col>
        <Col md={2}>
          <div style={{ float: "right" }}>
            {!isReadOnly && (
              <DropdownButton
                bsStyle="success"
                title={
                  <span
                    className="glyphicon glyphicon-option-horizontal"
                    aria-hidden="true"
                  />
                }
                noCaret
                pullRight
                id="header_actions"
              >
                <MenuItem eventKey="1" onClick={this.onSaveVersion}>
                  Save As Version
                </MenuItem>
                <MenuItem eventKey="2" onClick={this.onSwitchVersions}>
                  Switch Version
                </MenuItem>
                <MenuItem eventKey="3" onClick={this.onDeleteProposal}>
                  Delete Proposal
                </MenuItem>
              </DropdownButton>
            )}
            {isReadOnly && (
              <DropdownButton
                bsStyle="success"
                title={
                  <span
                    className="glyphicon glyphicon-option-horizontal"
                    aria-hidden="true"
                  />
                }
                noCaret
                pullRight
                id="header_actions"
              >
                <MenuItem eventKey="1" onClick={this.onSwitchVersions}>
                  Switch Version
                </MenuItem>
                <MenuItem eventKey="2" onClick={this.onUnorder}>
                  Unorder
                </MenuItem>
                <MenuItem eventKey="3" onClick={this.onGenerateSCX}>
                  Generate SCX
                </MenuItem>
              </DropdownButton>
            )}
          </div>
        </Col>
      </Row>
    );
  }
}

ProposalHeaderActions.defaultProps = {
  // isReadOnly: false,
};

ProposalHeaderActions.propTypes = {
  proposal: PropTypes.object.isRequired,
  initialdata: PropTypes.object.isRequired,
  proposalEditForm: PropTypes.object.isRequired,
  updateProposalEditForm: PropTypes.func.isRequired,
  getProposalVersions: PropTypes.func.isRequired,
  deleteProposal: PropTypes.func.isRequired,
  saveProposalAsVersion: PropTypes.func.isRequired,
  unorderProposal: PropTypes.func.isRequired,
  toggleModal: PropTypes.func.isRequired,
  generateScx: PropTypes.func.isRequired,
  isReadOnly: PropTypes.bool.isRequired
};
