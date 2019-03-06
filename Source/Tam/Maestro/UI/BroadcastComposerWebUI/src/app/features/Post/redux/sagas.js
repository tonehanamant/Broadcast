import { takeEvery, put, select } from "redux-saga/effects";
import moment from "moment";
import { update } from "lodash";
import { setOverlayLoading, toggleModal, deployError } from "Main/redux/ducks";
import sagaWrapper from "Utils/saga-wrapper";
import searcher from "Utils/searcher";
import api from "API";

import {
  PROCESS_NTI_FILE,
  LOAD_POST,
  REQUEST_FILTERED_POST,
  RECEIVE_FILTERED_POST
} from "Post/redux/ducks";

/* ////////////////////////////////// */
/* SELECTORS */
/* ////////////////////////////////// */
export const selectUnfilteredData = state =>
  state.post.master.postUnfilteredGridData;

/* ////////////////////////////////// */
/* SAGAS */
/* ////////////////////////////////// */
export function adjustPost(posts) {
  const adjustPost = posts.map(item => {
    const post = item;
    post.searchContractId = String(post.ContractId);
    post.searchSpotsInSpec = String(post.SpotsInSpec);
    post.searchSpotsOutOfSpec = String(post.SpotsOutOfSpec);
    post.searchUploadDate = post.UploadDate
      ? moment(post.UploadDate).format("MM/DD/YYYY")
      : "-";
    return post;
  });
  return adjustPost;
}

export function* requestPost() {
  const { getPosts } = api.post;
  try {
    yield put(setOverlayLoading({ id: "postPosts", loading: true }));
    const { status, data } = yield getPosts();
    update(data, "Data.Post", () => adjustPost(data.Data.Posts));
    return { status, data };
  } finally {
    yield put(setOverlayLoading({ id: "postPosts", loading: false }));
  }
}

const postSearchKeys = [
  "searchContractId",
  "ContractName",
  "Advertiser",
  "UploadDate",
  "serchSpotsInSpec",
  "searchSpotsOutOfSpec"
];

export function* requestPostFiltered({ payload: query }) {
  const data = yield select(selectUnfilteredData);
  try {
    const filtered = yield searcher(data, postSearchKeys, query);
    yield put({
      type: RECEIVE_FILTERED_POST,
      data: filtered
    });
  } catch (e) {
    if (e.message) {
      yield put(deployError({ message: e.message }));
    }
  }
}

export function* requestProcessNtiFile(payload) {
  const { uploadNtiTransmittal } = api.post;
  try {
    yield put(setOverlayLoading({ id: "PostNTIUpload", loading: true }));
    return yield uploadNtiTransmittal(payload);
  } finally {
    yield put(setOverlayLoading({ id: "PostNTIUpload", loading: false }));
  }
}

export function* processNtiFileSuccess(req) {
  const isList = Array.isArray(req.data.Data) || req.data.Data.length;
  yield put(
    toggleModal({
      modal: "confirmModal",
      active: true,
      properties: {
        bodyClass: isList ? "modalBodyScroll" : null,
        titleText: "Upload Complete",
        bodyText: req.data.Message,
        bodyList: isList ? req.data.Data : "",
        closeButtonDisabled: true,
        actionButtonText: "OK",
        actionButtonBsStyle: "success",
        action: () => {},
        dismiss: () => {}
      }
    })
  );
}

/* ////////////////////////////////// */
/* WATCHERS */
/* ////////////////////////////////// */

function* watchRequestPost() {
  yield takeEvery(LOAD_POST.request, sagaWrapper(requestPost, LOAD_POST));
}

function* watchRequestPostFiltered() {
  yield takeEvery(REQUEST_FILTERED_POST, requestPostFiltered);
}

function* watchRequestProcessNtiFile() {
  yield takeEvery(
    PROCESS_NTI_FILE.request,
    sagaWrapper(requestProcessNtiFile, PROCESS_NTI_FILE)
  );
}

function* watchProcessNtiFileSuccess() {
  yield takeEvery(PROCESS_NTI_FILE.success, processNtiFileSuccess);
}

export default [
  watchProcessNtiFileSuccess,
  watchRequestProcessNtiFile,
  watchRequestPostFiltered,
  watchRequestPost
];
