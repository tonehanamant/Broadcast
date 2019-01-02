import React from "react";
import PropTypes from "prop-types";
import PricingGuideGridHeader from "./PricingGuideGridHeader";
import PricingGuideViewGrid from "./PricingGuideViewGrid";
import PricingGuideMasterGrid from "./PricingGuideMasterGrid";

import "./index.scss";

function PricingGuideGrid({
  activeOpenMarketData,
  hasOpenMarketData,
  onAllocateSpots,
  onSortedChange,
  sorted,
  isOpenMarketDataSortName,
  onSelectMarket,
  selectedMarket,
  isGuideEditing
}) {
  return (
    <div className="pricing-open-market-grid">
      <PricingGuideGridHeader
        activeOpenMarketData={activeOpenMarketData}
        hasOpenMarketData={hasOpenMarketData}
        isOpenMarketDataSortName={isOpenMarketDataSortName}
        isGuideEditing={isGuideEditing}
      />
      <div className="pricing-open-market-tables">
        <PricingGuideMasterGrid
          activeOpenMarketData={activeOpenMarketData}
          onSelectMarket={onSelectMarket}
          selectedMarket={selectedMarket}
          onSortedChange={onSortedChange}
          sorted={sorted}
        />
        <PricingGuideViewGrid
          activeOpenMarketData={activeOpenMarketData}
          onAllocateSpots={onAllocateSpots}
          selectedMarket={selectedMarket}
        />
      </div>
    </div>
  );
}

PricingGuideGrid.propTypes = {
  activeOpenMarketData: PropTypes.object.isRequired,
  hasOpenMarketData: PropTypes.bool.isRequired,
  isOpenMarketDataSortName: PropTypes.bool.isRequired,
  onAllocateSpots: PropTypes.func.isRequired,
  onSortedChange: PropTypes.func.isRequired,
  onSelectMarket: PropTypes.func.isRequired,
  sorted: PropTypes.array.isRequired,
  selectedMarket: PropTypes.object.isRequired,
  isGuideEditing: PropTypes.bool.isRequired
};

PricingGuideGrid.defaultProps = {
  selectedMarket: {}
};

export default PricingGuideGrid;
