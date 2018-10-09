import React, { Component } from "react";
import PropTypes from "prop-types";
import ContextMenuRow from "Components/shared/ContextMenuRow";
import CustomPager from "Components/shared/CustomPager";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import { Grid, Actions } from "react-redux-grid";
import { undoArchivedIscis } from "Ducks/tracker";
import moment from "moment";
import {
  getDateInFormat,
  getSecondsToTimeString
} from "../../../../utils/dateFormatter";

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
      undoArchive: undoArchivedIscis,
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
    const { archivedIscisData } = this.props;
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
          // note: as is selections undefined as multi select not taking on this grid
          // const stateKey = 'archived_grid';
          const selectedIds = this.props.selection.get(stateKey).get("indexes");
          const rowData = this.props.dataSource.get(stateKey).toJSON(); // currentRecords or data - array
          const activeSelections = [];
          // get just slected data FileDetailId for each for API call
          selectedIds.forEach(idx => {
            activeSelections.push(rowData.data[idx].FileDetailId);
          });
          // console.log('undo archive selections', activeSelections, selectedIds, rowData, metaData);
          this.props.undoArchive(activeSelections);
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
          const selectedIds = this.props.selection.getIn([stateKey, "indexes"]);
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
      <Grid
        {...grid}
        data={archivedIscisData}
        store={this.context.store}
        height={460}
        // pageSize={archivedIscisData.length}
        // infinite
      />
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
