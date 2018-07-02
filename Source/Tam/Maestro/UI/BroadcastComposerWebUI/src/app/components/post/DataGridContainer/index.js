import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import { bindActionCreators } from 'redux';
import { toggleModal, createAlert } from 'Ducks/app';
import { getPost, getPostClientScrubbing } from 'Ducks/post';
import { Grid, Actions } from 'react-redux-grid';
import CustomPager from 'Components/shared/CustomPager';
import ContextMenuRow from 'Components/shared/ContextMenuRow';
import Sorter from 'Utils/react-redux-grid-sorter';
import numeral from 'numeral';

const { MenuActions, SelectionActions, GridActions } = Actions;
const { showMenu, hideMenu } = MenuActions;
const { selectRow, deselectAll } = SelectionActions;
const { doLocalSort } = GridActions;

const stateKey = 'gridPostMain';

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
    showMenu,
    hideMenu,
    selectRow,
    deselectAll,
    doLocalSort,
    getPostClientScrubbing,
  }, dispatch)
);

export class DataGridContainer extends Component {
  constructor(props, context) {
		super(props, context);
    this.context = context;
    this.showscrubbingModal = this.showscrubbingModal.bind(this);
    this.deselectAll = this.deselectAll.bind(this);
    this.selectRow = this.selectRow.bind(this);
  }

  componentWillMount() {
    this.props.getPost();
  }

  componentDidUpdate(prevProps) {
    if (prevProps.postGridData !== this.props.postGridData) {
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
      }, 0);
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
  selectRow(rowId) {
    this.deselectAll();
    this.props.selectRow({ rowId, stateKey });
  }
  deselectAll() {
    this.props.deselectAll({ stateKey });
  }
  showscrubbingModal(Id) {
    // change to params
    this.props.getPostClientScrubbing({ proposalId: Id, showModal: true, filterKey: 'All' });
  }

  render() {
    const menuItems = [
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
      {
        text: 'NSI Post Report with Overnights',
        key: 'menu-post-nsi-report-overnight',
        EVENT_HANDLER: ({ metaData }) => {
          const inSpec = metaData.rowData.SpotsInSpec !== 0;
          // console.log('nsi overnight menu', metaData, inSpec);
          if (inSpec) {
            window.open(`${window.location.origin}/broadcast/api/Post/DownloadNSIPostReportWithOvernight/${metaData.rowData.ContractId}`, '_blank');
          } else {
            this.props.createAlert({
              type: 'warning',
              headline: 'NSI Report with Overnights Unavailable',
              message: 'There are no in-spec spots for this proposal.',
            });
          }
        },
      },
      {
        text: 'MYEvents Report',
        key: 'menu-post-myevents-report',
        EVENT_HANDLER: ({ metaData }) => {
          const inSpec = metaData.rowData.SpotsInSpec !== 0;
          if (inSpec) {
            window.open(`${window.location.origin}/broadcast/api/Post/DownloadMyEventsReport/${metaData.rowData.ContractId}`, '_blank');
          } else {
            this.props.createAlert({
              type: 'warning',
              headline: 'MYEvents Report Unavailable',
              message: 'There are no in-spec spots for this proposal.',
            });
          }
        },
      },
    ];

    const columns = [
      {
        name: 'Contract',
        dataIndex: 'ContractName',
        width: '15%',
      },
      {
        name: 'Contract Id',
        dataIndex: 'ContractId',
        width: '10%',
      },
      {
        name: 'Advertiser',
        dataIndex: 'Advertiser',
        width: '15%',
      },
      {
        name: 'Affidavit Upload Date',
        dataIndex: 'UploadDate',
        defaultSortDirection: 'ASC',
        width: '15%',
        renderer: ({ row }) => (
          <span>{row.DisplayUploadDate}</span>
        ),
      },
      {
        name: 'Impressions in Spec',
        dataIndex: 'SpotsInSpec',
        width: '15%',
      },
      {
        name: 'Spots Out of Spec',
        dataIndex: 'SpotsOutOfSpec',
        width: '15%',
      },
      {
        name: 'Primary Demo Booked',
        dataIndex: 'PrimaryAudienceBookedImpressions',
        width: '15%',
        renderer: ({ row }) => (
          row.PrimaryAudienceBookedImpressions ? numeral(row.PrimaryAudienceBookedImpressions / 1000).format('0,0.[000]') : '-'
        ),
      },
      {
        // name: 'Primary Demo Imp',
        name: 'Primary Demo Delivered',
        dataIndex: 'PrimaryAudienceDeliveredImpressions',
        width: '15%',
        renderer: ({ row }) => (
          row.PrimaryAudienceDeliveredImpressions ? numeral(row.PrimaryAudienceDeliveredImpressions / 1000).format('0,0.[000]') : '-'
        ),
      },
      {
        name: 'Primary Demo % Delivery',
        dataIndex: 'PrimaryAudienceDelivery',
        width: '15%',
        // renderer: ({ row }) => (
        //   // row.PrimaryAudienceDelivery ? numeral(row.PrimaryAudienceDelivery).format('0,0%') : '-'
        //   row.PrimaryAudienceDelivery ? numeral(row.PrimaryAudienceDelivery).format('0,0.[00]%') : '-'
        // ),
        renderer: ({ row }) => {
          const val = row.PrimaryAudienceDelivery ? numeral(row.PrimaryAudienceDelivery).format('0,0.[00]') : false;
          return val ? `${val}%` : '-';
        },
      },
      {
        name: 'Household Delivered',
        dataIndex: 'HouseholdDeliveredImpressions',
        width: '15%',
        renderer: ({ row }) => (
          row.HouseholdDeliveredImpressions ? numeral(row.HouseholdDeliveredImpressions / 1000).format('0,0.[000]') : '-'
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
          </ContextMenuRow>),
      },
    };

    const events = {
      HANDLE_BEFORE_SORT: () => {
        this.deselectAll();
      },
      HANDLE_ROW_DOUBLE_CLICK: (row) => {
          const Id = row.row.ContractId;
          this.showscrubbingModal(Id);
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
  postGridData: PropTypes.array.isRequired,

  getPost: PropTypes.func.isRequired,
  getPostClientScrubbing: PropTypes.func.isRequired,
  toggleModal: PropTypes.func.isRequired,
  createAlert: PropTypes.func.isRequired,

  showMenu: PropTypes.func.isRequired,
  hideMenu: PropTypes.func.isRequired,
  selectRow: PropTypes.func.isRequired,
  deselectAll: PropTypes.func.isRequired,
  doLocalSort: PropTypes.func.isRequired,
};

export default connect(mapStateToProps, mapDispatchToProps)(DataGridContainer);
