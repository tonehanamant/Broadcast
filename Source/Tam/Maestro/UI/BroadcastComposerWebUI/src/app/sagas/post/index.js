import { takeEvery, put, call, select } from 'redux-saga/effects';
import FuzzySearch from 'fuzzy-search';
import moment from 'moment';
import _ from 'lodash';
import * as appActions from 'Ducks/app/actionTypes';
import * as postActions from 'Ducks/post/actionTypes';
import { setOverlayLoading, toggleModal } from 'Ducks/app';
import { selectModal } from 'Ducks/app/selectors';
import { getPost } from 'Ducks/post';
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
    data.Data.filterKey = params.filterKey; // set for ref in store
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
    let hasActiveScrubbingFilters = false;
    // TBD iterate existing or acting only?
    const filteredResult = listUnfiltered.filter((item) => {
       let ret = true;

      _.forEach(activeFilters, (value) => {
        if (value.active && ret === true) {
          hasActiveScrubbingFilters = true;
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
      return { filteredClientScrubs: listFiltered, actingFilter, activeFilters: originalFilters, alertEmpty: true, hasActiveScrubbingFilters };
    }
    return { filteredClientScrubs: filteredResult, actingFilter, activeFilters, alertEmpty: false, hasActiveScrubbingFilters };
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


/* ////////////////////////////////// */
/* refilter scrubs following override */
/* ////////////////////////////////// */
export function refilterOnOverride(clientScrubs, keys, status, isRemove) {
  // if is Remove filter out the keys
  // else for each key find scrub and change Status/overide true
  if (isRemove) {
    return clientScrubs.filter(item => !_.includes(keys, item.ScrubbingClientId));
  }

  return clientScrubs.map((item) => {
    if (_.includes(keys, item.ScrubbingClientId)) {
      return { ...item, Status: status, StatusOverride: true };
    }
    return { ...item };
  });
}

/* ////////////////////////////////// */
/* reset filter options following override - specific cases only */
/* ////////////////////////////////// */
export function resetfilterOptionsOnOverride(activeFilters, newFilters) {
  // if options need changing (delete above)
  // compare new to active filters and change filterOptions, exclusions etc
  // return new filterOptions
  // console.log('reset filter options', activeFilters, newFilters);
  const adjustedFilters = {};
  // console.log('current active filters >>>>>>>>>>', activeFilters);
  _.forEach(activeFilters, (filter, key) => {
    const newOptions = newFilters[filter.distinctKey];
    // console.log('filter options reset', filter, newOptions);
    if (filter && filter.filterOptions && filter.filterOptions.length) {
      const filterOptions = filter.filterOptions.filter(item => _.includes(newOptions, item.Value));
      adjustedFilters[key] = Object.assign({}, filter, { filterOptions });
    }
  });
  return adjustedFilters;
}

/* ////////////////////////////////// */
/* REQUEST POST OVERRIDE STATUS */
/* ////////////////////////////////// */
export function* requestOverrideStatus({ payload: params }) {
  const { overrideStatus } = api.post;

  try {
    yield put({
      type: ACTIONS.SET_OVERLAY_LOADING,
      overlay: {
        id: 'postOverrideStatus',
        loading: true },
      });
      // change All for BE to NULL
    const adjustParams = (params.ReturnStatusFilter === 'All') ? Object.assign(params, { ReturnStatusFilter: null }) : params;
    const response = yield overrideStatus(adjustParams);
    const { status, data } = response;
    const hasActiveScrubbingFilters = yield select(state => state.post.hasActiveScrubbingFilters);
    yield put({
      type: ACTIONS.SET_OVERLAY_LOADING,
      overlay: {
        id: 'postOverrideStatus',
        loading: false,
      },
    });
    if (status !== 200) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'No post override status returned.',
          message: `The server encountered an error processing the request (post override status). Please try again or contact your administrator to review error logs. (HTTP Status: ${status})`,
        },
      });
      throw new Error();
    }
    if (!data.Success) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'No post override status returned.',
          message: data.Message || 'The server encountered an error processing the request (post override status). Please try again or contact your administrator to review error logs.',
        },
      });
      throw new Error();
    }
    // if no scrubbing filters - process as receive; else handle filters
    if (hasActiveScrubbingFilters) {
      const scrubs = yield select(state => state.post.proposalHeader.activeScrubbingData.ClientScrubs);
      const status = params.OverrideStatus === 'InSpec' ? 2 : 1;
      const isRemove = (params.ReturnStatusFilter === 'All') ? false : (params.ReturnStatusFilter !== params.OverrideStatus);
      const adjustedScrubbing = refilterOnOverride(scrubs, params.ScrubIds, status, isRemove);
      const activeFilters = _.cloneDeep(yield select(state => state.post.activeScrubbingFilters));
      let adjustedFilters = null;
      // remove so redjust filter options as needed
      if (isRemove) {
        // const activeFilters = _.cloneDeep(yield select(state => state.post.activeScrubbingFilters));
        adjustedFilters = resetfilterOptionsOnOverride(activeFilters, data.Data.Filters);
        // console.log('adjusted filters', adjustedFilters);
      }
      const ret = { filteredClientScrubs: adjustedScrubbing, scrubbingData: data.Data, activeFilters: isRemove ? adjustedFilters : activeFilters };
      // console.log('remove test', isRemove, ret);
      // clear the data so grid registers as update
      yield call(requestClearScrubbingDataFiltersList);
      yield put({
        type: ACTIONS.RECEIVE_POST_OVERRIDE_STATUS,
        data: ret,
      });
    } else {
      // clear the data so grid registers as update
      yield call(requestClearScrubbingDataFiltersList);
      yield put({
        type: ACTIONS.RECEIVE_POST_CLIENT_SCRUBBING,
        data,
      });
    }
  } catch (e) {
    if (e.response) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: 'No post override status returned.',
          message: 'The server encountered an error processing the request (post override status). Please try again or contact your administrator to review error logs.',
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

export function* loadArchivedIsci() {
  const { getArchivedIscis } = api.post;
  try {
    yield put(setOverlayLoading({ id: 'loadArchiveIsci', loading: true }));
    return yield getArchivedIscis();
  } finally {
    yield put(setOverlayLoading({ id: 'loadArchiveIsci', loading: false }));
  }
}

export function* rescrubUnlinkedIsci({ ids }) {
  yield console.log(ids);
  return { status: 200, data: { Success: true } };
}

export function* closeUnlinkedIsciModal({ modalPrams }) {
  yield put(toggleModal({
    modal: 'postUnlinkedIsciModal',
    active: false,
    properties: modalPrams,
  }));
  yield put(getPost());
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
  yield takeEvery([
    ACTIONS.UNLINKED_ISCIS_DATA.request,
    ACTIONS.ARCHIVE_UNLIKED_ISCI.success,
    ACTIONS.RESCRUB_UNLIKED_ISCI.success,
  ],
    sagaWrapper(requestUnlinkedIscis, ACTIONS.UNLINKED_ISCIS_DATA),
  );
}

export function* watchRequestUniqueIscisSuccess() {
  yield takeEvery(ACTIONS.UNLINKED_ISCIS_DATA.success, unlinkedIscisSuccess);
}

export function* watchArchiveUnlinkedIsci() {
  yield takeEvery(ACTIONS.ARCHIVE_UNLIKED_ISCI.request, sagaWrapper(archiveUnlinkedIsci, ACTIONS.ARCHIVE_UNLIKED_ISCI));
}

export function* watchRequestOverrideStatus() {
  yield takeEvery(ACTIONS.REQUEST_POST_OVERRIDE_STATUS, requestOverrideStatus);
}
export function* watchLoadArchivedIscis() {
  yield takeEvery(ACTIONS.LOAD_ARCHIVED_ISCI.request, sagaWrapper(loadArchivedIsci, ACTIONS.LOAD_ARCHIVED_ISCI));
}

export function* watchRescrubUnlinkedIsci() {
  yield takeEvery(ACTIONS.RESCRUB_UNLIKED_ISCI.request, sagaWrapper(rescrubUnlinkedIsci, ACTIONS.RESCRUB_UNLIKED_ISCI));
}

export function* watchCloseUnlinkedIsciModal() {
  yield takeEvery(ACTIONS.CLOSE_UNLINKED_ISCI_MODAL, closeUnlinkedIsciModal);
}
