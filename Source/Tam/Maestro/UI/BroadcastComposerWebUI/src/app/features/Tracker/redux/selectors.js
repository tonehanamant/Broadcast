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
export const selectActiveClientScrubs = state =>
  state.tracker.proposalHeader.activeScrubbingData.ClientScrubs;
