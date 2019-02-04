import React, { Component } from "react";
import PropTypes from "prop-types";
import Table, { withGrid } from "Lib/react-table";
import {
  generateStationData,
  stationColumns,
  generateProgramData,
  generateProgramColumns
} from "./util";
import { rowColors, updateItem } from "../util";

const WARN_MESSAGE_NOT_SELECTED = "Please select market to display programs.";
const WARN_MESSAGE_NO_DATA = "No data found.";

const getPlaceholder = selectedMarket =>
  selectedMarket ? WARN_MESSAGE_NO_DATA : WARN_MESSAGE_NOT_SELECTED;

const initialState = {
  expanded: {}
};

class PricingGuideGridView extends Component {
  constructor(props) {
    super(props);

    this.state = initialState;
    this.onCellChange = this.onCellChange.bind(this);
    this.onExpandedChange = this.onExpandedChange.bind(this);
  }

  componentDidUpdate(nextProps) {
    const { selectedMarket } = this.props;
    if (selectedMarket.marketId !== nextProps.selectedMarket.marketId) {
      this.clearState();
    }
  }

  onCellChange(fieldName, value, row) {
    const { activeOpenMarketData, onAllocateSpots } = this.props;
    const updatedMarkets = updateItem(
      activeOpenMarketData.Markets,
      fieldName,
      value,
      row
    );
    onAllocateSpots(
      { ...activeOpenMarketData, Markets: updatedMarkets },
      { ...row, [fieldName]: value }
    );
  }

  onExpandedChange(nextValue) {
    this.setState({ expanded: nextValue });
  }

  clearState() {
    this.setState(initialState);
  }

  render() {
    const { activeOpenMarketData, selectedMarket } = this.props;
    const { expanded } = this.state;
    const stationData = generateStationData(
      activeOpenMarketData.Markets,
      selectedMarket.marketId
    );
    const placeholder = getPlaceholder(selectedMarket);
    return (
      <Table
        id="pricing-guide-detail-table"
        sortable={false}
        collapseOnDataChange={false}
        data={stationData}
        expanded={expanded}
        onExpandedChange={this.onExpandedChange}
        noDataText={placeholder}
        columns={stationColumns}
        selection="none"
        getTrProps={(state, rowInfo) => ({
          style: { backgroundColor: rowColors[rowInfo.original.rowType] }
        })}
        SubComponent={row => {
          const data = generateProgramData(row.original);
          const columns = generateProgramColumns(this.onCellChange);
          return <Table data={data} columns={columns} selection="none" />;
        }}
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
