/* eslint-disable react/prop-types */
import React from "react";
import PropTypes from "prop-types";
import numeral from "numeral";
import { isNil, update, findIndex, get } from "lodash";
import { EditableCell } from "Lib/react-table";

const findFieldIdx = (data, fieldName, rowData) =>
  findIndex(data, it => it[fieldName] === rowData[fieldName]);

const buildFieldPath = (data, rowData, fieldName) => {
  const marketIndex = findFieldIdx(data, "MarketId", rowData);
  let fieldPath = `[${marketIndex}]`;
  if (!rowData.isMarket) {
    fieldPath = `${fieldPath}.Stations`;
    const station = get(data, fieldPath);
    const stationIndex = findFieldIdx(station, "StationCode", rowData);
    fieldPath = `${fieldPath}[${stationIndex}]`;
    if (!rowData.isStation) {
      fieldPath = `${fieldPath}.Programs`;
      const program = get(data, fieldPath);
      const programIndex = findFieldIdx(program, "ProgramId", rowData);
      fieldPath = `${fieldPath}[${programIndex}]`;
    }
  }
  return `${fieldPath}.${fieldName}`;
};

export const updateItem = (data, fieldName, value, rowData) => {
  const fieldPath = buildFieldPath(data, rowData, fieldName);
  return update(data, fieldPath, () => value);
};

const rowTypes = {
  TITLE: "TITLE",
  SUB_TITLE: "SUB_TITLE",
  DATA_ROW: "DATA_ROW",
  EMPTY_ROW: "EMPTY_ROW"
};
const rowColors = {
  TITLE: "#dedede",
  SUB_TITLE: "#E9E9E9",
  DATA_ROW: "#fff",
  EMPTY_ROW: "#fff"
};

const boldRowTypes = [rowTypes.TITLE, rowTypes.SUB_TITLE];

const generateProgramData = (markets, selectedMarket) => {
  const data = [];
  if (!selectedMarket) return data;
  const market = markets.find(m => m.MarketId === selectedMarket);
  market.Stations.filter(({ Programs }) => Programs && Programs.length).forEach(
    (station, idx) => {
      data.push({
        rowType: rowTypes.SUB_TITLE,
        AiringTime: `${station.CallLetters} (${station.LegacyCallLetters})`,
        isStation: true,
        MarketId: market.MarketId,
        StationCode: station.StationCode
      });

      station.Programs.forEach(program => {
        data.push({
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
          MarketId: market.MarketId,
          StationCode: station.StationCode,
          ProgramId: program.ProgramId,
          Cost: program.DisplayCost,
          isProgram: true // need for future use
        });
      });
      if (idx + 1 === station.length) {
        data.push({ rowType: rowTypes.EMPTY_ROW });
      }
    }
  );

  return data;
};

const generateMarketData = markets =>
  markets.map(market => ({
    rowType: rowTypes.TITLE,
    MarketRank: market.MarketRank,
    MarketName: market.MarketName,
    MarketCoverage: market.MarketCoverage,
    CPM: market.CPM,
    Spots: market.TotalSpots,
    Impressions: market.TotalImpressions
      ? market.TotalImpressions / 1000
      : market.TotalImpressions,
    StationImpressions: market.DisplayStationImpressions
      ? market.DisplayStationImpressions / 1000
      : market.DisplayStationImpressions,
    // OvernightImpressions: market.TotalOvernightImpressions,
    Cost: market.TotalCost,
    MarketId: market.MarketId,
    isMarket: true // need for future use
  }));

const GreyDisplay = (value, isGrey) => {
  const color = isGrey ? "#8f8f8f" : "black";
  return <div style={{ color }}>{value}</div>;
};

const NumberCell = ({ value, original }) => {
  if (isNil(value)) return "";
  const retVal = value !== 0 ? numeral(value).format("0,0") : "-";
  const inactive = original.isProgram
    ? original.Spots === 0 || original.Impressions > 0
    : false;
  return GreyDisplay(retVal, inactive);
};

const PercentCell = ({ value, original }) => {
  if (isNil(value)) return "";
  const retVal = value !== 0 ? numeral(value).format("0,0.[000]") : "-";
  const inactive = original.isProgram
    ? original.Spots === 0 || original.Impressions > 0
    : false;
  const displayVal = `${retVal}%`;
  return GreyDisplay(displayVal, inactive);
};

const SpotCell = ({ value, original }) => {
  if (isNil(value)) return "-";
  const inactive = original.isProgram
    ? original.Spots === 0 || original.Impressions > 0
    : false;
  return GreyDisplay(numeral(value).format("0,0"), inactive);
};

const ImpressionCell = ({ value, original }) => {
  if (isNil(value)) return "";
  const inactive = original.isProgram
    ? original.Spots === 0 || original.Impressions > 0
    : false;
  const retVal = value !== 0 ? numeral(value).format("0,0.[000]") : "-";
  return GreyDisplay(retVal, inactive);
};

const DollarCell = ({ value, original }) => {
  if (isNil(value)) return "";
  const retVal = value !== 0 ? numeral(value).format("$0,0.[00]") : "-";
  const inactive = original.isProgram
    ? original.Spots === 0 || original.Impressions > 0
    : false;
  return GreyDisplay(retVal, inactive);
};

const GroupingCell = ({ original: { rowType }, value }) => {
  const fontWeight = boldRowTypes.includes(rowType) ? "bold" : "300";
  return <div style={{ fontWeight }}>{value}</div>;
};

GroupingCell.propTypes = {
  original: PropTypes.object.isRequired,
  value: PropTypes.string.isRequired
};

const renderSpots = onChange => ({ value, original }) =>
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

const generateProgramColumns = onChange => [
  {
    Header: "Airing Time",
    accessor: "AiringTime",
    minWidth: 90,
    Cell: GroupingCell
  },
  {
    Header: "Program",
    accessor: "Program",
    minWidth: 150
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

const marketColumns = [
  {
    Header: "#",
    accessor: "MarketRank",
    minWidth: 25,
    Cell: GroupingCell
  },
  {
    Header: "Market",
    accessor: "MarketName",
    minWidth: 110,
    Cell: GroupingCell
  },
  {
    Header: "Coverage",
    accessor: "MarketCoverage",
    Cell: PercentCell
  },
  {
    Header: "CPM",
    accessor: "CPM",
    Cell: DollarCell
  },
  {
    Header: "Spots",
    accessor: "Spots",
    Cell: NumberCell
  },
  {
    Header: "Impressions(000)",
    accessor: "Impressions",
    Cell: ImpressionCell
  },
  {
    Header: "Cost",
    accessor: "Cost",
    Cell: DollarCell
  }
];

export {
  generateProgramColumns,
  marketColumns,
  generateMarketData,
  generateProgramData,
  rowColors,
  rowTypes
};
