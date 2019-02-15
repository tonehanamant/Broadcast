import { takeEvery, put, call, select } from "redux-saga/effects";
import FuzzySearch from "fuzzy-search";
import moment from "moment";
import { forEach, cloneDeep, includes, update } from "lodash";
import { types as appActions } from "Main";
import {
  setOverlayLoading,
  setOverlayProccesing,
  toggleModal,
  deployError
} from "Main/redux/actions";
import { selectModal } from "Main/redux/selectors";
import sagaWrapper from "Utils/saga-wrapper";
import {
  selectActiveScrubs,
  selectUnfilteredData,
  selectFilteredIscis,
  selectActiveScrubbingFilters,
  selectClientScrubs,
  selectActiveFilterKey
} from "Post/redux/selectors";
import {
  getPost,
  saveActiveScrubData,
  savePostDisplay
} from "Post/redux/actions";
import api from "API";

import * as postActions from "./types";

const ACTIONS = { ...appActions, ...postActions };

/* ////////////////////////////////// */
/* Adjust POST Data return */
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

export const assignDisplay = posts =>
  posts.map(post => ({
    ...post,
    DisplayUploadDate:
      post.UploadDate !== null
        ? moment(post.UploadDate).format("M/D/YYYY")
        : "-"
  }));

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

/* ////////////////////////////////// */
/* ASSIGN POST DISPLAY */
/* ////////////////////////////////// */
export function* assignPostDisplay({ payload: request }) {
  try {
    yield put(setOverlayLoading({ id: "postPostsDisplay", loading: true }));
    const Posts = yield assignDisplay(request.data.Posts);
    const post = Object.assign({}, request.data, { Posts });
    yield put(savePostDisplay(post));
  } catch (e) {
    if (e.message) {
      yield put(deployError({ message: e.message }));
    }
  } finally {
    yield put(setOverlayLoading({ id: "postPostsDisplay", loading: false }));
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

const searcher = (data, searchKeys, query) => {
  const searcher = new FuzzySearch(data, searchKeys, {
    caseSensitive: false
  });
  return searcher.search(query);
};

export function* requestPostFiltered({ payload: query }) {
  const data = yield select(selectUnfilteredData);
  try {
    const filtered = yield searcher(data, postSearchKeys, query);
    yield put({
      type: ACTIONS.RECEIVE_FILTERED_POST,
      data: filtered
    });
  } catch (e) {
    if (e.message) {
      yield put(deployError({ message: e.message }));
    }
  }
}

const iscisSearchKeys = ["ISCI"];

export function* requestUnlinkedFiltered({ payload: query }) {
  const data = yield select(selectFilteredIscis);
  // for each post, convert all properties to string to enable use on FuzzySearch object
  data.map(post => Object.keys(post).map(key => post[key]));

  try {
    const filtered = yield searcher(data, iscisSearchKeys, query);
    yield put({
      type: ACTIONS.RECEIVE_FILTERED_UNLINKED,
      data: { query, filteredData: filtered }
    });
  } catch (e) {
    if (e.message) {
      yield put(deployError({ message: e.message }));
    }
  }
}

export function* requestArchivedFiltered({ payload: query }) {
  const data = yield select(selectFilteredIscis);
  // for each post, convert all properties to string to enable use on FuzzySearch object
  data.map(post => Object.keys(post).map(key => post[key]));

  if (data.length > 0) {
    try {
      const filtered = yield searcher(data, iscisSearchKeys, query);
      yield put({
        type: ACTIONS.RECEIVE_FILTERED_ARCHIVED,
        data: { query, filteredData: filtered }
      });
    } catch (e) {
      if (e.message) {
        yield put(deployError({ message: e.message }));
      }
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
      data: []
    });
  } catch (e) {
    if (e.message) {
      yield put(deployError({ message: e.message }));
    }
  }
}

/* ////////////////////////////////// */
/* CLEAR FILTERED SCRUBBING DATA - resets filters */
/* ////////////////////////////////// */
const delay = ms => new Promise(res => setTimeout(res, ms));

export function* clearFilteredScrubbingData() {
  yield call(requestClearScrubbingDataFiltersList);
  yield delay(500);
  const originalFilters = yield select(selectActiveScrubbingFilters);
  const originalScrubs = yield select(selectClientScrubs);
  const activeFilters = forEach(originalFilters, filter => {
    if (filter.active) {
      const isList = filter.type === "filterList";
      filter.active = false;
      if (isList) {
        filter.exclusions = [];
        if (filter.hasMatchSpec) {
          filter.activeMatch = false;
          filter.matchOptions.outOfSpec = true;
          filter.matchOptions.inSpec = true;
        }
        filter.filterOptions.forEach(option => {
          option.Selected = true;
        });
      } else {
        filter.exclusions = false;
        if (filter.type === "timeInput") {
          filter.filterOptions.TimeAiredStart =
            filter.filterOptions.originalTimeAiredStart;
          filter.filterOptions.TimeAiredEnd =
            filter.filterOptions.originalTimeAiredEnd;
        }
        if (filter.type === "dateInput") {
          filter.filterOptions.DateAiredStart =
            filter.filterOptions.originalDateAiredStart;
          filter.filterOptions.DateAiredEnd =
            filter.filterOptions.originalDateAiredEnd;
        }
      }
    }
  });
  const ret = {
    activeFilters,
    originalScrubs
  };
  try {
    yield put(setOverlayLoading({ id: "PostScrubbingFilter", loading: true }));
    yield put({
      type: ACTIONS.RECEIVE_CLEAR_FILTERED_SCRUBBING_DATA,
      data: ret
    });
  } finally {
    yield put(setOverlayLoading({ id: "PostScrubbingFilter", loading: false }));
  }
}

/* ////////////////////////////////// */
/* REQUEST POST CLIENT SCRUBBING */
/* ////////////////////////////////// */
// allow for params (todo from BE) to filterKey All, InSpec, OutOfSpec; optional showModal (from Post landing);
// if not from modal show processing, else show loading (loading not shown inside modal)
export function* requestPostClientScrubbing(params) {
  const { getPostClientScrubbing } = api.post;
  try {
    if (params.showModal) {
      yield put(setOverlayLoading("PostClientScrubbing", true));
    } else {
      yield put(setOverlayProccesing("PostClientScrubbing", true));
    }
    // clear the data so filters grid registers as update - if not from modal update
    if (!params.showModal) {
      yield call(requestClearScrubbingDataFiltersList);
    }
    const { status, data } = yield getPostClientScrubbing(params);
    if (params.filterKey) {
      update(data, "Data.filterKey", () => params.filterKey);
    }
    return { status, data };
  } finally {
    if (params.showModal) {
      yield put(setOverlayLoading("PostClientScrubbing", false));
    } else {
      yield put(setOverlayProccesing("PostClientScrubbing", false));
    }
  }
}

export function* requestPostClientScrubbingSuccess({ payload: params }) {
  if (params.showModal) {
    yield put(
      toggleModal({
        modal: "postScrubbingModal",
        active: true,
        properties: {
          titleText: "POST SCRUBBING MODAL",
          bodyText: "Post Scrubbing details will be shown here!"
        }
      })
    );
  }
}

// FILTERING
// tbd how to iterate multiple versus single and determine set to check active or original
// todo break down original scrubbing to ClientScrubs etc
export function* requestScrubbingDataFiltered({ payload: query }) {
  const listUnfiltered = yield select(
    state => state.post.proposalHeader.scrubbingData.ClientScrubs
  );
  const listFiltered = yield select(
    state => state.post.proposalHeader.activeScrubbingData.ClientScrubs
  );
  const activeFilters = cloneDeep(
    yield select(state => state.post.activeScrubbingFilters)
  );
  const originalFilters = yield select(
    state => state.post.activeScrubbingFilters
  );
  const actingFilter = activeFilters[query.filterKey]; // this is undefined
  // console.log('request scrub filter', query, activeFilters, actingFilter);
  const applyFilter = () => {
    const isList = actingFilter.type === "filterList";
    // active -depends on if clearing etc; also now if matching in play
    let isActive = false;
    let hasActiveScrubbingFilters = false;
    if (isList) {
      isActive = query.exclusions.length > 0 || query.activeMatch;
      actingFilter.matchOptions = query.matchOptions;
      actingFilter.activeMatch = query.activeMatch;
    } else {
      isActive = query.exclusions; // bool for date/time aired
    }
    actingFilter.active = isActive;
    actingFilter.exclusions = query.exclusions;
    // leave originals in place if not list
    actingFilter.filterOptions = isList
      ? query.filterOptions
      : Object.assign(actingFilter.filterOptions, query.filterOptions);
    // TBD date/time aired versus list
    const filteredResult = listUnfiltered.filter(item => {
      let ret = true;
      forEach(activeFilters, value => {
        if (value.active && ret === true) {
          hasActiveScrubbingFilters = true;
          if (value.type === "filterList") {
            if (value.activeMatch) {
              // just base on one or the other?
              const toMatch = value.matchOptions.inSpec === true;
              ret =
                !includes(value.exclusions, item[value.filterKey]) &&
                item[value.matchOptions.matchKey] === toMatch;
              // console.log('filter each', ret, item[value.filterKey]);
            } else {
              ret = !includes(value.exclusions, item[value.filterKey]);
            }
          } else if (value.type === "dateInput") {
            // tbd check range based on value.filterOptions
            // todo: need to check if the 2 values are equal
            ret = moment(item[value.filterKey]).isBetween(
              value.filterOptions.DateAiredStart,
              value.filterOptions.DateAiredEnd,
              "day",
              true
            );
          } else if (value.type === "timeInput") {
            // tbd check range based on value.filterOptions
            // todo: need to check if the 2 values are equal
            ret = moment(item[value.filterKey]).isBetween(
              value.filterOptions.TimeAiredStart,
              value.filterOptions.TimeAiredEnd,
              "seconds",
              true
            );
          }
        }
      });
      return ret;
    });
    // console.log('request apply filter', actingFilter, activeFilters);
    // test to make sure there is returned data
    if (filteredResult.length < 1) {
      return {
        filteredClientScrubs: listFiltered,
        actingFilter,
        activeFilters: originalFilters,
        alertEmpty: true,
        hasActiveScrubbingFilters
      };
    }
    return {
      filteredClientScrubs: filteredResult,
      actingFilter,
      activeFilters,
      alertEmpty: false,
      hasActiveScrubbingFilters
    };
  };

  try {
    // show processing?
    yield put({
      type: ACTIONS.SET_OVERLAY_PROCESSING,
      overlay: {
        id: "PostScrubbingFilter",
        processing: true
      }
    });
    // clear the data so grid registers as update
    yield call(requestClearScrubbingDataFiltersList);
    const filtered = yield applyFilter();

    yield put({
      type: ACTIONS.SET_OVERLAY_PROCESSING,
      overlay: {
        id: "PostScrubbingFilter",
        processing: false
      }
    });
    // if empty show alert - will set to original state
    if (filtered.alertEmpty) {
      const msg = `${
        filtered.actingFilter.filterDisplay
      } Filter will remove all data.`;
      yield put({
        type: ACTIONS.CREATE_ALERT,
        alert: {
          type: "warning",
          headline: "Filter Not Applied",
          message: msg
        }
      });
    }

    yield put({
      type: ACTIONS.RECEIVE_FILTERED_SCRUBBING_DATA,
      data: filtered
    });
  } catch (e) {
    if (e.message) {
      // todo should reset activeFilters (cleared) if error?
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
/* REQUEST POST SCRUBBING HEADER */
/* ////////////////////////////////// */
export function* requestUnlinkedIscis() {
  const { getUnlinkedIscis } = api.post;
  try {
    yield put(setOverlayLoading({ id: "PostUniqueIscis", loading: true }));
    return yield getUnlinkedIscis();
  } finally {
    yield put(setOverlayLoading({ id: "PostUniqueIscis", loading: false }));
  }
}

export function* unlinkedIscisSuccess() {
  const activeQuery = yield select(state => state.post.activeIsciFilterQuery);
  // console.log('unlinked isci active query success>>>>>>>', activeQuery);
  const modal = select(selectModal, "postUnlinkedIsciModal");
  if (modal && !modal.active) {
    yield put(
      toggleModal({
        modal: "postUnlinkedIsciModal",
        active: true,
        properties: {
          titleText: "POST Unique Iscis",
          bodyText: "Isci Details"
        }
      })
    );
  }
  if (activeQuery.length) {
    yield call(requestUnlinkedFiltered, { payload: activeQuery });
  }
}

export function* archivedIscisSuccess() {
  const activeQuery = yield select(state => state.post.activeIsciFilterQuery);
  if (activeQuery.length) {
    yield call(requestArchivedFiltered, { payload: activeQuery });
  }
}

export function* archiveUnlinkedIsci({ ids }) {
  const { archiveUnlinkedIscis } = api.post;
  try {
    yield put(setOverlayLoading({ id: "postArchiveIsci", loading: true }));
    return yield archiveUnlinkedIscis(ids);
  } finally {
    yield put(setOverlayLoading({ id: "postArchiveIsci", loading: false }));
  }
}

export function* undoArchivedIscis({ ids }) {
  const { undoArchivedIscis } = api.post;
  try {
    yield put(setOverlayLoading({ id: "postArchiveIsci", loading: true }));
    return yield undoArchivedIscis(ids);
  } finally {
    yield put(setOverlayLoading({ id: "postArchiveIsci", loading: false }));
  }
}

/* ////////////////////////////////// */
/* refilter scrubs following override */
/* ////////////////////////////////// */
export function refilterOnOverride(clientScrubs, keys, status, isRemove) {
  // if is Remove filter out the keys
  // else for each key find scrub and change Status/overide true
  if (isRemove) {
    return clientScrubs.filter(item => !includes(keys, item.ScrubbingClientId));
  }

  return clientScrubs.map(item => {
    if (includes(keys, item.ScrubbingClientId)) {
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
  // compare new to active filters and change filterOptions, exclusions etc; handle date/time separately
  // return new filterOptions
  const adjustedFilters = {};
  forEach(activeFilters, (filter, key) => {
    if (filter && filter.filterOptions) {
      if (filter.type === "filterList") {
        const newOptions = newFilters[filter.distinctKey];
        // console.log('filter options reset', filter, newOptions);
        if (filter.filterOptions.length) {
          const filterOptions = filter.filterOptions.filter(item =>
            includes(newOptions, item.Value)
          );
          adjustedFilters[key] = Object.assign({}, filter, { filterOptions });
        }
      } else if (filter.type === "dateInput") {
        // change originals - modifying active could beak what user has changed
        const filterOptions = {
          DateAiredStart: filter.filterOptions.DateAiredStart,
          DateAiredEnd: filter.filterOptions.DateAiredEnd,
          originalDateAiredStart: newFilters.DateAiredStart,
          originalDateAiredEnd: newFilters.DateAiredEnd
        };
        adjustedFilters[key] = Object.assign({}, filter, { filterOptions });
      } else if (filter.type === "timeInput") {
        // change originals - modifying active could beak what user has changed
        const filterOptions = {
          TimeAiredStart: filter.filterOptions.TimeAiredStart,
          TimeAiredEnd: filter.filterOptions.TimeAiredEnd,
          originalTimeAiredStart: newFilters.originalTimeAiredStart,
          originalTimeAiredEnd: newFilters.originalTimeAiredEnd
        };
        adjustedFilters[key] = Object.assign({}, filter, { filterOptions });
      }
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
        id: "postOverrideStatus",
        loading: true
      }
    });
    // change All for BE to NULL; fix so does not override initial params ReturnStatusFilter
    const adjustParams =
      params.ReturnStatusFilter === "All"
        ? Object.assign({}, params, { ReturnStatusFilter: null })
        : params;
    const response = yield overrideStatus(adjustParams);
    const { status, data } = response;
    const hasActiveScrubbingFilters = yield select(
      state => state.post.hasActiveScrubbingFilters
    );
    yield put({
      type: ACTIONS.SET_OVERLAY_LOADING,
      overlay: {
        id: "postOverrideStatus",
        loading: false
      }
    });
    if (status !== 200) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "No post override status returned.",
          message: `The server encountered an error processing the request (post override status). Please try again or contact your administrator to review error logs. (HTTP Status: ${status})`
        }
      });
      throw new Error();
    }
    if (!data.Success) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "No post override status returned.",
          message:
            data.Message ||
            "The server encountered an error processing the request (post override status). Please try again or contact your administrator to review error logs."
        }
      });
      throw new Error();
    }
    // if no scrubbing filters - process as receive; else handle filters
    if (hasActiveScrubbingFilters) {
      const scrubs = yield select(
        state => state.post.proposalHeader.activeScrubbingData.ClientScrubs
      );
      const status = params.OverrideStatus === "InSpec" ? 2 : 1;
      const isRemove =
        params.ReturnStatusFilter === "All"
          ? false
          : params.ReturnStatusFilter !== params.OverrideStatus;
      // console.log('refilter needed as>>>>>>', params.ReturnStatusFilter);
      const adjustedScrubbing = refilterOnOverride(
        scrubs,
        params.ScrubIds,
        status,
        isRemove
      );
      const activeFilters = cloneDeep(
        yield select(state => state.post.activeScrubbingFilters)
      );
      let adjustedFilters = null;
      // remove so redjust filter options as needed
      if (isRemove) {
        // const activeFilters = cloneDeep(yield select(state => state.post.activeScrubbingFilters));
        adjustedFilters = resetfilterOptionsOnOverride(
          activeFilters,
          data.Data.Filters
        );
        // console.log('adjusted filters', adjustedFilters);
      }
      const ret = {
        filteredClientScrubs: adjustedScrubbing,
        scrubbingData: data.Data,
        activeFilters: isRemove ? adjustedFilters : activeFilters
      };
      // console.log('remove test', isRemove, ret);
      // clear the data so grid registers as update
      yield call(requestClearScrubbingDataFiltersList);
      yield put({
        type: ACTIONS.RECEIVE_POST_OVERRIDE_STATUS,
        data: ret
      });
    } else {
      // clear the data so grid registers as update
      yield call(requestClearScrubbingDataFiltersList);
      // as currently stands need to reset the filter key on data or is removed : TODO REVISE
      data.Data.filterKey = yield select(state => state.post.activeFilterKey);
      yield put({
        type: ACTIONS.LOAD_POST_CLIENT_SCRUBBING.success,
        data
      });
    }
  } catch (e) {
    if (e.response) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "No post override status returned.",
          message:
            "The server encountered an error processing the request (post override status). Please try again or contact your administrator to review error logs.",
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

export function* swapProposalDetail({ payload: params }) {
  const { swapProposalDetail } = api.post;

  try {
    yield put({
      type: ACTIONS.SET_OVERLAY_LOADING,
      overlay: {
        id: "swapDetail",
        loading: true
      }
    });
    const response = yield swapProposalDetail(params);
    const { status, data } = response;
    yield put({
      type: ACTIONS.SET_OVERLAY_LOADING,
      overlay: {
        id: "swapDetail",
        loading: false
      }
    });
    if (status !== 200) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "No swap proposal detail returned.",
          message: `The server encountered an error processing the request (swap proposal detail). Please try again or contact your administrator to review error logs. (HTTP Status: ${status})`
        }
      });
      throw new Error();
    }
    if (!data.Success) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "No swap proposal detail returned.",
          message:
            data.Message ||
            "The server encountered an error processing the request (swap proposal detail). Please try again or contact your administrator to review error logs."
        }
      });
      throw new Error();
    }
    yield put({
      type: ACTIONS.CREATE_ALERT,
      alert: {
        type: "success",
        headline: "Swap Proposal Detail",
        message: "Records updated successfully"
      }
    });
    yield put(
      toggleModal({
        modal: "swapDetailModal",
        active: false,
        properties: {}
      })
    );
    // refresh scrubbing
    const id = yield select(
      state => state.post.proposalHeader.activeScrubbingData.Id
    );
    const refreshParams = { proposalId: id, showModal: true, filterKey: "All" };
    yield call(requestPostClientScrubbing, { payload: refreshParams });
  } catch (e) {
    if (e.response) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "No swap proposal detail returned.",
          message:
            "The server encountered an error processing the request (swap proposal detail). Please try again or contact your administrator to review error logs.",
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

export function* loadArchivedIsci() {
  const { getArchivedIscis } = api.post;
  try {
    yield put(setOverlayLoading({ id: "loadArchiveIsci", loading: true }));
    return yield getArchivedIscis();
  } finally {
    yield put(setOverlayLoading({ id: "loadArchiveIsci", loading: false }));
  }
}

export function* loadValidIscis({ query }) {
  const { getValidIscis } = api.post;
  return yield getValidIscis(query);
}

export function* rescrubUnlinkedIsci({ isci }) {
  const { rescrubUnlinkedIscis } = api.post;
  try {
    yield put(setOverlayLoading({ id: "rescrubIsci", loading: true }));
    return yield rescrubUnlinkedIscis(isci);
  } finally {
    yield put(setOverlayLoading({ id: "rescrubIsci", loading: false }));
  }
}

export function* mapUnlinkedIsci(payload) {
  const { mapUnlinkedIscis } = api.post;
  try {
    yield put(setOverlayLoading({ id: "mapUnlinkedIsci", loading: true }));
    return yield mapUnlinkedIscis(payload);
  } finally {
    yield put(setOverlayLoading({ id: "mapUnlinkedIsci", loading: false }));
  }
}

export function* mapUnlinkedIsciSuccess() {
  yield put(
    toggleModal({
      modal: "mapUnlinkedIsci",
      active: false,
      properties: {}
    })
  );
}

export function* closeUnlinkedIsciModal({ modalPrams }) {
  yield put({
    type: ACTIONS.RECEIVE_CLEAR_ISCI_FILTER
  });
  yield put(
    toggleModal({
      modal: "postUnlinkedIsciModal",
      active: false,
      properties: modalPrams
    })
  );
  yield put(getPost());
}

const filterMap = {
  InSpec: 2,
  OutOfSpec: 1
};

export function* undoScrubStatus(payload) {
  const { undoScrubStatus } = api.post;
  const activeFilterKey = yield select(selectActiveFilterKey);
  let params = payload;
  if (activeFilterKey !== "All") {
    params = { ...payload, ReturnStatusFilter: filterMap[activeFilterKey] };
  }
  try {
    yield put(setOverlayLoading({ id: "undoScrubStatus", loading: true }));
    return yield undoScrubStatus(params);
  } finally {
    yield put(setOverlayLoading({ id: "undoScrubStatus", loading: false }));
  }
}

export function* undoScrubStatusSuccess({
  data: { Data },
  payload: { ScrubIds }
}) {
  const { ClientScrubs } = Data;
  const activeScrubData = yield select(selectActiveScrubs);
  const updatedScrubs = ScrubIds.map(id =>
    ClientScrubs.find(({ ScrubbingClientId }) => ScrubbingClientId === id)
  ).filter(it => it);
  const newClientScrubs = activeScrubData.ClientScrubs.filter(it =>
    ClientScrubs.find(
      originalIt => it.ScrubbingClientId === originalIt.ScrubbingClientId
    )
  ).map(it => {
    if (ScrubIds.includes(it.ScrubbingClientId)) {
      const newItem = updatedScrubs.find(
        ({ ScrubbingClientId }) => ScrubbingClientId === it.ScrubbingClientId
      );
      return { ...it, ...newItem };
    }
    return it;
  });
  yield put(
    saveActiveScrubData(
      { ...activeScrubData, ClientScrubs: newClientScrubs },
      Data
    )
  );
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
  const list =
    Array.isArray(req.data.Data) || req.data.Data.length ? req.data.Data : "";
  const scrollable =
    Array.isArray(req.data.Data) || req.data.Data.length
      ? "modalBodyScroll"
      : null;
  yield put({
    type: ACTIONS.TOGGLE_MODAL,
    modal: {
      modal: "confirmModal",
      active: true,
      properties: {
        bodyClass: scrollable,
        titleText: "Upload Complete",
        bodyText: req.data.Message,
        bodyList: list,
        closeButtonDisabled: true,
        actionButtonText: "OK",
        actionButtonBsStyle: "success",
        action: () => {},
        dismiss: () => {}
      }
    }
  });
}

/* ////////////////////////////////// */
/* WATCHERS */
/* ////////////////////////////////// */

function* watchRequestPost() {
  yield takeEvery(
    ACTIONS.LOAD_POST.request,
    sagaWrapper(requestPost, ACTIONS.LOAD_POST)
  );
}

function* watchRequestAssignPostDisplay() {
  yield takeEvery(ACTIONS.REQUEST_ASSIGN_POST_DISPLAY, savePostDisplay);
}

function* watchRequestPostFiltered() {
  yield takeEvery(ACTIONS.REQUEST_FILTERED_POST, requestPostFiltered);
}

function* watchRequestUnlinkedFiltered() {
  yield takeEvery(ACTIONS.REQUEST_FILTERED_UNLINKED, requestUnlinkedFiltered);
}

function* watchRequestArchivedFiltered() {
  yield takeEvery(ACTIONS.REQUEST_FILTERED_ARCHIVED, requestArchivedFiltered);
}

function* watchRequestPostClientScrubbing() {
  yield takeEvery(
    ACTIONS.LOAD_POST_CLIENT_SCRUBBING.request,
    sagaWrapper(requestPostClientScrubbing, ACTIONS.LOAD_POST_CLIENT_SCRUBBING)
  );
}

function* watchRequestPostClientScrubbingSuccess() {
  yield takeEvery(
    ACTIONS.LOAD_POST_CLIENT_SCRUBBING.success,
    requestPostClientScrubbingSuccess
  );
}

function* watchRequestScrubbingDataFiltered() {
  yield takeEvery(
    ACTIONS.REQUEST_FILTERED_SCRUBBING_DATA,
    requestScrubbingDataFiltered
  );
}

function* watchRequestClearScrubbingFiltersList() {
  yield takeEvery(
    ACTIONS.REQUEST_CLEAR_SCRUBBING_FILTERS_LIST,
    requestClearScrubbingDataFiltersList
  );
}

function* watchRequestUniqueIscis() {
  yield takeEvery(
    [
      ACTIONS.UNLINKED_ISCIS_DATA.request,
      ACTIONS.ARCHIVE_UNLIKED_ISCI.success,
      ACTIONS.RESCRUB_UNLIKED_ISCI.success,
      ACTIONS.MAP_UNLINKED_ISCI.success
    ],
    sagaWrapper(requestUnlinkedIscis, ACTIONS.UNLINKED_ISCIS_DATA)
  );
}

function* watchRequestUniqueIscisSuccess() {
  yield takeEvery(ACTIONS.UNLINKED_ISCIS_DATA.success, unlinkedIscisSuccess);
}

function* watchRequestArchivedIscisSuccess() {
  yield takeEvery(ACTIONS.LOAD_ARCHIVED_ISCI.success, archivedIscisSuccess);
}

function* watchArchiveUnlinkedIsci() {
  yield takeEvery(
    ACTIONS.ARCHIVE_UNLIKED_ISCI.request,
    sagaWrapper(archiveUnlinkedIsci, ACTIONS.ARCHIVE_UNLIKED_ISCI)
  );
}

function* watchRequestOverrideStatus() {
  yield takeEvery(ACTIONS.REQUEST_POST_OVERRIDE_STATUS, requestOverrideStatus);
}

function* watchSwapProposalDetail() {
  yield takeEvery(ACTIONS.REQUEST_SWAP_PROPOSAL_DETAIL, swapProposalDetail);
}

function* watchLoadArchivedIscis() {
  yield takeEvery(
    [ACTIONS.LOAD_ARCHIVED_ISCI.request, ACTIONS.UNDO_ARCHIVED_ISCI.success],
    sagaWrapper(loadArchivedIsci, ACTIONS.LOAD_ARCHIVED_ISCI)
  );
}

function* watchLoadValidIscis() {
  yield takeEvery(
    ACTIONS.LOAD_VALID_ISCI.request,
    sagaWrapper(loadValidIscis, ACTIONS.LOAD_VALID_ISCI)
  );
}

function* watchRescrubUnlinkedIsci() {
  yield takeEvery(
    ACTIONS.RESCRUB_UNLIKED_ISCI.request,
    sagaWrapper(rescrubUnlinkedIsci, ACTIONS.RESCRUB_UNLIKED_ISCI)
  );
}

function* watchMapUnlinkedIsci() {
  yield takeEvery(
    ACTIONS.MAP_UNLINKED_ISCI.request,
    sagaWrapper(mapUnlinkedIsci, ACTIONS.MAP_UNLINKED_ISCI)
  );
}

function* watchCloseUnlinkedIsciModal() {
  yield takeEvery(ACTIONS.CLOSE_UNLINKED_ISCI_MODAL, closeUnlinkedIsciModal);
}

function* watchMapUnlinkedIsciSuccess() {
  yield takeEvery(ACTIONS.MAP_UNLINKED_ISCI.success, mapUnlinkedIsciSuccess);
}

function* watchUndoArchivedIscis() {
  yield takeEvery(
    ACTIONS.UNDO_ARCHIVED_ISCI.request,
    sagaWrapper(undoArchivedIscis, ACTIONS.UNDO_ARCHIVED_ISCI)
  );
}

function* watchUndoScrubStatus() {
  yield takeEvery(
    ACTIONS.UNDO_SCRUB_STATUS.request,
    sagaWrapper(undoScrubStatus, ACTIONS.UNDO_SCRUB_STATUS)
  );
}

function* watchUndoScrubStatusSuccess() {
  yield takeEvery(ACTIONS.UNDO_SCRUB_STATUS.success, undoScrubStatusSuccess);
}

function* watchRequestClearFilteredScrubbingData() {
  yield takeEvery(
    ACTIONS.REQUEST_CLEAR_FILTERED_SCRUBBING_DATA,
    clearFilteredScrubbingData
  );
}

function* watchRequestProcessNtiFile() {
  yield takeEvery(
    ACTIONS.PROCESS_NTI_FILE.request,
    sagaWrapper(requestProcessNtiFile, ACTIONS.PROCESS_NTI_FILE)
  );
}

function* watchProcessNtiFileSuccess() {
  yield takeEvery(ACTIONS.PROCESS_NTI_FILE.success, processNtiFileSuccess);
}

export default [
  watchProcessNtiFileSuccess,
  watchRequestProcessNtiFile,
  watchRequestClearFilteredScrubbingData,
  watchUndoScrubStatusSuccess,
  watchUndoScrubStatus,
  watchUndoArchivedIscis,
  watchMapUnlinkedIsciSuccess,
  watchCloseUnlinkedIsciModal,
  watchMapUnlinkedIsci,
  watchRescrubUnlinkedIsci,
  watchLoadValidIscis,
  watchLoadArchivedIscis,
  watchSwapProposalDetail,
  watchRequestOverrideStatus,
  watchArchiveUnlinkedIsci,
  watchRequestArchivedIscisSuccess,
  watchRequestUniqueIscisSuccess,
  watchRequestUniqueIscis,
  watchRequestClearScrubbingFiltersList,
  watchRequestScrubbingDataFiltered,
  watchRequestPostClientScrubbing,
  watchRequestPostClientScrubbingSuccess,
  watchRequestArchivedFiltered,
  watchRequestUnlinkedFiltered,
  watchRequestPostFiltered,
  watchRequestAssignPostDisplay,
  watchRequestPost
];
