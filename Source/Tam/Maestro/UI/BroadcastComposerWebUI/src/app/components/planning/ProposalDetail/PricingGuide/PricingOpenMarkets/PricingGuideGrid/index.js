import React, { Component } from "react";
import PropTypes from "prop-types";
import PricingGuideGridHeader from "./PricingGuideGridHeader";
import PricingGuideViewGrid from "./PricingGuideViewGrid";
import PricingGuideMasterGrid from "./PricingGuideMasterGrid";

import "./index.scss";

class PricingGuideGrid extends Component {
  constructor(props) {
    super(props);

    this.onSelectMarket = this.onSelectMarket.bind(this);
    this.state = {
      selectedMarket: null
    };
  }

  onSelectMarket(rowIndex, row) {
    this.setState({ selectedMarket: row.original.MarketId });
  }

  render() {
    const {
      activeOpenMarketData,
      hasOpenMarketData,
      onAllocateSpots,
      onSortedChange,
      sorted,
      isOpenMarketDataSortName,
      isGuideEditing
    } = this.props;
    const { selectedMarket } = this.state;

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
            onSelectMarket={this.onSelectMarket}
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
}

PricingGuideGrid.propTypes = {
  activeOpenMarketData: PropTypes.object.isRequired,
  hasOpenMarketData: PropTypes.bool.isRequired,
  isOpenMarketDataSortName: PropTypes.bool.isRequired,
  onAllocateSpots: PropTypes.func.isRequired,
  onSortedChange: PropTypes.func.isRequired,
  sorted: PropTypes.array.isRequired,
  isGuideEditing: PropTypes.bool.isRequired
};

PricingGuideGrid.defaultProps = {
  detailId: null
};

export default PricingGuideGrid;
