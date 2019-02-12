import { takeEvery, put, call } from "redux-saga/effects";
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

const read = f =>
  new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = resolve;
    reader.onabort = reject;
    reader.onerror = reject;
    reader.readAsDataURL(f);
  });

const getBase64 = e => e.target.result.split("base64,")[1];

export function* requestReadFileB64({ payload: file }) {
  try {
    const dataURL = yield call(read, file);
    const b64 = yield getBase64(dataURL);
    yield put({
      type: ACTIONS.STORE_FILE_B64,
      data: b64
    });
  } catch (e) {
    if (e.message) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          message: e.message
        }
      });
    }
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

function* watchReadFileB64() {
  yield takeEvery(ACTIONS.READ_FILE_B64, requestReadFileB64);
}

export default [watchLoadEnvironment, watchLoadEmployee, watchReadFileB64];
