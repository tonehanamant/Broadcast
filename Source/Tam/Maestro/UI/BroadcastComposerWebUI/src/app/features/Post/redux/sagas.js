import { takeEvery, put, call, select } from "redux-saga/effects";
import FuzzySearch from "fuzzy-search";
import moment from "moment";
import { forEach, cloneDeep, includes, update } from "lodash";
import {
  setOverlayLoading,
  setOverlayProcessing,
  toggleModal,
  createAlert,
  deployError
} from "Main/redux/ducks";
import { selectModal } from "Main/redux/sagas";
import sagaWrapper from "Utils/saga-wrapper";
import {
  ARCHIVE_UNLIKED_ISCI,
  UNLINKED_ISCIS_DATA,
  LOAD_ARCHIVED_ISCI,
  LOAD_VALID_ISCI,
  MAP_UNLINKED_ISCI,
  UNDO_ARCHIVED_ISCI,
  UNDO_SCRUB_STATUS,
  PROCESS_NTI_FILE,
  RESCRUB_UNLIKED_ISCI,
  LOAD_POST,
  LOAD_POST_CLIENT_SCRUBBING,
  SWAP_PROPOSAL_DETAIL,
  FILTERED_SCRUBBING_DATA,
  POST_OVERRIDE_STATUS,
  REQUEST_CLEAR_FILTERED_SCRUBBING_DATA,
  RECEIVE_CLEAR_FILTERED_SCRUBBING_DATA,
  CLOSE_UNLINKED_ISCI_MODAL,
  REQUEST_ASSIGN_POST_DISPLAY,
  REQUEST_FILTERED_POST,
  RECEIVE_FILTERED_POST,
  REQUEST_FILTERED_UNLINKED,
  RECEIVE_FILTERED_UNLINKED,
  REQUEST_FILTERED_ARCHIVED,
  RECEIVE_FILTERED_ARCHIVED,
  REQUEST_CLEAR_SCRUBBING_FILTERS_LIST,
  RECEIVE_CLEAR_SCRUBBING_FILTERS_LIST,
  getPost,
  saveActiveScrubData,
  reveiveClearIsciFilter,
  reveiveFilteredScrubbingData,
  savePostDisplay
} from "Post/redux/ducks";
import api from "API";

/* ////////////////////////////////// */
/* SELECTORS */
/* ////////////////////////////////// */
export const selectActiveScrubs = state =>
  state.post.proposalHeader.activeScrubbingData;
export const selectActiveFilterKey = state => state.post.activeFilterKey;
export const selectUnfilteredData = state => state.post.postUnfilteredGridData;
export const selectFilteredIscis = state => state.post.unlinkedFilteredIscis;
export const selectActiveScrubbingFilters = state =>
  state.post.activeScrubbingFilters;
export const selectClientScrubs = state =>
  state.post.proposalHeader.scrubbingData.ClientScrubs;
export const selectActiveClientScrubs = state =>
  state.post.proposalHeader.activeScrubbingData.ClientScrubs;
export const selectActiveScrubId = state =>
  state.post.proposalHeader.activeScrubbingData.Id;
export const selectHasActiveScrubbingFilters = state =>
  state.post.hasActiveScrubbingFilters;

