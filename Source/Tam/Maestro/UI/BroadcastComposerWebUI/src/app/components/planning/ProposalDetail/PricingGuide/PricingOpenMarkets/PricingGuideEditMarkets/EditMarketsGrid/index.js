import React, { Component } from "react";
import PropTypes from "prop-types";
import Table, { withGrid } from "Lib/react-table";
import { Button, Glyphicon } from "react-bootstrap";
import numeral from "numeral";

const cpmTargetsMap = {
  1: "MinCpm",
  2: "AvgCpm",
  3: "MaxCpm"
};

const newMarketStyle = (markets, marketId) =>
  markets.includes(marketId) ? { backgroundColor: "#eeeeee" } : {};

const getGlyph = isAvailableMarkets =>
  isAvailableMarkets ? (
    <Glyphicon style={{ color: "green" }} glyph="plus" />
  ) : (
    <Glyphicon style={{ color: "#c12e2a" }} glyph="trash" />
  );

const generateColums = (
  openCpmTarget,
  isAvailableMarkets,
  onEditMarketAction
) => [
  {
    Header: "Market",
    accessor: "Name",
    filterable: true,
    minWidth: 180,
    filterMethod: (filter, row) =>
      row[filter.id].toLowerCase().includes(filter.value.toLowerCase())
  },
  {
    Header: "Coverage",
    accessor: "Coverage",
    Cell: row => numeral(row.value).format("0,0.[000]")
  },
  {
    Header: "Stations",
    accessor: "Stations"
  },
  {
    Header: "Programs",
    accessor: "Programs"
  },
  {
    Header: "CPM",
    Cell: row => {
      const mapped = cpmTargetsMap[openCpmTarget];
      const val = row.original[mapped] || 0;
      const display = numeral(val).format("0,0.[00]");
      return `$${display}`;
    }
  },
  {
    Header: "Action",
    sortable: false,
    minWidth: 50,
    Cell: row => (
      <div style={{ textAlign: "center" }}>
        <Button
          bsStyle="link"
          bsSize="xsmall"
          onClick={() => onEditMarketAction(row)}
        >
          {getGlyph(isAvailableMarkets)}
        </Button>
      </div>
    )
  },
  {
    Header: "New",
    accessor: "newItem",
    id: "newItem",
    show: false
  }
];

const generateData = (markets, changedMarkets) =>
  markets.map(it => ({ ...it, newItem: changedMarkets.includes(it.Id) }));

class EditMarketsGrid extends Component {
  constructor(props) {
    super(props);
    this.onEditMarketAction = this.onEditMarketAction.bind(this);
  }

  onEditMarketAction(row) {
    this.props.editMarketAction(row.original);
  }

  render() {
    const {
      editMarketsData,
      isAvailableMarkets,
      openCpmTarget,
      changedMarkets,
      sorted,
      onSortedChange
    } = this.props;

    const columns = generateColums(
      openCpmTarget,
      isAvailableMarkets,
      this.onEditMarketAction
    );
    const data = generateData(editMarketsData, changedMarkets);
    return (
      <Table
        data={data}
        style={{ marginTop: "6px", maxHeight: "300px" }}
        columns={columns}
        selection="none"
        sortable
        sorted={sorted}
        onSortedChange={onSortedChange}
        getTrProps={(state, { original: { Id } }) => {
          const newMarket = newMarketStyle(changedMarkets, Id);
          return { style: newMarket };
        }}
      />
    );
  }
}

EditMarketsGrid.propTypes = {
  editMarketsData: PropTypes.array.isRequired,
  changedMarkets: PropTypes.array.isRequired,
  sorted: PropTypes.array.isRequired,
  isAvailableMarkets: PropTypes.bool.isRequired,
  editMarketAction: PropTypes.func.isRequired,
  onSortedChange: PropTypes.func.isRequired,
  openCpmTarget: PropTypes.number.isRequired
};

EditMarketsGrid.defaultProps = {
  editMarketAction: () => {},
  isAvailableMarkets: false
};

export default withGrid(EditMarketsGrid);
