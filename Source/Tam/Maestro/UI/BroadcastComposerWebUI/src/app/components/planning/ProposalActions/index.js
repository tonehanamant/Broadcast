import React, { Component } from 'react';
import PropTypes from 'prop-types';

import { ButtonToolbar, Button, Row, Col } from 'react-bootstrap';


export default class ProposalActions extends Component {
  constructor(props) {
    super(props);
		this.state = {};
		this.checkValid = this.checkValid.bind(this);
		this.save = this.save.bind(this);
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
		} else {
			this.props.createAlert({
				type: 'danger',
				headline: '',
				message: 'Proposal cannot be saved: Required Inputs Incomplete (in red)',
			});
		}
	}

  render() {
    // const { proposalEditForm, updateProposalEditForm } = this.props;
    return (
      <div id="proposal-actions">
				<Row>
					<Col md={12}>
						<hr />
						<ButtonToolbar style={{ float: 'right' }}>
							<Button bsStyle="default" href="/broadcast/planning">Cancel</Button>
							<Button bsStyle="success" onClick={this.save}>Save</Button>
						</ButtonToolbar>
					</Col>
				</Row>
			</div>
    );
  }
}

ProposalActions.defaultProps = {
};

/* eslint-disable react/no-unused-prop-types */
ProposalActions.propTypes = {
	proposal: PropTypes.object.isRequired,
	proposalEditForm: PropTypes.object.isRequired,
	updateProposalEditForm: PropTypes.func.isRequired,
	saveProposal: PropTypes.func.isRequired,
	toggleModal: PropTypes.func.isRequired,
	createAlert: PropTypes.func.isRequired,
	setProposalValidationState: PropTypes.func.isRequired,
	isValidProposalForm: PropTypes.func.isRequired,
	isValidProposalDetails: PropTypes.func.isRequired,
	isValidProposalDetailGrids: PropTypes.func.isRequired,
};
