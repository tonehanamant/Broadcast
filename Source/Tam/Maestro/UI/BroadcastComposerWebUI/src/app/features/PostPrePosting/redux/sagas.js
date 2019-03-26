import { takeEvery, put, select } from "redux-saga/effects";
import FuzzySearch from "fuzzy-search";
import moment from "moment";

import {
  setOverlayLoading,
  setOverlayProcessing,
  createAlert,
  toggleModal,
  deployError,
  clearFile
} from "Main/redux/ducks";

import sagaWrapper from "Utils/saga-wrapper";
import api from "API";
import {
  POST_PRE_POSTING_INITIALDATA,
  POST_PRE_POSTING,
  FILTERED_POST_PRE_POSTING,
  DELETE_POST_PRE_POSTING,
  POST_PRE_POSTING_FILE_EDIT,
  POST_PRE_POSTING_FILE_SAVE,
  POST_PRE_POSTING_FILE_UPLOAD,
  getPostPrePosting,
  clearFileUploadForm,
  receiveFilteredPostPrePosting
} from "PostPrePosting/redux/ducks";

const assignDisplay = data =>
  data.map(item => {
    const post = item;
    post.DisplayDemos = post.DemoLookups.map((demo, index, arr) => {
      const separator = index === arr.length - 1 ? "" : ", ";
      return `${demo.Display}${separator}`;
    });
    post.DisplayUploadDate = moment(post.UploadDate).format("M/D/YYYY");
    post.DisplayModifiedDate = moment(post.ModifiedDate).format("M/D/YYYY");
    return post;
  });

export function* requestPostPrePostingInitialData() {
  const { getInitialData } = api.postPrePosting;
  try {
    yield put(setOverlayLoading({ id: "postInitialData", loading: true }));
    return yield getInitialData();
  } finally {
    yield put(setOverlayLoading({ id: "postInitialData", loading: false }));
  }
}

export function* requestPostPrePosting() {
  const { getPosts } = api.postPrePosting;
  try {
    yield put(setOverlayLoading({ id: "postPosts", loading: true }));
    const { status, data } = yield getPosts();
    const assignedData = assignDisplay(data.Data);
    return { status, data: { ...data, Data: assignedData } };
  } finally {
    yield put(setOverlayLoading({ id: "postPosts", loading: false }));
  }
}

const postFiltered = (postUnfiltered, query) => {
  const searcher = new FuzzySearch(
    postUnfiltered,
    ["FileName", "DisplayDemos", "DisplayUploadDate", "DisplayModifiedDate"],
    { caseSensitive: false }
  );
  return searcher.search(query);
};

export function* requestPostPrePostingFiltered({ payload: query }) {
  const postUnfiltered = yield select(
    state => state.postPrePosting.postUnfiltered
  );
  try {
    const filtered = yield postFiltered(postUnfiltered, query);
    yield put(receiveFilteredPostPrePosting(filtered));
  } catch (e) {
    if (e.message) {
      yield put(deployError({ message: e.message }));
    }
  }
}

export function* deletePostPrePostingById(id) {
  const { deletePost } = api.postPrePosting;
  return yield deletePost(id);
}

export function* deletePostPrePostingByIdSuccess({ payload }) {
  yield put(
    createAlert({
      type: "success",
      headline: "Posting Removed",
      message: `${payload} was successfully removed.`
    })
  );
  yield put(getPostPrePosting());
}

export function* postPrePostingFileEdit(id) {
  const { getPost } = api.postPrePosting;
  try {
    yield put(setOverlayLoading({ id: "getPost", loading: true }));
    return yield getPost(id);
  } finally {
    yield put(setOverlayLoading({ id: "getPost", loading: false }));
  }
}

export function* postPrePostingFileEditSuccess() {
  yield put(toggleModal({ modal: "postFileEditModal", active: true }));
}

export function* postPrePostingFileSave(params) {
  const { savePost } = api.postPrePosting;
  try {
    yield put(setOverlayProcessing({ id: "savePostEdit", processing: true }));
    return yield savePost(params);
  } finally {
    yield put(setOverlayProcessing({ id: "savePostEdit", processing: false }));
  }
}

