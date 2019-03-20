import React, { Component } from "react";
import PropTypes from "prop-types";
import CSSModules from "react-css-modules";
import PricingGuideGridHeader from "../OpenMarketsGridHeader";
import PricingGuideViewGrid from "../OpenMarketsViewGrid";
import PricingGuideMasterGrid from "../OpenMarketsMasterGrid";

import styles from "./index.style.scss";

class PricingGuideGrid extends Component {
  componentDidMount() {
    const { resetTable } = this.props;
    resetTable();
  }

  componentWillUnmount() {
    const { onSelectMarket } = this.props;
    onSelectMarket();
  }

  render() {
    const {
      activeOpenMarketData,
      hasOpenMarketData,
      onAllocateSpots,
      onSortedChange,
      sorted,
      isOpenMarketDataSortName,
      onSelectMarket,
      selectedMarket,
      isGuideEditing
    } = this.props;
    return (
      <div styleName="pricing-open-market-grid">
        <PricingGuideGridHeader
          activeOpenMarketData={activeOpenMarketData}
          hasOpenMarketData={hasOpenMarketData}
          isOpenMarketDataSortName={isOpenMarketDataSortName}
          isGuideEditing={isGuideEditing}
        />
        <div styleName="pricing-open-market-tables">
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
}

PricingGuideGrid.propTypes = {
  activeOpenMarketData: PropTypes.object.isRequired,
  hasOpenMarketData: PropTypes.bool.isRequired,
  isOpenMarketDataSortName: PropTypes.bool.isRequired,
  onAllocateSpots: PropTypes.func.isRequired,
  onSortedChange: PropTypes.func.isRequired,
  onSelectMarket: PropTypes.func.isRequired,
  resetTable: PropTypes.func.isRequired,
  sorted: PropTypes.array.isRequired,
  selectedMarket: PropTypes.object.isRequired,
  isGuideEditing: PropTypes.bool.isRequired
};

PricingGuideGrid.defaultProps = {};

export default CSSModules(PricingGuideGrid, styles);
