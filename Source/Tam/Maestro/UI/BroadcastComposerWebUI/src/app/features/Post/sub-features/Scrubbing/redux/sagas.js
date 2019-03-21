import { takeEvery, put, call, select } from "redux-saga/effects";
import moment from "moment";
import { forEach, cloneDeep, includes, update, map } from "lodash";
import {
  setOverlayLoading,
  setOverlayProcessing,
  toggleModal,
  createAlert,
  deployError
} from "Main/redux/ducks";
import sagaWrapper from "Utils/saga-wrapper";
import api from "API";

import {
  UNDO_SCRUB_STATUS,
  LOAD_POST_CLIENT_SCRUBBING,
  SWAP_PROPOSAL_DETAIL,
  FILTERED_SCRUBBING_DATA,
  POST_OVERRIDE_STATUS,
  REQUEST_CLEAR_FILTERED_SCRUBBING_DATA,
  RECEIVE_CLEAR_FILTERED_SCRUBBING_DATA,
  REQUEST_CLEAR_SCRUBBING_FILTERS_LIST,
  RECEIVE_CLEAR_SCRUBBING_FILTERS_LIST,
  actions
} from "./ducks";

/* ////////////////////////////////// */
/* SELECTORS */
/* ////////////////////////////////// */
export const selectActiveScrubs = state =>
  state.post.scrubbing.proposalHeader.activeScrubbingData;
export const selectActiveFilterKey = state =>
  state.post.scrubbing.activeFilterKey;
export const selectActiveScrubbingFilters = state =>
  state.post.scrubbing.activeScrubbingFilters;
export const selectClientScrubs = state =>
  state.post.scrubbing.proposalHeader.scrubbingData.ClientScrubs;
export const selectActiveClientScrubs = state =>
  state.post.scrubbing.proposalHeader.activeScrubbingData.ClientScrubs;
export const selectActiveScrubId = state =>
  state.post.scrubbing.proposalHeader.activeScrubbingData.Id;
export const selectHasActiveScrubbingFilters = state =>
  state.post.scrubbing.hasActiveScrubbingFilters;

