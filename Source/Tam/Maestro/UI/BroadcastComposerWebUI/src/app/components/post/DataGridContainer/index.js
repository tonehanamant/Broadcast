import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { bindActionCreators } from 'redux';
import { toggleModal, createAlert, setOverlayLoading } from 'Ducks/app';
import { getPost } from 'Ducks/post';
import { Grid, Actions } from 'react-redux-grid';
import CustomPager from 'Components/shared/CustomPager';
import Sorter from 'Utils/react-redux-grid-sorter';
import NumberCommaWhole from 'Components/shared/TextFormatters/NumberCommaWhole';

const { MenuActions, SelectionActions, GridActions } = Actions;
const { showMenu, hideMenu } = MenuActions;
const { selectRow, deselectAll } = SelectionActions;
const { doLocalSort } = GridActions;

const mapStateToProps = ({ post: { post }, grid, dataSource, menu }) => ({
  post,
  grid,
  dataSource,
  menu,
});

const mapDispatchToProps = dispatch => (bindActionCreators(
  {
    getPost,
    createAlert,
    toggleModal,
    setOverlayLoading,
    showMenu,
    hideMenu,
    selectRow,
    deselectAll,
    doLocalSort,
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
    if (prevProps.post !== this.props.post) {
      this.props.setOverlayLoading({
        id: 'gridPostMain',
        loading: true,
      });

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

        this.props.setOverlayLoading({
          id: 'gridPostMain',
          loading: false,
        });
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
  showscrubbingModal() {
    this.props.toggleModal({
      modal: 'postScrubbingModal',
      active: true,
      properties: {
        titleText: 'POST SCRUBBING MODAL',
        bodyText: 'Post Scrubbing details will be shown here!',
      },
    });
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
        name: 'Spot in Spec',
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
          <NumberCommaWhole number={(row.PrimaryDemo !== undefined) ? row.PrimaryDemo : ''} dash={false} />
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
    };

    const events = {
      HANDLE_BEFORE_SORT: () => {
        this.deselectAll({ stateKey });
        this.hideContextMenu({ stateKey });
      },
      HANDLE_ROW_CLICK: () => {
          this.showscrubbingModal();
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

DataGridContainer.propTypes = {
  grid: PropTypes.object.isRequired,
  dataSource: PropTypes.object.isRequired,
  menu: PropTypes.object.isRequired,
  post: PropTypes.array.isRequired,

  getPost: PropTypes.func.isRequired,
  toggleModal: PropTypes.func.isRequired,
  createAlert: PropTypes.func.isRequired,
  setOverlayLoading: PropTypes.func.isRequired,

  showMenu: PropTypes.func.isRequired,
  hideMenu: PropTypes.func.isRequired,
  selectRow: PropTypes.func.isRequired,
  deselectAll: PropTypes.func.isRequired,
  doLocalSort: PropTypes.func.isRequired,
};

export default connect(mapStateToProps, mapDispatchToProps)(DataGridContainer);
