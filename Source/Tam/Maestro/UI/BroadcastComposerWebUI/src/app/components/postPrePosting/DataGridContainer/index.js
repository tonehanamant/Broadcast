import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { bindActionCreators } from 'redux';
import { toggleModal, createAlert, setOverlayLoading } from 'Ducks/app';
import { getPostPrePostingInitialData, getPostPrePosting, getPostPrePostingFiltered, deletePostPrePosting, getPostPrePostingFileEdit } from 'Ducks/postPrePosting';
import { Grid, Actions } from 'react-redux-grid';
import CustomPager from 'Components/shared/CustomPager';
import ContextMenuRow from 'Components/shared/ContextMenuRow';
import Sorter from 'Utils/react-redux-grid-sorter';

/* ////////////////////////////////// */
/* // REACT-REDUX-GRID ACTIONS
/* ////////////////////////////////// */
const { MenuActions, SelectionActions, GridActions } = Actions;
const { showMenu, hideMenu } = MenuActions;
const { selectRow, deselectAll } = SelectionActions;
const { doLocalSort } = GridActions;

const stateKey = 'gridPostPrePostingMain';

/* ////////////////////////////////// */
/* // MAPPING STATE AND DISPATCH
/* ////////////////////////////////// */
const mapStateToProps = ({ postPrePosting: { initialdata }, postPrePosting: { post }, grid, dataSource, menu }) => ({
  // App
  initialdata,
  post,
  // React-Redux-Grid
  grid,
  dataSource,
  menu,
});

const mapDispatchToProps = dispatch => (bindActionCreators(
  {
    // App
    getPostPrePostingInitialData,
    getPostPrePosting,
    getPostPrePostingFiltered,
    createAlert,
    toggleModal,
    deletePostPrePosting,
    getPostPrePostingFileEdit,
    setOverlayLoading,
    // React-Redux-Grid
    showMenu,
    hideMenu,
    selectRow,
    deselectAll,
    doLocalSort,
  }, dispatch)
);

/* ////////////////////////////////// */
/* // DATAGRIDCONTAINER COMPONENT
/* ////////////////////////////////// */

export class DataGridContainer extends Component {
  constructor(props, context) {
		super(props, context);
    this.context = context;
    this.contextMenuDeleteAction = this.contextMenuDeleteAction.bind(this);
    this.contextMenuFileSettingsAction = this.contextMenuFileSettingsAction.bind(this);
    this.deselectAll = this.deselectAll.bind(this);
    this.selectRow = this.selectRow.bind(this);
  }

  /* ////////////////////////////////// */
  /* LIFE-CYCLE METHODS */
  /* ////////////////////////////////// */

  componentDidUpdate(prevProps) {
    if (prevProps.post !== this.props.post) {
      this.props.setOverlayLoading({
        id: 'gridPostPrePostingMain',
        loading: true,
      });
      // Evaluate if sort directions is set on column or default
      // Sort dataSource using Sorter
      setTimeout(() => {
        const cols = this.props.grid.get('gridPostPrePostingMain').get('columns');
        let sortCol = cols.find(x => x.sortDirection);
        if (!sortCol) sortCol = cols.find(x => x.defaultSortDirection);
        if (sortCol) {
          const datasource = this.props.dataSource.get('gridPostPrePostingMain');
          const sorted = Sorter.sortBy(sortCol.dataIndex, sortCol.sortDirection || sortCol.defaultSortDirection, datasource);
          this.props.doLocalSort({
            data: sorted,
            stateKey: 'gridPostPrePostingMain',
          });
        }
        this.props.setOverlayLoading({
          id: 'gridPostPrePostingMain',
          loading: false,
        });
      }, 0); // SET_DATA is completed, for dataSource

      // Hide Context Menu (assumes visible)
      this.props.hideMenu({ stateKey: 'gridPostPrePostingMain' });
    }
  }

  /* ////////////////////////////////// */
  /* GRID CONTEXT MENU METHODS  */
  /* ////////////////////////////////// */
  contextMenuDeleteAction(rowData) {
    this.props.toggleModal({
      modal: 'confirmModal',
      active: true,
      properties: {
        titleText: 'Delete Post File',
        bodyText: `Are you sure you want to delete ${rowData.FileName}?`,
        closeButtonText: 'Cancel',
        actionButtonText: 'Continue',
        actionButtonBsStyle: 'danger',
        action: () => this.props.deletePostPrePosting(rowData.Id),
        dismiss: () => {},
      },
    });
  }

