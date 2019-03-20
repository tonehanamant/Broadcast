import React, { Component } from "react";
import PropTypes from "prop-types";
import CSSModules from "react-css-modules";
import { sortBy } from "lodash";
import { Typeahead } from "react-bootstrap-typeahead";
import { ListGroup, ListGroupItem, Button, Well } from "react-bootstrap";

import styles from "./index.style.scss";

class MarketSelector extends Component {
  constructor(props) {
    super(props);

    this.onMarketSelected = this.onMarketSelected.bind(this);
    this.onMarketExcluded = this.onMarketExcluded.bind(this);
    this.onClearMarketList = this.onClearMarketList.bind(this);

    this.typeaheadOptions = props.markets;
  }

  // clear typeahead and update parent state (market groups are placed in the beginning)
  onMarketSelected(typeaheadSelection) {
    const { name } = this.props;
    if (typeaheadSelection.length > 0) {
      const newMarket = typeaheadSelection[0];
      this.typeahead.instanceRef.clear();

      const { selectedMarkets, onMarketsSelectionChange } = this.props;
      if (newMarket.Count) {
        const marketListWithGroup = selectedMarkets.filter(
          market => market && market.Count === undefined
        );
        marketListWithGroup.unshift(newMarket);
        onMarketsSelectionChange(marketListWithGroup, name);
      } else if (!selectedMarkets.some(market => market.Id === newMarket.Id)) {
        const marketListWithSingle = [...selectedMarkets, newMarket];
        onMarketsSelectionChange(marketListWithSingle, name);
      }
    }
  }

  onMarketExcluded(marketId) {
    const { selectedMarkets, onMarketsSelectionChange, name } = this.props;
    const filteredSelectedMarkets = selectedMarkets.filter(
      market => market && market.Id !== marketId
    );
    onMarketsSelectionChange(filteredSelectedMarkets, name);
  }

  onClearMarketList() {
    const { name, onMarketsSelectionChange } = this.props;
    onMarketsSelectionChange([], name);
  }

  render() {
    const { isReadOnly, selectedMarkets, name } = this.props;
    let marketCount = 0;
    const sortedList = sortBy(selectedMarkets, ["Display"]);
    const marketList = sortedList.map(market => {
      if (market) {
        marketCount += market.Count ? market.Count : 1;

        return (
          <ListGroupItem key={market.Id}>
            <Button
              styleName="close pull-left"
              style={{ marginRight: "5px" }}
              disabled={isReadOnly}
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
          styleName={`pull-right ${styles.trash}`}
          bsStyle="link"
          disabled={isReadOnly}
          onClick={() => this.onClearMarketList()}
        >
          <span className="glyphicon glyphicon-trash pull-right" />
        </Button>

        <span style={{ fontSize: "18px" }}>
          {name} ({marketCount})
        </span>

        <Well>
          <Typeahead
            multiple
            ref={ref => {
              this.typeahead = ref;
            }}
            placeholder="Type to add..."
            options={this.typeaheadOptions}
            disabled={isReadOnly}
            labelKey="Display"
            onChange={this.onMarketSelected}
          />

          <br />

          <ListGroup>{marketList}</ListGroup>
        </Well>
      </div>
    );
  }
}

MarketSelector.propTypes = {
  name: PropTypes.string.isRequired,

  markets: PropTypes.arrayOf(
    PropTypes.shape({
      Id: PropTypes.number,
      Display: PropTypes.string
    })
  ),

  selectedMarkets: PropTypes.arrayOf(
    PropTypes.shape({
      Id: PropTypes.number,
      Display: PropTypes.string
    })
  ),

  onMarketsSelectionChange: PropTypes.func,
  isReadOnly: PropTypes.bool
};

MarketSelector.defaultProps = {
  markets: [],
  selectedMarkets: [],
  onMarketsSelectionChange: null,
  isReadOnly: false
};

export default CSSModules(MarketSelector, styles);