/* ////////////////////////////////// */
/* REQUEST CLEAR SCRUBBING FILTER LIST - so grid will update object data */
/* ////////////////////////////////// */
export function* requestClearScrubbingDataFiltersList() {
  try {
    yield put({
      type: RECEIVE_CLEAR_SCRUBBING_FILTERS_LIST,
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
  const activeFilters = map(originalFilters, filter => {
    const newFilter = filter;
    if (filter.active) {
      const isList = filter.type === "filterList";
      newFilter.active = false;
      if (isList) {
        newFilter.exclusions = [];
        if (filter.hasMatchSpec) {
          newFilter.activeMatch = false;
          newFilter.matchOptions.outOfSpec = true;
          newFilter.matchOptions.inSpec = true;
        }
        newFilter.filterOptions.map(option => ({
          ...option,
          Selected: true
        }));
      } else {
        newFilter.exclusions = false;
        if (filter.type === "timeInput") {
          newFilter.filterOptions.TimeAiredStart =
            filter.filterOptions.originalTimeAiredStart;
          newFilter.filterOptions.TimeAiredEnd =
            filter.filterOptions.originalTimeAiredEnd;
        }
        if (filter.type === "dateInput") {
          newFilter.filterOptions.DateAiredStart =
            filter.filterOptions.originalDateAiredStart;
          newFilter.filterOptions.DateAiredEnd =
            filter.filterOptions.originalDateAiredEnd;
        }
      }
    }
    return newFilter;
  });
  const ret = {
    activeFilters,
    originalScrubs
  };
  try {
    yield put(setOverlayLoading({ id: "PostScrubbingFilter", loading: true }));
    yield put({
      type: RECEIVE_CLEAR_FILTERED_SCRUBBING_DATA,
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
      yield put(
        setOverlayLoading({ id: "PostClientScrubbing", loading: true })
      );
    } else {
      yield put(
        setOverlayProcessing({ id: "PostClientScrubbing", processing: true })
      );
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
      yield put(
        setOverlayLoading({ id: "PostClientScrubbing", loading: false })
      );
    } else {
      yield put(
        setOverlayProcessing({ id: "PostClientScrubbing", processing: false })
      );
    }
  }
}

export function* requestPostClientScrubbingSuccess({ payload: params }) {
  if (params && params.showModal) {
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

const getFilteredResult = (listUnfiltered, filters) => {
  let hasActiveScrubbingFilters = false;
  const filteredResult = listUnfiltered.filter(item => {
    let ret = true;
    forEach(filters, value => {
      if (value.active && ret === true) {
        hasActiveScrubbingFilters = true;
        if (value.type === "filterList") {
          if (value.activeMatch) {
            // just base on one or the other?
            const toMatch = value.matchOptions.inSpec === true;
            ret =
              !includes(value.exclusions, item[value.filterKey]) &&
              item[value.matchOptions.matchKey] === toMatch;
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
  return { filteredResult, hasActiveScrubbingFilters };
};

const applyFilter = (filters, filter, query, listUnfiltered, listFiltered) => {
  const newFilter = cloneDeep(filter);
  const originalFilters = cloneDeep(filters);
  const isList = filter.type === "filterList";
  // active -depends on if clearing etc; also now if matching in play
  let isActive = false;
  if (isList) {
    isActive = query.exclusions.length > 0 || query.activeMatch;
    newFilter.matchOptions = query.matchOptions;
    newFilter.activeMatch = query.activeMatch;
  } else {
    isActive = query.exclusions; // bool for date/time aired
  }
  newFilter.active = isActive;
  newFilter.exclusions = query.exclusions;
  newFilter.filterOptions = isList
    ? query.filterOptions
    : Object.assign(filter.filterOptions, query.filterOptions);
  const { filteredResult, hasActiveScrubbingFilters } = getFilteredResult(
    listUnfiltered,
    filters
  );
  if (filteredResult.length < 1) {
    return {
      filteredClientScrubs: listFiltered,
      activeFilter: newFilter,
      activeFilters: originalFilters,
      alertEmpty: true,
      hasActiveScrubbingFilters
    };
  }
  return {
    filteredClientScrubs: filteredResult,
    activeFilter: newFilter,
    activeFilters: filters,
    alertEmpty: false,
    hasActiveScrubbingFilters
  };
};

// FILTERING
// tbd how to iterate multiple versus single and determine set to check active or original
// todo break down original scrubbing to ClientScrubs etc
export function* requestScrubbingDataFiltered({ payload: query }) {
  const listUnfiltered = yield select(selectClientScrubs);
  const listFiltered = yield select(selectActiveClientScrubs);
  const filters = yield select(selectActiveScrubbingFilters);
  const filter = filters[query.filterKey]; // this is undefined
  try {
    yield put(
      setOverlayProcessing({
        id: "PostScrubbingFilter",
        processing: true
      })
    );
    yield call(requestClearScrubbingDataFiltersList);
    const filtered = yield applyFilter(
      filters,
      filter,
      query,
      listUnfiltered,
      listFiltered
    );

    if (filtered.alertEmpty) {
      const msg = `${
        filtered.activeFilter.filterDisplay
      } Filter will remove all data.`;
      yield put(
        createAlert({
          type: "warning",
          headline: "Filter Not Applied",
          message: msg
        })
      );
    }

    yield put(actions.reveiveFilteredScrubbingData(filtered));
  } catch ({ message }) {
    if (message) {
      yield put(deployError({ message }));
    }
  } finally {
    yield put(
      setOverlayProcessing({
        id: "PostScrubbingFilter",
        processing: false
      })
    );
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
export function* requestOverrideStatus(params) {
  const { overrideStatus } = api.post;

  try {
    yield put(setOverlayLoading({ id: "postOverrideStatus", loading: true }));
    const adjustParams =
      params.ReturnStatusFilter === "All"
        ? Object.assign({}, params, { ReturnStatusFilter: null })
        : params;
    return yield overrideStatus(adjustParams);
  } finally {
    yield put(setOverlayLoading({ id: "postOverrideStatus", loading: false }));
  }
}

export function* requestOverrideStatusSuccess({ data, payload: params }) {
  const hasActiveFilters = yield select(selectHasActiveScrubbingFilters);
  if (hasActiveFilters) {
    const scrubs = yield select(selectActiveClientScrubs);
    const status = params.OverrideStatus === "InSpec" ? 2 : 1;
    const isRemove =
      params.ReturnStatusFilter === "All"
        ? false
        : params.ReturnStatusFilter !== params.OverrideStatus;
    const adjustedScrubbing = refilterOnOverride(
      scrubs,
      params.ScrubIds,
      status,
      isRemove
    );
    const activeFilters = yield select(selectActiveScrubbingFilters);
    let adjustedFilters = null;
    if (isRemove) {
      adjustedFilters = resetfilterOptionsOnOverride(
        activeFilters,
        data.Data.Filters
      );
    }
    const ret = {
      filteredClientScrubs: adjustedScrubbing,
      scrubbingData: data.Data,
      activeFilters: isRemove ? adjustedFilters : activeFilters
    };
    yield call(requestClearScrubbingDataFiltersList);
    yield put({
      type: POST_OVERRIDE_STATUS.store,
      data: ret
    });
  } else {
    yield call(requestClearScrubbingDataFiltersList);
    const filterKey = yield select(selectActiveFilterKey);
    yield put({
      type: LOAD_POST_CLIENT_SCRUBBING.success,
      data: { ...data, data: { ...data.Data, filterKey } }
    });
  }
}

export function* swapProposalDetail({ payload: params }) {
  const { swapProposalDetail } = api.post;
  try {
    yield put(setOverlayLoading({ id: "swapDetail", loading: true }));
    return yield swapProposalDetail(params);
  } finally {
    yield put(setOverlayLoading({ id: "swapDetail", loading: false }));
  }
}

export function* swapProposalDetailSuccess() {
  yield put(
    createAlert({
      type: "success",
      headline: "Swap Proposal Detail",
      message: "Records updated successfully"
    })
  );
  yield put(
    toggleModal({
      modal: "swapDetailModal",
      active: false,
      properties: {}
    })
  );
  // refresh scrubbing
  const id = yield select(selectActiveScrubId);
  const refreshParams = { proposalId: id, showModal: true, filterKey: "All" };
  yield call(requestPostClientScrubbing, { payload: refreshParams });
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
    actions.saveActiveScrubData(
      { ...activeScrubData, ClientScrubs: newClientScrubs },
      Data
    )
  );
}

/* ////////////////////////////////// */
/* WATCHERS */
/* ////////////////////////////////// */

function* watchRequestPostClientScrubbing() {
  yield takeEvery(
    LOAD_POST_CLIENT_SCRUBBING.request,
    sagaWrapper(requestPostClientScrubbing, LOAD_POST_CLIENT_SCRUBBING)
  );
}

function* watchRequestPostClientScrubbingSuccess() {
  yield takeEvery(
    LOAD_POST_CLIENT_SCRUBBING.success,
    requestPostClientScrubbingSuccess
  );
}

function* watchRequestScrubbingDataFiltered() {
  yield takeEvery(
    FILTERED_SCRUBBING_DATA.request,
    requestScrubbingDataFiltered
  );
}

function* watchRequestClearScrubbingFiltersList() {
  yield takeEvery(
    REQUEST_CLEAR_SCRUBBING_FILTERS_LIST,
    requestClearScrubbingDataFiltersList
  );
}

function* watchRequestOverrideStatus() {
  yield takeEvery(
    POST_OVERRIDE_STATUS.request,
    sagaWrapper(requestOverrideStatus, POST_OVERRIDE_STATUS)
  );
}

function* watchRequestOverrideStatusSuccess() {
  yield takeEvery(POST_OVERRIDE_STATUS.success, requestOverrideStatusSuccess);
}

function* watchSwapProposalDetailSuccess() {
  yield takeEvery(SWAP_PROPOSAL_DETAIL.success, swapProposalDetailSuccess);
}

function* watchSwapProposalDetail() {
  yield takeEvery(
    SWAP_PROPOSAL_DETAIL.request,
    sagaWrapper(swapProposalDetail, SWAP_PROPOSAL_DETAIL)
  );
}

function* watchUndoScrubStatus() {
  yield takeEvery(
    UNDO_SCRUB_STATUS.request,
    sagaWrapper(undoScrubStatus, UNDO_SCRUB_STATUS)
  );
}

function* watchUndoScrubStatusSuccess() {
  yield takeEvery(UNDO_SCRUB_STATUS.success, undoScrubStatusSuccess);
}

function* watchRequestClearFilteredScrubbingData() {
  yield takeEvery(
    REQUEST_CLEAR_FILTERED_SCRUBBING_DATA,
    clearFilteredScrubbingData
  );
}

export default [
  watchRequestClearFilteredScrubbingData,
  watchUndoScrubStatusSuccess,
  watchUndoScrubStatus,
  watchSwapProposalDetail,
  watchSwapProposalDetailSuccess,
  watchRequestOverrideStatus,
  watchRequestOverrideStatusSuccess,
  watchRequestClearScrubbingFiltersList,
  watchRequestScrubbingDataFiltered,
  watchRequestPostClientScrubbing,
  watchRequestPostClientScrubbingSuccess
];