  contextMenuFileSettingsAction(id) {
    this.props.getPostPrePostingFileEdit(id);
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

  selectRow(rowId) {
    this.deselectAll();
    this.props.selectRow({ rowId, stateKey });
  }
  deselectAll() {
    this.props.deselectAll({ stateKey });
  }

  /* ////////////////////////////////// */
  /* // COMPONENT RENDER FUNC
  /* ////////////////////////////////// */
  render() {
    /* ////////////////////////////////// */
    /* // REACT-REDUX-GRID CONFIGURATION
    /* ////////////////////////////////// */

    /* GRID RENDERERS */
    // const renderers = {
    //   uploadDate: ({ value, row }) => (
    //     <span>{row.DisplayUploadDate}</span>
    //   ),
    //   modifiedDate: ({ value, row }) => (
    //     <span>{row.DisplayModifiedDate}</span>
    //   ),
    // };

    /* GRID COLUMNS */
    const columns = [
      {
          name: 'File Name',
          dataIndex: 'FileName',
          width: '20%',
      },
      {
          name: 'Demos',
          dataIndex: 'DisplayDemos',
          width: '40%',
      },
      {
          name: 'Upload Date',
          dataIndex: 'UploadDate',
          defaultSortDirection: 'ASC',
          width: '20%',
          renderer: ({ row }) => (
            <span>{row.DisplayUploadDate}</span>
          ),
      },
      {
          name: 'Last Modified',
          dataIndex: 'ModifiedDate',
          width: '20%',
          renderer: ({ row }) => (
            <span>{row.DisplayModifiedDate}</span>
          ),
      },
    ];

    const menuItems = [
      {
        text: 'File Settings',
        key: 'menu-file-settings',
        EVENT_HANDLER: ({ metaData }) => {
          this.contextMenuFileSettingsAction(metaData.rowData.Id);
        },
      },
      {
        text: 'Post Report',
        key: 'menu-post-report',
        EVENT_HANDLER: ({ metaData }) => {
          window.open(`${window.location.origin}/broadcast/api/PostPrePosting/Report/${metaData.rowData.Id}`, '_blank');
        },
      },
      {
        text: 'Delete',
        key: 'menu-delete',
        EVENT_HANDLER: ({ metaData }) => {
          this.contextMenuDeleteAction(metaData.rowData);
        },
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
      PAGER: {
        enabled: false,
        pagingType: 'local',
        pagerComponent: (
            <CustomPager stateKey={stateKey} idProperty="Id" />
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
      ROW: {
        enabled: true,
        renderer: ({ cells, ...rowData }) => (
            <ContextMenuRow
              {...rowData}
              menuItems={menuItems}
              stateKey={stateKey}
              beforeOpenMenu={this.selectRow}
            >
              {cells}
            </ContextMenuRow>
          ),
      },
    };

    /* GRID EVENT HANDLERS */
    const events = {
      HANDLE_BEFORE_SORT: () => {
        this.deselectAll();
      },
    };

    const grid = {
      columns,
      plugins,
      events,
      stateKey,
    };
    return (
      <Grid {...grid} data={this.props.post} store={this.context.store} />
    );
  }
}

/* ////////////////////////////////// */
/* // PROPTYPES
/* ////////////////////////////////// */
DataGridContainer.propTypes = {
  grid: PropTypes.object.isRequired,
  dataSource: PropTypes.object.isRequired,
  menu: PropTypes.object.isRequired,
  post: PropTypes.array.isRequired,

  getPostPrePostingInitialData: PropTypes.func.isRequired,
  getPostPrePosting: PropTypes.func.isRequired,
  getPostPrePostingFiltered: PropTypes.func.isRequired,
  toggleModal: PropTypes.func.isRequired,
  createAlert: PropTypes.func.isRequired,
  deletePostPrePosting: PropTypes.func.isRequired,
  getPostPrePostingFileEdit: PropTypes.func.isRequired,
  setOverlayLoading: PropTypes.func.isRequired,

  showMenu: PropTypes.func.isRequired,
  hideMenu: PropTypes.func.isRequired,
  selectRow: PropTypes.func.isRequired,
  deselectAll: PropTypes.func.isRequired,
  doLocalSort: PropTypes.func.isRequired,
};

export default connect(mapStateToProps, mapDispatchToProps)(DataGridContainer);
