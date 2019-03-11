import { createAction } from "Utils/action-creator";
import { ducksRoot } from "Tracker/redux";

const ROOT = `${ducksRoot}/unlinked-iscis`;

export const ARCHIVE_UNLIKED_ISCI = createAction(
  `${ROOT}/ARCHIVE_UNLIKED_ISCI`
);
export const UNLINKED_ISCIS_DATA = createAction(`${ROOT}/UNLINKED_ISCIS_DATA`);
export const LOAD_ARCHIVED_ISCI = createAction(`${ROOT}/LOAD_ARCHIVED_ISCI`);
export const LOAD_VALID_ISCI = createAction(`${ROOT}/LOAD_VALID_ISCI`);
export const RESCRUB_UNLIKED_ISCI = createAction(
  `${ROOT}/RESCRUB_UNLIKED_ISCI`
);
export const MAP_UNLINKED_ISCI = createAction(`${ROOT}/MAP_UNLINKED_ISCI`);
export const UNDO_ARCHIVED_ISCI = createAction(`${ROOT}/UNDO_ARCHIVED_ISCI`);
export const CLOSE_UNLINKED_ISCI_MODAL = `${ROOT}/CLOSE_UNLINKED_ISCI_MODAL`;

export const REQUEST_FILTERED_UNLINKED = `${ROOT}/REQUEST_FILTERED_UNLINKED`;
export const RECEIVE_FILTERED_UNLINKED = `${ROOT}/RECEIVE_FILTERED_UNLINKED`;

export const REQUEST_FILTERED_ARCHIVED = `${ROOT}/REQUEST_FILTERED_ARCHIVED`;
export const RECEIVE_FILTERED_ARCHIVED = `${ROOT}/RECEIVE_FILTERED_ARCHIVED`;

export const RECEIVE_CLEAR_ISCI_FILTER = `${ROOT}/RECEIVE_CLEAR_ISCI_FILTER`;

export const REQUEST_UNLINKED_ISCIS_DATA = `${ROOT}/REQUEST_UNLINKED_ISCIS_DATA`;
export const RECEIVE_UNLINKED_ISCIS_DATA = `${ROOT}/RECEIVE_UNLINKED_ISCIS_DATA`;

const initialState = {
  loadingValidIscis: false,
  typeaheadIscisList: [],
  unlinkedIscisData: [],
  archivedIscisData: [],
  unlinkedIscisLength: 0,
  activeIsciFilterQuery: ""
};

// Reducer
export default function reducer(state = initialState, action) {
  const { type, data } = action;

  switch (type) {
    case RECEIVE_FILTERED_UNLINKED:
      return {
        ...state,
        unlinkedIscisData: data.filteredData,
        activeIsciFilterQuery: data.query
      };

    case RECEIVE_FILTERED_ARCHIVED:
      return {
        ...state,
        archivedIscisData: data.filteredData,
        activeIsciFilterQuery: data.query
      };

    case RECEIVE_CLEAR_ISCI_FILTER:
      return {
        ...state,
        activeIsciFilterQuery: ""
      };
    case LOAD_ARCHIVED_ISCI.success:
      return {
        ...state,
        archivedIscisData: data.Data,
        unlinkedFilteredIscis: data.Data
      };
    case UNLINKED_ISCIS_DATA.success:
      return {
        ...state,
        unlinkedIscisData: data.Data,
        unlinkedFilteredIscis: data.Data
      };

    case LOAD_VALID_ISCI.request:
      return {
        ...state,
        loadingValidIscis: true
      };

    case LOAD_VALID_ISCI.success:
      return {
        ...state,
        typeaheadIscisList: data.Data,
        loadingValidIscis: false
      };

    case LOAD_VALID_ISCI.failure:
      return {
        ...state,
        loadingValidIscis: false
      };

    default:
      return state;
  }
}
const getUnlinkedFiltered = query => ({
  type: REQUEST_FILTERED_UNLINKED,
  payload: query
});

const getArchivedFiltered = query => ({
  type: REQUEST_FILTERED_ARCHIVED,
  payload: query
});
const getUnlinkedIscis = () => ({
  type: UNLINKED_ISCIS_DATA.request,
  payload: {}
});

const archiveUnlinkedIscis = ids => ({
  type: ARCHIVE_UNLIKED_ISCI.request,
  payload: { ids }
});

const loadArchivedIscis = () => ({
  type: LOAD_ARCHIVED_ISCI.request,
  payload: {}
});

const loadValidIscis = query => ({
  type: LOAD_VALID_ISCI.request,
  payload: { query }
});

const mapUnlinkedIsci = payload => ({
  type: MAP_UNLINKED_ISCI.request,
  payload
});

const undoArchivedIscis = ids => ({
  type: UNDO_ARCHIVED_ISCI.request,
  payload: { ids }
});

// toggle unlinked tabs
const tabsMap = {
  unlinked: getUnlinkedIscis,
  archived: loadArchivedIscis
};

const toggleUnlinkedTab = tab => {
  const tabFunction = tabsMap[tab];
  if (tabFunction) {
    return tabFunction();
  }
  console.error(
    "You should add function in the tabsMap to load your tab values"
  );
  return undefined;
};

const rescrubUnlinkedIscis = isci => ({
  type: RESCRUB_UNLIKED_ISCI.request,
  payload: { isci }
});

const closeUnlinkedIsciModal = modalPrams => ({
  type: CLOSE_UNLINKED_ISCI_MODAL,
  payload: { modalPrams }
});

const reveiveClearIsciFilter = () => ({
  type: RECEIVE_CLEAR_ISCI_FILTER
});

export const actions = {
  reveiveClearIsciFilter,
  closeUnlinkedIsciModal,
  rescrubUnlinkedIscis,
  toggleUnlinkedTab,
  getUnlinkedFiltered,
  getUnlinkedIscis,
  getArchivedFiltered,
  archiveUnlinkedIscis,
  loadValidIscis,
  loadArchivedIscis,
  mapUnlinkedIsci,
  undoArchivedIscis
};
