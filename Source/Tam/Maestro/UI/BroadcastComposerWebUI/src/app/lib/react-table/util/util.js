import { isNaN, isNil } from "lodash";
import { multiSelectRow, singleSelectRow } from "../actions/actions";

export const SELECTION = {
  SINGLE: "single",
  MULTI: "multi",
  NONE: "none"
};

export const rowSelection = {
  [SELECTION.SINGLE]: singleSelectRow,
  [SELECTION.MULTI]: multiSelectRow
};

export const isNumeric = val => !isNaN(val);

export const generetaColumns = (columns, displayColumns) =>
  columns.map(c => {
    const isShow = displayColumns[c.id];
    return {
      show: isNil(isShow) ? true : isShow,
      ...c
    };
  });
