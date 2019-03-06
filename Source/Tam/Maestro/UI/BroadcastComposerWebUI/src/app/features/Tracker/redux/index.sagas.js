import { takeEvery, put, call, select } from "redux-saga/effects";
import FuzzySearch from "fuzzy-search";
import moment from "moment";
import { forEach, cloneDeep, includes, update } from "lodash";
import {
  setOverlayLoading,
  setOverlayProcessing,
  createAlert,
  toggleModal,
  deployError
} from "Main/redux/index.ducks";
import { selectModal } from "Main/redux/index.saga";
import sagaWrapper from "Utils/saga-wrapper";
import {
  LOAD_TRACKER,
  LOAD_TRACKER_CLIENT_SCRUBBING,
  LOAD_VALID_ISCI,
  UNDO_ARCHIVED_ISCI,
  LOAD_ARCHIVED_ISCI,
  CLOSE_UNLINKED_ISCI_MODAL,
  UNDO_SCRUB_STATUS,
  UNLINKED_ISCIS_DATA,
  ARCHIVE_UNLIKED_ISCI,
  RESCRUB_UNLIKED_ISCI,
  MAP_UNLINKED_ISCI,
  FILE_UPLOAD,
  RECEIVE_FILTERED_TRACKER,
  REQUEST_FILTERED_TRACKER,
  RECEIVE_FILTERED_UNLINKED,
  REQUEST_FILTERED_UNLINKED,
  RECEIVE_FILTERED_ARCHIVED,
  REQUEST_FILTERED_ARCHIVED,
  RECEIVE_CLEAR_SCRUBBING_FILTERS_LIST,
  REQUEST_CLEAR_SCRUBBING_FILTERS_LIST,
  RECEIVE_CLEAR_FILTERED_SCRUBBING_DATA,
  REQUEST_CLEAR_FILTERED_SCRUBBING_DATA,
  RECEIVE_FILTERED_SCRUBBING_DATA,
  REQUEST_FILTERED_SCRUBBING_DATA,
  RECEIVE_TRACKER_OVERRIDE_STATUS,
  RECEIVE_CLEAR_ISCI_FILTER,
  REQUEST_TRACKER_OVERRIDE_STATUS,
  REQUEST_SWAP_PROPOSAL_DETAIL,
  REQUEST_ASSIGN_TRACKER_DISPLAY,
  getTracker,
  saveActiveScrubData,
  saveTrackerDisplay
} from "Tracker/redux/index.ducks";
import api from "API";

/* ////////////////////////////////// */
/* SELECTORS */
/* ////////////////////////////////// */
export const selectActiveScrubs = state =>
  state.tracker.proposalHeader.activeScrubbingData;
export const selectActiveFilterKey = state => state.tracker.activeFilterKey;
export const selectUnfilteredData = state =>
  state.tracker.trackerUnfilteredGridData;
export const selectFilteredIscis = state => state.tracker.unlinkedFilteredIscis;
export const selectActiveScrubbingFilters = state =>
  state.tracker.activeScrubbingFilters;
export const selectClientScrubs = state =>
  state.tracker.proposalHeader.scrubbingData.ClientScrubs;

/* ////////////////////////////////// */
/* SAGAS */
/* ////////////////////////////////// */

/* ////////////////////////////////// */
/* Adjust Tracker Data return */
/* ////////////////////////////////// */

export function adjustTracker(posts) {
  const adjustTracker = posts.map(item => {
    const tracker = item;
    tracker.searchContractId = String(tracker.ContractId);
    tracker.searchSpotsInSpec = String(tracker.SpotsInSpec);
    tracker.searchSpotsOutOfSpec = String(tracker.SpotsOutOfSpec);
    tracker.searchUploadDate = tracker.UploadDate
      ? moment(tracker.UploadDate).format("MM/DD/YYYY")
      : "-";
    return tracker;
  });
  return adjustTracker;
}

export const assignDisplay = posts =>
  posts.map(post => ({
    ...post,
    DisplayUploadDate:
      post.UploadDate !== null
        ? moment(post.UploadDate).format("M/D/YYYY")
        : "-"
  }));

