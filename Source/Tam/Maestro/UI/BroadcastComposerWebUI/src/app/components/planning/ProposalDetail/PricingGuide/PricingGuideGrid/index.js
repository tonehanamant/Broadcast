import React, { Component } from "react";
import PropTypes from "prop-types";
import Table, { withGrid } from "Lib/react-table";
import PricingGuideGridHeader from "./PricingGuideGridHeader";
import { generateData, rowColors, generateColumns, updateItem } from "./util";

class PricingGuideGrid extends Component {
  constructor(props) {
    super(props);

    this.onCellChange = this.onCellChange.bind(this);
  }

  onCellChange(...args) {
    // const { activeOpenMarketData, allocateSpots, detailId } = this.props;
    const { activeOpenMarketData, onAllocateSpots } = this.props;
    const updatedMarkets = updateItem(activeOpenMarketData.Markets, ...args);
    /* allocateSpots(
      { ...activeOpenMarketData, Markets: updatedMarkets },
      detailId
    ); */
    // intercept to update Distribution Request
    onAllocateSpots({ ...activeOpenMarketData, Markets: updatedMarkets });
  }

  render() {
    const {
      activeOpenMarketData,
      openMarketLoading,
      hasOpenMarketData,
      isOpenMarketDataSortName
    } = this.props;
    const data = generateData(activeOpenMarketData.Markets);
    const columns = generateColumns(this.onCellChange);

    return (
      <div>
        <PricingGuideGridHeader
          activeOpenMarketData={activeOpenMarketData}
          hasOpenMarketData={hasOpenMarketData}
          isOpenMarketDataSortName={isOpenMarketDataSortName}
        />
        <Table
          data={data}
          style={{ marginTop: "6px", maxHeight: "500px" }}
          columns={columns}
          selection="none"
          sortable={false}
          loading={openMarketLoading}
          getTrProps={(state, rowInfo) => ({
            style: { backgroundColor: rowColors[rowInfo.original.rowType] }
          })}
        />
      </div>
    );
  }
}

PricingGuideGrid.propTypes = {
  activeOpenMarketData: PropTypes.object.isRequired,
  hasOpenMarketData: PropTypes.bool.isRequired,
  isOpenMarketDataSortName: PropTypes.bool.isRequired,
  openMarketLoading: PropTypes.bool.isRequired,
  // detailId: PropTypes.number,
  // allocateSpots: PropTypes.func.isRequired
  onAllocateSpots: PropTypes.func.isRequired
};

PricingGuideGrid.defaultProps = {
  detailId: null
};

export default withGrid(PricingGuideGrid);
