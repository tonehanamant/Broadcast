/* import { takeEvery, put, select } from "redux-saga/effects";
import FuzzySearch from "fuzzy-search";
import moment from "moment";

import * as appActions from "Ducks/app/actionTypes";
import {
  setOverlayLoading,
  setOverlayProcessing,
  createAlert,
  toggleModal,
  deployError,
  clearFile
} from "Ducks/app/index";

import sagaWrapper from "Utils/saga-wrapper";
import api from "API";
import {
  getPostPrePosting,
  clearFileUploadForm,
  receiveFilteredPostPrePosting
} from "./actions";
import * as postPrePostingActions from "./types"; */

// const ACTIONS = { ...appActions, ...postPrePostingActions };

/* const assignDisplay = data =>
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
} */

/* ////////////////////////////////// */
/* WATCHERS */
/* ////////////////////////////////// */
/* function* watchRequestPostPrePostingInitialData() {
  yield takeEvery(
    ACTIONS.POST_PRE_POSTING_INITIALDATA.request,
    sagaWrapper(
      requestPostPrePostingInitialData,
      ACTIONS.POST_PRE_POSTING_INITIALDATA
    )
  );
}

function* watchRequestPostPrePosting() {
  yield takeEvery(
    ACTIONS.POST_PRE_POSTING.request,
    sagaWrapper(requestPostPrePosting, ACTIONS.POST_PRE_POSTING)
  );
}

function* watchRequestPostPrePostingFiltered() {
  yield takeEvery(
    ACTIONS.FILTERED_POST_PRE_POSTING.request,
    requestPostPrePostingFiltered
  );
}

function* watchDeletePostPrePostingById() {
  yield takeEvery(
    ACTIONS.DELETE_POST_PRE_POSTING.request,
    sagaWrapper(deletePostPrePostingById, ACTIONS.DELETE_POST_PRE_POSTING)
  );
}

function* watchDeletePostPrePostingByIdSuccess() {
  yield takeEvery(
    ACTIONS.DELETE_POST_PRE_POSTING.success,
    deletePostPrePostingByIdSuccess
  );
}

function* watchRequestPostPrePostingFileEdit() {
  yield takeEvery(
    ACTIONS.POST_PRE_POSTING_FILE_EDIT.request,
    sagaWrapper(postPrePostingFileEdit, ACTIONS.POST_PRE_POSTING_FILE_EDIT)
  );
}

function* watchPostPrePostingFileEditSuccess() {
  yield takeEvery(
    ACTIONS.POST_PRE_POSTING_FILE_EDIT.success,
    postPrePostingFileEditSuccess
  );
}

function* watchPostPrePostingFileSave() {
  yield takeEvery(
    ACTIONS.POST_PRE_POSTING_FILE_SAVE.request,
    sagaWrapper(postPrePostingFileSave, ACTIONS.POST_PRE_POSTING_FILE_SAVE)
  );
}

function* watchPostPrePostingFileSaveSuccess() {
  yield takeEvery(
    ACTIONS.POST_PRE_POSTING_FILE_SAVE.success,
    postPrePostingFileSaveSuccess
  );
}

function* watchUploadPostPrePostingFile() {
  yield takeEvery(
    ACTIONS.POST_PRE_POSTING_FILE_UPLOAD.request,
    sagaWrapper(uploadPostPrePostingFile, ACTIONS.POST_PRE_POSTING_FILE_UPLOAD)
  );
}

function* watchUploadPostPrePostingFileSuccess() {
  yield takeEvery(
    ACTIONS.POST_PRE_POSTING_FILE_UPLOAD.success,
    uploadPostPrePostingFileSuccess
  );
} */

/* export default [
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
]; */

//
import { takeEvery, put, call } from "redux-saga/effects";
// import sagaWrapper from "Utils/saga-wrapper";
import api from "API";
// import * as ACTIONS from "Ducks/app/actionTypes";
import * as ACTIONS from "./types";

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
        id: "appEnvironment",
        loading: true
      }
    });
    // Yield getEnvirontment
    const response = yield getEnvironment();
    const { status, data } = response;
    // Unset loading overlay
    yield put({
      type: ACTIONS.SET_OVERLAY_LOADING,
      overlay: {
        id: "appEnvironment",
        loading: false
      }
    });
    // Check for 200 & response.data.Success
    if (status !== 200) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "No environment info returned.",
          message: `The server encountered an error processing the request (environment). Please try again or contact your administrator to review error logs. (HTTP Status: ${status})`
        }
      });
      throw new Error();
    }
    if (!data.Success) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "No environment info returned.",
          message:
            data.Message ||
            "The server encountered an error processing the request (environment). Please try again or contact your administrator to review error logs."
        }
      });
      throw new Error();
    }
    // Pass response.data to reducer
    yield put({
      type: ACTIONS.RECEIVE_ENVIRONMENT,
      data
    });
  } catch (e) {
    // Default error for try
    if (e.response) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "No environment info returned.",
          message:
            "The server encountered an error processing the request (environment). Please try again or contact your administrator to review error logs.",
          exception: e.response.data.ExceptionMessage || ""
        }
      });
    }
    if (!e.response && e.message) {
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
/* REQUEST EMPLOYEE */
/* ////////////////////////////////// */
export function* requestEmployee() {
  const { getEmployee } = api.app;

  try {
    yield put({
      type: ACTIONS.SET_OVERLAY_LOADING,
      overlay: {
        id: "appEmployee",
        loading: true
      }
    });
    const response = yield getEmployee();
    const { status, data } = response;
    yield put({
      type: ACTIONS.SET_OVERLAY_LOADING,
      overlay: {
        id: "appEmployee",
        loading: false
      }
    });
    if (status !== 200) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "No employee info returned.",
          message: `The server encountered an error processing the request (employee). Please try again or contact your administrator to review error logs. (HTTP Status: ${status})`
        }
      });
      throw new Error();
    }
    if (!data.Success) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "No employee info returned.",
          message:
            data.Message ||
            "The server encountered an error processing the request (employee). Please try again or contact your administrator to review error logs."
        }
      });
      throw new Error();
    }
    yield put({
      type: ACTIONS.RECEIVE_EMPLOYEE,
      data
    });
  } catch (e) {
    if (e.response) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "No employee info returned.",
          message:
            "The server encountered an error processing the request (employee). Please try again or contact your administrator to review error logs.",
          exception: e.response.data.ExceptionMessage || ""
        }
      });
    }
    if (!e.response && e.message) {
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
/* REQUEST READ FILE B64 */
/* ////////////////////////////////// */
export function* requestReadFileB64({ payload: file }) {
  const read = f =>
    new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onload = resolve;
      reader.onabort = reject;
      reader.onerror = reject;
      reader.readAsDataURL(f);
    });

  const getBase64 = e => e.target.result.split("base64,")[1];

  try {
    const dataURL = yield call(read, file);
    const b64 = yield getBase64(dataURL);
    // console.log('BASE64 FILE', b64);
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
export function* watchRequestEnvironment() {
  yield takeEvery(ACTIONS.REQUEST_ENVIRONMENT, requestEnvironment);
}

export function* watchRequestEmployee() {
  yield takeEvery(ACTIONS.REQUEST_EMPLOYEE, requestEmployee);
}

export function* watchReadFileB64() {
  yield takeEvery(ACTIONS.READ_FILE_B64, requestReadFileB64);
}

export default [
  watchRequestEnvironment,
  watchRequestEmployee,
  watchReadFileB64
];
