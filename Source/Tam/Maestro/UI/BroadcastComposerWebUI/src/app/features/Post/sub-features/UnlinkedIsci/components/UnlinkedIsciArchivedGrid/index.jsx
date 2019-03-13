import React, { Component } from "react";
import PropTypes from "prop-types";
import ContextMenuRow from "Patterns/ContextMenuRow";
import CustomPager from "Patterns/CustomPager";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import { Grid, Actions } from "react-redux-grid";
import { unlinkedIsciActions } from "Post";
import moment from "moment";
import { getDateInFormat, getSecondsToTimeString } from "Utils/dateFormatter";

const {
  SelectionActions: { deselectAll, selectRow }
} = Actions;

const mapStateToProps = ({ grid, selection, dataSource }) => ({
  selection,
  dataSource,
  grid
});

const mapDispatchToProps = dispatch =>
  bindActionCreators(
    {
      undoArchive: unlinkedIsciActions.undoArchivedIscis,
      deselectAll,
      selectRow
    },
    dispatch
  );

export class ArchivedIsciGrid extends Component {
  constructor(props, context) {
    super(props, context);
    this.context = context;
  }

  render() {
    const {
      archivedIscisData,
      selection,
      dataSource,
      undoArchive
    } = this.props;
    const { store } = this.context;
    const stateKey = "archived_grid";

    const columns = [
      {
        name: "ISCI",
        dataIndex: "ISCI",
        width: "15%"
      },
      {
        name: "Date Aired",
        dataIndex: "DateAired",
        width: "10%",
        renderer: ({ row }) => (
          <span>{getDateInFormat(row.DateAired) || "-"}</span>
        )
      },
      {
        name: "Time Aired",
        dataIndex: "TimeAired",
        width: "10%",
        renderer: ({ row }) =>
          row.TimeAired ? getSecondsToTimeString(row.TimeAired) : "-"
      },
      {
        name: "Spot Length",
        dataIndex: "SpotLength",
        width: "8%",
        renderer: ({ row }) => <span>{row.SpotLength || "-"}</span>
      },
      {
        name: "Program",
        dataIndex: "ProgramName",
        width: "20%"
      },
      {
        name: "Genre",
        dataIndex: "Genre",
        width: "10%"
      },
      {
        name: "Affiliate",
        dataIndex: "Affiliate",
        width: "9%",
        renderer: ({ row }) => <span>{row.Affiliate || "-"}</span>
      },
      {
        name: "Market",
        dataIndex: "Market",
        width: "9%",
        renderer: ({ row }) => <span>{row.Market || "-"}</span>
      },
      {
        name: "Station",
        dataIndex: "Station",
        width: "9%",
        renderer: ({ row }) => <span>{row.Station || "-"}</span>
      },
      {
        name: "Date Archived",
        dataIndex: "DateAdded",
        width: "15%",
        renderer: ({ row }) => (
          <span>{moment(row.DateAdded).format("MM/DD/YYYY hh:mm:ss A")}</span>
        )
      }
    ];

    const menuItems = [
      {
        text: "Undo Archive",
        key: "menu-undo-archive",
        EVENT_HANDLER: () => {
          const selectedIds = selection.get(stateKey).get("indexes");
          const rowData = dataSource.get(stateKey).toJSON();
          const activeSelections = [];
          selectedIds.forEach(idx => {
            activeSelections.push(rowData.data[idx].FileDetailId);
          });
          undoArchive(activeSelections);
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
        pagerComponent: <CustomPager stateKey={stateKey} idProperty="ISCI" />
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
          const selectedIds = selection.getIn([stateKey, "indexes"]);
          const isRender = !!(selectedIds && selectedIds.size);
          return (
            <ContextMenuRow
              {...rowData}
              menuItems={menuItems}
              stateKey={stateKey}
              isRender={isRender}
            >
              {cells}
            </ContextMenuRow>
          );
        }
      }
    };

    const grid = {
      columns,
      plugins,
      stateKey
    };

    return (
      <Grid {...grid} data={archivedIscisData} store={store} height={460} />
    );
  }
}

ArchivedIsciGrid.propTypes = {
  archivedIscisData: PropTypes.array.isRequired,
  undoArchive: PropTypes.func.isRequired,
  dataSource: PropTypes.object.isRequired,
  selection: PropTypes.object.isRequired,
  grid: PropTypes.object.isRequired
};

export default connect(
  mapStateToProps,
  mapDispatchToProps
)(ArchivedIsciGrid);
