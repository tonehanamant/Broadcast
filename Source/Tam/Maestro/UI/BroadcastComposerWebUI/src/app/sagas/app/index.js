/* eslint-disable import/prefer-default-export */
import { takeEvery, put, call } from 'redux-saga/effects';

import * as ACTIONS from 'Ducks/app/actionTypes';
import api from '../api';

/* ////////////////////////////////// */
/* REQUEST ENVIRONMENT */
/* ////////////////////////////////// */
export function* requestEnvironment() {
  const { getEnvironment } = api.app;
  try {
    // Set loading overlay
    yield put({
      type: ACTIONS.SET_OVERLAY_LOADING,
      overlay: {
        id: 'appEnvironment',
        loading: true,
      },
    });
    // Yield getEnvirontment
    const response = yield getEnvironment();
    const { status, data } = response;
    console.log('RESPONSE', response);
    // Unset loading overlay
    yield put({
      type: ACTIONS.SET_OVERLAY_LOADING,
      overlay: {
        id: 'appEnvironment',
        loading: false,
      },
    });
    // Check for 200 & response.data.Success
    if (status !== 200) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'No environment info returned.',
          message: `The server encountered an error processing the request (environment). Please try again or contact your administrator to review error logs. (HTTP Status: ${status})`,
        },
      });
      throw new Error();
    }
    if (!data.Success) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'No environment info returned.',
          message: data.Message || 'The server encountered an error processing the request (environment). Please try again or contact your administrator to review error logs.',
        },
      });
      throw new Error();
    }
    // Pass response.data to reducer
    yield put({
      type: ACTIONS.RECEIVE_ENVIRONMENT,
      data,
    });
  } catch (e) {
    // Default error for try
    if (e.response) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'No environment info returned.',
          message: 'The server encountered an error processing the request (environment). Please try again or contact your administrator to review error logs.',
          exception: `${e.response.data.ExceptionMessage}`,
        },
      });
    }
    if (!e.response && e.message) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          message: e.message,
        },
      });
    }
  }
}

/* ////////////////////////////////// */
/* REQUEST EMPLOYEE */
/* ////////////////////////////////// */
export function* requestEmployee() {
  const { getEmployee } = api.app;

  try {
    yield put({
      type: ACTIONS.SET_OVERLAY_LOADING,
      overlay: {
        id: 'appEmployee',
        loading: true },
      });
    const response = yield getEmployee();
    const { status, data } = response;
    yield put({
      type: ACTIONS.SET_OVERLAY_LOADING,
      overlay: {
        id: 'appEmployee',
        loading: false },
      });
    if (status !== 200) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'No employee info returned.',
          message: `The server encountered an error processing the request (employee). Please try again or contact your administrator to review error logs. (HTTP Status: ${status})`,
        },
      });
      throw new Error();
    }
    if (!data.Success) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'No employee info returned.',
          message: data.Message || 'The server encountered an error processing the request (employee). Please try again or contact your administrator to review error logs.',
        },
      });
      throw new Error();
    }
    yield put({
      type: ACTIONS.RECEIVE_EMPLOYEE,
      data,
    });
  } catch (e) {
    if (e.response) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'No employee info returned.',
          message: 'The server encountered an error processing the request (employee). Please try again or contact your administrator to review error logs.',
          exception: `${e.response.data.ExceptionMessage}`,
        },
      });
    }
    if (!e.response && e.message) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          message: e.message,
        },
      });
    }
  }
}

/* ////////////////////////////////// */
/* REQUEST READ FILE B64 */
/* ////////////////////////////////// */
export function* requestReadFileB64({ payload: file }) {
  const read = f => new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = resolve;
    reader.onabort = reject;
    reader.onerror = reject;
    reader.readAsDataURL(f);
  });

  const getBase64 = e => e.target.result.split('base64,')[1];

  try {
    const dataURL = yield call(read, file);
    const b64 = yield getBase64(dataURL);
    // console.log('BASE64 FILE', b64);
    yield put({
      type: ACTIONS.STORE_FILE_B64,
      data: b64,
    });
  } catch (e) {
    if (e.message) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          message: e.message,
        },
      });
    }
  }
}

/* ////////////////////////////////// */
/* WATCHERS */
/* ////////////////////////////////// */
export function* watchRequestEnvironment() {
  yield takeEvery(ACTIONS.REQUEST_ENVIRONMENT, requestEnvironment);
}

export function* watchRequestEmployee() {
  yield takeEvery(ACTIONS.REQUEST_EMPLOYEE, requestEmployee);
}

export function* watchReadFileB64() {
  yield takeEvery(ACTIONS.READ_FILE_B64, requestReadFileB64);
}
