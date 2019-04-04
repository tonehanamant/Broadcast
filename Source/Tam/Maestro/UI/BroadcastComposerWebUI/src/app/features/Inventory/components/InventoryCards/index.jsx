import React from "react";
import PropTypes from "prop-types";
import CSSModules from "react-css-modules";
import InventoryCard from "Inventory/components/InventoryCard";
import { displayQuarter, displayQuarterRates } from "Utils/cards";

import styles from "./index.style.scss";

const BARTER_CARD = 2;

const cardsBuildersMap = {
  [BARTER_CARD]: (card, source) => (
    <InventoryCard
      key={`inventory-card_#${source.Id}`}
      title={card.InventorySourceName}
      updatedTime={card.LastUpdatedDate}
      markets={card.TotalMarkets}
      stations={card.TotalStations}
      units={card.TotalUnits}
      dayparts={card.TotalDaypartCodes}
      impression={card.HouseholdImpressions}
      rates={displayQuarterRates(
        card.RatesAvailableFromQuarter,
        card.RatesAvailableToQuarter
      )}
      quarter={displayQuarter(card.Quarter)}
    />
  )
};

const buildCardComponent = (data, source) => {
  const cardBuilder = cardsBuildersMap[source.InventoryType];
  if (cardBuilder) {
    return cardBuilder(data, source);
  }
  return null;
};

const transformCards = (data, sources) =>
  sources.map(source => {
    const cardData = data.find(it => it.InventorySourceId === source.Id);
    return buildCardComponent(cardData, source);
  });

const InventoryCards = ({ data, initialData }) => {
  const cards = transformCards(data, initialData.InventorySources);
  return <section styleName="card-list-section">{cards}</section>;
};

InventoryCards.propTypes = {
  initialData: PropTypes.object.isRequired,
  data: PropTypes.array.isRequired
};

export default CSSModules(InventoryCards, styles);
