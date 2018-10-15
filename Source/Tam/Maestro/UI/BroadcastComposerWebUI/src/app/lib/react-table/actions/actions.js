export const MULTI_SELECT_ROW = "MULTI_SELECT_ROW";
export const SINGLE_SELECT_ROW = "SINGLE_SELECT_ROW";
export const DESELECT_ROW = "DESELECT_ROW";
export const DESELECT_ROWS = "DESELECT_ROWS";

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
