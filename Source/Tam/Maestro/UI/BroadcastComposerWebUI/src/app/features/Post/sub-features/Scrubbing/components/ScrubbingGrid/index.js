import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import { Badge } from "react-bootstrap";
import { Grid, Actions } from "react-redux-grid";
import { overrideStatus, undoScrubStatus } from "Post/redux/actions";
import ContextMenuRow from "Patterns/ContextMenuRow";
import {
  getDateInFormat,
  getSecondsToTimeString,
  getDay
} from "Utils/dateFormatter";
import SwapDetailModal from "Post/sub-features/Scrubbing/components/ScrubbingSwapDetailModal";

import "./index.scss";

const { MenuActions } = Actions;
const { showMenu, hideMenu } = MenuActions;

const stateKey = "PostScrubbingGrid";

const mapStateToProps = ({ grid, selection, dataSource, menu }) => ({
  grid,
  selection,
  dataSource,
  menu
});

const mapDispatchToProps = dispatch =>
  bindActionCreators(
    {
      showMenu,
      hideMenu,
      overrideStatus,
      undoScrubStatus
    },
    dispatch
  );

export class PostScrubbingGrid extends Component {
  constructor(props, context) {
    super(props, context);
    this.context = context;
    this.getScrubbingSelections = this.getScrubbingSelections.bind(this);
    this.processManualOverrides = this.processManualOverrides.bind(this);
    this.hideContextMenu = this.hideContextMenu.bind(this);
    this.showContextMenu = this.showContextMenu.bind(this);
    this.openSwapDetailModal = this.openSwapDetailModal.bind(this);
  }

  componentDidUpdate(prevProps) {
    if (prevProps.activeScrubbingData !== this.props.activeScrubbingData) {
      this.props.hideMenu({ stateKey: "PostScrubbingGrid" });
    }
  }

  getScrubbingSelections() {
    const selectedIds = this.props.selection.get(stateKey).get("indexes");
    const rowData = this.props.dataSource.get(stateKey).toJSON(); // currentRecords or data - array
    const activeSelections = [];

    selectedIds.forEach(idx => {
      activeSelections.push(rowData.data[idx]);
    });

    return activeSelections;
  }
  // tbd call api with params - slections to Ids, filterKey, etc
  processManualOverrides(overrideType, selections) {
    const activeFilterKey = this.props.activeScrubbingData.filterKey;
    const proposalId = this.props.activeScrubbingData.Id;
    const selectionIds = selections.map(
      ({ ScrubbingClientId }) => ScrubbingClientId
    );
    const params = {
      ProposalId: proposalId,
      ScrubIds: selectionIds,
      ReturnStatusFilter: activeFilterKey,
      OverrideStatus: overrideType
    };
    this.props.overrideStatus(params);
  }

  hideContextMenu(ref) {
    this.props.hideMenu(ref);
  }
  showContextMenu(ref) {
    this.props.showMenu(ref);
  }

  /* ////////////////////////////////// */
  /* // GRID ACTION METHOD BINDINGS
    /* ////////////////////////////////// */

  selectRow(ref) {
    this.props.selectRow(ref);
  }

  deselectAll(ref) {
    this.props.deselectAll(ref);
  }

  openSwapDetailModal(selections) {
    this.props.toggleModal({
      modal: "swapDetailModal",
      active: true,
      properties: { selections }
    });
  }

