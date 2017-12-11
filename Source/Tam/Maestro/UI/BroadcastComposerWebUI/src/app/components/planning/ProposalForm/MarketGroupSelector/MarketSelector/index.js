import React, { Component } from 'react';
import PropTypes from 'prop-types';
import CSSModules from 'react-css-modules';
import { Typeahead } from 'react-bootstrap-typeahead';
import { ListGroup, ListGroupItem, Button, Well } from 'react-bootstrap/lib';

import styles from './index.scss';

class MarketSelector extends Component {
  constructor(props) {
    super(props);

    this.onMarketSelected = this.onMarketSelected.bind(this);
    this.onMarketExcluded = this.onMarketExcluded.bind(this);
    this.onClearMarketList = this.onClearMarketList.bind(this);

    const { marketGroups, markets } = this.props;
    this.typeaheadOptions = marketGroups.concat(markets);
  }

  // clear typeahead and update parent state (market groups are placed in the beginning)
  onMarketSelected(typeaheadSelection) {
    if (typeaheadSelection.length > 0) {
      const newMarket = typeaheadSelection[0];
      this.typeahead.instanceRef.clear();

      const { selectedMarkets, onMarketsSelectionChange } = this.props;
      if (newMarket.Count) {
        const marketListWithGroup = selectedMarkets.filter(market => market && market.Count === undefined);
        marketListWithGroup.unshift(newMarket);
        onMarketsSelectionChange(marketListWithGroup, this.props.name);
      } else if (!selectedMarkets.some(market => market.Id === newMarket.Id)) {
          const marketListWithSingle = [...selectedMarkets, newMarket];
          onMarketsSelectionChange(marketListWithSingle, this.props.name);
      }
    }
  }

  onMarketExcluded(marketId) {
    const { selectedMarkets, onMarketsSelectionChange } = this.props;
    const filteredSelectedMarkets = selectedMarkets.filter(market => market && market.Id !== marketId);
    onMarketsSelectionChange(filteredSelectedMarkets, this.props.name);
  }

  onClearMarketList() {
    this.props.onMarketsSelectionChange([], this.props.name);
  }

  render() {
    let marketCount = 0;
    const marketList = this.props.selectedMarkets.map((market) => {
      if (market) {
        marketCount += market.Count ? market.Count : 1;

        return (
          <ListGroupItem key={market.Id}>
            <Button
              className="close pull-left"
              style={{ marginRight: '5px' }}
              onClick={() => this.onMarketExcluded(market.Id)}
            >
              <span aria-hidden="true">&times;</span>
            </Button>

            {market.Display}
          </ListGroupItem>
        );
      }

      return market;
    });

    return (
      <div>
        <Button
          className={`pull-right ${styles.trash}`}
          bsStyle="link"
          onClick={() => this.onClearMarketList()}
        >
          <span className="glyphicon glyphicon-trash pull-right" />
        </Button>

        <span style={{ fontSize: '18px' }}>{this.props.name} ({marketCount})</span>

        <Well>
          <Typeahead
            multiple
            ref={(ref) => { this.typeahead = ref; }}
            placeholder="Type to add..."
            options={this.typeaheadOptions}
            labelKey="Display"
            onChange={this.onMarketSelected}
          />

          <br />

          <ListGroup>
            {marketList}
          </ListGroup>
        </Well>
      </div>
    );
  }
}

MarketSelector.propTypes = {
  name: PropTypes.string.isRequired,

  marketGroups: PropTypes.arrayOf(PropTypes.shape({
    Id: PropTypes.number,
    Display: PropTypes.string,
    Count: PropTypes.number,
  })),

  markets: PropTypes.arrayOf(PropTypes.shape({
    Id: PropTypes.number,
    Display: PropTypes.string,
  })),

  selectedMarkets: PropTypes.arrayOf(PropTypes.shape({
    Id: PropTypes.number,
    Display: PropTypes.string,
  })),

  onMarketsSelectionChange: PropTypes.func.isRequired,
};

MarketSelector.defaultProps = {
  marketGroups: [],
  markets: [],
  selectedMarkets: [],
};

export default CSSModules(MarketSelector, styles);
