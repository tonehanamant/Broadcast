import React from "react";
import numeral from "numeral";
import { get, isNumber } from "lodash";
import PricingGoal from "./PricingGoal";
import PricingOpenMarkets from "./PricingOpenMarkets";
import PricingProprietary from "./PricingProprietary";

export const initialState = {
  isDistributionRunned: true,
  isGuideChanged: false,
  isSpotsChanged: false,
  isGuideEditing: false,
  distribution: false,
  discard: false,
  discardSpots: false,
  confirmationDistribution: false,
  isAutoDistribution: true,
  isGuideApplied: false,
  changedPrograms: [],

  // goals/adjustments - editing version separate state to cancel/save individually
  impression: 0,
  budget: 0,
  margin: 0,
  impressionLoss: 0,
  inflation: 0,

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

export const parseProgramsToList = (data = []) => {
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
          ProgramId: program.ProgramId,
          Spots: program.Spots,
          ImpressionsPerSpot: program.ImpressionsPerSpot,
          SpotsEditedManually: program.SpotsEditedManually,
          StationImpressionsPerSpot: program.StationImpressionsPerSpot,
          CostPerSpot: program.CostPerSpot
        });
      });
    });
  });
  return programs;
};

export const getDistributionPrograms = (data, changedPrograms) => {
  const programs = [];
  data.forEach(market => {
    market.Stations.forEach(station => {
      station.Programs.forEach(program => {
        if (changedPrograms.includes(program.ProgramId) && program.Spots > 0) {
          programs.push({
            ManifestDaypartId: program.ManifestDaypartId,
            Spots: program.Spots
          });
        }
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

export const parseToPercent = value => (value ? value * 100 : value);
export const parseFromPercent = value => (value ? value / 100 : value);
