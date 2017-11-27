import React, { Component } from 'react';
import CSSModules from 'react-css-modules';
import { Typeahead } from 'react-bootstrap-typeahead';
import { ListGroup, ListGroupItem } from 'react-bootstrap/lib';

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

    this.state = {
      selectedMarkets: [],
    };
  }

  onMarketSelected(selectedMarkets) {
    if (selectedMarkets.length > 0) {
      const newMarket = selectedMarkets[0];

      if (!this.state.selectedMarkets.some(market => market.Id === newMarket.Id)) {
        this.setState({ selectedMarkets: [...this.state.selectedMarkets, newMarket] });
      }

      this.typeahead.instanceRef.clear();
    }
  }

  render() {
    const marketList = this.state.selectedMarkets.map(market => <ListGroupItem key={market.Id}>{market.Display}</ListGroupItem>);

    return (
      <div>
        <Typeahead
          multiple
          ref={(ref) => { this.typeahead = ref; }}
          placeholder="Type to add..."
          options={markets}
          labelKey="Display"
          onChange={this.onMarketSelected}
        />

        <br />

        <ListGroup>{marketList}</ListGroup>
      </div>
    );
  }
}

export default CSSModules(MarketSelector, styles);