/* ////////////////////////////////// */
/* SAGAS */
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
      type: RECEIVE_FILTERED_POST,
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
      type: RECEIVE_FILTERED_UNLINKED,
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
        type: RECEIVE_FILTERED_ARCHIVED,
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
  const originalFilters = cloneDeep(filters);
  const isList = filter.type === "filterList";
  // active -depends on if clearing etc; also now if matching in play
  let isActive = false;
  if (isList) {
    isActive = query.exclusions.length > 0 || query.activeMatch;
    filter.matchOptions = query.matchOptions;
    filter.activeMatch = query.activeMatch;
  } else {
    isActive = query.exclusions; // bool for date/time aired
  }
  filter.active = isActive;
  filter.exclusions = query.exclusions;
  filter.filterOptions = isList
    ? query.filterOptions
    : Object.assign(filter.filterOptions, query.filterOptions);
  const { filteredResult, hasActiveScrubbingFilters } = getFilteredResult(
    listUnfiltered,
    filters
  );
  if (filteredResult.length < 1) {
    return {
      filteredClientScrubs: listFiltered,
      activeFilter: filter,
      activeFilters: originalFilters,
      alertEmpty: true,
      hasActiveScrubbingFilters
    };
  }
  return {
    filteredClientScrubs: filteredResult,
    activeFilter: filter,
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

    yield put(reveiveFilteredScrubbingData(filtered));
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
    data.Data.filterKey = yield select(selectActiveFilterKey);
    yield put({
      type: LOAD_POST_CLIENT_SCRUBBING.success,
      data
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
  yield put(reveiveClearIsciFilter());
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
  const isList = Array.isArray(req.data.Data) || req.data.Data.length;
  yield put(
    toggleModal({
      modal: "confirmModal",
      active: true,
      properties: {
        bodyClass: isList ? "modalBodyScroll" : null,
        titleText: "Upload Complete",
        bodyText: req.data.Message,
        bodyList: isList ? req.data.Data : "",
        closeButtonDisabled: true,
        actionButtonText: "OK",
        actionButtonBsStyle: "success",
        action: () => {},
        dismiss: () => {}
      }
    })
  );
}

/* ////////////////////////////////// */
/* WATCHERS */
/* ////////////////////////////////// */

function* watchRequestPost() {
  yield takeEvery(LOAD_POST.request, sagaWrapper(requestPost, LOAD_POST));
}

function* watchRequestAssignPostDisplay() {
  yield takeEvery(REQUEST_ASSIGN_POST_DISPLAY, savePostDisplay);
}

function* watchRequestPostFiltered() {
  yield takeEvery(REQUEST_FILTERED_POST, requestPostFiltered);
}

function* watchRequestUnlinkedFiltered() {
  yield takeEvery(REQUEST_FILTERED_UNLINKED, requestUnlinkedFiltered);
}

function* watchRequestArchivedFiltered() {
  yield takeEvery(REQUEST_FILTERED_ARCHIVED, requestArchivedFiltered);
}

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

function* watchRequestUniqueIscis() {
  yield takeEvery(
    [
      UNLINKED_ISCIS_DATA.request,
      ARCHIVE_UNLIKED_ISCI.success,
      RESCRUB_UNLIKED_ISCI.success,
      MAP_UNLINKED_ISCI.success
    ],
    sagaWrapper(requestUnlinkedIscis, UNLINKED_ISCIS_DATA)
  );
}

function* watchRequestUniqueIscisSuccess() {
  yield takeEvery(UNLINKED_ISCIS_DATA.success, unlinkedIscisSuccess);
}

function* watchRequestArchivedIscisSuccess() {
  yield takeEvery(LOAD_ARCHIVED_ISCI.success, archivedIscisSuccess);
}

function* watchArchiveUnlinkedIsci() {
  yield takeEvery(
    ARCHIVE_UNLIKED_ISCI.request,
    sagaWrapper(archiveUnlinkedIsci, ARCHIVE_UNLIKED_ISCI)
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

function* watchLoadArchivedIscis() {
  yield takeEvery(
    [LOAD_ARCHIVED_ISCI.request, UNDO_ARCHIVED_ISCI.success],
    sagaWrapper(loadArchivedIsci, LOAD_ARCHIVED_ISCI)
  );
}

function* watchLoadValidIscis() {
  yield takeEvery(
    LOAD_VALID_ISCI.request,
    sagaWrapper(loadValidIscis, LOAD_VALID_ISCI)
  );
}

function* watchRescrubUnlinkedIsci() {
  yield takeEvery(
    RESCRUB_UNLIKED_ISCI.request,
    sagaWrapper(rescrubUnlinkedIsci, RESCRUB_UNLIKED_ISCI)
  );
}

function* watchMapUnlinkedIsci() {
  yield takeEvery(
    MAP_UNLINKED_ISCI.request,
    sagaWrapper(mapUnlinkedIsci, MAP_UNLINKED_ISCI)
  );
}

function* watchCloseUnlinkedIsciModal() {
  yield takeEvery(CLOSE_UNLINKED_ISCI_MODAL, closeUnlinkedIsciModal);
}

function* watchMapUnlinkedIsciSuccess() {
  yield takeEvery(MAP_UNLINKED_ISCI.success, mapUnlinkedIsciSuccess);
}

function* watchUndoArchivedIscis() {
  yield takeEvery(
    UNDO_ARCHIVED_ISCI.request,
    sagaWrapper(undoArchivedIscis, UNDO_ARCHIVED_ISCI)
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

function* watchRequestProcessNtiFile() {
  yield takeEvery(
    PROCESS_NTI_FILE.request,
    sagaWrapper(requestProcessNtiFile, PROCESS_NTI_FILE)
  );
}

function* watchProcessNtiFileSuccess() {
  yield takeEvery(PROCESS_NTI_FILE.success, processNtiFileSuccess);
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
  watchSwapProposalDetailSuccess,
  watchRequestOverrideStatus,
  watchRequestOverrideStatusSuccess,
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
