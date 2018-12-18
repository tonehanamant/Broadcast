import React, { Component } from "react";
import PropTypes from "prop-types";
import Table, { withGrid } from "Lib/react-table";
import {
  generateProgramData,
  generateProgramColumns,
  rowColors,
  updateItem
} from "../util";

class PricingGuideGridView extends Component {
  constructor(props) {
    super(props);

    this.onCellChange = this.onCellChange.bind(this);
  }

  onCellChange(...args) {
    const { activeOpenMarketData, onAllocateSpots } = this.props;
    const updatedMarkets = updateItem(activeOpenMarketData.Markets, ...args);
    onAllocateSpots({ ...activeOpenMarketData, Markets: updatedMarkets });
  }

  render() {
    const { activeOpenMarketData, selectedMarket } = this.props;
    const programs = generateProgramData(
      activeOpenMarketData.Markets,
      selectedMarket
    );
    const programsColumns = generateProgramColumns(this.onCellChange);
    return (
      <Table
        data={programs}
        noDataText="Please select market to display programs."
        columns={programsColumns}
        selection="none"
        sortable={false}
        getTrProps={(state, rowInfo) => ({
          style: { backgroundColor: rowColors[rowInfo.original.rowType] }
        })}
      />
    );
  }
}

PricingGuideGridView.propTypes = {
  activeOpenMarketData: PropTypes.object.isRequired,
  selectedMarket: PropTypes.number,
  onAllocateSpots: PropTypes.func.isRequired
};

PricingGuideGridView.defaultProps = {
  selectedMarket: null
};

export default withGrid(PricingGuideGridView);
