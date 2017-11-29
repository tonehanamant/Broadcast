import React, { Component } from 'react';
import PropTypes from 'prop-types';

import { ButtonToolbar, Button, Row, Col } from 'react-bootstrap';


export default class ProposalActions extends Component {
  constructor(props) {
    super(props);
		this.state = {};
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
					},
			});
		}
	}

	save() {
		this.props.saveProposal({ proposal: this.props.proposalEditForm });
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
};