  render() {
    const style = { color: "#FF0000" };
    const stateKey = "PostScrubbingGrid";
    const { activeScrubbingData, details } = this.props;
    const { ClientScrubs = [] } = activeScrubbingData;

    const gridContextMenu = [
      {
        text: "Override In Spec",
        key: "menu-post-override-in",
        EVENT_HANDLER: () => {
          // todo process as just Ids? or need to handle response
          const selections = this.getScrubbingSelections();
          this.processManualOverrides("InSpec", selections);
        }
      },
      {
        text: "Override Out of Spec",
        key: "menu-post-override-out",
        EVENT_HANDLER: () => {
          // todo process as just Ids? or need to handle response
          const selections = this.getScrubbingSelections();
          this.processManualOverrides("OutOfSpec", selections);
        }
      },
      {
        text: "Swap Proposal Detail",
        key: "menu-post-swap-detail",
        EVENT_HANDLER: () => {
          // todo process as just Ids? or need to handle response
          const selections = this.getScrubbingSelections();
          this.openSwapDetailModal(selections);
        }
      },
      {
        text: "Undo",
        key: "menu-post-undo",
        EVENT_HANDLER: () => {
          const { activeScrubbingData } = this.props;
          const selections = this.getScrubbingSelections();
          const selectedIds = selections.map(it => it.ScrubbingClientId);
          this.props.undoScrubStatus(activeScrubbingData.Id, selectedIds);
        }
      }
    ];

    const columns = [
      {
        name: "Status",
        dataIndex: "Status",
        width: 59,
        renderer: ({ row }) => {
          const override = row.StatusOverride;
          let iconClassName = "";
          if (override) {
            iconClassName =
              row.Status === 2 ? "fa-check-circle-o" : "fa-times-circle-o";
          } else {
            iconClassName =
              row.Status === 2 ? "fa-check-circle" : "fa-times-circle";
          }
          return <i className={`status-icon fa ${iconClassName}`} />;
        }
      },
      {
        name: "Detail ID",
        dataIndex: "Sequence",
        width: 75,
        renderer: ({ row }) => <span>{row.Sequence || "-"}</span>
      },
      {
        name: "Market",
        dataIndex: "Market",
        width: 150,
        renderer: ({ row }) => {
          const Market = row.MatchMarket ? (
            <span>{row.Market || "-"}</span>
          ) : (
            <span style={style}>{row.Market || "-"}</span>
          );
          return Market;
        }
      },
      {
        name: "Station",
        dataIndex: "Station",
        width: 75,
        renderer: ({ row }) => {
          const Station = row.MatchStation ? (
            <span>{row.Station || "-"}</span>
          ) : (
            <span style={style}>{row.Station || "-"}</span>
          );
          return Station;
        }
      },
      {
        name: "Affiliate",
        dataIndex: "Affiliate",
        width: 75,
        renderer: ({ row }) => <span>{row.Affiliate || "-"}</span>
      },
      {
        name: "Week Start",
        dataIndex: "WeekStart",
        width: 100,
        renderer: ({ row }) => {
          const weekStart = (
            <span>
              {(row.WeekStart && getDateInFormat(row.WeekStart)) || "-"}
            </span>
          );
          return weekStart;
        }
      },
      {
        name: "Day",
        dataIndex: "DayOfWeek",
        width: 80,
        renderer: ({ row }) => {
          const DayOfWeek = row.MatchIsciDays ? (
            <span>{getDay(row.DayOfWeek) || "-"}</span>
          ) : (
            <span style={style}>{getDay(row.DayOfWeek) || "-"}</span>
          );
          return DayOfWeek;
        }
      },
      {
        name: "Date",
        dataIndex: "DateAired",
        width: 100,
        renderer: ({ row }) => {
          const date = row.MatchDate ? (
            <span>{getDateInFormat(row.DateAired) || "-"}</span>
          ) : (
            <span style={style}>{getDateInFormat(row.DateAired) || "-"}</span>
          );
          return date;
        }
      },
      {
        name: "Time Aired",
        dataIndex: "TimeAired",
        width: 100,
        renderer: ({ row }) => {
          const TimeAired = row.MatchTime ? (
            <span>{getSecondsToTimeString(row.TimeAired) || "-"}</span>
          ) : (
            <span style={style}>
              {getSecondsToTimeString(row.TimeAired) || "-"}
            </span>
          );
          return TimeAired;
        }
      },
      {
        name: "Program",
        dataIndex: "ProgramName",
        width: 200,
        renderer: ({ row }) => {
          const programName = row.MatchProgram ? (
            <span>{row.ProgramName || "-"}</span>
          ) : (
            <span style={style}>{row.ProgramName || "-"}</span>
          );
          return row.SuppliedProgramNameUsed ? (
            <div>
              {programName}
              <Badge style={{ fontSize: "9px", marginTop: "4px" }} pullRight>
                SP
              </Badge>
            </div>
          ) : (
            programName
          );
        }
      },
      {
        name: "Genre",
        dataIndex: "GenreName",
        width: 100,
        renderer: ({ row }) => {
          const GenreName = row.MatchGenre ? (
            <span>{row.GenreName || "-"}</span>
          ) : (
            <span style={style}>{row.GenreName || "-"}</span>
          );
          return GenreName;
        }
      },
      {
        name: "Show Type",
        dataIndex: "ShowTypeName",
        width: 100,
        renderer: ({ row: { ShowTypeName, MatchShowType } }) => {
          const showTypeRow = (
            <span style={MatchShowType ? {} : style}>
              {ShowTypeName || "-"}
            </span>
          );
          return showTypeRow;
        }
      },
      {
        name: "Spot Length",
        dataIndex: "SpotLength",
        width: 95,
        renderer: ({ row }) => <span>{row.SpotLength || "-"}</span>
      },
      {
        name: "House ISCI",
        dataIndex: "ISCI",
        width: 150,
        renderer: ({ row }) => <span>{row.ISCI || "-"}</span>
      },
      {
        name: "Client ISCI",
        dataIndex: "ClientISCI",
        width: 150,
        renderer: ({ row }) => <span>{row.ClientISCI || "-"}</span>
      },
      {
        name: "Comments",
        dataIndex: "Comments",
        width: "100%",
        renderer: ({ row }) => <span>{row.Comments || "-"}</span>
      }
    ];

    const plugins = {
      COLUMN_MANAGER: {
        resizable: false,
        moveable: false,
        sortable: {
          enabled: true,
          method: "local"
        }
      },
      LOADER: {
        enabled: false
      },
      SELECTION_MODEL: {
        mode: "multi",
        enabled: true,
        allowDeselect: true,
        activeCls: "active",
        selectionEvent: "singleclick"
      },
      ROW: {
        enabled: true,
        renderer: ({ cells, ...rowData }) => {
          const selectedIds = this.props.selection.get(stateKey).get("indexes");
          const isShowContextMenu = !!(selectedIds && selectedIds.size);
          return (
            <ContextMenuRow
              {...rowData}
              menuItems={gridContextMenu}
              stateKey={stateKey}
              isRender={isShowContextMenu}
            >
              {cells}
            </ContextMenuRow>
          );
        }
      }
    };

    const events = {
      HANDLE_BEFORE_SORT: () => {
        this.deselectAll({ stateKey });
        this.hideContextMenu({ stateKey });
      }
    };

    const grid = {
      columns,
      plugins,
      events,
      stateKey
    };

    return (
      <div>
        <Grid
          {...grid}
          data={ClientScrubs}
          store={this.context.store}
          height={340}
        />
        <SwapDetailModal details={details} />
      </div>
    );
  }
}

PostScrubbingGrid.propTypes = {
  grid: PropTypes.object.isRequired,
  dataSource: PropTypes.object.isRequired,
  activeScrubbingData: PropTypes.object.isRequired,
  selection: PropTypes.object.isRequired,
  selectRow: PropTypes.func.isRequired,
  deselectAll: PropTypes.func.isRequired,
  doLocalSort: PropTypes.func.isRequired,
  showMenu: PropTypes.func.isRequired,
  hideMenu: PropTypes.func.isRequired,
  overrideStatus: PropTypes.func.isRequired,
  details: PropTypes.array.isRequired,
  toggleModal: PropTypes.func.isRequired,
  undoScrubStatus: PropTypes.func.isRequired
};

export default connect(
  mapStateToProps,
  mapDispatchToProps
)(PostScrubbingGrid);
