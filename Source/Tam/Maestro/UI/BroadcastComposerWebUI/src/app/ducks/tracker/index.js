import * as ACTIONS from './actionTypes.js';

const initialState = {
  loadingValidIscis: false,
  typeaheadIscisList: [],
  tracker: {},
  // trackerGridData: [],
  proposalHeader: {},
  unlinkedIscisData: [],
  archivedIscisData: [],
  modals: {},
  unlinkedIscisLength: 0,
  activeIsciFilterQuery: '',
  activeFilterKey: 'All', // represents global Filter state: 'All', 'InSpec', 'OutOfSpec'
};

// Reducer
export default function reducer(state = initialState, action) {
  const { type, data } = action;

  switch (type) {
    case ACTIONS.TRACKER_RECEIVE_FILTERED_UNLINKED:
      return {
        ...state,
        unlinkedIscisData: data.filteredData,
        activeIsciFilterQuery: data.query,
      };

    case ACTIONS.TRACKER_RECEIVE_FILTERED_ARCHIVED:
      return {
        ...state,
        archivedIscisData: data.filteredData,
        activeIsciFilterQuery: data.query,
      };

    case ACTIONS.TRACKER_RECEIVE_FILTERED_SCRUBBING_DATA:
    return {
      ...state,
      proposalHeader: {
        ...state.proposalHeader,
        activeScrubbingData: {
          ...state.proposalHeader.activeScrubbingData,
          ClientScrubs: data.filteredClientScrubs,
        },
      },
      activeScrubbingFilters: data.activeFilters,
      scrubbingFiltersList: [data.activeFilters],
      hasActiveScrubbingFilters: data.hasActiveScrubbingFilters,
    };

    case ACTIONS.TRACKER_LOAD_ARCHIVED_ISCI.success:
      return {
        ...state,
        archivedIscisData: data.Data,
        unlinkedFilteredIscis: data.Data,
      };
    case ACTIONS.TRACKER_UNLINKED_ISCIS_DATA.success:
    return {
      ...state,
      unlinkedIscisData: data.Data,
      unlinkedFilteredIscis: data.Data,
    };

    case ACTIONS.TRACKER_LOAD_VALID_ISCI.request:
      return {
        ...state,
        loadingValidIscis: true,
      };

    case ACTIONS.TRACKER_LOAD_VALID_ISCI.success:
      return {
        ...state,
        typeaheadIscisList: data.Data,
        loadingValidIscis: false,
      };

    case ACTIONS.TRACKER_LOAD_VALID_ISCI.failure:
      return {
        ...state,
        loadingValidIscis: false,
      };

    default:
      return state;
  }
}

export const uploadTrackerFile = params => ({
  type: ACTIONS.TRACKER_FILE_UPLOAD.request,
  payload: params,
});

export const getUnlinkedFiltered = query => ({
  type: ACTIONS.TRACKER_REQUEST_FILTERED_UNLINKED,
  payload: query,
});

export const getArchivedFiltered = query => ({
  type: ACTIONS.TRACKER_REQUEST_FILTERED_ARCHIVED,
  payload: query,
});

export const archiveUnlinkedIscis = ids => ({
  type: ACTIONS.TRACKER_ARCHIVE_UNLIKED_ISCI.request,
  payload: { ids },
});

export const rescrubUnlinkedIscis = isci => ({
  type: ACTIONS.TRACKER_RESCRUB_UNLIKED_ISCI.request,
  payload: { isci },
});

export const getUnlinkedIscis = () => ({
  type: ACTIONS.TRACKER_UNLINKED_ISCIS_DATA.request,
  payload: {},
});

export const loadArchivedIscis = () => ({
  type: ACTIONS.TRACKER_LOAD_ARCHIVED_ISCI.request,
  payload: {},
});

export const loadValidIscis = query => ({
  type: ACTIONS.TRACKER_LOAD_VALID_ISCI.request,
  payload: { query },
});

export const undoArchivedIscis = ids => ({
  type: ACTIONS.TRACKER_UNDO_ARCHIVED_ISCI.request,
  payload: { ids },
});

export const closeUnlinkedIsciModal = modalPrams => ({
  type: ACTIONS.TRACKER_CLOSE_UNLINKED_ISCI_MODAL,
  payload: { modalPrams },
});

export const mapUnlinkedIsci = payload => ({
  type: ACTIONS.TRACKER_MAP_UNLINKED_ISCI.request,
  payload,
});

// toggle unlinked tabs
const tabsMap = {
  unlinked: getUnlinkedIscis,
  archived: loadArchivedIscis,
};

export const toggleUnlinkedTab = (tab) => {
  const tabFunction = tabsMap[tab];
  if (tabFunction) {
    return tabFunction();
  }
  console.error('You should add function in the tabsMap to load your tab values');
  return undefined;
};
