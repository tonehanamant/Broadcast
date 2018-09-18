/* eslint-disable react/prefer-stateless-function */
import React, { Component } from 'react';
import PropTypes from 'prop-types';
import Table, { withGrid } from 'Lib/react-table';
// import { Well } from 'react-bootstrap';
import PricingGuideGridHeader from './PricingGuideGridHeader';
import { generateData, rowColors, columns } from './util';


class PricingGuideGrid extends Component {
  render() {
    const { openMarketData, openMarketLoading } = this.props;
    const data = generateData(openMarketData.Markets);
    return (
      // <Well bsSize="small">
      <div>
      <PricingGuideGridHeader
        openMarketsData={openMarketData}
      />
      <Table
        data={data}
        style={{ marginTop: '6px' }}
        columns={columns}
        selection="none"
        sortable={false}
        loading={openMarketLoading}
        getTrProps={(state, rowInfo) => ({
            style: { backgroundColor: rowColors[rowInfo.original.rowType] },
        })}
      />
      </div>
    // </Well>
    );
  }
}

PricingGuideGrid.propTypes = {
  openMarketData: PropTypes.object.isRequired,
  openMarketLoading: PropTypes.bool.isRequired,
};

PricingGuideGrid.defaultProps = {};

export default withGrid(PricingGuideGrid);
