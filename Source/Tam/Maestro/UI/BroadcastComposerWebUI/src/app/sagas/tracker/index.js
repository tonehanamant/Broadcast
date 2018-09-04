import { takeEvery, put } from 'redux-saga/effects';
// import FuzzySearch from 'fuzzy-search';
// import moment from 'moment';

import * as appActions from 'Ducks/app/actionTypes';
import * as trackerActions from 'Ducks/tracker/actionTypes';
import {
  // setOverlayLoading,
  setOverlayProcessing,
  // createAlert,
  // toggleModal,
  // deployError,
  // clearFile,
} from 'Ducks/app/index';
import {
  // getPostPrePosting,
  // clearFileUploadForm,
  // receiveFilteredPostPrePosting,
} from 'Ducks/tracker/index';

import sagaWrapper from '../wrapper';
import api from '../api';

const ACTIONS = { ...appActions, ...trackerActions };

/* ////////////////////////////////// */
/* UPLOAD Tracker FILE */
/* ////////////////////////////////// */
export function* uploadTrackerFile(params) {
  // console.log('uploadTrackerFile', params);
  const { uploadTracker } = api.tracker;
  try {
    yield put(setOverlayProcessing({ id: 'uploadTracker', processing: true }));
    return yield uploadTracker(params);
  } finally {
    yield put(setOverlayProcessing({ id: 'uploadTracker', processing: false }));
  }
}

export function* watchUploadTrackerFile() {
  yield takeEvery(
    ACTIONS.TRACKER_FILE_UPLOAD.request,
    sagaWrapper(uploadTrackerFile, ACTIONS.TRACKER_FILE_UPLOAD),
  );
}
