import {
  rowTypes,
  GroupingCell,
  PercentCell,
  DollarCell,
  NumberCell,
  ImpressionCell
} from "../util";

export const generateMarketData = markets =>
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
    Cost: market.TotalCost,
    MarketId: market.MarketId,
    isMarket: true // need for future use
  }));

export const marketColumns = [
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
