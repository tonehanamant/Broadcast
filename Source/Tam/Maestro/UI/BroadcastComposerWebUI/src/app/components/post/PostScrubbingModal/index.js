import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { bindActionCreators } from 'redux';
import { Actions } from 'react-redux-grid';

import { Button, Modal } from 'react-bootstrap';
import { toggleModal, setOverlayLoading } from 'Ducks/app';
// import { getPostScrubbingDetail, clearPostScrubbingDetail } from 'Ducks/post'; //  ?

import PostScrubbingHeader from './PostScrubbingHeader';
import PostScrubbingDetail from './PostScrubbingDetail';

const { SelectionActions, GridActions } = Actions;
const { selectRow, deselectAll } = SelectionActions;
const { doLocalSort } = GridActions;

/* eslint-disable */
const mapStateToProps = ({ app: { modals: { postScrubbingModal: modal } }, post: { proposalHeader = { }, scrubbingFiltersList = [] }, grid, dataSource }) => ({
	modal,
  proposalHeader,
  scrubbingFiltersList,
	grid,
  dataSource,
});

const mapDispatchToProps = dispatch => (
	bindActionCreators({
		// clearPostScrubbingDetail,
		// getPostScrubbingDetail,
		toggleModal,
		selectRow,
		deselectAll,
		doLocalSort,
		setOverlayLoading,
	}, dispatch)
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

		// this.props.clearPostScrubbingDetail();  //??
	}

	dismiss() {
		this.props.modal.properties.dismiss();
		this.close();
	}

	render() {
	// const { getPostScrubbingDetail } = this.props;
		const { proposalHeader, scrubbingFiltersList } = this.props;
		const { scrubbingData = {}, activeScrubbingData = {} } = proposalHeader;
		const { Advertiser, Id, Name, Markets, GuaranteedDemo, SecondaryDemos, Notes, MarketGroupId, Details } = scrubbingData;
		const { grid, dataSource } = this.props;
		const { selectRow, deselectAll, doLocalSort, setOverlayLoading } = this.props;

		return (
			<Modal ref={this.setWrapperRef} show={this.props.modal.active} dialogClassName="post-scrubbing-modal" enforceFocus={false}>
					<Modal.Header>
            <Modal.Title style={{ display: 'inline-block' }}>Scrubbing Screen</Modal.Title>
						<Button className="close" bsStyle="link" onClick={this.close} style={{ display: 'inline-block', float: 'right' }}>
							<span>&times;</span>
						</Button>
          </Modal.Header>
          <Modal.Body style={{ overflowX: 'auto', paddingBottom: 0 }}>
						<PostScrubbingHeader
							advertiser={Advertiser}
							details={Details}
							guaranteedDemo={GuaranteedDemo}
							Id={Id}
							market={Markets}
							marketId={MarketGroupId}
							name={Name}
							notes={Notes}
							secondaryDemo={SecondaryDemos}
							// getPostScrubbingDetail={getPostScrubbingDetail}
						/>
						<PostScrubbingDetail
              activeScrubbingData={activeScrubbingData}
              scrubbingFiltersList={scrubbingFiltersList}
							grid={grid}
							dataSource={dataSource}
							selectRow={selectRow}
							deselectAll={deselectAll}
							doLocalSort={doLocalSort}
							setOverlayLoading={setOverlayLoading}
						/>
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
	grid: PropTypes.object.isRequired,
	dataSource: PropTypes.object.isRequired,
	proposalHeader: PropTypes.object.isRequired,
  scrubbingFiltersList: PropTypes.array.isRequired,
  setOverlayLoading: PropTypes.func.isRequired,
  selectRow: PropTypes.func.isRequired,
  deselectAll: PropTypes.func.isRequired,
  doLocalSort: PropTypes.func.isRequired,
};

export default connect(mapStateToProps, mapDispatchToProps)(PostScrubbingModal);
