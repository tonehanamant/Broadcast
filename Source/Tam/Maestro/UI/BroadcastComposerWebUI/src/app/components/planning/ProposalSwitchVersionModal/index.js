import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';

import { Button, Modal } from 'react-bootstrap';
import { Grid } from 'react-redux-grid';

import DateMDYYYY from 'Components/shared/TextFormatters/DateMDYYYY';

const mapStateToProps = ({ app: { modals: { planningSwitchVersionsModal: modal } }, selection, dataSource }) => ({
	modal,
	selection,
	dataSource,
});

export class ProposalSwitchVersionModal extends Component {
  constructor(props, context) {
		super(props, context);
		this.context = context;
		this.close = this.close.bind(this);
		this.openVersion = this.openVersion.bind(this);
  }

  close() {
    this.props.toggleModal({
      modal: 'planningSwitchVersionsModal',
      active: false,
      properties: this.props.modal.properties,
    });
	}

	openVersion() {
		const stateKey = 'gridPlanningVersions';
		const selection = this.props.selection.toJS()[stateKey];
		const keys = Object.keys(selection);
		const rowKey = keys.filter(key => selection[key] === true)[0];

		if (rowKey) {
			const data = this.props.dataSource.get(stateKey).get('data').toJS();
			const row = data.find(obj => obj._key === rowKey);
			const id = row.Version;
			if (id) {
				window.location.assign(`/broadcastreact/planning/proposal/${this.props.proposal.Id}/version/${id}`);
				this.close();
			}
		}
	}

  render() {
		const { Statuses } = this.props.initialdata;
		/* ////////////////////////////////// */
    /* // REACT-REDUX-GRID CONFIGURATION
    /* ////////////////////////////////// */
    const stateKey = 'gridPlanningVersions';

		/* GRID COLUMNS */
		const columns = [
			{
					name: 'Version',
					dataIndex: 'Version',
					width: '5%',
			},
			{
					name: 'Status',
					dataIndex: 'Status',
					width: '15%',
					renderer: ({ value }) => (
						Statuses.map((status) => {
							if (status.Id === value) {
								return <span key={status.Id}>{status.Display}</span>;
							}
							return <span key={status.Id} />;
						})
					),
			},
			{
					name: 'Advertiser',
					dataIndex: 'Advertiser',
					width: '10%',
			},
			{
					name: 'Flight',
					dataIndex: 'StartDate',
					width: '20%',
					renderer: ({ row }) => (
						<span><DateMDYYYY date={row.StartDate} /> - <DateMDYYYY date={row.EndDate} /></span>
					),
			},
			{
					name: 'Guaranteed Demos',
					dataIndex: 'GuaranteedAudience',
					width: '15%',
			},
			{
					name: 'Owner',
					dataIndex: 'Owner',
					width: '15%',
			},
			{
					name: 'Date Modified',
					dataIndex: 'DateModified',
					width: '10%',
					renderer: ({ value }) => (
						<span><DateMDYYYY date={value} /></span>
					),
			},
			{
					name: 'Notes',
					dataIndex: 'Notes',
					width: '10%',
			},
		];

		/* GRID PLGUINS */
		const plugins = {
			COLUMN_MANAGER: {
				resizable: true,
				moveable: false,
				sortable: {
						enabled: true,
						method: 'local',
				},
			},
			EDITOR: {
				type: 'inline',
				enabled: false,
			},
			LOADER: {
				enabled: false,
			},
			ROW: {
				enabled: true,
				renderer: ({ rowProps, cells }) => (
						<tr {...rowProps}>{ cells }</tr>
					),
			},
		};

		/* GRID EVENTS */
		const events = {
			HANDLE_ROW_DOUBLE_CLICK: () => {
				this.openVersion();
			},
		};

		const grid = {
			columns,
			plugins,
			events,
			stateKey,
		};

    return (
      <Modal show={this.props.modal.active} onHide={this.close} dialogClassName="large-80-modal">
        <Modal.Header>
          <Modal.Title style={{ display: 'inline-block' }}>Switch Proposal Version</Modal.Title>
          <Button className="close" bsStyle="link" onClick={this.close} style={{ display: 'inline-block', float: 'right' }}>
            <span>&times;</span>
          </Button>
        </Modal.Header>
        <Modal.Body>
					<Grid {...grid} data={this.props.versions} store={this.context.store} />
        </Modal.Body>
        <Modal.Footer>
					<Button onClick={this.close}>Cancel</Button>
          <Button onClick={this.openVersion} bsStyle="success">Open</Button>
        </Modal.Footer>
      </Modal>
    );
  }
}

ProposalSwitchVersionModal.defaultProps = {
  modal: {
    active: false, // modal closed by default
    properties: {},
  },
};

ProposalSwitchVersionModal.propTypes = {
	modal: PropTypes.object,
	selection: PropTypes.object.isRequired,
	dataSource: PropTypes.object.isRequired,

	toggleModal: PropTypes.func.isRequired,
	initialdata: PropTypes.object.isRequired,
	proposal: PropTypes.object.isRequired,
	versions: PropTypes.array.isRequired,
};

export default connect(mapStateToProps)(ProposalSwitchVersionModal);
