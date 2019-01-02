import React, { Component } from "react";
import PropTypes from "prop-types";
import Table, { withGrid } from "Lib/react-table";
import {
  generateProgramData,
  generateProgramColumns,
  rowColors,
  updateItem
} from "../util";

const WARN_MESSAGE_NOT_SELECTED = "Please select market to display programs.";
const WARN_MESSAGE_NO_DATA = "No data found.";

const getPlaceholder = selectedMarket =>
  selectedMarket ? WARN_MESSAGE_NO_DATA : WARN_MESSAGE_NOT_SELECTED;

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
      selectedMarket.marketId
    );
    const programsColumns = generateProgramColumns(this.onCellChange);
    const placeholder = getPlaceholder(selectedMarket);
    return (
      <Table
        data={programs}
        noDataText={placeholder}
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
  selectedMarket: PropTypes.object.isRequired,
  onAllocateSpots: PropTypes.func.isRequired
};

PricingGuideGridView.defaultProps = {};

export default withGrid(PricingGuideGridView);
