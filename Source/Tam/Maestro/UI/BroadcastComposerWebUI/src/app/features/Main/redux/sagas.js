import { takeEvery, put } from "redux-saga/effects";
import api from "API";
import sagaWrapper from "Utils/saga-wrapper";
import { setOverlayLoading, LOAD_ENVIRONMENT, LOAD_EMPLOYEE } from "./ducks";

/* ////////////////////////////////// */
/* SELECTORS */
/* ////////////////////////////////// */
export const selectModal = (state, modalName) => state.app.modals[modalName];

/* ////////////////////////////////// */
/* SAGAS */
/* ////////////////////////////////// */
export function* loadEnvironment() {
  const { getEnvironment } = api.app;
  try {
    yield put(setOverlayLoading({ id: "appEnvironment", loading: true }));
    return yield getEnvironment();
  } finally {
    yield put(setOverlayLoading({ id: "appEnvironment", loading: false }));
  }
}

export function* loadEmployee() {
  const { getEmployee } = api.app;
  try {
    yield put(setOverlayLoading({ id: "appEmployee", loading: true }));
    return yield getEmployee();
  } finally {
    yield put(setOverlayLoading({ id: "appEmployee", loading: false }));
  }
}

/* ////////////////////////////////// */
/* WATCHERS */
/* ////////////////////////////////// */
function* watchLoadEnvironment() {
  yield takeEvery(
    LOAD_ENVIRONMENT.request,
    sagaWrapper(loadEnvironment, LOAD_ENVIRONMENT)
  );
}

function* watchLoadEmployee() {
  yield takeEvery(
    LOAD_EMPLOYEE.request,
    sagaWrapper(loadEmployee, LOAD_EMPLOYEE)
  );
}

export default [watchLoadEnvironment, watchLoadEmployee];
