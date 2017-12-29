import { takeEvery, put, select } from 'redux-saga/effects';
import FuzzySearch from 'fuzzy-search';
import * as appActions from 'Ducks/app/actionTypes';
import * as postActions from 'Ducks/post/actionTypes';
import api from '../api';

const ACTIONS = { ...appActions, ...postActions };

export function* requestPost() {
  const { getPosts } = api.post;

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
/* WATCHERS */
/* ////////////////////////////////// */

export function* watchRequestPost() {
  yield takeEvery(ACTIONS.REQUEST_POST, requestPost);
}

export function* watchRequestPostFiltered() {
  yield takeEvery(ACTIONS.REQUEST_FILTERED_POST, requestPostFiltered);
}
