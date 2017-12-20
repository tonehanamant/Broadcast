import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { bindActionCreators } from 'redux';
import { toggleModal, createAlert, setOverlayLoading } from 'Ducks/app';
import { getPostInitialData, getPost, getPostFiltered, deletePost, getPostFileEdit } from 'Ducks/post';
import { Grid, Actions } from 'react-redux-grid';
import CustomPager from 'Components/shared/CustomPager';
import Sorter from 'Utils/react-redux-grid-sorter';

/* eslint-disable no-unused-vars */
/* eslint-disable no-shadow */

/* ////////////////////////////////// */
/* // REACT-REDUX-GRID ACTIONS
/* ////////////////////////////////// */
const { MenuActions, SelectionActions, GridActions } = Actions;
const { showMenu, hideMenu } = MenuActions;
const { selectRow, deselectAll } = SelectionActions;
const { doLocalSort } = GridActions;

/* ////////////////////////////////// */
/* // MAPPING STATE AND DISPATCH
/* ////////////////////////////////// */
const mapStateToProps = ({ post: { initialdata }, post: { post }, grid, dataSource, menu }) => ({
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
    getPostInitialData,
    getPost,
    getPostFiltered,
    createAlert,
    toggleModal,
    deletePost,
    getPostFileEdit,
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
  }

  /* ////////////////////////////////// */
  /* LIFE-CYCLE METHODS */
  /* ////////////////////////////////// */
  componentWillMount() {
    this.props.getPostInitialData();
    this.props.getPost();
  }

  componentDidUpdate(prevProps) {
    if (prevProps.post !== this.props.post) {
      this.props.setOverlayLoading({
        id: 'gridPostMain',
        loading: true,
      });
      // Evaluate if sort directions is set on column or default
      // Sort dataSource using Sorter
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
        this.props.setOverlayLoading({
          id: 'gridPostMain',
          loading: false,
        });
      }, 0); // SET_DATA is completed, for dataSource

      // Hide Context Menu (assumes visible)
      this.props.hideMenu({ stateKey: 'gridPostMain' });
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
        action: () => this.props.deletePost(rowData.Id),
        dismiss: () => {},
      },
    });
  }

  contextMenuFileSettingsAction(id) {
    this.props.getPostFileEdit(id);
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

  /* ////////////////////////////////// */
  /* // COMPONENT RENDER FUNC
  /* ////////////////////////////////// */
  render() {
    /* ////////////////////////////////// */
    /* // REACT-REDUX-GRID CONFIGURATION
    /* ////////////////////////////////// */
    const stateKey = 'gridPostMain';

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
          renderer: ({ value, row }) => (
            <span>{row.DisplayUploadDate}</span>
          ),
      },
      {
          name: 'Last Modified',
          dataIndex: 'ModifiedDate',
          width: '20%',
          renderer: ({ value, row }) => (
            <span>{row.DisplayModifiedDate}</span>
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
      GRID_ACTIONS: {
        iconCls: 'action-icon',
        menu: [
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
              window.open(`${window.location.origin}/broadcast/api/Post/Report/${metaData.rowData.Id}`, '_blank');
            },
          },
          {
            text: 'Delete',
            key: 'menu-delete',
            EVENT_HANDLER: ({ metaData }) => {
              this.contextMenuDeleteAction(metaData.rowData);
            },
          },
        ],
      },
      ROW: {
        enabled: true,
        renderer: ({ rowProps, cells, row }) => {
          const stateKey = cells[0].props.stateKey;
          const rowId = cells[0].props.rowId;
          const updatedRowProps = { ...rowProps,
            onClick: (e) => {
              rowProps.onClick(e);
              this.hideContextMenu({ stateKey });
            },
            onContextMenu: (e) => {
              e.preventDefault();

              const rowElement = e.target.closest('.react-grid-row');
              const contextMenuContainer = rowElement.querySelector('.react-grid-action-icon');
              contextMenuContainer.setAttribute('style', `right: ${(rowElement.clientWidth - e.clientX) - 102}px`); // 102 contextMenu width

              this.deselectAll({ stateKey });
              this.selectRow({ rowId, stateKey });
              this.showContextMenu({ id: rowId, stateKey });
            },
          };
          return (
            <tr {...updatedRowProps}>{ cells }</tr>
          );
        },
      },
    };

    /* GRID EVENT HANDLERS */
    const events = {
      HANDLE_BEFORE_SORT: () => {
        this.deselectAll({ stateKey });
        this.hideContextMenu({ stateKey });
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

  getPostInitialData: PropTypes.func.isRequired,
  getPost: PropTypes.func.isRequired,
  getPostFiltered: PropTypes.func.isRequired,
  toggleModal: PropTypes.func.isRequired,
  createAlert: PropTypes.func.isRequired,
  deletePost: PropTypes.func.isRequired,
  getPostFileEdit: PropTypes.func.isRequired,
  setOverlayLoading: PropTypes.func.isRequired,

  showMenu: PropTypes.func.isRequired,
  hideMenu: PropTypes.func.isRequired,
  selectRow: PropTypes.func.isRequired,
  deselectAll: PropTypes.func.isRequired,
  doLocalSort: PropTypes.func.isRequired,
};

export default connect(mapStateToProps, mapDispatchToProps)(DataGridContainer);
