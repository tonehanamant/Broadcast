import { takeEvery, put, select } from 'redux-saga/effects';
import FuzzySearch from 'fuzzy-search';
import moment from 'moment';


import * as appActions from 'Ducks/app/actionTypes';
import * as postPrePostingActions from 'Ducks/postPrePosting/actionTypes';
import {
  setOverlayLoading,
  setOverlayProcessing,
  createAlert,
  toggleModal,
  deployError,
  clearFile,
} from 'Ducks/app/index';
import {
  getPostPrePosting,
  clearFileUploadForm,
  receiveFilteredPostPrePosting,
} from 'Ducks/postPrePosting/index';

import sagaWrapper from '../wrapper';
import api from '../api';

const ACTIONS = { ...appActions, ...postPrePostingActions };

const assignDisplay = data => (
  data.map((item) => {
    const post = item;
    // DemoLookups
    post.DisplayDemos = post.DemoLookups.map((demo, index, arr) => {
      if (index === arr.length - 1) {
        return `${demo.Display}`;
      }
      return `${demo.Display}, `;
    });
    // UploadDate
    post.DisplayUploadDate = moment(post.UploadDate).format('M/D/YYYY');
    // ModifiedDate
    post.DisplayModifiedDate = moment(post.ModifiedDate).format('M/D/YYYY');
    return post;
  })
);

/* ////////////////////////////////// */
/* REQUEST POST PRE POSTING INITIAL DATA */
/* ////////////////////////////////// */
export function* requestPostPrePostingInitialData() {
  const { getInitialData } = api.postPrePosting;
  try {
    yield put(setOverlayLoading({ id: 'postInitialData', loading: true }));
    return yield getInitialData();
  } finally {
    yield put(setOverlayLoading({ id: 'postInitialData', loading: false }));
  }
}

/* ////////////////////////////////// */
/* REQUEST POST PRE POSTING */
/* ////////////////////////////////// */
export function* requestPostPrePosting() {
  const { getPosts } = api.postPrePosting;
  try {
    yield put(setOverlayLoading({ id: 'postPosts', loading: true }));
    const { status, data } = yield getPosts();
    const assignedData = assignDisplay(data.Data);
    return { status, data: { ...data, Data: assignedData } };
  } finally {
    yield put(setOverlayLoading({ id: 'postPosts', loading: false }));
  }
}


/* ////////////////////////////////// */
/* REQUEST POST PRE POSTING FILTERED */
/* ////////////////////////////////// */
export function* requestPostPrePostingFiltered({ payload: query }) {
  const postUnfiltered = yield select(state => state.postPrePosting.postUnfiltered);
  const searcher = new FuzzySearch(postUnfiltered, ['FileName', 'DisplayDemos', 'DisplayUploadDate', 'DisplayModifiedDate'], { caseSensitive: false });
  const postFiltered = () => searcher.search(query);
  try {
    const filtered = yield postFiltered();
    yield put(receiveFilteredPostPrePosting(filtered));
  } catch (e) {
    if (e.message) {
      yield put(deployError({ message: e.message }));
    }
  }
}

/* ////////////////////////////////// */
/* DELETE POST PRE POSTING BY ID */
/* ////////////////////////////////// */

export function* deletePostPrePostingById(id) {
  const { deletePost } = api.postPrePosting;
  return yield deletePost(id);
}

export function* deletePostPrePostingByIdSuccess({ payload }) {
  yield put(createAlert({
    type: 'success',
    headline: 'Posting Removed',
    message: `${payload} was successfully removed.`,
  }));
  yield put(getPostPrePosting());
}

/* ////////////////////////////////// */
/* REQUEST POST FILE EDIT  */
/* ////////////////////////////////// */
export function* postPrePostingFileEdit(id) {
  const { getPost } = api.postPrePosting;
  try {
    yield put(setOverlayLoading({ id: 'getPost', loading: true }));
    return yield getPost(id);
  } finally {
    yield put(setOverlayLoading({ id: 'getPost', loading: false }));
  }
}

export function* postPrePostingFileEditSuccess() {
  yield put(toggleModal({ modal: 'postFileEditModal', active: true }));
}

