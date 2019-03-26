import { takeEvery, put, select } from "redux-saga/effects";
import moment from "moment";
import { update } from "lodash";
import { setOverlayLoading, createAlert, deployError } from "Main/redux/ducks";
import sagaWrapper from "Utils/saga-wrapper";
import searcher from "Utils/searcher";
import api from "API";

import {
  FILE_UPLOAD,
  LOAD_TRACKER,
  REQUEST_FILTERED_TRACKER,
  RECEIVE_FILTERED_TRACKER
} from "Tracker/redux/ducks";

/* ////////////////////////////////// */
/* SELECTORS */
/* ////////////////////////////////// */
export const selectUnfilteredData = state =>
  state.tracker.master.trackerUnfilteredGridData;

/* ////////////////////////////////// */
/* SAGAS */
/* ////////////////////////////////// */

/* ////////////////////////////////// */
/* Adjust Tracker Data return */
/* ////////////////////////////////// */

export function adjustTracker(posts) {
  const adjustTracker = posts.map(item => {
    const tracker = item;
    tracker.searchContractId = String(tracker.ContractId);
    tracker.searchSpotsInSpec = String(tracker.SpotsInSpec);
    tracker.searchSpotsOutOfSpec = String(tracker.SpotsOutOfSpec);
    tracker.searchUploadDate = tracker.UploadDate
      ? moment(tracker.UploadDate).format("MM/DD/YYYY")
      : "-";
    return tracker;
  });
  return adjustTracker;
}

export function* requestTracker() {
  const { getTracker } = api.tracker;
  try {
    yield put(setOverlayLoading({ id: "trackerPosts", loading: true }));
    const { status, data } = yield getTracker();
    update(data, "Data.Post", () => adjustTracker(data.Data.Posts));
    return { status, data };
  } finally {
    yield put(setOverlayLoading({ id: "trackerPosts", loading: false }));
  }
}

const trackerSearchKeys = [
  "searchContractId",
  "ContractName",
  "Advertiser",
  "searchUploadDate",
  "searchSpotsInSpec",
  "searchSpotsOutOfSpec"
];

export function* requestTrackerFiltered({ payload: query }) {
  const data = yield select(selectUnfilteredData);
  try {
    const filtered = yield searcher(data, trackerSearchKeys, query);
    yield put({
      type: RECEIVE_FILTERED_TRACKER,
      data: filtered
    });
  } catch (e) {
    if (e.message) {
      yield put(deployError({ message: e.message }));
    }
  }
}

/* ////////////////////////////////// */
/* UPLOAD Tracker FILE */
/* ////////////////////////////////// */
export function* uploadTrackerFile(params) {
  const { uploadTracker } = api.tracker;
  try {
    yield put(setOverlayLoading({ id: "uploadTracker", loading: true }));
    return yield uploadTracker(params);
  } finally {
    yield put(setOverlayLoading({ id: "uploadTracker", loading: false }));
  }
}

export function* uploadTrackerFileSuccess() {
  yield put(createAlert({ type: "success", headline: "CSV Files Uploaded" }));
}

/* ////////////////////////////////// */
/* WATCHERS */
/* ////////////////////////////////// */

function* watchRequestTracker() {
  yield takeEvery(
    LOAD_TRACKER.request,
    sagaWrapper(requestTracker, LOAD_TRACKER)
  );
}

function* watchRequestTrackerFiltered() {
  yield takeEvery(REQUEST_FILTERED_TRACKER, requestTrackerFiltered);
}

export function* watchUploadTrackerFile() {
  yield takeEvery(
    FILE_UPLOAD.request,
    sagaWrapper(uploadTrackerFile, FILE_UPLOAD)
  );
}

export function* watchUploadTrackerFileSuccess() {
  yield takeEvery(FILE_UPLOAD.success, uploadTrackerFileSuccess);
}

export default [
  watchUploadTrackerFileSuccess,
  watchUploadTrackerFile,
  watchRequestTrackerFiltered,
  watchRequestTracker
];
