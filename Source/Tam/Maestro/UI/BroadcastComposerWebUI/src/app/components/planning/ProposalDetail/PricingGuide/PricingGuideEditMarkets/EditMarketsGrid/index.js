/* eslint-disable react/prefer-stateless-function */
import React, { Component } from "react";
import PropTypes from "prop-types";
import Table, { withGrid } from "Lib/react-table";
import { Button, Glyphicon } from "react-bootstrap";
import numeral from "numeral";
// import { generateData, rowColors, columns } from "./util";

class EditMarketsGrid extends Component {
  constructor(props) {
    super(props);
    this.onEditMarketAction = this.onEditMarketAction.bind(this);
  }

  onEditMarketAction(row) {
    // console.log("onEditMarketAction", rec);
    this.props.editMarketAction(row.original);
  }

  render() {
    const { editMarketsData, isAvailableMarkets } = this.props;
    // console.log(editMarketsData, isAvailableMarkets);
    const glyph = isAvailableMarkets ? (
      <Glyphicon style={{ color: "green" }} glyph="plus" />
    ) : (
      <Glyphicon style={{ color: "#c12e2a" }} glyph="trash" />
    );
    const columns = [
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
        Cell: value => numeral(value).format("0,0.[000]")
        // minWidth: 50
      },
      {
        Header: "Stations",
        accessor: "Stations"
        // minWidth: 50
      },
      {
        Header: "Programs",
        accessor: "Programs"
        // minWidth: 70
      },
      {
        Header: "Action",
        sortable: false,
        minWidth: 50,
        // accessor: "Impressions",
        // Cell: ImpressionCell
        Cell: row => (
          <div style={{ textAlign: "center" }}>
            <Button
              bsStyle="link"
              bsSize="xsmall"
              onClick={() => this.onEditMarketAction(row)}
            >
              {glyph}
            </Button>
          </div>
        )
      }
    ];
    return (
      // <Well bsSize="small">// <Well bsSize="small">
      <div>
        <Table
          data={editMarketsData}
          style={{ marginTop: "6px", maxHeight: "300px" }}
          columns={columns}
          selection="none"
          sortable
          // loading={openMarketLoading}
          /* getTrProps={(state, rowInfo) => ({
            style: { backgroundColor: rowColors[rowInfo.original.rowType] }
          })} */
        />
      </div>
      // </Well>
    );
  }
}

EditMarketsGrid.propTypes = {
  editMarketsData: PropTypes.array.isRequired,
  isAvailableMarkets: PropTypes.bool.isRequired,
  editMarketAction: PropTypes.func.isRequired
};

EditMarketsGrid.defaultProps = {
  editMarketAction: () => {},
  isAvailableMarkets: false
};

export default withGrid(EditMarketsGrid);
