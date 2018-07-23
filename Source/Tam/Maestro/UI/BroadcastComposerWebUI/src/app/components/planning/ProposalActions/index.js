import React, { Component } from 'react';
import PropTypes from 'prop-types';

import { ButtonToolbar, Button, Row, Col } from 'react-bootstrap';


export default class ProposalActions extends Component {
  constructor(props) {
    super(props);
		this.state = {};
		this.checkValid = this.checkValid.bind(this);
    this.save = this.save.bind(this);
    this.cancel = this.cancel.bind(this);
	}

	componentDidUpdate() {
		if (this.props.proposalEditForm.ValidationWarning && this.props.proposalEditForm.ValidationWarning.HasWarning) {
			this.props.toggleModal({
					modal: 'confirmModal',
					active: true,
					properties: {
						titleText: 'Warning',
						bodyText: this.props.proposalEditForm.ValidationWarning.Message,
						closeButtonText: 'Cancel',
						actionButtonText: 'Continue',
						actionButtonBsStyle: 'warning',
						action: () => {
							this.props.updateProposalEditForm({ key: 'ForceSave', value: true });
							this.props.updateProposalEditForm({ key: 'ValidationWarning', value: null });
							this.props.saveProposal({ proposal: this.props.proposalEditForm, force: true });
						},
						dismiss: () => {},
					},
			});
		}
	}

	checkValid() {
		const formValid = this.props.isValidProposalForm();
		const detailValid = this.props.isValidProposalDetails();
		const detailGridsValid = this.props.isValidProposalDetailGrids();

		this.props.setProposalValidationState({ type: 'FormInvalid', state: !formValid });
		this.props.setProposalValidationState({ type: 'DetailInvalid', state: !detailValid });
		this.props.setProposalValidationState({ type: 'DetailGridsInvalid', state: !detailGridsValid });

		return formValid && detailValid && detailGridsValid;
	}

	save() {
		if (this.checkValid()) {
			this.props.saveProposal({ proposal: this.props.proposalEditForm });
			/* this.props.toggleEditIsciClass(false);
			this.props.toggleEditGridCellClass(false); */
		} else {
			this.props.createAlert({
				type: 'danger',
				headline: 'Proposal Cannot Be Saved',
				message: 'Required Inputs Incomplete (in red)',
			});
		}
  }
  cancel() {
    if (!this.props.isCreate) {
      // console.log('cancel', this.props.isCreate, this.props.proposal.Id);
      this.props.getProposalUnlock(this.props.proposal.Id);
    }
    setTimeout(() => {
      window.location = '/broadcastreact/planning';
    }, 500);
  }

  render() {
    // const { proposalEditForm, updateProposalEditForm } = this.props;
    return (
      <div id="proposal-actions">
				<Row>
					<Col md={12}>
						<hr />
						<ButtonToolbar style={{ float: 'right' }}>
							<Button bsStyle="default" onClick={this.cancel}>Cancel</Button>
							<Button bsStyle="success" onClick={this.save}>Save</Button>
						</ButtonToolbar>
					</Col>
				</Row>
			</div>
    );
  }
}

ProposalActions.defaultProps = {
  getProposalUnlock: () => {},
};

/* eslint-disable react/no-unused-prop-types */
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
	isValidProposalDetailGrids: PropTypes.func.isRequired,
	// toggleEditIsciClass: PropTypes.func.isRequired,
	// toggleEditGridCellClass: PropTypes.func.isRequired,
};
