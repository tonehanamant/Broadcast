import React from "react";
import numeral from "numeral";
import { get, isNumber } from "lodash";
import PricingGoal from "./PricingGoal";
import PricingOpenMarkets from "./PricingOpenMarkets";
import PricingProprietary from "./PricingProprietary";

export const initialState = {
  isDistributionRunned: false,
  isGuideChanged: false,
  distribution: false,
  discard: false,
  // goals/adjustments - editing version separate state to cancel/save individually
  impression: 0,
  budget: 0,
  margin: 0,
  rateInflation: 0,
  impressionInflation: 0,

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
    id: "goal",
    render: props => <PricingGoal {...props} />
  },
  {
    id: "proprietary",
    render: props => <PricingProprietary {...props} />
  },
  {
    id: "open-markets",
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

export const calculateBalanceSum = (inventorySrc, props) =>
  inventorySrc.reduce(
    (currentValue, i) =>
      (props[`propImpressions${i.Display}`] || 0) + currentValue,
    0
  );