export function* postPrePostingFileSaveSuccess() {
  yield put(toggleModal({ modal: "postFileEditModal", active: false }));
  yield put(createAlert({ type: "success", headline: "Post File Updated" }));
  yield put(getPostPrePosting());
}

export function* uploadPostPrePostingFile(params) {
  const { uploadPost } = api.postPrePosting;
  try {
    yield put(setOverlayProcessing({ id: "uploadPost", processing: true }));
    return yield uploadPost(params);
  } finally {
    yield put(setOverlayProcessing({ id: "uploadPost", processing: false }));
  }
}

export function* uploadPostPrePostingFileSuccess() {
  yield put(toggleModal({ modal: "postFileUploadModal", active: false }));
  yield put(createAlert({ type: "success", headline: "Post File Updated" }));
  yield put(clearFile());
  yield put(clearFileUploadForm());
  yield put(getPostPrePosting());
}

/* ////////////////////////////////// */
/* WATCHERS */
/* ////////////////////////////////// */
function* watchRequestPostPrePostingInitialData() {
  yield takeEvery(
    POST_PRE_POSTING_INITIALDATA.request,
    sagaWrapper(requestPostPrePostingInitialData, POST_PRE_POSTING_INITIALDATA)
  );
}

function* watchRequestPostPrePosting() {
  yield takeEvery(
    POST_PRE_POSTING.request,
    sagaWrapper(requestPostPrePosting, POST_PRE_POSTING)
  );
}

function* watchRequestPostPrePostingFiltered() {
  yield takeEvery(
    FILTERED_POST_PRE_POSTING.request,
    requestPostPrePostingFiltered
  );
}

function* watchDeletePostPrePostingById() {
  yield takeEvery(
    DELETE_POST_PRE_POSTING.request,
    sagaWrapper(deletePostPrePostingById, DELETE_POST_PRE_POSTING)
  );
}

function* watchDeletePostPrePostingByIdSuccess() {
  yield takeEvery(
    DELETE_POST_PRE_POSTING.success,
    deletePostPrePostingByIdSuccess
  );
}

function* watchRequestPostPrePostingFileEdit() {
  yield takeEvery(
    POST_PRE_POSTING_FILE_EDIT.request,
    sagaWrapper(postPrePostingFileEdit, POST_PRE_POSTING_FILE_EDIT)
  );
}

function* watchPostPrePostingFileEditSuccess() {
  yield takeEvery(
    POST_PRE_POSTING_FILE_EDIT.success,
    postPrePostingFileEditSuccess
  );
}

function* watchPostPrePostingFileSave() {
  yield takeEvery(
    POST_PRE_POSTING_FILE_SAVE.request,
    sagaWrapper(postPrePostingFileSave, POST_PRE_POSTING_FILE_SAVE)
  );
}

function* watchPostPrePostingFileSaveSuccess() {
  yield takeEvery(
    POST_PRE_POSTING_FILE_SAVE.success,
    postPrePostingFileSaveSuccess
  );
}

function* watchUploadPostPrePostingFile() {
  yield takeEvery(
    POST_PRE_POSTING_FILE_UPLOAD.request,
    sagaWrapper(uploadPostPrePostingFile, POST_PRE_POSTING_FILE_UPLOAD)
  );
}

function* watchUploadPostPrePostingFileSuccess() {
  yield takeEvery(
    POST_PRE_POSTING_FILE_UPLOAD.success,
    uploadPostPrePostingFileSuccess
  );
}

export default [
  watchUploadPostPrePostingFileSuccess,
  watchUploadPostPrePostingFile,
  watchPostPrePostingFileSaveSuccess,
  watchPostPrePostingFileSave,
  watchPostPrePostingFileEditSuccess,
  watchRequestPostPrePostingFileEdit,
  watchDeletePostPrePostingByIdSuccess,
  watchDeletePostPrePostingById,
  watchRequestPostPrePostingFiltered,
  watchRequestPostPrePosting,
  watchRequestPostPrePostingInitialData
];
