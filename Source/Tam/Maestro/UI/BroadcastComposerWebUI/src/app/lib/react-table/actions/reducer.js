import {
  MULTI_SELECT_ROW,
  SINGLE_SELECT_ROW,
  DESELECT_ROW,
  DESELECT_ROWS
} from "./actions";

/** ================================================================================== */
/** INITIAL STATE */
/** ================================================================================== */

export const initialState = {
  selected: []
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

/** ================================================================================== */
/** REDUCER */
/** ================================================================================== */

const reducer = {
  [MULTI_SELECT_ROW]: multiSelect,
  [SINGLE_SELECT_ROW]: singleSelect,
  [DESELECT_ROW]: deselectRow,
  [DESELECT_ROWS]: deselectRows
};

export default reducer;
