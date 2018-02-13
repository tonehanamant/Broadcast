import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { bindActionCreators } from 'redux';

import { Button, Modal, Row, Col, ControlLabel, FormGroup, FormControl } from 'react-bootstrap';
import { toggleModal } from 'Ducks/app';
import Select from 'react-select';

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
		this.marketSelectorOptionRenderer = this.marketSelectorOptionRenderer.bind(this);
	}

	marketSelectorOptionRenderer(option) {
		let count = option.Count;
		const divStyle = { overflow: 'hidden' };
		const countStyle = { color: '#c0c0c0' };
		// custom
		if (option.Id === 255) {
			count = this.state.customMarketCount;
		}
		// select custom
		const isOpenCustomOption = option.Id === -1;
		if (isOpenCustomOption) {
			countStyle.Display = 'none';
		}
		return (
		<div style={divStyle} href="">
			{isOpenCustomOption ? <hr style={{ margin: '8px' }} /> : null}
			<span className="pull-left">{option.Display}</span>
			<span className="pull-right" style={countStyle}>{count}</span>
		</div>
		);
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
						<Row>
							<Col md={12}><ControlLabel><strong>Proposal ID : 123</strong></ControlLabel></Col>
						</Row>
						<Row>
							<Col md={6}>
								<Row>
									<Col md={4}>
										<FormGroup controlId="proposalName">
											<ControlLabel><strong>ProposalName</strong></ControlLabel>
											<FormControl
												type="text"
												defaultValue={'Bio Quil'}
											/>
										</FormGroup>
									</Col>
									<Col md={4}>
										<FormGroup controlId="advertiser">
											<ControlLabel><strong>Advertiser</strong></ControlLabel>
											<FormControl
												type="text"
												defaultValue={'Bio Quil'}
											/>
										</FormGroup>
									</Col>
									<Col md={3}>
										<FormGroup controlId="proposalMarket">
											<ControlLabel><strong>Market</strong></ControlLabel>
											<Select
												name="marketGroup"
												placeholder="Choose a market..."
												value={[{ Id: 1, Display: 'Top 50' }]}
												optionRenderer={this.marketSelectorOptionRenderer}
												options={[{ Id: 1, Display: 'Top 50' }]}
												clearable={false}
												valueKey="Id"
											/>
										</FormGroup>
									</Col>
								</Row>
							</Col>
							<Col md={6}>
								<Row>
									<Col md={4}>
										<FormGroup controlId="gaurantedDemo">
											<ControlLabel><strong>Gauranted Demo</strong></ControlLabel>
											<FormControl
												type="text"
												defaultValue={'Bio Quil'}
											/>
										</FormGroup>
									</Col>
									<Col md={4}>
										<FormGroup controlId="proposalSecondaryDemo">
											<ControlLabel><strong>Secondary Demo</strong></ControlLabel>
											<Select
												name="proposalSecondaryDemo"
												value={[{ Display: 'Secondary', Id: 'Secondary Demo' }]}
												labelKey="Display"
												valueKey="Id"
												multi
												options={[{ Display: 'Secondary', Id: 'Secondary Demo' }]}
												closeOnSelect
											/>
										</FormGroup>
									</Col>
									<Col md={4}>
										<FormGroup controlId="proposalNotes">
											<ControlLabel>Notes</ControlLabel>
											<FormControl
												componentClass="textarea"
												defaultValue={'Notes' || ''}
											/>
										</FormGroup>
									</Col>
								</Row>
							</Col>
						</Row>
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
