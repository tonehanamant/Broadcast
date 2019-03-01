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
