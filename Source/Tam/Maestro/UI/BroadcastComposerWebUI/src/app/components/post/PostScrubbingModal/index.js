import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { bindActionCreators } from 'redux';

import { Button, Modal } from 'react-bootstrap';
import { toggleModal } from 'Ducks/app';
import { getPostScrubbingDetail, clearPostScrubbingDetail } from 'Ducks/post';

import PostScrubbingHeader from './PostScrubbingHeader';
import PostScrubbingDetail from './PostScrubbingDetail';
/* eslint-disable */
const mapStateToProps = ({ app: { modals: { postScrubbingModal: modal } }, post: { proposalHeader, proposalDetail } }) => ({
modal, proposalHeader, proposalDetail
});

const mapDispatchToProps = dispatch => (
	bindActionCreators({ clearPostScrubbingDetail, getPostScrubbingDetail, toggleModal }, dispatch)
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

		this.props.clearPostScrubbingDetail();
	}

	dismiss() {
		this.props.modal.properties.dismiss();
		this.close();
	}

	render() {
		const { proposalHeader: { Advertiser, Id, Name, Markets, GuaranteedDemo, SecondaryDemos, Notes, MarketGroupId, Details } } = this.props;
		const { getPostScrubbingDetail } = this.props;
		const { proposalDetail } = this.props;

		return (
			<Modal show={this.props.modal.active} onHide={this.close} dialogClassName="planning-versions-modal">
					<Modal.Header>
						<Button className="close" bsStyle="link" onClick={this.close} style={{ display: 'inline-block', float: 'right' }}>
							<span>&times;</span>
						</Button>
						<PostScrubbingHeader
							advertiser={Advertiser}
							date={Details}
							gaurantedDemo={GuaranteedDemo}
							Id={Id}
							market={Markets}
							marketId={MarketGroupId}
							name={Name}
							notes={Notes}
							secondaryDemo={SecondaryDemos}
							getPostScrubbingDetail={getPostScrubbingDetail}
						/>
					</Modal.Header>
					<Modal.Body>
						{<PostScrubbingDetail />}
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
