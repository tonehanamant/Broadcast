import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { bindActionCreators } from 'redux';

import { Button, Modal } from 'react-bootstrap';
import { toggleModal } from 'Ducks/app';

const mapStateToProps = ({ app: { modals: { postScrubbingModal: modal } } }) => ({
modal,
});

const mapDispatchToProps = dispatch => (
	bindActionCreators({ toggleModal }, dispatch)
);


export class PostScrubbingModal extends Component {
	constructor(props) {
		super(props);
		this.close = this.close.bind(this);
		this.dismiss = this.dismiss.bind(this);
	}

	close() {
    this.props.toggleModal({
      modal: 'postScrubbingModal',
      active: false,
      properties: this.props.modal.properties,
    });
	}

	dismiss() {
		this.props.modal.properties.dismiss();
		this.close();
	}

	render() {
		return (
			<Modal show={this.props.modal.active} onHide={this.close} dialogClassName="planning-versions-modal">
					<Modal.Header>
						<Modal.Title style={{ display: 'inline-block' }}>Switch Proposal Version</Modal.Title>
						<Button className="close" bsStyle="link" onClick={this.close} style={{ display: 'inline-block', float: 'right' }}>
							<span>&times;</span>
						</Button>
					</Modal.Header>
					<Modal.Body>
						<div>{this.props.modal.properties.bodyText} </div>
					</Modal.Body>
					<Modal.Footer>
						<Button onClick={this.close} bsStyle={this.props.modal.properties.closeButtonBsStyle} >Cancel</Button>
						<Button onClick={this.close} bsStyle="success">OK</Button>
					</Modal.Footer>
				</Modal>
			);
		}
}

PostScrubbingModal.defaultProps = {
	modal: {
		active: false,
		properties: {
			titleText: 'Post Scrubbing details',
			bodyText: 'under construction',
			closeButtonText: 'Close',
			closeButtonBsStyle: 'default',
			actionButtonText: 'Save',
			actionButtonBsStyle: 'sucuess',
			dismiss: () => {},
		},
	},
};

PostScrubbingModal.propTypes = {
	modal: PropTypes.object.isRequired,
	toggleModal: PropTypes.func.isRequired,
};

export default connect(mapStateToProps, mapDispatchToProps)(PostScrubbingModal);
