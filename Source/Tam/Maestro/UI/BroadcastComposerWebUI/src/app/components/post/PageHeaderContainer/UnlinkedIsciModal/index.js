import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { bindActionCreators } from 'redux';
import { Button, Modal } from 'react-bootstrap';
import { Grid, Actions } from 'react-redux-grid';
import { archiveUnlinkedIscis } from 'Ducks/post';

import { getDateInFormat, getSecondsToTimeString } from '../../../../utils/dateFormatter';

const { MenuActions, SelectionActions } = Actions;
const { showMenu, hideMenu } = MenuActions;
const { selectRow, deselectAll } = SelectionActions;

const setPosition = (e) => {
  const rowElement = e.target.closest('.react-grid-row');
  const contextMenuContainer = rowElement.querySelector('.react-grid-action-icon');
  contextMenuContainer.setAttribute('style', `right: ${(rowElement.clientWidth - e.clientX) + 45}px`);
};

const stateKey = 'unlinked-isci-modal';

const mapStateToProps = ({ app: { modals: { postUnlinkedIsciModal: modal } } }) => ({
	modal,
});

const mapDispatchToProps = dispatch => (
  bindActionCreators({
    showMenu,
    hideMenu,
    selectRow,
    deselectAll,
    archiveIscis: archiveUnlinkedIscis,
  }, dispatch)
);

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
    const { archiveIscis } = this.props;
		/* ////////////////////////////////// */
    /* // REACT-REDUX-GRID CONFIGURATION
    /* ////////////////////////////////// */

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
        renderer: ({ row }) => (<span>{getDateInFormat(row.DateAired) || '-'}</span>),
      },
      {
        name: 'Time Aired',
        dataIndex: 'TimeAired',
        width: '10%',
        renderer: ({ row }) => (row.TimeAired ? getSecondsToTimeString(row.TimeAired) : '-'),
      },
      {
        name: 'Spot Length',
        dataIndex: 'SpotLength',
        width: '8%',
        renderer: ({ row }) => (<span>{row.SpotLength || '-'}</span>),
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
        renderer: ({ row }) => (<span>{row.Affiliate || '-'}</span>),
      },
      {
        name: 'Market',
        dataIndex: 'Market',
        width: '9%',
        renderer: ({ row }) => (<span>{row.Market || '-'}</span>),
      },
      {
        name: 'Station',
        dataIndex: 'Station',
        width: '9%',
        renderer: ({ row }) => (<span>{row.Station || '-'}</span>),
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
      GRID_ACTIONS: {
        iconCls: 'action-icon',
        menu: [
          {
            text: 'Archive ISCI',
            key: 'menu-archive-isci',
            EVENT_HANDLER: ({ metaData }) => {
              archiveIscis([metaData.rowData.FileDetailId]);
            },
          },
        ],
      },
      ROW: {
        enabled: true,
        renderer: ({ rowProps, cells, row }) => {
          const { showMenu, hideMenu, selectRow, deselectAll } = this.props;
          const rowId = row.get('_key');
          const updatedRowProps = {
            ...rowProps,
            tabIndex: 1,
            onBlur: () => {
              if (rowId) {
                hideMenu({ stateKey });
              }
            },
            onContextMenu: (e) => {
              e.preventDefault();
              setPosition(e);
              deselectAll({ stateKey });
              selectRow({ rowId, stateKey });
              showMenu({ id: rowId, stateKey });
            },
          };
          return (
            <tr {...updatedRowProps}>{ cells }</tr>
          );
        },
      },
    };


		const grid = {
			columns,
      plugins,
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

UnlinkedIsciModal.propTypes = {
	modal: PropTypes.object,
	toggleModal: PropTypes.func.isRequired,
  unlinkedIscis: PropTypes.array.isRequired,
  archiveIscis: PropTypes.array.isRequired,

	deselectAll: PropTypes.func.isRequired,
	selectRow: PropTypes.func.isRequired,
	showMenu: PropTypes.func.isRequired,
	hideMenu: PropTypes.func.isRequired,
};

export default connect(mapStateToProps, mapDispatchToProps)(UnlinkedIsciModal);
