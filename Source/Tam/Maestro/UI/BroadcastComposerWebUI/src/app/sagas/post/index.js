import { takeEvery, put, call, select } from 'redux-saga/effects';
import FuzzySearch from 'fuzzy-search';
import moment from 'moment';
import _ from 'lodash';
import * as appActions from 'Ducks/app/actionTypes';
import * as postActions from 'Ducks/post/actionTypes';
import { setOverlayLoading, toggleModal } from 'Ducks/app';
import { selectModal } from 'Ducks/app/selectors';
import api from '../api';
import sagaWrapper from '../wrapper';

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
  const assignDisplay = () => request.data.Posts.map((item) => {
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
    const Posts = yield assignDisplay();
    const post = Object.assign({}, request.data, { Posts });
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
  const postListUnfiltered = yield select(state => state.post.postUnfilteredGridData);

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
/* REQUEST CLEAR SCRUBBING FILTER LIST - so grid will update object data */
/* ////////////////////////////////// */
export function* requestClearScrubbingDataFiltersList() {
  try {
    yield put({
      type: ACTIONS.RECEIVE_CLEAR_SCRUBBING_FILTERS_LIST,
      data: [],
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
/* REQUEST POST CLIENT SCRUBBING */
/* ////////////////////////////////// */
// allow for params (todo from BE) to filterKey All, InSpec, OutOfSpec; optional showModal (from Post landing);
// if not from modal show processing, else show loading (loading not shown inside modal)
export function* requestPostClientScrubbing({ payload: params }) {
  const { getPostClientScrubbing } = api.post;
  try {
    if (params.showModal) {
      yield put({
        type: ACTIONS.SET_OVERLAY_LOADING,
        overlay: {
          id: 'PostClientScrubbing',
          loading: true },
      });
    } else {
      yield put({
        type: ACTIONS.SET_OVERLAY_PROCESSING,
        overlay: {
          id: 'PostClientScrubbing',
          processing: true },
      });
    }
    // clear the data so filters grid registers as update - if not from modal update
    if (!params.showModal) {
      yield call(requestClearScrubbingDataFiltersList);
    }
    const response = yield getPostClientScrubbing(params);
    const { status, data } = response;

    if (params.showModal) {
      yield put({
        type: ACTIONS.SET_OVERLAY_LOADING,
        overlay: {
          id: 'PostClientScrubbing',
          loading: false },
      });
    } else {
      yield put({
        type: ACTIONS.SET_OVERLAY_PROCESSING,
        overlay: {
          id: 'PostClientScrubbing',
          processing: false },
      });
    }
    if (status !== 200) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'No proposal client scrubbing data returned.',
          message: `The server encountered an error processing the request (proposal scrubbing). Please try again or contact your administrator to review error logs. (HTTP Status: ${status})`,
        },
      });
      throw new Error();
    }
    if (!data.Success) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'No proposal client scrubbing data returned.',
          message: data.Message || 'The server encountered an error processing the request (proposal scrubbing). Please try again or contact your administrator to review error logs.',
        },
      });
      throw new Error();
    }
    yield put({
      type: ACTIONS.RECEIVE_POST_CLIENT_SCRUBBING,
      data,
    });
    if (params.showModal) {
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
    }
  } catch (e) {
    if (e.response) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'No proposal scrubbing data returned.',
          message: 'The server encountered an error processing the request (proposal scrubbing). Please try again or contact your administrator to review error logs.',
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

// FILTERING
// tbd how to iterate multiple versus single and determine set to check active or original
// todo break down original scrubbing to ClientScrubs etc
export function* requestScrubbingDataFiltered({ payload: query }) {
  const listUnfiltered = yield select(state => state.post.proposalHeader.scrubbingData.ClientScrubs);
  const listFiltered = yield select(state => state.post.proposalHeader.activeScrubbingData.ClientScrubs);
  const activeFilters = _.cloneDeep(yield select(state => state.post.activeScrubbingFilters));
  const originalFilters = yield select(state => state.post.activeScrubbingFilters);
  const actingFilter = activeFilters[query.filterKey]; // this is undefined
  // console.log('request scrub filter', query, activeFilters, actingFilter);
  const applyFilter = () => {
     // active -depends on if clearing etc; also now if matching in play
    const isActive = (query.exclusions.length > 0) || query.activeMatch;
     // todo should apply copy?
    actingFilter.active = isActive;
    actingFilter.exclusions = query.exclusions;
    actingFilter.filterOptions = query.filterOptions;
    actingFilter.matchOptions = query.matchOptions;
    actingFilter.activeMatch = query.activeMatch;
    // TBD iterate existing or acting only?
    const filteredResult = listUnfiltered.filter((item) => {
       let ret = true;
      _.forEach(activeFilters, (value) => {
        if (value.active && ret === true) {
          if (value.activeMatch) {
            // just base on one or the other?
            const toMatch = (value.matchOptions.inSpec === true);
           ret = !_.includes(value.exclusions, item[value.filterKey]) && item[value.matchOptions.matchKey] === toMatch;
           // console.log('filter each', ret, item[value.filterKey]);
          } else {
            ret = !_.includes(value.exclusions, item[value.filterKey]);
          }
        }
      });
      return ret;
    });
    // console.log('request apply filter', actingFilter, activeFilters);
    // test to make sure there is returned data
    if (filteredResult.length < 1) {
      return { filteredClientScrubs: listFiltered, actingFilter, activeFilters: originalFilters, alertEmpty: true };
    }
    return { filteredClientScrubs: filteredResult, actingFilter, activeFilters, alertEmpty: false };
  };

  try {
    // show processing?
    yield put({
      type: ACTIONS.SET_OVERLAY_PROCESSING,
      overlay: {
        id: 'PostScrubbingFilter',
        processing: true },
    });
    // clear the data so grid registers as update
    yield call(requestClearScrubbingDataFiltersList);
    const filtered = yield applyFilter();

    yield put({
      type: ACTIONS.SET_OVERLAY_PROCESSING,
      overlay: {
        id: 'PostScrubbingFilter',
        processing: false },
    });
    // if empty show alert - will set to original state
    if (filtered.alertEmpty) {
      const msg = `${filtered.actingFilter.filterDisplay} Filter will remove all data.`;
      yield put({
        type: ACTIONS.CREATE_ALERT,
        alert: {
          type: 'warning',
          headline: 'Filter Not Applied',
          message: msg,
        },
      });
    }

    yield put({
      type: ACTIONS.RECEIVE_FILTERED_SCRUBBING_DATA,
      data: filtered,
    });
  } catch (e) {
    if (e.message) {
      // todo should reset activeFilters (cleared) if error?
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
export function* requestUnlinkedIscis() {
  const { getUnlinkedIscis } = api.post;
  try {
    yield put(setOverlayLoading({ id: 'PostUniqueIscis', loading: true }));
    return yield getUnlinkedIscis();
  } finally {
    yield put(setOverlayLoading({ id: 'PostUniqueIscis', loading: false }));
  }
}

export function* unlinkedIscisSuccess() {
  const modal = select(selectModal, 'postUnlinkedIsciModal');
  if (modal && !modal.active) {
    yield put(toggleModal({
      modal: 'postUnlinkedIsciModal',
      active: true,
      properties: {
        titleText: 'POST Unique Iscis',
        bodyText: 'Isci Details',
      },
    }));
  }
}


export function* archiveUnlinkedIsci({ ids }) {
  const { archiveUnlinkedIscis } = api.post;
  try {
    yield put(setOverlayLoading({ id: 'postArchiveIsci', loading: true }));
    return yield archiveUnlinkedIscis(ids);
  } finally {
    yield put(setOverlayLoading({ id: 'postArchiveIsci', loading: false }));
  }
}


export function* loadArchivedIsci() {
  const { getArchivedIscis } = api.post;
  try {
    yield put(setOverlayLoading({ id: 'loadArchiveIsci', loading: true }));
    return yield getArchivedIscis();
  } finally {
    yield put(setOverlayLoading({ id: 'loadArchiveIsci', loading: false }));
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

export function* watchRequestPostClientScrubbing() {
  yield takeEvery(ACTIONS.REQUEST_POST_CLIENT_SCRUBBING, requestPostClientScrubbing);
}

export function* watchRequestScrubbingDataFiltered() {
  yield takeEvery(ACTIONS.REQUEST_FILTERED_SCRUBBING_DATA, requestScrubbingDataFiltered);
}

export function* watchRequestClearScrubbingFiltersList() {
  yield takeEvery(ACTIONS.REQUEST_CLEAR_SCRUBBING_FILTERS_LIST, requestClearScrubbingDataFiltersList);
}

export function* watchRequestUniqueIscis() {
  yield takeEvery(
    [ACTIONS.UNLINKED_ISCIS_DATA.request, ACTIONS.ARCHIVE_UNLIKED_ISCI.success],
    sagaWrapper(requestUnlinkedIscis, ACTIONS.UNLINKED_ISCIS_DATA),
  );
}

export function* watchRequestUniqueIscisSuccess() {
  yield takeEvery(ACTIONS.UNLINKED_ISCIS_DATA.success, unlinkedIscisSuccess);
}

export function* watchArchiveUnlinkedIsci() {
  yield takeEvery(ACTIONS.ARCHIVE_UNLIKED_ISCI.request, sagaWrapper(archiveUnlinkedIsci, ACTIONS.ARCHIVE_UNLIKED_ISCI));
}

export function* watchLoadArchivedIscis() {
  yield takeEvery(ACTIONS.LOAD_ARCHIVED_ISCI.request, sagaWrapper(loadArchivedIsci, ACTIONS.LOAD_ARCHIVED_ISCI));
}
