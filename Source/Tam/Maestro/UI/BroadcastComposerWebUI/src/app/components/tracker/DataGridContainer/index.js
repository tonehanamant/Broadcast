import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import { Badge } from "react-bootstrap";
import { toggleModal, createAlert } from "Ducks/app";
import { getTracker, getTrackerClientScrubbing } from "Ducks/tracker";
import { Grid, Actions } from "react-redux-grid";
import CustomPager from "Components/shared/CustomPager";
import ContextMenuRow from "Components/shared/ContextMenuRow";
import Sorter from "Utils/react-redux-grid-sorter";
import numeral from "numeral";

const { MenuActions, SelectionActions, GridActions } = Actions;
const { showMenu, hideMenu } = MenuActions;
const { selectRow, deselectAll } = SelectionActions;
const { doLocalSort } = GridActions;

const stateKey = "gridTrackerMain";

const mapStateToProps = ({
  tracker: { trackerGridData },
  grid,
  dataSource,
  menu
}) => ({
  trackerGridData,
  grid,
  dataSource,
  menu
});

const mapDispatchToProps = dispatch =>
  bindActionCreators(
    {
      getTracker,
      createAlert,
      toggleModal,
      showMenu,
      hideMenu,
      selectRow,
      deselectAll,
      doLocalSort,
      getTrackerClientScrubbing
    },
    dispatch
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
    this.props.getTracker();
  }

  componentDidUpdate(prevProps) {
    if (prevProps.trackerGridData !== this.props.trackerGridData) {
      // evaluate column sort direction
      setTimeout(() => {
        const cols = this.props.grid.get("gridTrackerMain").get("columns");
        let sortCol = cols.find(x => x.sortDirection);
        if (!sortCol) sortCol = cols.find(x => x.defaultSortDirection);
        if (sortCol) {
          const datasource = this.props.dataSource.get("gridTrackerMain");
          const sorted = Sorter.sortBy(
            sortCol.dataIndex,
            sortCol.sortDirection || sortCol.defaultSortDirection,
            datasource
          );

          this.props.doLocalSort({
            data: sorted,
            stateKey: "gridTrackerMain"
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
    this.props.getTrackerClientScrubbing({
      proposalId: Id,
      showModal: true,
      filterKey: "All"
    });
  }
  render() {
    const menuItems = [
      {
        text: "Spot Tracker Report",
        key: "menu-tracker-spot-tracker-report",
        EVENT_HANDLER: ({ metaData }) => {
          window.open(
            `${__API__}SpotTracker/SpotTrackerReport/${
              metaData.rowData.ContractId
            }`,
            "_blank"
          );
        }
      }
    ];

    const columns = [
      {
        name: "Contract",
        dataIndex: "ContractName",
        width: "15%"
      },
      {
        name: "Contract Id",
        dataIndex: "ContractId",
        width: "10%"
      },
      {
        name: "Contract Id",
        dataIndex: "searchContractId",
        width: "10%",
        hidden: true,
        hideable: true
      },
      {
        name: "Advertiser",
        dataIndex: "Advertiser",
        width: "15%"
      },
      {
        name: "Post Log Upload Date",
        dataIndex: "searchUploadDate",
        defaultSortDirection: "ASC",
        width: "15%"
      },
      {
        name: "Spots in Spec",
        dataIndex: "SpotsInSpec",
        width: "15%"
      },
      {
        name: "Spots in Spec",
        dataIndex: "searchSpotsInSpec",
        width: "15%",
        hidden: true,
        hideable: true
      },
      {
        name: "Spots Out of Spec",
        dataIndex: "SpotsOutOfSpec",
        width: "15%"
      },
      {
        name: "Spots Out of Spec",
        dataIndex: "searchSpotsOutOfSpec",
        width: "15%",
        hidden: true,
        hideable: true
      },
      {
        name: "Primary Demo Booked",
        dataIndex: "PrimaryAudienceBookedImpressions",
        width: "15%",
        renderer: ({ row }) =>
          row.PrimaryAudienceBookedImpressions
            ? numeral(row.PrimaryAudienceBookedImpressions / 1000).format(
                "0,0.[000]"
              )
            : "-"
      },
      {
        // name: 'Primary Demo Imp',
        name: "Primary Demo Delivered",
        dataIndex: "PrimaryAudienceDeliveredImpressions",
        width: "15%",
        renderer: ({ row }) => {
          // handle equivalized indicator as badge if true
          const val = row.PrimaryAudienceDeliveredImpressions
            ? numeral(row.PrimaryAudienceDeliveredImpressions / 1000).format(
                "0,0.[000]"
              )
            : "-";
          return row.Equivalized ? (
            <div>
              {val}
              <Badge style={{ fontSize: "9px", marginTop: "4px" }} pullRight>
                EQ
              </Badge>
            </div>
          ) : (
            val
          );
        }
      },
      {
        name: "Primary Demo % Delivery",
        dataIndex: "PrimaryAudienceDelivery",
        width: "15%",
        renderer: ({ row }) => {
          const val = row.PrimaryAudienceDelivery
            ? numeral(row.PrimaryAudienceDelivery).format("0,0.[00]")
            : false;
          return val ? `${val}%` : "-";
        }
      },
      {
        name: "Household Delivered",
        dataIndex: "HouseholdDeliveredImpressions",
        width: "15%",
        renderer: ({ row }) => {
          // handle equivalized indicator as badge if true
          const val = row.HouseholdDeliveredImpressions
            ? numeral(row.HouseholdDeliveredImpressions / 1000).format(
                "0,0.[000]"
              )
            : "-";
          return row.Equivalized ? (
            <div>
              {val}
              <Badge style={{ fontSize: "9px", marginTop: "4px" }} pullRight>
                EQ
              </Badge>
            </div>
          ) : (
            val
          );
        }
      }
    ];

    const plugins = {
      COLUMN_MANAGER: {
        resizable: true,
        moveable: false,
        sortable: {
          enabled: true,
          method: "local"
        }
      },
      EDITOR: {
        type: "inline",
        enabled: false
      },
      PAGER: {
        enabled: false,
        pagingType: "local",
        pagerComponent: (
          <CustomPager stateKey={stateKey} idProperty="ContractId" />
        )
      },
      LOADER: {
        enabled: false
      },
      SELECTION_MODEL: {
        mode: "single",
        enabled: true,
        allowDeselect: true,
        activeCls: "active",
        selectionEvent: "singleclick"
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
        )
      }
    };

    const events = {
      HANDLE_BEFORE_SORT: () => {
        this.deselectAll();
      },
      HANDLE_ROW_DOUBLE_CLICK: row => {
        const Id = row.row.ContractId;
        this.showscrubbingModal(Id);
      }
    };

    const grid = {
      columns,
      plugins,
      events,
      stateKey
    };
    return (
      <Grid
        {...grid}
        data={this.props.trackerGridData}
        store={this.context.store}
      />
    );
  }
}

DataGridContainer.propTypes = {
  grid: PropTypes.object.isRequired,
  dataSource: PropTypes.object.isRequired,
  menu: PropTypes.object.isRequired,
  trackerGridData: PropTypes.array.isRequired,

  getTracker: PropTypes.func.isRequired,
  getTrackerClientScrubbing: PropTypes.func.isRequired,
  toggleModal: PropTypes.func.isRequired,
  createAlert: PropTypes.func.isRequired,

  showMenu: PropTypes.func.isRequired,
  hideMenu: PropTypes.func.isRequired,
  selectRow: PropTypes.func.isRequired,
  deselectAll: PropTypes.func.isRequired,
  doLocalSort: PropTypes.func.isRequired
};

export default connect(
  mapStateToProps,
  mapDispatchToProps
)(DataGridContainer);
