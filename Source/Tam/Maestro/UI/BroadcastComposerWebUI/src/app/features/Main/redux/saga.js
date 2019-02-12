import { takeEvery, put } from "redux-saga/effects";
import api from "API";
import sagaWrapper from "Utils/saga-wrapper";
import * as ACTIONS from "./types";
import { setOverlayLoading } from "./actions";

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
    ACTIONS.LOAD_ENVIRONMENT.request,
    sagaWrapper(loadEnvironment, ACTIONS.LOAD_ENVIRONMENT)
  );
}

function* watchLoadEmployee() {
  yield takeEvery(
    ACTIONS.LOAD_EMPLOYEE.request,
    sagaWrapper(loadEmployee, ACTIONS.LOAD_EMPLOYEE)
  );
}

export default [watchLoadEnvironment, watchLoadEmployee];