export function* requestTracker() {
  const { getTracker } = api.tracker;
  try {
    yield put(setOverlayLoading({ id: "trackerPosts", loading: true }));
    const { status, data } = yield getTracker();
    update(data, "Data.Post", () => adjustTracker(data.Data.Posts));
    return { status, data };
  } finally {
    yield put(setOverlayLoading({ id: "trackerPosts", loading: false }));
  }
}

/* ////////////////////////////////// */
/* ASSIGN Tracker DISPLAY */
/* ////////////////////////////////// */
export function* assignTrackerDisplay({ payload: request }) {
  try {
    yield put(setOverlayLoading({ id: "trackerPostsDisplay", loading: true }));
    const Posts = yield assignDisplay(request.data.Posts);
    const post = Object.assign({}, request.data, { Posts });
    yield put(saveTrackerDisplay(post));
  } catch (e) {
    if (e.message) {
      yield put(deployError({ message: e.message }));
    }
  } finally {
    yield put(setOverlayLoading({ id: "trackerPostsDisplay", loading: false }));
  }
}

const trackerSearchKeys = [
  "searchContractId",
  "ContractName",
  "Advertiser",
  "searchUploadDate",
  "searchSpotsInSpec",
  "searchSpotsOutOfSpec"
];

const searcher = (data, searchKeys, query) => {
  const searcher = new FuzzySearch(data, searchKeys, {
    caseSensitive: false
  });
  return searcher.search(query);
};

