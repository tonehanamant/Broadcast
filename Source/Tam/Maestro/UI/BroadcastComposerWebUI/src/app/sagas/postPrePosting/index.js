/* eslint-disable import/prefer-default-export */
import { takeEvery, put, select } from 'redux-saga/effects';
import FuzzySearch from 'fuzzy-search';
import moment from 'moment';

import * as appActions from 'Ducks/app/actionTypes';
import * as postPrePostingActions from 'Ducks/postPrePosting/actionTypes';
import api from '../api';

const ACTIONS = { ...appActions, ...postPrePostingActions };

/* ////////////////////////////////// */
/* REQUEST POST PRE POSTING INITIAL DATA */
/* ////////////////////////////////// */
export function* requestPostPrePostingInitialData() {
  const { getPrePostInitialData } = api.postPrePosting;

  try {
    yield put({
      type: ACTIONS.SET_OVERLAY_LOADING,
      overlay: {
        id: 'postInitialData',
        loading: true },
      });
    const response = yield getPrePostInitialData();
    const { status, data } = response;
    yield put({
      type: ACTIONS.SET_OVERLAY_LOADING,
      overlay: {
        id: 'postInitialData',
        loading: false,
      },
    });
    if (status !== 200) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'No post initial data returned.',
          message: `The server encountered an error processing the request (post initial data). Please try again or contact your administrator to review error logs. (HTTP Status: ${status})`,
        },
      });
      throw new Error();
    }
    if (!data.Success) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'No post initial data returned.',
          message: data.Message || 'The server encountered an error processing the request (post initial data). Please try again or contact your administrator to review error logs.',
        },
      });
      throw new Error();
    }
    yield put({
      type: ACTIONS.RECEIVE_POST_PRE_POSTING_INITIALDATA,
      data,
    });
  } catch (e) {
    if (e.response) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'No post initial data returned.',
          message: 'The server encountered an error processing the request (post initial data). Please try again or contact your administrator to review error logs.',
          exception: e.response.data.ExceptionMessage || '',
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
/* REQUEST POST PRE POSTING */
/* ////////////////////////////////// */
export function* requestPostPrePosting() {
  const { getPosts } = api.postPrePosting;

  try {
    yield put({
      type: ACTIONS.SET_OVERLAY_LOADING,
      overlay: {
        id: 'postPosts',
        loading: true },
      });
    const response = yield getPosts();
    const { status, data } = response;
    yield put({
      type: ACTIONS.SET_OVERLAY_LOADING,
      overlay: {
        id: 'postPosts',
        loading: false,
      },
    });
    if (status !== 200) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'No post returned.',
          message: `The server encountered an error processing the request (post). Please try again or contact your administrator to review error logs. (HTTP Status: ${status})`,
        },
      });
      throw new Error();
    }
    if (!data.Success) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'No post returned.',
          message: data.Message || 'The server encountered an error processing the request (post). Please try again or contact your administrator to review error logs.',
        },
      });
      throw new Error();
    }
    yield put({
      type: ACTIONS.RECEIVE_POST_PRE_POSTING,
      data,
    });
    yield put({
      type: ACTIONS.REQUEST_ASSIGN_POST_DISPLAY,
      payload: {
        data: data.Data,
      },
    });
  } catch (e) {
    if (e.response) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'No post returned.',
          message: 'The server encountered an error processing the request (post). Please try again or contact your administrator to review error logs.',
          exception: e.response.data.ExceptionMessage || '',
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
/* ASSIGN POST DISPLAY */
/* ////////////////////////////////// */
export function* assignPostDisplay({ payload: request }) {
  const assignDisplay = () => request.data.map((item) => {
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
    },
  );

  try {
    yield put({
      type: ACTIONS.SET_OVERLAY_LOADING,
      overlay: {
        id: 'postPostsDisplay',
        loading: true },
      });
    const post = yield assignDisplay();
    yield put({
      type: ACTIONS.SET_OVERLAY_LOADING,
      overlay: {
        id: 'postPostsDisplay',
        loading: false },
      });
    yield put({
      type: ACTIONS.ASSIGN_POST_DISPLAY,
      data: post,
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
/* REQUEST POST PRE POSTING FILTERED */
/* ////////////////////////////////// */
export function* requestPostPrePostingFiltered({ payload: query }) {
  const postUnfiltered = yield select(state => state.post.postUnfiltered);
  const searcher = new FuzzySearch(postUnfiltered, ['FileName', 'DisplayDemos', 'DisplayUploadDate', 'DisplayModifiedDate'], { caseSensitive: false });
  const postFiltered = () => searcher.search(query);

  try {
    const filtered = yield postFiltered();
    yield put({
      type: ACTIONS.RECEIVE_FILTERED_POST_PRE_POSTING,
      data: filtered,
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
/* DELETE POST PRE POSTING BY ID */
/* ////////////////////////////////// */
export function* deletePostPrePostingById({ payload: id }) {
  const { deletePost } = api.postPrePosting;

  try {
    const response = yield deletePost(id);
    const { status, data } = response;
    if (status !== 200) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'Post not deleted.',
          message: `The server encountered an error processing the request (delete post ${id}). Please try again or contact your administrator to review error logs. (HTTP Status: ${status})`,
        },
      });
      throw new Error();
    }
    if (!data.Success) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'Post not deleted.',
          message: data.Message || `The server encountered an error processing the request (delete post ${id}). Please try again or contact your administrator to review error logs.`,
        },
      });
      throw new Error();
    }
    yield put({
      type: ACTIONS.CREATE_ALERT,
      alert: {
        type: 'success',
        headline: 'Posting Removed',
        message: `${id} was successfully removed.`,
      },
    });
    yield put({
      type: ACTIONS.REQUEST_POST_PRE_POSTING });
  } catch (e) {
    if (e.response) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'Post not deleted.',
          message: 'The server encountered an error processing the request (delete post). Please try again or contact your administrator to review error logs.',
          exception: e.response.data.ExceptionMessage || '',
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
/* REQUEST POST FILE EDIT  */
/* ////////////////////////////////// */
export function* requestPostPrePostingFileEdit({ payload: id }) {
  const { getPost } = api.postPrePosting;
  try {
    yield put({
      type: ACTIONS.SET_OVERLAY_LOADING,
      overlay: {
        id: 'getPost',
        loading: true,
      },
    });
    const response = yield getPost(id);
    const { status, data } = response;
    yield put({
      type: ACTIONS.SET_OVERLAY_LOADING,
      overlay: {
        id: 'getPost',
        loading: false,
      },
    });
    if (status !== 200) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'No post data returned.',
          message: `The server encountered an error processing the request (post ${id}). Please try again or contact your administrator to review error logs. (HTTP Status: ${status})`,
        },
      });
      throw new Error();
    }
    if (!data.Success) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'No post data returned.',
          message: data.Message || `The server encountered an error processing the request (post ${id}). Please try again or contact your administrator to review error logs.`,
        },
      });
      throw new Error();
    }
    yield put({
      type: ACTIONS.RECEIVE_POST_PRE_POSTING_FILE_EDIT,
      data,
    });
    yield put({
      type: ACTIONS.TOGGLE_MODAL,
      modal: {
        modal: 'postFileEditModal',
        active: true,
      },
    });
  } catch (e) {
    if (e.response) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'No post data returned.',
          message: 'The server encountered an error processing the request (post). Please try again or contact your administrator to review error logs.',
          exception: e.response.data.ExceptionMessage || '',
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
/* SAVE POST PRE POSTING FILE EDIT */
/* ////////////////////////////////// */
export function* savePostPrePostingFileEdit({ payload: params }) {
  const { savePost } = api.postPrePosting;
  try {
    yield put({
      type: ACTIONS.SET_OVERLAY_PROCESSING,
      overlay: {
        id: 'savePostEdit',
        processing: true,
      },
    });
    const response = yield savePost(params);
    const { status, data } = response;
    yield put({
      type: ACTIONS.SET_OVERLAY_PROCESSING,
      overlay: {
        id: 'savePostEdit',
        processing: false,
      },
    });
    if (status !== 200) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'Post not saved.',
          message: `The server encountered an error processing the request (save post ${params.FileId}). Please try again or contact your administrator to review error logs. (HTTP Status: ${status})`,
        },
      });
      throw new Error();
    }
    if (!data.Success) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'Post not saved.',
          message: data.Message || `The server encountered an error processing the request (save post ${params.FileId}). Please try again or contact your administrator to review error logs.`,
        },
      });
      throw new Error();
    }
    yield put({
      type: ACTIONS.RECEIVE_POST_PRE_POSTING_FILE_EDIT_SAVE,
      data,
    });
    yield put({
      type: ACTIONS.TOGGLE_MODAL,
      modal: {
        modal: 'postFileEditModal',
        active: false,
      },
    });
    yield put({
      type: ACTIONS.CREATE_ALERT,
      alert: {
        type: 'success',
        headline: 'Post File Updated',
      },
    });
    yield put({
      type: ACTIONS.REQUEST_POST_PRE_POSTING,
    });
  } catch (e) {
    if (e.response) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'Post not saved.',
          message: 'The server encountered an error processing the request (save post). Please try again or contact your administrator to review error logs.',
          exception: e.response.data.ExceptionMessage || '',
        },
      });
    }
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
/* UPLOAD POST PRE POSTING FILE */
/* ////////////////////////////////// */
export function* uploadPostPrePostingFile({ payload: params }) {
  const { uploadPost } = api.postPrePosting;
  try {
    yield put({
      type: ACTIONS.SET_OVERLAY_PROCESSING,
      overlay: {
        id: 'uploadPost',
        processing: true,
      },
    });
    const response = yield uploadPost(params);
    const { status, data } = response;
    yield put({
      type: ACTIONS.SET_OVERLAY_PROCESSING,
      overlay: {
        id: 'uploadPost',
        processing: false,
      },
    });
    if (status !== 200) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'Post file not created.',
          message: `The server encountered an error processing the request (create post ${params.FileId}). Please try again or contact your administrator to review error logs. (HTTP Status: ${status})`,
        },
      });
      throw new Error();
    }
    if (!data.Success) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'Post file not created.',
          message: data.Message || `The server encountered an error processing the request (create post ${params.FileId}). Please try again or contact your administrator to review error logs.`,
        },
      });
      throw new Error();
    }
    // yield put({
    //   type: ACTIONS.RECEIVE_POST_FILE,
    //   data,
    // });
    yield put({
      type: ACTIONS.TOGGLE_MODAL,
      modal: {
        modal: 'postFileUploadModal',
        active: false,
      },
    });
    yield put({
      type: ACTIONS.CREATE_ALERT,
      alert: {
        type: 'success',
        headline: 'Post File Uploaded',
      },
    });
    yield put({
      type: ACTIONS.CLEAR_FILE,
    });
    yield put({
      type: ACTIONS.CLEAR_FILE_UPLOAD_FORM,
    });
    yield put({
      type: ACTIONS.REQUEST_POST_PRE_POSTING,
    });
  } catch (e) {
    if (e.response) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'Post file not created.',
          message: 'The server encountered an error processing the request (create post). Please try again or contact your administrator to review error logs.',
          exception: e.response.data.ExceptionMessage || '',
        },
      });
    }
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
export function* watchRequestPostPrePostingInitialData() {
  yield takeEvery(ACTIONS.REQUEST_POST_PRE_POSTING_INITIALDATA, requestPostPrePostingInitialData);
}

