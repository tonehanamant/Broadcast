export const MULTI_SELECT_ROW = "MULTI_SELECT_ROW";
export const SINGLE_SELECT_ROW = "SINGLE_SELECT_ROW";
export const DESELECT_ROW = "DESELECT_ROW";
export const DESELECT_ROWS = "DESELECT_ROWS";
export const VISIBLE_COLUMN = "VISIBLE_COLUMN";
export const INIT_TABLE = "INIT_TABLE";

export const singleSelectRow = rowIdx => ({
  type: SINGLE_SELECT_ROW,
  rowIdx
});

export const multiSelectRow = rowIdx => ({
  type: MULTI_SELECT_ROW,
  rowIdx
});

export const deselectRows = () => ({
  type: DESELECT_ROWS
});

export const deselectRow = rowIdx => ({
  type: DESELECT_ROW,
  rowIdx
});

export const visibleColumn = (rowIdx, value) => ({
  type: VISIBLE_COLUMN,
  rowIdx,
  value
});

export const initTable = payload => ({
  type: INIT_TABLE,
  payload
});
