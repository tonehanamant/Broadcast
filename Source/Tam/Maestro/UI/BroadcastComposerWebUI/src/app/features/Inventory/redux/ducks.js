import { createAction } from "Utils/action-creator";

const ROOT = "inventory";

export const INVENTORY_INITIALDATA = createAction(
  `${ROOT}/INVENTORY_INITIALDATA`
);

export const INVENTORY_LOAD_CARDS = createAction(
  `${ROOT}/INVENTORY_LOAD_CARDS`
);

export const INVENTORY_FILTER_CARDS = createAction(
  `${ROOT}/INVENTORY_FILTER_CARDS`
);

const initialState = {
  initialInventoryData: null,
  cardsInventoryData: null
};

// Reducer
export default function reducer(state = initialState, action) {
  const { type, data } = action;

  switch (type) {
    case INVENTORY_INITIALDATA.success:
      return {
        ...state,
        initialInventoryData: data.Data
      };

    case INVENTORY_LOAD_CARDS.success:
      return {
        ...state,
        cardsInventoryData: data.Data
      };

    case INVENTORY_FILTER_CARDS.success:
      return {
        ...state,
        cardsInventoryData: data.Data
      };

    default:
      return state;
  }
}

// Action Creators
/* export const receiveInventoryInitialData = data => ({
  type: INVENTORY_INITIALDATA.success,
  data
});
 */

export const getInventoryInitialData = () => ({
  type: INVENTORY_INITIALDATA.request,
  payload: {}
});

export const requestLoadInventoryCards = params => ({
  type: INVENTORY_LOAD_CARDS.request,
  payload: params
});

export const filterInventoryCards = params => ({
  type: INVENTORY_FILTER_CARDS.request,
  payload: params
});
