import React, { Component } from 'react';
import PropTypes from 'prop-types';
import CSSModules from 'react-css-modules';
import { Typeahead } from 'react-bootstrap-typeahead';
import { ListGroup, ListGroupItem, Button } from 'react-bootstrap/lib';

import styles from './index.scss';

const markets = [
  { Id: 10000, Display: 'All', Count: 210 },
  { Id: 10050, Display: 'Top 50', Count: 50 },
  { Id: 10100, Display: 'Top 100', Count: 100 },
  { Id: 262, Display: 'Abilene-Sweetwater' },
  { Id: 125, Display: 'Albany, GA' },
  { Id: 132, Display: 'Albany-Schenectady-Troy' },
];

class MarketSelector extends Component {
  constructor(props) {
    super(props);

    this.onMarketSelected = this.onMarketSelected.bind(this);
    this.onMarketExcluded = this.onMarketExcluded.bind(this);
    this.clearMarketList = this.clearMarketList.bind(this);

    this.state = {
      selectedMarkets: [],
    };
  }

  onMarketSelected(selectedMarkets) {
    if (selectedMarkets.length > 0) {
      const newMarket = selectedMarkets[0];

      // special market groups (which have a 'Count' property) must be unique and placed at the top
      if (newMarket.Count) {
        const filteredSelectedMarkets = this.state.selectedMarkets.filter(market => market.Count === undefined);
        filteredSelectedMarkets.unshift(newMarket);

        this.setState({
          selectedMarkets: filteredSelectedMarkets,
        });
      } else if (!this.state.selectedMarkets.some(market => market.Id === newMarket.Id)) {
        this.setState({ selectedMarkets: [...this.state.selectedMarkets, newMarket] });
      }

      this.typeahead.instanceRef.clear();
    }
  }

  onMarketExcluded(marketId) {
    const filteredSelectedMarkets = this.state.selectedMarkets.filter(market => market.Id !== marketId);
    this.setState({ selectedMarkets: filteredSelectedMarkets });
  }

  clearMarketList() {
    this.setState({
      selectedMarkets: [],
    });
  }

  render() {
    let marketCount = 0;
    const marketList = this.state.selectedMarkets.map((market) => {
      marketCount += market.Count || 1;

      return (
        <ListGroupItem key={market.Id}>
          <Button
            className="close pull-left"
            onClick={() => this.onMarketExcluded(market.Id)}
          >
            <span aria-hidden="true">&times;</span>
          </Button>

          {market.Display}
        </ListGroupItem>
      );
    });

    return (
      <div>
        <Button
          bsStyle={`btn btn-link proposal-detail-remove-btn pull-right ${styles.trash}`}
          onClick={() => this.clearMarketList()}
        >
          <span className="glyphicon glyphicon-trash pull-right" />
        </Button>

        <span>{this.props.name} ({marketCount})</span>

        <Typeahead
          multiple
          ref={(ref) => { this.typeahead = ref; }}
          placeholder="Type to add..."
          options={markets}
          labelKey="Display"
          onChange={this.onMarketSelected}
        />

        <br />

        <ListGroup>
          {marketList}
        </ListGroup>
      </div>
    );
  }
}

MarketSelector.propTypes = {
  name: PropTypes.string.isRequired,
};

export default CSSModules(MarketSelector, styles);
