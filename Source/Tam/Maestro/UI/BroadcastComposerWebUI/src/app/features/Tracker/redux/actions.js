import * as ACTIONS from "./types";

export const getTracker = () => ({
  type: ACTIONS.LOAD_TRACKER.request,
  payload: {}
});

export const getTrackerFiltered = query => ({
  type: ACTIONS.REQUEST_FILTERED_TRACKER,
  payload: query
});

export const getUnlinkedFiltered = query => ({
  type: ACTIONS.REQUEST_FILTERED_UNLINKED,
  payload: query
});

export const getArchivedFiltered = query => ({
  type: ACTIONS.REQUEST_FILTERED_ARCHIVED,
  payload: query
});

export const getTrackerClientScrubbing = params => ({
  type: ACTIONS.LOAD_TRACKER_CLIENT_SCRUBBING.request,
  payload: params
});

export const getScrubbingDataFiltered = query => ({
  type: ACTIONS.REQUEST_FILTERED_SCRUBBING_DATA,
  payload: query
});

export const clearScrubbingFiltersList = () => ({
  type: ACTIONS.REQUEST_CLEAR_SCRUBBING_FILTERS_LIST,
  payload: {}
});

export const getUnlinkedIscis = () => ({
  type: ACTIONS.UNLINKED_ISCIS_DATA.request,
  payload: {}
});

export const overrideStatus = params => ({
  type: ACTIONS.REQUEST_TRACKER_OVERRIDE_STATUS,
  payload: params
});

export const swapProposalDetail = params => ({
  type: ACTIONS.REQUEST_SWAP_PROPOSAL_DETAIL,
  payload: params
});

export const archiveUnlinkedIscis = ids => ({
  type: ACTIONS.ARCHIVE_UNLIKED_ISCI.request,
  payload: { ids }
});

export const loadArchivedIscis = () => ({
  type: ACTIONS.LOAD_ARCHIVED_ISCI.request,
  payload: {}
});

export const loadValidIscis = query => ({
  type: ACTIONS.LOAD_VALID_ISCI.request,
  payload: { query }
});

export const mapUnlinkedIsci = payload => ({
  type: ACTIONS.MAP_UNLINKED_ISCI.request,
  payload
});

export const undoArchivedIscis = ids => ({
  type: ACTIONS.UNDO_ARCHIVED_ISCI.request,
  payload: { ids }
});

// toggle unlinked tabs
const tabsMap = {
  unlinked: getUnlinkedIscis,
  archived: loadArchivedIscis
};

export const toggleUnlinkedTab = tab => {
  const tabFunction = tabsMap[tab];
  if (tabFunction) {
    return tabFunction();
  }
  console.error(
    "You should add function in the tabsMap to load your tab values"
  );
  return undefined;
};

export const rescrubUnlinkedIscis = isci => ({
  type: ACTIONS.RESCRUB_UNLIKED_ISCI.request,
  payload: { isci }
});

export const closeUnlinkedIsciModal = modalPrams => ({
  type: ACTIONS.CLOSE_UNLINKED_ISCI_MODAL,
  payload: { modalPrams }
});

export const undoScrubStatus = (proposalId, scrubIds) => ({
  type: ACTIONS.UNDO_SCRUB_STATUS.request,
  payload: {
    ProposalId: proposalId,
    ScrubIds: scrubIds
  }
});

export const saveActiveScrubData = (newData, fullList) => ({
  type: ACTIONS.SAVE_NEW_CLIENT_SCRUBS,
  payload: { Data: newData, FullData: fullList }
});

export const clearFilteredScrubbingData = () => ({
  type: ACTIONS.CLEAR_FILTERED_SCRUBBING_DATA,
  payload: {}
});

export const getClearScrubbingDataFiltered = () => ({
  type: ACTIONS.REQUEST_CLEAR_FILTERED_SCRUBBING_DATA
});

export const uploadTrackerFile = params => ({
  type: ACTIONS.FILE_UPLOAD.request,
  payload: params
});

export const saveTrackerDisplay = post => ({
  type: ACTIONS.ASSIGN_TRACKER_DISPLAY,
  data: post
});
