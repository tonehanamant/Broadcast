import React from 'react';
import PropTypes from 'prop-types';
import numeral from 'numeral';
import { isNil } from 'lodash';

const rowTypes = {
  TITLE: 'TITLE',
  SUB_TITLE: 'SUB_TITLE',
  DATA_ROW: 'DATA_ROW',
  EMPTY_ROW: 'EMPTY_ROW',
};
const rowColors = {
  TITLE: '#dedede',
  SUB_TITLE: '#E9E9E9',
  DATA_ROW: '#fff',
  EMPTY_ROW: '#fff',
};

const boldRowTypes = [
  rowTypes.TITLE,
  rowTypes.SUB_TITLE,
];

const generateData = (markets) => {
  const data = [];
  // should add Ids for future?
  markets.forEach((market) => {
    data.push({
      rowType: rowTypes.TITLE,
      AiringTime: `${market.MarketRank}. ${market.MarketName}`,
      Spots: market.TotalSpots,
      Impressions: market.TotalImpressions ? market.TotalImpressions / 1000 : market.TotalImpressions,
      StationImpressions: market.TotalStationImpressions ? market.TotalStationImpressions / 1000 : market.TotalStationImpressions,
      // OvernightImpressions: market.TotalOvernightImpressions,
      Cost: market.TotalCost,
      isMarket: true, // need for future use
    });
    market.Stations.forEach((station) => {
      // station TotalStationImpressions?
      data.push({ rowType: rowTypes.SUB_TITLE, AiringTime: `${station.CallLetters} (${station.LegacyCallLetters})`, isStation: true });

      station.Programs.forEach((program) => {
        data.push({
          rowType: rowTypes.DATA_ROW,
          AiringTime: program.Daypart.Display,
          Program: program.ProgramName,
          CPM: program.BlendedCpm,
          Spots: program.Spots,
          Impressions: program.Impressions ? program.Impressions / 1000 : program.Impressions,
          StationImpressions: program.StationImpressions ? program.StationImpressions / 1000 : program.StationImpressions,
          // OvernightImpressions: program.OvernightImpressions,
          Cost: program.Cost,
          isProgram: true, // need for future use
        });
      });

      data.push({ rowType: rowTypes.EMPTY_ROW });
    });
  });

  return data;
};

const GreyDisplay = (value, isGrey) => {
  const color = isGrey ? '#8f8f8f' : 'black';
  return (<div style={{ color }}>{value}</div>);
};

const NumberCell = ({ value, original }) => {
  // console.log('number row', original);
  if (isNil(value)) return '';
  const retVal = value !== 0 ? numeral(value).format('0,0') : '-';
  const inactive = original.isProgram ? (original.Spots === 0 || original.Impressions > 0) : false;
  return GreyDisplay(retVal, inactive);
};

const ImpressionCell = ({ value, original }) => {
  // console.log('impression row', original);
  if (isNil(value)) return '';
  const inactive = original.isProgram ? (original.Spots === 0 || original.Impressions > 0) : false;
  const retVal = value !== 0 ? numeral(value).format('0,0.[000]') : '-';
  return GreyDisplay(retVal, inactive);
};

const DollarCell = ({ value, original }) => {
  if (isNil(value)) return '';
  const retVal = value !== 0 ? numeral(value).format('$0,0.[00]') : '-';
  const inactive = original.isProgram ? (original.Spots === 0 || original.Impressions > 0) : false;
  return GreyDisplay(retVal, inactive);
};

const GroupingCell = ({ original: { rowType }, value }) => {
  const fontWeight = (boldRowTypes.includes(rowType)) ? 'bold' : '300';
  return (<div style={{ fontWeight }}>{value}</div>);
};

GroupingCell.propTypes = {
  original: PropTypes.object.isRequired,
  value: PropTypes.string.isRequired,
};

const columns = [{
  Header: 'Airing Time',
  accessor: 'AiringTime',
  minWidth: 150,
  Cell: GroupingCell,
}, {
  Header: 'Program',
  accessor: 'Program',
  minWidth: 200,
}, {
  Header: 'CPM',
  accessor: 'CPM',
  Cell: DollarCell,
}, {
  Header: 'Spots',
  accessor: 'Spots',
  Cell: NumberCell,
}, {
  Header: 'Impressions(OOO)',
  accessor: 'Impressions',
  Cell: ImpressionCell,
}, {
  Header: 'Station Impressions',
  accessor: 'StationImpressions',
  Cell: ImpressionCell,
}, /* {
  Header: 'Overnight Impressions',
  accessor: 'OvernightImpressions',
  Cell: NumberCell,
}, */ {
  Header: 'Cost',
  accessor: 'Cost',
  Cell: DollarCell,
}];

export { columns, generateData, rowColors, rowTypes };
