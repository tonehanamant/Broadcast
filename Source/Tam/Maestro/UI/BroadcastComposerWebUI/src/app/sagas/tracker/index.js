import { takeEvery, put, call, select } from "redux-saga/effects";
import FuzzySearch from "fuzzy-search";
import moment from "moment";
import _ from "lodash";
import sagaWrapper from "Utils/saga-wrapper";
import * as appActions from "Ducks/app/actionTypes";
import * as trackerActions from "Ducks/tracker/actionTypes";
import {
  setOverlayProcessing,
  createAlert,
  setOverlayLoading,
  toggleModal
} from "Ducks/app";
import { selectModal } from "Ducks/app/selectors";
import {
  selectActiveScrubs,
  selectActiveFilterKey
} from "Ducks/tracker/selectors";
import { getTracker, saveActiveScrubData } from "Ducks/tracker";
import api from "API";

const ACTIONS = { ...appActions, ...trackerActions };

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
/* Adjust TRACKER Data return */
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

export function* requestTracker() {
  const { getTracker } = api.tracker;

  try {
    yield put({
      type: ACTIONS.SET_OVERLAY_LOADING,
      overlay: {
        id: "trackerPosts",
        loading: true
      }
    });
    const response = yield getTracker();
    const { status, data } = response;
    yield put({
      type: ACTIONS.SET_OVERLAY_LOADING,
      overlay: {
        id: "trackerPosts",
        loading: false
      }
    });
    if (status !== 200) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "No post returned.",
          message: `The server encountered an error processing the request (post). Please try again or contact your administrator to review error logs. (HTTP Status: ${status})`
        }
      });
      throw new Error();
    }
    if (!data.Success) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "No post returned.",
          message:
            data.Message ||
            "The server encountered an error processing the request (post). Please try again or contact your administrator to review error logs."
        }
      });
      throw new Error();
    }
    // adjust the data for grid handling
    data.Data.Posts = yield adjustTracker(data.Data.Posts);
    yield put({
      type: ACTIONS.RECEIVE_TRACKER,
      data
    });
  } catch (e) {
    if (e.response) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "No post returned.",
          message:
            "The server encountered an error processing the request (post). Please try again or contact your administrator to review error logs.",
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

export function* requestTrackerFiltered({ payload: query }) {
  const trackerListUnfiltered = yield select(
    state => state.tracker.trackerUnfilteredGridData
  );

  const keys = [
    "searchContractId",
    "ContractName",
    "Advertiser",
    "searchUploadDate",
    "searchSpotsInSpec",
    "searchSpotsOutOfSpec"
  ];
  const searcher = new FuzzySearch(trackerListUnfiltered, keys, {
    caseSensitive: false
  });
  const trackerFiltered = () => searcher.search(query);

  try {
    const filtered = yield trackerFiltered();
    yield put({
      type: ACTIONS.RECEIVE_FILTERED_TRACKER,
      data: filtered
    });
  } catch (e) {
    if (e.message) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          message: e.message
        }
      });
    }
  }
}

export function* requestUnlinkedFiltered({ payload: query }) {
  const unlinkedListUnfiltered = yield select(
    state => state.tracker.unlinkedFilteredIscis
  );

  // for each post, convert all properties to string to enable use on FuzzySearch object
  unlinkedListUnfiltered.map(post => Object.keys(post).map(key => post[key]));

  const keys = ["ISCI"];
  const searcher = new FuzzySearch(unlinkedListUnfiltered, keys, {
    caseSensitive: false
  });
  const unlinkedFiltered = () => searcher.search(query);

  try {
    const filtered = yield unlinkedFiltered();
    yield put({
      type: ACTIONS.TRACKER_RECEIVE_FILTERED_UNLINKED,
      data: { query, filteredData: filtered }
    });
  } catch (e) {
    if (e.message) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          message: e.message
        }
      });
    }
  }
}

