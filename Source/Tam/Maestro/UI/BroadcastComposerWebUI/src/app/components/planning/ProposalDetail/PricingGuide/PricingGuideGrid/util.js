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
      Impressions: market.TotalImpressions,
      StationImpressions: null, // market.StationImpressions,
      OvernightImpressions: market.TotalOvernightImpressions,
      Cost: market.TotalCost,
    });
    market.Stations.forEach((station) => {
      // station TotalStationImpressions?
      data.push({ rowType: rowTypes.SUB_TITLE, AiringTime: `${station.CallLetters} (${station.LegacyCallLetters})` });

      station.Programs.forEach((program) => {
        data.push({
          rowType: rowTypes.DATA_ROW,
          AiringTime: program.Daypart.Display,
          Program: program.ProgramName,
          CPM: program.BlendedCpm,
          Spots: program.Spots,
          Impressions: program.Impressions,
          StationImpressions: null, // program.StationImpressions,
          OvernightImpressions: program.OvernightImpressions,
          Cost: program.Cost,
        });
      });

      data.push({ rowType: rowTypes.EMPTY_ROW });
    });
  });

  return data;
};

const NumberCell = ({ value }) => {
  if (isNil(value)) return '';
  return value !== 0 ? numeral(value).format('0,0.[00]') : '-';
};

const DollarCell = ({ value }) => {
  if (isNil(value)) return '';
  return value !== 0 ? numeral(value).format('$0,0.[00]') : '-';
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
  Cell: GroupingCell,
}, {
  Header: 'Program',
  accessor: 'Program',
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
  Cell: NumberCell,
}, {
  Header: 'Station Impressions',
  accessor: 'StationImpressions',
  Cell: NumberCell,
}, {
  Header: 'Overnight Impressions',
  accessor: 'OvernightImpressions',
  Cell: NumberCell,
}, {
  Header: 'Cost',
  accessor: 'Cost',
  Cell: DollarCell,
}];

export { columns, generateData, rowColors, rowTypes };
