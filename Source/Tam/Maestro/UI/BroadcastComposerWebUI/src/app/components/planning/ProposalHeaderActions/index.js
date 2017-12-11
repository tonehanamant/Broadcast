import React, { Component } from 'react';
import PropTypes from 'prop-types';

import { Row, Col, Form, FormGroup, ControlLabel, DropdownButton, MenuItem } from 'react-bootstrap';
import Select from 'react-select';

export default class ProposalHeaderActions extends Component {
  constructor(props) {
    super(props);
    this.state = {};
    this.onChangeStatus = this.onChangeStatus.bind(this);
    this.onSaveVersion = this.onSaveVersion.bind(this);
    this.onSwitchVersions = this.onSwitchVersions.bind(this);
    this.onDeleteProposal = this.onDeleteProposal.bind(this);
  }

  onChangeStatus(value) {
    this.props.updateProposalEditForm({ key: 'Status', value: value ? value.Id : null });
  }

  onSaveVersion() {
    this.props.saveProposalAsVersion(this.props.proposalEditForm);
  }

  onSwitchVersions() {
    this.props.getProposalVersions(this.props.proposalEditForm.Id);
  }

  onDeleteProposal() {
    this.props.toggleModal({
      modal: 'confirmModal',
      active: true,
      properties: {
        titleText: 'Delete Proposal',
        bodyText: 'Are you sure you want to delete this proposal? Any reserved inventory will be lost.',
        closeButtonText: 'Cancel',
        actionButtonText: 'Continue',
        actionButtonBsStyle: 'danger',
        action: () => this.props.deleteProposal(this.props.proposalEditForm.Id),
      },
    });
  }

  render() {
    // console.log('ProposalHeaderActions', this.props);
    const { initialdata, proposalEditForm } = this.props;
    const copyStatuses = [...initialdata.Statuses];
    const statusOptions = (proposalEditForm.Status !== 4) ? copyStatuses.filter(item => item.Id !== 4) : copyStatuses;
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
                  value={proposalEditForm.Status}
                  // placeholder=""
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
          <div style={{ float: 'right' }}>
            { this.props.proposalEditForm.Status !== 3 &&
              <DropdownButton bsStyle="success" title={<span className="glyphicon glyphicon-option-horizontal" aria-hidden="true" />} noCaret pullRight id="header_actions">
                  <MenuItem eventKey="1" onClick={this.onSaveVersion}>Save As Version</MenuItem>
                  <MenuItem eventKey="2" onClick={this.onSwitchVersions}>Switch Version</MenuItem>
                  <MenuItem eventKey="3" onClick={this.onDeleteProposal}>Delete Proposal</MenuItem>

              </DropdownButton>
            }
            { this.props.proposalEditForm.Status === 3 &&
              <DropdownButton bsStyle="success" title={<span className="glyphicon glyphicon-option-horizontal" aria-hidden="true" />} noCaret pullRight id="header_actions">
                  <MenuItem eventKey="1" onClick={this.onSwitchVersions}>Save As Version</MenuItem>
                  <MenuItem eventKey="2" onClick={this.onUnorder}>Unorder</MenuItem>
                  <MenuItem eventKey="3" onClick={this.onGenerateSCX}>Generate SCX</MenuItem>
              </DropdownButton>
            }
          </div>
        </Col>
      </Row>
    );
  }
}

ProposalHeaderActions.defaultProps = {
};

/* eslint-disable react/no-unused-prop-types */
ProposalHeaderActions.propTypes = {
  initialdata: PropTypes.object.isRequired,
  proposalEditForm: PropTypes.object.isRequired,
  updateProposalEditForm: PropTypes.func.isRequired,
  getProposalVersions: PropTypes.func.isRequired,
  deleteProposal: PropTypes.func.isRequired,
  saveProposalAsVersion: PropTypes.func.isRequired,
  toggleModal: PropTypes.func.isRequired,
};
