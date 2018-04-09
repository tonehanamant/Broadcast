import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { bindActionCreators } from 'redux';
import { toggleModal, createAlert } from 'Ducks/app';
import { getPost, getProposalHeader } from 'Ducks/post';
import { Grid, Actions } from 'react-redux-grid';
import CustomPager from 'Components/shared/CustomPager';
import Sorter from 'Utils/react-redux-grid-sorter';
// import NumberCommaWhole from 'Components/shared/TextFormatters/NumberCommaWhole';
import numeral from 'numeral';

/* eslint-disable no-unused-vars */
/* eslint-disable no-shadow */

const { MenuActions, SelectionActions, GridActions } = Actions;
const { showMenu, hideMenu } = MenuActions;
const { selectRow, deselectAll } = SelectionActions;
const { doLocalSort } = GridActions;

const mapStateToProps = ({ post: { postGridData }, grid, dataSource, menu }) => ({
  postGridData,
  grid,
  dataSource,
  menu,
});

const mapDispatchToProps = dispatch => (bindActionCreators(
  {
    getPost,
    createAlert,
    toggleModal,
    // setOverlayLoading,
    showMenu,
    hideMenu,
    selectRow,
    deselectAll,
    doLocalSort,
    getProposalHeader,
  }, dispatch)
);

export class DataGridContainer extends Component {
  constructor(props, context) {
		super(props, context);
    this.context = context;
    this.showscrubbingModal = this.showscrubbingModal.bind(this);
  }

  componentWillMount() {
    this.props.getPost();
  }

  componentDidUpdate(prevProps) {
    if (prevProps.postGridData !== this.props.postGridData) {
     /*  this.props.setOverlayLoading({
        id: 'gridPostMain',
        loading: true,
      }); */

      // evaluate column sort direction
      setTimeout(() => {
        const cols = this.props.grid.get('gridPostMain').get('columns');
        let sortCol = cols.find(x => x.sortDirection);
        if (!sortCol) sortCol = cols.find(x => x.defaultSortDirection);

        if (sortCol) {
          const datasource = this.props.dataSource.get('gridPostMain');
          const sorted = Sorter.sortBy(sortCol.dataIndex, sortCol.sortDirection || sortCol.defaultSortDirection, datasource);

          this.props.doLocalSort({
            data: sorted,
            stateKey: 'gridPostMain',
          });
        }

        /* this.props.setOverlayLoading({
          id: 'gridPostMain',
          loading: false,
        }); */
      }, 0);

      // Hide Context Menu (assumes visible)
      this.props.hideMenu({ stateKey: 'gridPostMain' });
    }
  }

