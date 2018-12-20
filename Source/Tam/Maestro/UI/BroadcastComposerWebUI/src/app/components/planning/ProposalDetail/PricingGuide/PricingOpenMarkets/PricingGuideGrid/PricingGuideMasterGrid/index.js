import React from "react";
import PropTypes from "prop-types";
import Table, { withGrid } from "Lib/react-table";
import { marketColumns, generateMarketData } from "../util";

function PricingGuideGridView({
  activeOpenMarketData,
  onSelectMarket,
  onSortedChange,
  selectedMarket,
  sorted
}) {
  const market = generateMarketData(activeOpenMarketData.Markets);
  return (
    <Table
      selectOnRender
      data={market}
      columns={marketColumns}
      selection="single"
      onSelect={onSelectMarket}
      onSortedChange={onSortedChange}
      sorted={sorted}
      selected={[selectedMarket.rowIndex]}
    />
  );
}

PricingGuideGridView.propTypes = {
  activeOpenMarketData: PropTypes.object.isRequired,
  onSelectMarket: PropTypes.func.isRequired,
  onSortedChange: PropTypes.func.isRequired,
  sorted: PropTypes.array.isRequired,
  selectedMarket: PropTypes.object.isRequired
};

PricingGuideGridView.defaultProps = {};

export default withGrid(PricingGuideGridView);
