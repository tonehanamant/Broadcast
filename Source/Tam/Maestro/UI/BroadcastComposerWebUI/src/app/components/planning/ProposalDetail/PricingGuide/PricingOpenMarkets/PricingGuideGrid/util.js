/* eslint-disable react/prop-types */
import React from "react";
import PropTypes from "prop-types";
import numeral from "numeral";
import { isNil, update, findIndex, get } from "lodash";

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

export const rowTypes = {
  TITLE: "TITLE",
  SUB_TITLE: "SUB_TITLE",
  DATA_ROW: "DATA_ROW",
  EMPTY_ROW: "EMPTY_ROW"
};
export const rowColors = {
  TITLE: "#dedede",
  SUB_TITLE: "#E9E9E9",
  DATA_ROW: "#fff",
  EMPTY_ROW: "#fff"
};

export const boldRowTypes = [rowTypes.TITLE, rowTypes.SUB_TITLE];

export const GreyDisplay = (value, isGrey) => {
  const color = isGrey ? "#8f8f8f" : "black";
  return <div style={{ color }}>{value}</div>;
};

export const NumberCell = ({ value, original }) => {
  if (isNil(value)) return "";
  const retVal = value !== 0 ? numeral(value).format("0,0") : "-";
  const inactive = original.isProgram
    ? original.Spots === 0 || original.Impressions > 0
    : false;
  return GreyDisplay(retVal, inactive);
};

export const PercentCell = ({ value }) => {
  if (isNil(value)) return "";
  const retVal = value !== 0 ? numeral(value).format("0,0.[000]") : "-";
  return `${retVal}%`;
};

export const SpotCell = ({ value, original }) => {
  if (isNil(value)) return "-";
  const inactive = original.isProgram
    ? original.Spots === 0 || original.Impressions > 0
    : false;
  return GreyDisplay(numeral(value).format("0,0"), inactive);
};

export const ImpressionCell = ({ value, original }) => {
  if (isNil(value)) return "";
  const inactive = original.isProgram
    ? original.Spots === 0 || original.Impressions > 0
    : false;
  const retVal = value !== 0 ? numeral(value).format("0,0.[000]") : "-";
  return GreyDisplay(retVal, inactive);
};

export const DollarCell = ({ value, original }) => {
  if (isNil(value)) return "";
  const retVal = value !== 0 ? numeral(value).format("$0,0.[00]") : "-";
  const inactive = original.isProgram
    ? original.Spots === 0 || original.Impressions > 0
    : false;
  return GreyDisplay(retVal, inactive);
};

export const GroupingCell = ({ original: { rowType }, value }) => {
  const fontWeight = boldRowTypes.includes(rowType) ? "bold" : "300";
  return <div style={{ fontWeight }}>{value}</div>;
};

GroupingCell.propTypes = {
  original: PropTypes.object.isRequired,
  value: PropTypes.string.isRequired
};