  /* ////////////////////////////////// */
  /* // GRID ACTION METHOD BINDINGS
  /* ////////////////////////////////// */
  hideContextMenu(ref) {
    this.props.hideMenu(ref);
  }
  showContextMenu(ref) {
    this.props.showMenu(ref);
  }
  selectRow(ref) {
    this.props.selectRow(ref);
  }
  deselectAll(ref) {
    this.props.deselectAll(ref);
  }
  showscrubbingModal(ID) {
    this.props.getProposalHeader(ID);
  }
  render() {
    const stateKey = 'gridPostMain';
    const columns = [
      {
          name: 'Contract',
          dataIndex: 'ContractName',
          width: '20%',
      },
      {
          name: 'Contract Id',
          dataIndex: 'ContractId',
          width: '15%',
      },
      {
        name: 'Upload Date',
        dataIndex: 'UploadDate',
        defaultSortDirection: 'ASC',
        width: '15%',
        renderer: ({ row }) => (
          <span>{row.DisplayUploadDate}</span>
        ),
      },
      {
        name: 'Spots in Spec',
        dataIndex: 'SpotsInSpec',
        width: '15%',
      },
      {
        name: 'Spots Out of Spec',
        dataIndex: 'SpotsOutOfSpec',
        width: '15%',
      },
      {
        name: 'Primary Demo Imp',
        dataIndex: 'PrimaryAudienceImpressions',
        width: '20%',
        renderer: ({ row }) => (
          // <NumberCommaWhole number={row.PrimaryAudienceImpressions / 1000} dash />
          row.PrimaryAudienceImpressions ? numeral(row.PrimaryAudienceImpressions / 1000).format('0,0.[000]') : '-'
        ),
      },
    ];

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
      PAGER: {
        enabled: false,
        pagingType: 'local',
        pagerComponent: (
            <CustomPager stateKey={stateKey} idProperty="ContractId" />
        ),
      },
      LOADER: {
        enabled: false,
      },
      SELECTION_MODEL: {
        mode: 'single',
        enabled: true,
        allowDeselect: true,
        activeCls: 'active',
        selectionEvent: 'singleclick',
      },
      GRID_ACTIONS: {
        iconCls: 'action-icon',
        menu: [
          {
            text: 'NSI Post Report',
            key: 'menu-post-nsi-report',
            EVENT_HANDLER: ({ metaData }) => {
              const inSpec = metaData.rowData.SpotsInSpec !== 0;
              // console.log('nsi menu', metaData, inSpec);
              if (inSpec) {
                window.open(`${window.location.origin}/broadcast/api/Post/DownloadNSIPostReport/${metaData.rowData.ContractId}`, '_blank');
              } else {
                this.props.createAlert({
                  type: 'warning',
                  headline: 'NSI Report Unavailable',
                  message: 'There are no in-spec spots for this proposal.',
                });
              }
            },
          },
        ],
      },
      ROW: {
        enabled: true,
        renderer: ({ rowProps, cells, row }) => {
          const stateKey = cells[0].props.stateKey;
          const rowId = cells[0].props.rowId;
          // const inSpec = cells[0].props.row.SpotsInSpec !== 0;
          // console.log('row props', cells[0], inSpec);
          const updatedRowProps = { ...rowProps,
            onClick: (e) => {
              rowProps.onClick(e);
              this.hideContextMenu({ stateKey });
            },
            onContextMenu: (e) => {
              e.preventDefault();
              // if (inSpec) {
              const rowElement = e.target.closest('.react-grid-row');
              const contextMenuContainer = rowElement.querySelector('.react-grid-action-icon');
              contextMenuContainer.setAttribute('style', `right: ${(rowElement.clientWidth - e.clientX) - 102}px`); // 102 contextMenu width

              this.deselectAll({ stateKey });
              this.selectRow({ rowId, stateKey });
              this.showContextMenu({ id: rowId, stateKey });
              // }
            },
          };
          return (
            <tr {...updatedRowProps}>{ cells }</tr>
          );
        },
      },
    };

    const events = {
      HANDLE_BEFORE_SORT: () => {
        this.deselectAll({ stateKey });
        this.hideContextMenu({ stateKey });
      },
      HANDLE_ROW_CLICK: (row) => {
          const ID = row.row.ContractId;
          this.showscrubbingModal(ID);
      },
    };

    const grid = {
      columns,
      plugins,
      events,
      stateKey,
    };
    return (
      <Grid {...grid} data={this.props.postGridData} store={this.context.store} />
    );
  }
}

DataGridContainer.propTypes = {
  grid: PropTypes.object.isRequired,
  dataSource: PropTypes.object.isRequired,
  menu: PropTypes.object.isRequired,
  // post: PropTypes.array.isRequired,
  postGridData: PropTypes.array.isRequired,

  getPost: PropTypes.func.isRequired,
  getProposalHeader: PropTypes.func.isRequired,
  toggleModal: PropTypes.func.isRequired,
  createAlert: PropTypes.func.isRequired,
  // setOverlayLoading: PropTypes.func.isRequired,

  showMenu: PropTypes.func.isRequired,
  hideMenu: PropTypes.func.isRequired,
  selectRow: PropTypes.func.isRequired,
  deselectAll: PropTypes.func.isRequired,
  doLocalSort: PropTypes.func.isRequired,
};

export default connect(mapStateToProps, mapDispatchToProps)(DataGridContainer);
