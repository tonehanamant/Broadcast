/* eslint-disable react/prop-types */
import React from "react";
import { EditableCell } from "Lib/react-table";
import {
  rowTypes,
  GroupingCell,
  DollarCell,
  ImpressionCell,
  SpotCell
} from "../util";

export const generateStationData = (markets, selectedMarket) => {
  if (!selectedMarket) return [];
  const market = markets.find(m => m.MarketId === selectedMarket);
  const data = market.Stations.filter(
    ({ Programs }) => Programs && Programs.length
  ).map(station => ({
    rowType: rowTypes.SUB_TITLE,
    Station: `${station.CallLetters} (${station.Affiliation})`,
    isStation: true,
    MarketId: market.MarketId,
    StationCode: station.StationCode,
    TotalCost: station.TotalCost,
    CPM: station.CPM,
    TotalImpressions: station.TotalImpressions
      ? station.TotalImpressions / 1000
      : station.TotalImpressions,
    TotalSpots: station.TotalSpots,
    Programs: station.Programs
  }));

  return data || [];
};

export const generateProgramData = station =>
  station.Programs.map(program => ({
    rowType: rowTypes.DATA_ROW,
    AiringTime: program.Daypart.Display,
    Program: program.ProgramName,
    CPM: program.BlendedCpm,
    Spots: program.Spots,
    Impressions: program.DisplayImpressions
      ? program.DisplayImpressions / 1000
      : program.DisplayImpressions,
    StationImpressions: program.DisplayStationImpressions
      ? program.DisplayStationImpressions / 1000
      : program.DisplayStationImpressions,
    HasImpressions: program.HasImpressions,
    MarketId: station.MarketId,
    StationCode: station.StationCode,
    ProgramId: program.ProgramId,
    Cost: program.DisplayCost,
    isProgram: true // need for future use
  }));

export const renderSpots = onChange => ({ value, original }) =>
  original.HasImpressions ? (
    <EditableCell
      allowSubmitEmpty
      mask="integer"
      value={value}
      onChange={(...arg) => {
        onChange(...arg, original);
      }}
      id="Spots"
    />
  ) : (
    SpotCell({ value, original })
  );

export const stationColumns = [
  {
    expander: true
  },
  {
    Header: "Station",
    accessor: "Station",
    Cell: GroupingCell
  },
  {
    Header: "CPM",
    accessor: "CPM",
    Cell: DollarCell
  },
  {
    Header: "Spots",
    accessor: "TotalSpots",
    Cell: SpotCell
  },
  {
    Header: "Impressions(000)",
    accessor: "TotalImpressions",
    Cell: ImpressionCell
  },
  {
    Header: "Cost",
    accessor: "TotalCost",
    Cell: DollarCell
  }
];

export const generateProgramColumns = onChange => [
  {
    Header: "Airing Time",
    accessor: "AiringTime",
    minWidth: 40,
    Cell: GroupingCell
  },
  {
    Header: "Program",
    accessor: "Program",
    minWidth: 120
  },
  {
    Header: "CPM",
    accessor: "CPM",
    Cell: DollarCell
  },
  {
    Header: "Spots",
    accessor: "Spots",
    Cell: renderSpots(onChange)
  },
  {
    Header: "Impressions(000)",
    accessor: "Impressions",
    Cell: ImpressionCell
  },
  {
    Header: "Station Impressions",
    accessor: "StationImpressions",
    Cell: ImpressionCell
  },
  {
    Header: "Cost",
    accessor: "Cost",
    Cell: DollarCell
  }
];