export function* requestArchivedFiltered({ payload: query }) {
  const archivedListUnfiltered = yield select(
    state => state.tracker.unlinkedFilteredIscis
  );

  // for each post, convert all properties to string to enable use on FuzzySearch object
  archivedListUnfiltered.map(post => Object.keys(post).map(key => post[key]));

  const keys = ["ISCI"];
  const searcher = new FuzzySearch(archivedListUnfiltered, keys, {
    caseSensitive: false
  });
  const archivedFiltered = () => searcher.search(query);

  try {
    const filtered = yield archivedFiltered();
    yield put({
      type: ACTIONS.TRACKER_RECEIVE_FILTERED_ARCHIVED,
      data: { query, filteredData: filtered }
    });
  } catch (e) {
    if (e.message) {
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
/* CLEAR FILTERED SCRUBBING DATA - resets filters */
/* ////////////////////////////////// */
export function* clearFilteredScrubbingData() {
  yield call(requestClearScrubbingDataFiltersList);
  const delay = ms => new Promise(res => setTimeout(res, ms));
  yield delay(500);
  const originalFilters = yield select(
    state => state.tracker.activeScrubbingFilters
  );
  const originalScrubs = yield select(
    state => state.tracker.proposalHeader.scrubbingData.ClientScrubs
  );
  const activeFilters = _.forEach(originalFilters, filter => {
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
  // console.log("clear filters", activeFilters, originalFilters);
  const ret = {
    activeFilters,
    originalScrubs
  };
  try {
    yield put(
      setOverlayLoading({ id: "TrackerScrubbingFilter", loading: true })
    );
    yield put({
      type: ACTIONS.RECEIVE_CLEAR_FILTERED_SCRUBBING_DATA,
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
export function* requestTrackerClientScrubbing({ payload: params }) {
  // console.log('requestTrackerClientScrubbing', params);
  const { getTrackerClientScrubbing } = api.tracker;
  try {
    if (params.showModal) {
      yield put({
        type: ACTIONS.SET_OVERLAY_LOADING,
        overlay: {
          id: "TrackerClientScrubbing",
          loading: true
        }
      });
    } else {
      yield put({
        type: ACTIONS.SET_OVERLAY_PROCESSING,
        overlay: {
          id: "TrackerClientScrubbing",
          processing: true
        }
      });
    }
    // clear the data so filters grid registers as update - if not from modal update
    if (!params.showModal) {
      yield call(requestClearScrubbingDataFiltersList);
    }
    const response = yield getTrackerClientScrubbing(params);
    const { status, data } = response;

    if (params.showModal) {
      yield put({
        type: ACTIONS.SET_OVERLAY_LOADING,
        overlay: {
          id: "TrackerClientScrubbing",
          loading: false
        }
      });
    } else {
      yield put({
        type: ACTIONS.SET_OVERLAY_PROCESSING,
        overlay: {
          id: "TrackerClientScrubbing",
          processing: false
        }
      });
    }
    if (status !== 200) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "No proposal client scrubbing data returned.",
          message: `The server encountered an error processing the request (proposal scrubbing). Please try again or contact your administrator to review error logs. (HTTP Status: ${status})`
        }
      });
      throw new Error();
    }
    if (!data.Success) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "No proposal client scrubbing data returned.",
          message:
            data.Message ||
            "The server encountered an error processing the request (proposal scrubbing). Please try again or contact your administrator to review error logs."
        }
      });
      throw new Error();
    }
    if (params.filterKey) {
      data.Data.filterKey = params.filterKey; // set for ref in store
    }
    // console.log('request post scrubbing>>>>>>>', params, data.Data);
    yield put({
      type: ACTIONS.RECEIVE_TRACKER_CLIENT_SCRUBBING,
      data
    });
    if (params.showModal) {
      yield put({
        type: ACTIONS.TOGGLE_MODAL,
        modal: {
          modal: "trackerScrubbingModal",
          active: true,
          properties: {
            titleText: "TRACKER SCRUBBING MODAL",
            bodyText: "Tracker Scrubbing details will be shown here!"
          }
        }
      });
    }
  } catch (e) {
    if (e.response) {
      yield put({
        type: ACTIONS.DEPLOY_ERROR,
        error: {
          error: "No proposal scrubbing data returned.",
          message:
            "The server encountered an error processing the request (proposal scrubbing). Please try again or contact your administrator to review error logs.",
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
  const activeFilters = _.cloneDeep(
    yield select(state => state.tracker.activeScrubbingFilters)
  );
  const originalFilters = yield select(
    state => state.tracker.activeScrubbingFilters
  );
  const actingFilter = activeFilters[query.filterKey]; // this is undefined
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
      _.forEach(activeFilters, value => {
        if (value.active && ret === true) {
          hasActiveScrubbingFilters = true;
          if (value.type === "filterList") {
            if (value.activeMatch) {
              // just base on one or the other?
              const toMatch = value.matchOptions.inSpec === true;
              ret =
                !_.includes(value.exclusions, item[value.filterKey]) &&
                item[value.matchOptions.matchKey] === toMatch;
              // console.log('filter each', ret, item[value.filterKey]);
            } else {
              ret = !_.includes(value.exclusions, item[value.filterKey]);
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
        id: "TrackerScrubbingFilter",
        processing: true
      }
    });
    // clear the data so grid registers as update
    yield call(requestClearScrubbingDataFiltersList);
    const filtered = yield applyFilter();

    yield put({
      type: ACTIONS.SET_OVERLAY_PROCESSING,
      overlay: {
        id: "TrackerScrubbingFilter",
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
  // console.log('archived isci active query success>>>>>>>', activeQuery);
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
  // console.log('refilterOnOverride', isRemove, status, keys);
  if (isRemove) {
    return clientScrubs.filter(
      item => !_.includes(keys, item.ScrubbingClientId)
    );
  }

  return clientScrubs.map(item => {
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
  // compare new to active filters and change filterOptions, exclusions etc; handle date/time separately
  // return new filterOptions
  // console.log('reset filter options', activeFilters, newFilters);
  const adjustedFilters = {};
  // console.log('current active filters >>>>>>>>>>', activeFilters);
  _.forEach(activeFilters, (filter, key) => {
    if (filter && filter.filterOptions) {
      if (filter.type === "filterList") {
        const newOptions = newFilters[filter.distinctKey];
        // console.log('filter options reset', filter, newOptions);
        if (filter.filterOptions.length) {
          const filterOptions = filter.filterOptions.filter(item =>
            _.includes(newOptions, item.Value)
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
        // adjustedFilters[key] = filter;
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
    yield put({
      type: ACTIONS.SET_OVERLAY_LOADING,
      overlay: {
        id: "trackerOverrideStatus",
        loading: true
      }
    });
    // change All for BE to NULL; fix so does not override initial params ReturnStatusFilter
    const adjustParams =
      params.ReturnStatusFilter === "All"
        ? Object.assign({}, params, { ReturnStatusFilter: null })
        : params;
    //  console.log('adjustParams>>>>>>>>>>>>>', params, adjustParams);
    const response = yield overrideStatus(adjustParams);
    const { status, data } = response;
    const hasActiveScrubbingFilters = yield select(
      state => state.tracker.hasActiveScrubbingFilters
    );
    yield put({
      type: ACTIONS.SET_OVERLAY_LOADING,
      overlay: {
        id: "trackerOverrideStatus",
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
      const activeFilters = _.cloneDeep(
        yield select(state => state.tracker.activeScrubbingFilters)
      );
      let adjustedFilters = null;
      // remove so redjust filter options as needed
      if (isRemove) {
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
        type: ACTIONS.RECEIVE_TRACKER_OVERRIDE_STATUS,
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
        type: ACTIONS.RECEIVE_TRACKER_CLIENT_SCRUBBING,
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
  const { swapProposalDetail } = api.tracker;

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
      state => state.tracker.proposalHeader.activeScrubbingData.Id
    );
    const refreshParams = { proposalId: id, showModal: true, filterKey: "All" };
    yield call(requestTrackerClientScrubbing, { payload: refreshParams });
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
    type: ACTIONS.RECEIVE_CLEAR_ISCI_FILTER
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
/* WATCHERS */
/* ////////////////////////////////// */

export function* watchRequestTracker() {
  yield takeEvery(ACTIONS.REQUEST_TRACKER, requestTracker);
}

export function* watchRequestTrackerFiltered() {
  yield takeEvery(ACTIONS.REQUEST_FILTERED_TRACKER, requestTrackerFiltered);
}

export function* watchRequestUnlinkedFiltered() {
  yield takeEvery(
    ACTIONS.TRACKER_REQUEST_FILTERED_UNLINKED,
    requestUnlinkedFiltered
  );
}

export function* watchRequestArchivedFiltered() {
  yield takeEvery(
    ACTIONS.TRACKER_REQUEST_FILTERED_ARCHIVED,
    requestArchivedFiltered
  );
}

export function* watchRequestTrackerClientScrubbing() {
  yield takeEvery(
    ACTIONS.REQUEST_TRACKER_CLIENT_SCRUBBING,
    requestTrackerClientScrubbing
  );
}

export function* watchRequestScrubbingDataFiltered() {
  yield takeEvery(
    ACTIONS.TRACKER_REQUEST_FILTERED_SCRUBBING_DATA,
    requestScrubbingDataFiltered
  );
}

export function* watchRequestClearScrubbingFiltersList() {
  yield takeEvery(
    ACTIONS.TRACKER_REQUEST_CLEAR_SCRUBBING_FILTERS_LIST,
    requestClearScrubbingDataFiltersList
  );
}

export function* watchRequestUniqueIscis() {
  yield takeEvery(
    [
      ACTIONS.TRACKER_UNLINKED_ISCIS_DATA.request,
      ACTIONS.TRACKER_ARCHIVE_UNLIKED_ISCI.success,
      ACTIONS.TRACKER_RESCRUB_UNLIKED_ISCI.success,
      ACTIONS.TRACKER_MAP_UNLINKED_ISCI.success
    ],
    sagaWrapper(requestUnlinkedIscis, ACTIONS.TRACKER_UNLINKED_ISCIS_DATA)
  );
}

export function* watchRequestUniqueIscisSuccess() {
  yield takeEvery(
    ACTIONS.TRACKER_UNLINKED_ISCIS_DATA.success,
    unlinkedIscisSuccess
  );
}

export function* watchRequestArchivedIscisSuccess() {
  yield takeEvery(
    ACTIONS.TRACKER_LOAD_ARCHIVED_ISCI.success,
    archivedIscisSuccess
  );
}

export function* watchArchiveUnlinkedIsci() {
  yield takeEvery(
    ACTIONS.TRACKER_ARCHIVE_UNLIKED_ISCI.request,
    sagaWrapper(archiveUnlinkedIsci, ACTIONS.TRACKER_ARCHIVE_UNLIKED_ISCI)
  );
}

export function* watchRequestOverrideStatus() {
  yield takeEvery(
    ACTIONS.REQUEST_TRACKER_OVERRIDE_STATUS,
    requestOverrideStatus
  );
}

export function* watchSwapProposalDetail() {
  yield takeEvery(
    ACTIONS.TRACKER_REQUEST_SWAP_PROPOSAL_DETAIL,
    swapProposalDetail
  );
}

export function* watchLoadArchivedIscis() {
  yield takeEvery(
    [
      ACTIONS.TRACKER_LOAD_ARCHIVED_ISCI.request,
      ACTIONS.TRACKER_UNDO_ARCHIVED_ISCI.success
    ],
    sagaWrapper(loadArchivedIsci, ACTIONS.TRACKER_LOAD_ARCHIVED_ISCI)
  );
}

export function* watchLoadValidIscis() {
  yield takeEvery(
    ACTIONS.TRACKER_LOAD_VALID_ISCI.request,
    sagaWrapper(loadValidIscis, ACTIONS.TRACKER_LOAD_VALID_ISCI)
  );
}

export function* watchRescrubUnlinkedIsci() {
  yield takeEvery(
    ACTIONS.TRACKER_RESCRUB_UNLIKED_ISCI.request,
    sagaWrapper(rescrubUnlinkedIsci, ACTIONS.TRACKER_RESCRUB_UNLIKED_ISCI)
  );
}

export function* watchMapUnlinkedIsci() {
  yield takeEvery(
    ACTIONS.TRACKER_MAP_UNLINKED_ISCI.request,
    sagaWrapper(mapUnlinkedIsci, ACTIONS.TRACKER_MAP_UNLINKED_ISCI)
  );
}

export function* watchCloseUnlinkedIsciModal() {
  yield takeEvery(
    ACTIONS.TRACKER_CLOSE_UNLINKED_ISCI_MODAL,
    closeUnlinkedIsciModal
  );
}

export function* watchMapUnlinkedIsciSuccess() {
  yield takeEvery(
    ACTIONS.TRACKER_MAP_UNLINKED_ISCI.success,
    mapUnlinkedIsciSuccess
  );
}

export function* watchUndoArchivedIscis() {
  yield takeEvery(
    ACTIONS.TRACKER_UNDO_ARCHIVED_ISCI.request,
    sagaWrapper(undoArchivedIscis, ACTIONS.TRACKER_UNDO_ARCHIVED_ISCI)
  );
}

export function* watchUndoScrubStatus() {
  yield takeEvery(
    ACTIONS.TRACKER_UNDO_SCRUB_STATUS.request,
    sagaWrapper(undoScrubStatus, ACTIONS.TRACKER_UNDO_SCRUB_STATUS)
  );
}

export function* watchUndoScrubStatusSuccess() {
  yield takeEvery(
    ACTIONS.TRACKER_UNDO_SCRUB_STATUS.success,
    undoScrubStatusSuccess
  );
}

export function* watchUploadTrackerFile() {
  yield takeEvery(
    ACTIONS.TRACKER_FILE_UPLOAD.request,
    sagaWrapper(uploadTrackerFile, ACTIONS.TRACKER_FILE_UPLOAD)
  );
}

export function* watchUploadTrackerFileSuccess() {
  yield takeEvery(
    ACTIONS.TRACKER_FILE_UPLOAD.success,
    uploadTrackerFileSuccess
  );
}

export function* watchRequestClearFilteredScrubbingData() {
  yield takeEvery(
    ACTIONS.REQUEST_CLEAR_FILTERED_SCRUBBING_DATA,
    clearFilteredScrubbingData
  );
}
