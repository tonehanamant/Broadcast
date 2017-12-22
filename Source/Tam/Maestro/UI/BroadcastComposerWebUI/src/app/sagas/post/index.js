/* eslint-disable import/prefer-default-export */
import { takeEvery, put, select } from 'redux-saga/effects';
import FuzzySearch from 'fuzzy-search';
import moment from 'moment';

import * as appActions from 'Ducks/app/actionTypes';
import * as postPrePostingActions from 'Ducks/post/actionTypes';
import api from '../api';

const ACTIONS = { ...appActions, ...postPrePostingActions };

/* ////////////////////////////////// */
/* REQUEST POST INITIAL DATA */
/* ////////////////////////////////// */
export function* requestPostInitialData() {
  const { getPostInitialData } = api.postPrePosting;

  try {
    yield put({
      type: ACTIONS.SET_OVERLAY_LOADING,
      overlay: {
        id: 'postInitialData',
        loading: true },
      });
    const response = yield getPostInitialData();
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
      type: ACTIONS.RECEIVE_POST_INITIALDATA,
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
/* REQUEST POST */
/* ////////////////////////////////// */
export function* requestPost() {
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
      type: ACTIONS.RECEIVE_POST,
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
/* REQUEST POST FILTERED */
/* ////////////////////////////////// */
export function* requestPostFiltered({ payload: query }) {
  const postUnfiltered = yield select(state => state.post.postUnfiltered);
  const searcher = new FuzzySearch(postUnfiltered, ['FileName', 'DisplayDemos', 'DisplayUploadDate', 'DisplayModifiedDate'], { caseSensitive: false });
  const postFiltered = () => searcher.search(query);

  try {
    const filtered = yield postFiltered();
    yield put({
      type: ACTIONS.RECEIVE_FILTERED_POST,
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
/* DELETE POST BY ID */
/* ////////////////////////////////// */
export function* deletePostById({ payload: id }) {
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
      type: ACTIONS.REQUEST_POST });
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
export function* requestPostFileEdit({ payload: id }) {
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
      type: ACTIONS.RECEIVE_POST_FILE_EDIT,
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
/* SAVE POST FILE EDIT */
/* ////////////////////////////////// */
export function* savePostFileEdit({ payload: params }) {
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
      type: ACTIONS.RECEIVE_POST_FILE_EDIT_SAVE,
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
      type: ACTIONS.REQUEST_POST,
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
/* UPLOAD POST FILE */
/* ////////////////////////////////// */
export function* uploadPostFile({ payload: params }) {
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
      type: ACTIONS.REQUEST_POST,
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
export function* watchRequestPostInitialData() {
  yield takeEvery(ACTIONS.REQUEST_POST_INITIALDATA, requestPostInitialData);
}

export function* watchRequestPost() {
  yield takeEvery(ACTIONS.REQUEST_POST, requestPost);
}

export function* watchRequestAssignPostDisplay() {
  yield takeEvery(ACTIONS.REQUEST_ASSIGN_POST_DISPLAY, assignPostDisplay);
}

export function* watchRequestPostFiltered() {
  yield takeEvery(ACTIONS.REQUEST_FILTERED_POST, requestPostFiltered);
}

export function* watchDeletePostById() {
  yield takeEvery(ACTIONS.DELETE_POST, deletePostById);
}

export function* watchRequestPostFileEdit() {
  yield takeEvery(ACTIONS.REQUEST_POST_FILE_EDIT, requestPostFileEdit);
}

export function* watchSavePostFileEdit() {
  yield takeEvery(ACTIONS.REQUEST_POST_FILE_EDIT_SAVE, savePostFileEdit);
}

export function* watchUploadPostFile() {
  yield takeEvery(ACTIONS.REQUEST_POST_FILE_UPLOAD, uploadPostFile);
}

// if assign watcher > assign in sagas/index rootSaga also
