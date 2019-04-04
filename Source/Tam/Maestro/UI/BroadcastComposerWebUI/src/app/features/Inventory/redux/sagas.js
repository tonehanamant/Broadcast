import { takeEvery, put } from "redux-saga/effects";

import { setOverlayLoading } from "Main/redux/ducks";

import sagaWrapper from "Utils/saga-wrapper";
import api from "API";
import {
  INVENTORY_INITIALDATA,
  INVENTORY_LOAD_CARDS,
  INVENTORY_FILTER_CARDS,
  requestLoadInventoryCards
} from "Inventory/redux/ducks";

export function* requestInventoryInitialData() {
  const { getInitialData } = api.inventory;
  try {
    yield put(setOverlayLoading({ id: "inventoryInitialData", loading: true }));
    return yield getInitialData();
  } finally {
    yield put(
      setOverlayLoading({ id: "inventoryInitialData", loading: false })
    );
  }
}

export function* requestInventoryInitialDataSuccess({ data }) {
  const params = { Quarter: data.Data.DefaultQuarter };
  yield put(requestLoadInventoryCards(params));
}

export function* loadInventoryCards(params) {
  const { loadCards } = api.inventory;
  try {
    yield put(setOverlayLoading({ id: "loadCards", loading: true }));
    return yield loadCards(params);
  } finally {
    yield put(setOverlayLoading({ id: "loadCards", loading: false }));
  }
}

export function* filterInventoryCards(params) {
  const { filterCards } = api.inventory;
  try {
    yield put(setOverlayLoading({ id: "filterCards", loading: true }));
    return yield filterCards(params);
  } finally {
    yield put(setOverlayLoading({ id: "filterCards", loading: false }));
  }
}

/* ////////////////////////////////// */
/* WATCHERS */
/* ////////////////////////////////// */
function* watchRequestInventoryInitialData() {
  yield takeEvery(
    INVENTORY_INITIALDATA.request,
    sagaWrapper(requestInventoryInitialData, INVENTORY_INITIALDATA)
  );
}

function* watchRequestInventoryInitialDataSuccess() {
  yield takeEvery(
    INVENTORY_INITIALDATA.success,
    requestInventoryInitialDataSuccess
  );
}

function* watchLoadInventoryCards() {
  yield takeEvery(
    INVENTORY_LOAD_CARDS.request,
    sagaWrapper(loadInventoryCards, INVENTORY_LOAD_CARDS)
  );
}

function* watchFilterInventoryCards() {
  yield takeEvery(
    INVENTORY_FILTER_CARDS.request,
    sagaWrapper(filterInventoryCards, INVENTORY_FILTER_CARDS)
  );
}

export default [
  watchRequestInventoryInitialData,
  watchRequestInventoryInitialDataSuccess,
  watchLoadInventoryCards,
  watchFilterInventoryCards
];
