import { takeEvery, put, select } from 'redux-saga/effects';
import FuzzySearch from 'fuzzy-search';
import moment from 'moment';

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

/* ////////////////////////////////// */
/* ASSIGN POST DISPLAY */
/* ////////////////////////////////// */
export function* assignPostDisplay({ payload: request }) {
  const assignDisplay = () => request.data.map((item) => {
      const post = item;

      // UploadDate
      post.DisplayUploadDate = post.UploadDate !== null ? moment(post.UploadDate).format('M/D/YYYY') : '-';
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

export function* requestPostFiltered({ payload: query }) {
  const postListUnfiltered = yield select(state => state.post.postUnfiltered);

  // for each post, convert all properties to string to enable use on FuzzySearch object
  postListUnfiltered.map(post => (
    Object.keys(post).map((key) => {
      if (post[key] !== null && post[key] !== undefined) {
        post[key] = post[key].toString(); // eslint-disable-line no-param-reassign
      }
      return post[key];
    })
  ));

  const keys = ['ContractId', 'ContractName', 'DisplayUploadDate', 'PrimaryAudienceImpressions', 'SpotsInSpec', 'SpotsOutOfSpec', 'UploadDate'];
  const searcher = new FuzzySearch(postListUnfiltered, keys, { caseSensitive: false });
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
/* REQUEST POST SCRUBBING HEADER */
/* ////////////////////////////////// */
export function* requestPostScrubbingHeader({ payload: proposalID }) {
  const { getPostScrubbingHeader } = api.post;
  try {
    yield put({
      type: ACTIONS.SET_OVERLAY_LOADING,
      overlay: {
        id: 'PostScrubbingHeader',
        loading: true },
      });
    const response = yield getPostScrubbingHeader(proposalID);
    const { status, data } = response;

    yield put({
      type: ACTIONS.SET_OVERLAY_LOADING,
      overlay: {
        id: 'PostScrubbingHeader',
        loading: false,
      },
    });
    if (status !== 200) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'No post proposal data returned.',
          message: `The server encountered an error processing the request (proposal). Please try again or contact your administrator to review error logs. (HTTP Status: ${status})`,
        },
      });
      throw new Error();
    }
    if (!data.Success) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'No proposal data returned.',
          message: data.Message || 'The server encountered an error processing the request (proposal). Please try again or contact your administrator to review error logs.',
        },
      });
      throw new Error();
    }
    yield put({
      type: ACTIONS.RECEIVE_POST_SCRUBBING_HEADER,
      data,
    });
    yield put({
      type: ACTIONS.TOGGLE_MODAL,
      modal: {
        modal: 'postScrubbingModal',
        active: true,
        properties: {
          titleText: 'POST SCRUBBING MODAL',
          bodyText: 'Post Scrubbing details will be shown here!',
        },
      },
    });
  } catch (e) {
    if (e.response) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'No proposal data returned.',
          message: 'The server encountered an error processing the request (proposal). Please try again or contact your administrator to review error logs.',
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

export function* requestScrubbingDataFiltered({ payload: query }) {
  const listUnfiltered = yield select(state => state.post.proposalHeader.scrubbingData);

  listUnfiltered.Details.forEach((details) => {
      details.ClientScrubs.forEach((item) => {
          item.map(post => (
            Object.keys(post).map((key) => {
              if (post[key] !== null && post[key] !== undefined) {
                post[key] = post[key].toString(); // eslint-disable-line no-param-reassign
              }
              return post[key];
            })
          ));
      });
  });

  const keys = ['WeekStart', 'TimeAired', 'MatchTime', 'DayOfWeek', 'SpotLength', 'ISCI', 'ProgramName', 'GenreName', 'Affiliate', 'Market', 'Station'];
  const searcher = new FuzzySearch(listUnfiltered, keys, { caseSensitive: false });
  const scrubbingDataFiltered = () => searcher.search(query);

  try {
    const filtered = yield scrubbingDataFiltered();
    yield put({
      type: ACTIONS.RECEIVE_FILTERED_SCRUBBING_DATA,
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

export function* watchRequestAssignPostDisplay() {
  yield takeEvery(ACTIONS.REQUEST_ASSIGN_POST_DISPLAY, assignPostDisplay);
}

export function* watchRequestPostFiltered() {
  yield takeEvery(ACTIONS.REQUEST_FILTERED_POST, requestPostFiltered);
}

export function* watchRequestPostScrubbingHeader() {
  yield takeEvery(ACTIONS.REQUEST_POST_SCRUBBING_HEADER, requestPostScrubbingHeader);
}

export function* watchRequestScrubbingDataFiltered() {
  yield takeEvery(ACTIONS.REQUEST_FILTERED_SCRUBBING_DATA, requestScrubbingDataFiltered);
}