export function* requestTrackerFiltered({ payload: query }) {
  const data = yield select(selectUnfilteredData);
  try {
    const filtered = yield searcher(data, trackerSearchKeys, query);
    yield put({
      type: RECEIVE_FILTERED_TRACKER,
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
    yield put(
      setOverlayLoading({ id: "TrackerScrubbingFilter", loading: true })
    );
    yield put({
      type: RECEIVE_CLEAR_FILTERED_SCRUBBING_DATA,
      data: ret
    });
  } finally {
    yield put(
      setOverlayLoading({ id: "TrackerScrubbingFilter", loading: false })
    );
  }
}

/* ////////////////////////////////// */
/* REQUEST TRACKER CLIENT SCRUBBING */
/* ////////////////////////////////// */
// allow for params (todo from BE) to filterKey All, InSpec, OutOfSpec; optional showModal (from Tracker landing);
// if not from modal show processing, else show loading (loading not shown inside modal)
export function* requestTrackerClientScrubbing(params) {
  const { getTrackerClientScrubbing } = api.tracker;
  try {
    if (params.showModal) {
      yield put(
        setOverlayLoading({ id: "TrackerClientScrubbing", loading: true })
      );
    } else {
      yield put(
        setOverlayProcessing({ id: "TrackerClientScrubbing", processing: true })
      );
    }
    // clear the data so filters grid registers as update - if not from modal update
    if (!params.showModal) {
      yield call(requestClearScrubbingDataFiltersList);
    }
    const { status, data } = yield getTrackerClientScrubbing(params);
    if (params.filterKey) {
      update(data, "Data.filterKey", () => params.filterKey);
    }
    return { status, data };
  } finally {
    if (params.showModal) {
      yield put(
        setOverlayLoading({ id: "TrackerClientScrubbing", loading: false })
      );
    } else {
      yield put(
        setOverlayProcessing({
          id: "TrackerClientScrubbing",
          processing: false
        })
      );
    }
  }
}

export function* requestTrackerClientScrubbingSuccess({ payload: params }) {
  if (params && params.showModal) {
    yield put(
      toggleModal({
        modal: "trackerScrubbingModal",
        active: true,
        properties: {
          titleText: "TRACKER SCRUBBING MODAL",
          bodyText: "Tracker Scrubbing details will be shown here!"
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
    state => state.tracker.proposalHeader.scrubbingData.ClientScrubs
  );
  const listFiltered = yield select(
    state => state.tracker.proposalHeader.activeScrubbingData.ClientScrubs
  );
  const activeFilters = cloneDeep(
    yield select(state => state.tracker.activeScrubbingFilters)
  );
  const originalFilters = yield select(
    state => state.tracker.activeScrubbingFilters
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
    yield put(
      setOverlayProcessing({
        id: "TrackerScrubbingFilter",
        processing: true
      })
    );
    // clear the data so grid registers as update
    yield call(requestClearScrubbingDataFiltersList);
    const filtered = yield applyFilter();

    yield put(
      setOverlayProcessing({
        id: "TrackerScrubbingFilter",
        processing: false
      })
    );
    // if empty show alert - will set to original state
    if (filtered.alertEmpty) {
      const msg = `${
        filtered.actingFilter.filterDisplay
      } Filter will remove all data.`;
      yield put(
        createAlert({
          type: "warning",
          headline: "Filter Not Applied",
          message: msg
        })
      );
    }

    yield put({
      type: RECEIVE_FILTERED_SCRUBBING_DATA,
      data: filtered
    });
  } catch (e) {
    if (e.message) {
      // todo should reset activeFilters (cleared) if error?
      yield put(
        deployError({
          message: e.message
        })
      );
    }
  }
}

/* ////////////////////////////////// */
/* REQUEST TRACKER SCRUBBING HEADER */
/* ////////////////////////////////// */
export function* requestUnlinkedIscis() {
  const { getUnlinkedIscis } = api.tracker;
  try {
    yield put(setOverlayLoading({ id: "TrackerUniqueIscis", loading: true }));
    return yield getUnlinkedIscis();
  } finally {
    yield put(setOverlayLoading({ id: "TrackerUniqueIscis", loading: false }));
  }
}

export function* unlinkedIscisSuccess() {
  const activeQuery = yield select(
    state => state.tracker.activeIsciFilterQuery
  );
  // console.log('unlinked isci active query success>>>>>>>', activeQuery);
  const modal = select(selectModal, "trackerUnlinkedIsciModal");
  if (modal && !modal.active) {
    yield put(
      toggleModal({
        modal: "trackerUnlinkedIsciModal",
        active: true,
        properties: {
          titleText: "TRACKER Unique Iscis",
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
  const activeQuery = yield select(
    state => state.tracker.activeIsciFilterQuery
  );
  if (activeQuery.length) {
    yield call(requestArchivedFiltered, { payload: activeQuery });
  }
}

export function* archiveUnlinkedIsci({ ids }) {
  const { archiveUnlinkedIscis } = api.tracker;
  try {
    yield put(setOverlayLoading({ id: "trackerArchiveIsci", loading: true }));
    return yield archiveUnlinkedIscis(ids);
  } finally {
    yield put(setOverlayLoading({ id: "trackerArchiveIsci", loading: false }));
  }
}

export function* undoArchivedIscis({ ids }) {
  const { undoArchivedIscis } = api.tracker;
  try {
    yield put(setOverlayLoading({ id: "trackerArchiveIsci", loading: true }));
    return yield undoArchivedIscis(ids);
  } finally {
    yield put(setOverlayLoading({ id: "trackerArchiveIsci", loading: false }));
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
/* REQUEST TRACKER OVERRIDE STATUS */
/* ////////////////////////////////// */
export function* requestOverrideStatus({ payload: params }) {
  const { overrideStatus } = api.tracker;

  try {
    yield put(
      setOverlayLoading({
        id: "trackerOverrideStatus",
        loading: true
      })
    );
    // change All for BE to NULL; fix so does not override initial params ReturnStatusFilter
    const adjustParams =
      params.ReturnStatusFilter === "All"
        ? Object.assign({}, params, { ReturnStatusFilter: null })
        : params;
    const response = yield overrideStatus(adjustParams);
    const { status, data } = response;
    const hasActiveScrubbingFilters = yield select(
      state => state.tracker.hasActiveScrubbingFilters
    );
    yield put(
      setOverlayLoading({
        id: "trackerOverrideStatus",
        loading: false
      })
    );
    if (status !== 200) {
      yield put(
        deployError({
          error: "No tracker override status returned.",
          message: `The server encountered an error processing the request (tracker override status). Please try again or contact your administrator to review error logs. (HTTP Status: ${status})`
        })
      );
      throw new Error();
    }
    if (!data.Success) {
      yield put(
        deployError({
          error: "No tracker override status returned.",
          message:
            data.Message ||
            "The server encountered an error processing the request (tracker override status). Please try again or contact your administrator to review error logs."
        })
      );
      throw new Error();
    }
    // if no scrubbing filters - process as receive; else handle filters
    if (hasActiveScrubbingFilters) {
      const scrubs = yield select(
        state => state.tracker.proposalHeader.activeScrubbingData.ClientScrubs
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
        yield select(state => state.tracker.activeScrubbingFilters)
      );
      let adjustedFilters = null;
      // remove so redjust filter options as needed
      if (isRemove) {
        // const activeFilters = cloneDeep(yield select(state => state.tracker.activeScrubbingFilters));
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
        type: RECEIVE_TRACKER_OVERRIDE_STATUS,
        data: ret
      });
    } else {
      // clear the data so grid registers as update
      yield call(requestClearScrubbingDataFiltersList);
      // as currently stands need to reset the filter key on data or is removed : TODO REVISE
      data.Data.filterKey = yield select(
        state => state.tracker.activeFilterKey
      );
      yield put({
        type: LOAD_TRACKER_CLIENT_SCRUBBING.success,
        data
      });
    }
  } catch (e) {
    if (e.response) {
      yield put(
        deployError({
          error: "No tracker override status returned.",
          message:
            "The server encountered an error processing the request (tracker override status). Please try again or contact your administrator to review error logs.",
          exception: e.response.data.ExceptionMessage || ""
        })
      );
    }
    if (!e.response && e.message) {
      yield put(
        deployError({
          message: e.message
        })
      );
    }
  }
}

export function* swapProposalDetail({ payload: params }) {
  const { swapProposalDetail } = api.tracker;

  try {
    yield put(
      setOverlayLoading({
        id: "swapDetail",
        loading: true
      })
    );
    const response = yield swapProposalDetail(params);
    const { status, data } = response;
    yield put(
      setOverlayLoading({
        id: "swapDetail",
        loading: false
      })
    );
    if (status !== 200) {
      yield put(
        deployError({
          error: "No swap proposal detail returned.",
          message: `The server encountered an error processing the request (swap proposal detail). Please try again or contact your administrator to review error logs. (HTTP Status: ${status})`
        })
      );
      throw new Error();
    }
    if (!data.Success) {
      yield put(
        deployError({
          error: "No swap proposal detail returned.",
          message:
            data.Message ||
            "The server encountered an error processing the request (swap proposal detail). Please try again or contact your administrator to review error logs."
        })
      );
      throw new Error();
    }
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
    const id = yield select(
      state => state.tracker.proposalHeader.activeScrubbingData.Id
    );
    const refreshParams = { proposalId: id, showModal: true, filterKey: "All" };
    yield call(requestTrackerClientScrubbing, { payload: refreshParams });
  } catch (e) {
    if (e.response) {
      yield put(
        deployError({
          error: "No swap proposal detail returned.",
          message:
            "The server encountered an error processing the request (swap proposal detail). Please try again or contact your administrator to review error logs.",
          exception: e.response.data.ExceptionMessage || ""
        })
      );
    }
    if (!e.response && e.message) {
      yield put(
        deployError({
          message: e.message
        })
      );
    }
  }
}

export function* loadArchivedIsci() {
  const { getArchivedIscis } = api.tracker;
  try {
    yield put(setOverlayLoading({ id: "loadArchiveIsci", loading: true }));
    return yield getArchivedIscis();
  } finally {
    yield put(setOverlayLoading({ id: "loadArchiveIsci", loading: false }));
  }
}

export function* loadValidIscis({ query }) {
  const { getValidIscis } = api.tracker;
  return yield getValidIscis(query);
}

export function* rescrubUnlinkedIsci({ isci }) {
  const { rescrubUnlinkedIscis } = api.tracker;
  try {
    yield put(setOverlayLoading({ id: "rescrubIsci", loading: true }));
    return yield rescrubUnlinkedIscis(isci);
  } finally {
    yield put(setOverlayLoading({ id: "rescrubIsci", loading: false }));
  }
}

export function* mapUnlinkedIsci(payload) {
  const { mapUnlinkedIscis } = api.tracker;
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
    type: RECEIVE_CLEAR_ISCI_FILTER
  });
  yield put(
    toggleModal({
      modal: "trackerUnlinkedIsciModal",
      active: false,
      properties: modalPrams
    })
  );
  yield put(getTracker());
}

const filterMap = {
  InSpec: 2,
  OutOfSpec: 1
};

export function* undoScrubStatus(payload) {
  const { undoScrubStatus } = api.tracker;
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

/* ////////////////////////////////// */
/* UPLOAD Tracker FILE */
/* ////////////////////////////////// */
export function* uploadTrackerFile(params) {
  const { uploadTracker } = api.tracker;
  try {
    yield put(setOverlayProcessing({ id: "uploadTracker", processing: true }));
    return yield uploadTracker(params);
  } finally {
    yield put(setOverlayProcessing({ id: "uploadTracker", processing: false }));
  }
}

export function* uploadTrackerFileSuccess() {
  yield put(createAlert({ type: "success", headline: "CSV Files Uploaded" }));
}

/* ////////////////////////////////// */
/* WATCHERS */
/* ////////////////////////////////// */

function* watchRequestTracker() {
  yield takeEvery(
    LOAD_TRACKER.request,
    sagaWrapper(requestTracker, LOAD_TRACKER)
  );
}

function* watchRequestAssignTrackerDisplay() {
  yield takeEvery(REQUEST_ASSIGN_TRACKER_DISPLAY, saveTrackerDisplay);
}

function* watchRequestTrackerFiltered() {
  yield takeEvery(REQUEST_FILTERED_TRACKER, requestTrackerFiltered);
}

function* watchRequestUnlinkedFiltered() {
  yield takeEvery(REQUEST_FILTERED_UNLINKED, requestUnlinkedFiltered);
}

function* watchRequestArchivedFiltered() {
  yield takeEvery(REQUEST_FILTERED_ARCHIVED, requestArchivedFiltered);
}

function* watchRequestTrackerClientScrubbing() {
  yield takeEvery(
    LOAD_TRACKER_CLIENT_SCRUBBING.request,
    sagaWrapper(requestTrackerClientScrubbing, LOAD_TRACKER_CLIENT_SCRUBBING)
  );
}

function* watchRequestTrackerClientScrubbingSuccess() {
  yield takeEvery(
    LOAD_TRACKER_CLIENT_SCRUBBING.success,
    requestTrackerClientScrubbingSuccess
  );
}

function* watchRequestScrubbingDataFiltered() {
  yield takeEvery(
    REQUEST_FILTERED_SCRUBBING_DATA,
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
  yield takeEvery(REQUEST_TRACKER_OVERRIDE_STATUS, requestOverrideStatus);
}

function* watchSwapProposalDetail() {
  yield takeEvery(REQUEST_SWAP_PROPOSAL_DETAIL, swapProposalDetail);
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

export function* watchUploadTrackerFile() {
  yield takeEvery(
    FILE_UPLOAD.request,
    sagaWrapper(uploadTrackerFile, FILE_UPLOAD)
  );
}

export function* watchUploadTrackerFileSuccess() {
  yield takeEvery(FILE_UPLOAD.success, uploadTrackerFileSuccess);
}

export default [
  watchUploadTrackerFileSuccess,
  watchUploadTrackerFile,
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
  watchRequestTrackerClientScrubbing,
  watchRequestTrackerClientScrubbingSuccess,
  watchRequestArchivedFiltered,
  watchRequestUnlinkedFiltered,
  watchRequestTrackerFiltered,
  watchRequestAssignTrackerDisplay,
  watchRequestTracker
];
