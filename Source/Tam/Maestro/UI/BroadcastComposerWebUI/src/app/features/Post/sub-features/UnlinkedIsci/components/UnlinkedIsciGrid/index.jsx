/* eslint-disable react/prop-types */
import React from "react";
import PropTypes from "prop-types";
import ContextMenuRow from "Patterns/ContextMenuRow";
import CustomPager from "Patterns/CustomPager";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import { Grid, Actions } from "Lib/react-redux-grid";
import { unlinkedIsciActions } from "Post";

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
      rescrubIscis: unlinkedIsciActions.rescrubUnlinkedIscis,
      archiveIscis: unlinkedIsciActions.archiveUnlinkedIscis,
      deselectAll,
      selectRow
    },
    dispatch
  );

function UnlinkedIsciGrid({
  unlinkedIscisData,
  archiveIscis,
  rescrubIscis,
  toggleModal
}) {
  const stateKey = "unlinked_grid";
  const columns = [
    {
      name: "ISCI",
      dataIndex: "ISCI",
      width: "25%"
    },
    {
      name: "Unlinked Reason",
      dataIndex: "UnlinkedReason",
      width: "25%",
      renderer: ({ row }) => <span>{row.UnlinkedReason || "-"}</span>
    },
    {
      name: "Count",
      dataIndex: "Count",
      width: "25%"
    },
    {
      name: "Spot Length",
      dataIndex: "SpotLength",
      width: "25%",
      renderer: ({ row }) => <span>{row.SpotLength || "-"}</span>
    }
  ];

  const menuItems = [
    {
      text: "Not a Cadent ISCI",
      key: "menu-archive-isci",
      EVENT_HANDLER: ({ metaData }) => {
        archiveIscis([metaData.rowData.ISCI]);
      }
    },
    {
      text: "Rescrub this ISCI",
      key: "menu-rescrub-isci",
      EVENT_HANDLER: ({ metaData }) => {
        rescrubIscis(metaData.rowData.ISCI);
      }
    },
    {
      text: "Map ISCI",
      key: "menu-map-isci",
      EVENT_HANDLER: ({ metaData }) => {
        toggleModal({
          modal: "mapUnlinkedIsci",
          active: true,
          properties: { rowData: metaData.rowData }
        });
      }
    }
  ];

  const beforeOpenMenu = rowId => {
    deselectAll({ stateKey });
    selectRow({ rowId, stateKey });
  };

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
          beforeOpenMenu={beforeOpenMenu}
        >
          {cells}
        </ContextMenuRow>
      )
    }
  };

  const grid = {
    columns,
    plugins,
    stateKey
  };

  return <Grid {...grid} data={unlinkedIscisData} height={460} />;
}

UnlinkedIsciGrid.propTypes = {
  toggleModal: PropTypes.func.isRequired,
  rescrubIscis: PropTypes.func.isRequired,
  unlinkedIscisData: PropTypes.array.isRequired,
  archiveIscis: PropTypes.func.isRequired,
  dataSource: PropTypes.object.isRequired,
  selection: PropTypes.object.isRequired,
  grid: PropTypes.object.isRequired
};

export default connect(
  mapStateToProps,
  mapDispatchToProps
)(UnlinkedIsciGrid);
