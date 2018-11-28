import React from "react";
import numeral from "numeral";
import { get, isNumber } from "lodash";
import PricingGoal from "./PricingGoal";
import PricingOpenMarkets from "./PricingOpenMarkets";
import PricingProprietary from "./PricingProprietary";

export const invSrcEnum = {
  0: "Blank",
  1: "OpenMarket",
  2: "Assembly",
  3: "TVB",
  4: "TTNW",
  5: "CNN",
  6: "Sinclair"
};

export const initialState = {
  isDistributionRunned: false,
  isGuideChanged: false,
  distribution: false,
  discard: false,
  // goals/adjustments - editing version separate state to cancel/save individually
  impression: "",
  budget: "",
  margin: "",
  rateInflation: "",
  impressionInflation: "",

  // proprietary based on array - break down here (uses hard coded values for CPM for now)
  propCpmCNN: 0,
  propCpmSinclair: 0,
  propCpmTTNW: 0,
  propCpmTVB: 0,
  propImpressionsCNN: 0,
  propImpressionsSinclair: 0,
  propImpressionsTTNW: 0,
  propImpressionsTVB: 0,
  // open market
  openCpmMin: null,
  openCpmMax: null,
  openUnitCap: null,
  openCpmTarget: 1
};

export const numberRender = (data, path, format, divideBy) => {
  let number = get(data, path);
  if (number && divideBy) number /= divideBy;
  return isNumber(number) ? numeral(number).format(format) : "--";
};

export const panelsList = [
  {
    render: props => <PricingGoal {...props} />
  },
  {
    render: props => <PricingProprietary {...props} />
  },
  {
    render: props => <PricingOpenMarkets {...props} />
  }
];

export const parsePrograms = (data = []) => {
  const programs = [];
  data.forEach(market => {
    market.Stations.forEach(station => {
      station.Programs.forEach(program => {
        programs.push({
          MarketId: market.MarketId,
          StationCode: station.StationCode,
          DaypartId: program.Daypart.Id,
          ManifestDaypartId: program.ManifestDaypartId,
          ProgramName: program.ProgramName,
          BlendedCpm: program.BlendedCpm,
          Spots: program.Spots,
          ImpressionsPerSpot: program.ImpressionsPerSpot,
          StationImpressionsPerSpot: program.StationImpressionsPerSpot,
          CostPerSpot: program.CostPerSpot
        });
      });
    });
  });
  return programs;
};
