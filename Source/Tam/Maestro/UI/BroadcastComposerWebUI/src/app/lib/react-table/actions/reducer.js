import {
  MULTI_SELECT_ROW,
  SINGLE_SELECT_ROW,
  DESELECT_ROW,
  DESELECT_ROWS,
  INIT_TABLE,
  VISIBLE_COLUMN
} from "./actions";

/** ================================================================================== */
/** INITIAL STATE */
/** ================================================================================== */

export const initialState = {
  selected: [],
  displayColumns: {}
};

/** ================================================================================== */
/** REDUCER HANDLERS */
/** ================================================================================== */

const multiSelect = (prevState, { rowIdx }) => {
  const { selected } = prevState;
  if (selected.includes(rowIdx)) {
    return {
      ...prevState,
      selected: selected.filter(idx => idx !== rowIdx)
    };
  }
  return {
    ...prevState,
    selected: selected.concat(rowIdx)
  };
};

const singleSelect = (prevState, { rowIdx }) => ({
  ...prevState,
  selected: [rowIdx]
});

const deselectRow = (prevState, { rowIdx }) => ({
  ...prevState,
  selected: prevState.selected.filter(idx => idx !== rowIdx)
});

const deselectRows = prevState => ({
  ...prevState,
  selected: []
});

const visibleColumn = (prevState, { rowIdx, value }) => ({
  ...prevState,
  displayColumns: {
    ...prevState.displayColumns,
    [rowIdx]: value
  }
});

const initTable = (prevState, payload) => ({
  ...prevState,
  ...payload
});

/** ================================================================================== */
/** REDUCER */
/** ================================================================================== */

const reducer = {
  [MULTI_SELECT_ROW]: multiSelect,
  [SINGLE_SELECT_ROW]: singleSelect,
  [DESELECT_ROW]: deselectRow,
  [DESELECT_ROWS]: deselectRows,
  [VISIBLE_COLUMN]: visibleColumn,
  [INIT_TABLE]: initTable
};

export default reducer;