export function* watchRequestPostPrePosting() {
  yield takeEvery(ACTIONS.REQUEST_POST_PRE_POSTING, requestPostPrePosting);
}

export function* watchRequestAssignPostDisplay() {
  yield takeEvery(ACTIONS.REQUEST_ASSIGN_POST_DISPLAY, assignPostDisplay);
}

export function* watchRequestPostPrePostingFiltered() {
  yield takeEvery(ACTIONS.REQUEST_FILTERED_POST_PRE_POSTING, requestPostPrePostingFiltered);
}

export function* watchDeletePostPrePostingById() {
  yield takeEvery(ACTIONS.DELETE_POST_PRE_POSTING, deletePostPrePostingById);
}

export function* watchRequestPostPrePostingFileEdit() {
  yield takeEvery(ACTIONS.REQUEST_POST_PRE_POSTING_FILE_EDIT, requestPostPrePostingFileEdit);
}

export function* watchSavePostPrePostingFileEdit() {
  yield takeEvery(ACTIONS.REQUEST_POST_PRE_POSTING_FILE_EDIT_SAVE, savePostPrePostingFileEdit);
}

export function* watchUploadPostPrePostingFile() {
  yield takeEvery(ACTIONS.REQUEST_POST_PRE_POSTING_FILE_UPLOAD, uploadPostPrePostingFile);
}

// if assign watcher > assign in sagas/index rootSaga also
