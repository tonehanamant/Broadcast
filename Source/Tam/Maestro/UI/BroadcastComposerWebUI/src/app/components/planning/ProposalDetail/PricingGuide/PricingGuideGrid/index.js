/* eslint-disable react/prefer-stateless-function */
import React, { Component } from 'react';
import PropTypes from 'prop-types';
import Table, { withGrid } from 'Lib/react-table';
import { generateData, rowColors, columns } from './util';


class PricingGuideGrid extends Component {
  render() {
    const { openMarketData, openMarketLoading } = this.props;
    const data = generateData(openMarketData.OpenMarkets);
    return (
      <Table
        data={data}
        columns={columns}
        selection="none"
        sortable={false}
        loading={openMarketLoading}
        getTrProps={(state, rowInfo) => ({
            style: { backgroundColor: rowColors[rowInfo.original.rowType] },
        })}
      />
    );
  }
}

PricingGuideGrid.propTypes = {
  openMarketData: PropTypes.object.isRequired,
  openMarketLoading: PropTypes.bool.isRequired,
};

PricingGuideGrid.defaultProps = {};

export default withGrid(PricingGuideGrid);
