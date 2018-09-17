import { takeEvery, put } from 'redux-saga/effects';

import * as appActions from 'Ducks/app/actionTypes';
import * as trackerActions from 'Ducks/tracker/actionTypes';
import {
  setOverlayProcessing,
  createAlert,
} from 'Ducks/app/index';

import sagaWrapper from '../wrapper';
import api from '../api';

const ACTIONS = { ...appActions, ...trackerActions };

/* ////////////////////////////////// */
/* UPLOAD Tracker FILE */
/* ////////////////////////////////// */
export function* uploadTrackerFile(params) {
  const { uploadTracker } = api.tracker;
  try {
    yield put(setOverlayProcessing({ id: 'uploadTracker', processing: true }));
    return yield uploadTracker(params);
  } finally {
    yield put(setOverlayProcessing({ id: 'uploadTracker', processing: false }));
  }
}

export function* uploadTrackerFileSuccess() {
  yield put(createAlert({ type: 'success', headline: 'CSV Files Uploaded' }));
}

export function* watchUploadTrackerFile() {
  yield takeEvery(
    ACTIONS.TRACKER_FILE_UPLOAD.request,
    sagaWrapper(uploadTrackerFile, ACTIONS.TRACKER_FILE_UPLOAD),
  );
}

export function* watchUploadTrackerFileSuccess() {
  yield takeEvery(ACTIONS.TRACKER_FILE_UPLOAD.success, uploadTrackerFileSuccess);
}