/* ////////////////////////////////// */
/* SAVE POST PRE POSTING FILE EDIT */
/* ////////////////////////////////// */
export function* postPrePostingFileSave(params) {
  const { savePost } = api.postPrePosting;
  try {
    yield put(setOverlayProcessing({ id: 'savePostEdit', processing: true }));
    return yield savePost(params);
  } finally {
    yield put(setOverlayProcessing({ id: 'savePostEdit', processing: false }));
  }
}

export function* postPrePostingFileSaveSuccess() {
  yield put(toggleModal({ modal: 'postFileEditModal', active: false }));
  yield put(createAlert({ type: 'success', headline: 'Post File Updated' }));
  yield put(getPostPrePosting());
}

/* ////////////////////////////////// */
/* UPLOAD POST PRE POSTING FILE */
/* ////////////////////////////////// */
export function* uploadPostPrePostingFile(params) {
  const { uploadPost } = api.postPrePosting;
  try {
    yield put(setOverlayProcessing({ id: 'uploadPost', processing: true }));
    return yield uploadPost(params);
  } finally {
    yield put(setOverlayProcessing({ id: 'uploadPost', processing: false }));
  }
}

export function* uploadPostPrePostingFileSuccess() {
  yield put(toggleModal({ modal: 'postFileUploadModal', active: false }));
  yield put(createAlert({ type: 'success', headline: 'Post File Updated' }));
  yield put(clearFile());
  yield put(clearFileUploadForm());
  yield put(getPostPrePosting());
}


/* ////////////////////////////////// */
/* WATCHERS */
/* ////////////////////////////////// */
export function* watchRequestPostPrePostingInitialData() {
  yield takeEvery(
    ACTIONS.POST_PRE_POSTING_INITIALDATA.request,
    sagaWrapper(requestPostPrePostingInitialData, ACTIONS.POST_PRE_POSTING_INITIALDATA),
  );
}

export function* watchRequestPostPrePosting() {
  yield takeEvery(
    ACTIONS.POST_PRE_POSTING.request,
    sagaWrapper(requestPostPrePosting, ACTIONS.POST_PRE_POSTING),
  );
}

export function* watchRequestPostPrePostingFiltered() {
  yield takeEvery(
    ACTIONS.FILTERED_POST_PRE_POSTING.request, requestPostPrePostingFiltered,
  );
}

export function* watchDeletePostPrePostingById() {
  yield takeEvery(
    ACTIONS.DELETE_POST_PRE_POSTING.request,
    sagaWrapper(deletePostPrePostingById, ACTIONS.DELETE_POST_PRE_POSTING),
  );
}

export function* watchDeletePostPrePostingByIdSuccess() {
  yield takeEvery(ACTIONS.DELETE_POST_PRE_POSTING.success, deletePostPrePostingByIdSuccess);
}

export function* watchRequestPostPrePostingFileEdit() {
  yield takeEvery(
    ACTIONS.POST_PRE_POSTING_FILE_EDIT.request,
    sagaWrapper(postPrePostingFileEdit, ACTIONS.POST_PRE_POSTING_FILE_EDIT),
  );
}

export function* watchPostPrePostingFileEditSuccess() {
  yield takeEvery(ACTIONS.POST_PRE_POSTING_FILE_EDIT.success, postPrePostingFileEditSuccess);
}

export function* watchPostPrePostingFileSave() {
  yield takeEvery(
    ACTIONS.POST_PRE_POSTING_FILE_SAVE.request,
    sagaWrapper(postPrePostingFileSave, ACTIONS.POST_PRE_POSTING_FILE_SAVE),
  );
}

export function* watchPostPrePostingFileSaveSuccess() {
  yield takeEvery(ACTIONS.POST_PRE_POSTING_FILE_SAVE.success, postPrePostingFileSaveSuccess);
}

export function* watchUploadPostPrePostingFile() {
  yield takeEvery(
    ACTIONS.POST_PRE_POSTING_FILE_UPLOAD.request,
    sagaWrapper(uploadPostPrePostingFile, ACTIONS.POST_PRE_POSTING_FILE_UPLOAD),
  );
}

export function* watchUploadPostPrePostingFileSuccess() {
  yield takeEvery(ACTIONS.POST_PRE_POSTING_FILE_UPLOAD.success, uploadPostPrePostingFileSuccess);
}

// if assign watcher > assign in sagas/index rootSaga also
