import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
// import moment from 'moment';
import { Button, Modal } from 'react-bootstrap';
import { Grid } from 'react-redux-grid';
import { getDateInFormat, getSecondsToTimeString } from '../../../../utils/dateFormatter';

// import DateMDYYYY from 'Components/shared/TextFormatters/DateMDYYYY';

const mapStateToProps = ({ app: { modals: { postUnlinkedIsciModal: modal } } }) => ({
	modal,
	// selection,
	// dataSource,
});

export class UnlinkedIsciModal extends Component {
  constructor(props, context) {
		super(props, context);
		this.context = context;
		this.close = this.close.bind(this);
  }

  close() {
    this.props.toggleModal({
      modal: 'postUnlinkedIsciModal',
      active: false,
      properties: this.props.modal.properties,
    });
	}

  render() {
		/* ////////////////////////////////// */
    /* // REACT-REDUX-GRID CONFIGURATION
    /* ////////////////////////////////// */
    const stateKey = 'gridPostIscis';

		/* GRID COLUMNS */
		const columns = [
			{
					name: 'ISCI',
					dataIndex: 'ISCI',
					width: '15%',
      },
      {
        name: 'Date Aired',
        dataIndex: 'DateAired',
        width: '10%',
        renderer: ({ row }) => {
            const date = <span>{getDateInFormat(row.DateAired) || '-'}</span>;
            return (
                date
            );
        },
    },
    {
        name: 'Time Aired',
        dataIndex: 'TimeAired',
        width: '10%',
        renderer: ({ row }) => {
            // const TimeAired = <span>{getDateInFormat(row.TimeAired, false, true) || '-'}</span>;
            // const TimeAired = row.TimeAired ? moment({}).seconds(row.TimeAired).format('h:mm:ss A') : '-';
            const TimeAired = row.TimeAired ? getSecondsToTimeString(row.TimeAired) : '-';
            return (
                TimeAired
            );
        },
    },
    {
      name: 'Spot Length',
      dataIndex: 'SpotLength',
      width: '8%',
      renderer: ({ row }) => (
          <span>{row.SpotLength || '-'}</span>
      ),
    },
    {
      name: 'Program',
      dataIndex: 'ProgramName',
      width: '20%',
    },
    {
      name: 'Genre',
      dataIndex: 'Genre',
      width: '10%',
    },
    {
      name: 'Affiliate',
      dataIndex: 'Affiliate',
      width: '9%',
      renderer: ({ row }) => (
          <span>{row.Affiliate || '-'}</span>
      ),
    },
    {
      name: 'Market',
      dataIndex: 'Market',
      width: '9%',
      renderer: ({ row }) => (
          <span>{row.Market || '-'}</span>
      ),
    },
    {
      name: 'Station',
      dataIndex: 'Station',
      width: '9%',
      renderer: ({ row }) => (
          <span>{row.Station || '-'}</span>
      ),
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
			// SELECTION_MODEL: {
			// 	mode: 'single',
			// 	enabled: true,
			// 	allowDeselect: true,
			// 	activeCls: 'active',
			// 	selectionEvent: 'singleclick',
			// },
		};

		/* GRID EVENTS */
		/* const events = {
			HANDLE_ROW_DOUBLE_CLICK: () => {
				this.openVersion();
			},
		}; */

		const grid = {
			columns,
			plugins,
			// events,
			stateKey,
		};

    return (
      <Modal show={this.props.modal.active} onHide={this.close} dialogClassName="large-80-modal">
        <Modal.Header>
          <Modal.Title style={{ display: 'inline-block' }}>Unlinked ISCIs</Modal.Title>
          <Button className="close" bsStyle="link" onClick={this.close} style={{ display: 'inline-block', float: 'right' }}>
            <span>&times;</span>
          </Button>
        </Modal.Header>
        <Modal.Body>
					<Grid {...grid} data={this.props.unlinkedIscis} store={this.context.store} height={460} />
        </Modal.Body>
        <Modal.Footer>
					<Button onClick={this.close}>Close</Button>
          {/* <Button onClick={this.saveScrub} bsStyle="success">Open</Button> */}
        </Modal.Footer>
      </Modal>
    );
  }
}

UnlinkedIsciModal.defaultProps = {
  modal: {
    active: false, // modal closed by default
    properties: {},
  },
};

/* eslint-disable react/no-unused-prop-types */
UnlinkedIsciModal.propTypes = {
	modal: PropTypes.object,
	// selection: PropTypes.object.isRequired,
	// dataSource: PropTypes.object.isRequired,

	toggleModal: PropTypes.func.isRequired,
	// initialdata: PropTypes.object.isRequired,
	// proposal: PropTypes.object.isRequired,
	unlinkedIscis: PropTypes.array.isRequired,
};

export default connect(mapStateToProps)(UnlinkedIsciModal);
